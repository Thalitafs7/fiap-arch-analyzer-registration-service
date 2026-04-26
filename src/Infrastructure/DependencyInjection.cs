using Amazon.SQS;
using Application.Common.Interfaces;
using Domain.Interfaces;
using Infrastructure.ExternalServices;
using Infrastructure.Http;
using Infrastructure.Logging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Services.MessageSQS;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Diagnostics.CodeAnalysis;





namespace Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool skipDbContext = false,
        bool skipMassTransit = false)
    {
        if (!skipDbContext)
        {
            services.AddDbContext<OrdensDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null
                        );
                    }
                );
            });
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IDiagramaRepository, DiagramaRepository>();
        services.AddScoped<IAnaliseRepository, AnaliseRepository>();
        services.AddScoped<IRelatorioRepository, RelatorioRepository>();


        var awsOptions = configuration.GetAWSOptions();
        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<Amazon.S3.IAmazonS3>();
        services.AddAWSService<Amazon.SQS.IAmazonSQS>();


        //var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? "us-east-1");
        //var accessKey = configuration["AWS:AccessKey"];
        //var secretKey = configuration["AWS:SecretKey"];
        //var credentials = new BasicAWSCredentials(accessKey, secretKey);

        //services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(credentials, region));
        //services.AddSingleton<IAmazonSQS>(sp => new AmazonSQSClient(credentials, region));

        var brokerProvider = configuration["MessageBroker:Provider"];
        if (brokerProvider == "SQS")
        {
            services.AddScoped<ISQSMessageService, SQSMessageService>();
            // garantir registro do IAmazonSQS (via AddAWSService ou manual)
        }
        else
        {
            // registrar implementação alternativa ou não registrar
        }


        var mongoConnectionString = configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017";
        var mongoDatabaseName = configuration["MongoDB:DatabaseName"] ?? "ordens_db";
        services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));


        services.Configure<SagaSettings>(configuration.GetSection(SagaSettings.SectionName));

        services.AddMemoryCache();

        services.AddTransient<CorrelationIdHttpMessageHandler>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileManagerService, FileManagerService>();
        services.AddScoped<ISQSMessageService>(services => new SQSMessageService(services.GetService<IAmazonSQS>(), "filaSQS"));
        services.AddScoped<ISQSManagerService>(services => new SQSManagerService(services.GetService<IAmazonSQS>(), "filaSQS"));



        //// C# - dentro de AddInfrastructure (quando brokerProvider == "SQS")
        //var queueUrl = configuration["AWS:QueueUrl"];
        //if (string.IsNullOrWhiteSpace(queueUrl))
        //{
        //    throw new InvalidOperationException("AWS:QueueUrl não configurada.");
        //}

        //services.AddScoped<ISQSMessageService>(sp =>
        //{
        //    var sqs = sp.GetRequiredService<IAmazonSQS>();
        //    return new SQSMessageService(sqs, queueUrl);
        //});

        //services.AddScoped<ISQSManagerService>(sp =>
        //{
        //    var sqs = sp.GetRequiredService<IAmazonSQS>();
        //    return new SQSManagerService(sqs, queueUrl);
        //});


        services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        services.AddScoped(typeof(ILogService<>), typeof(LogService<>));

        services.AddScoped<IWebhookSignatureValidator, WebhookSignatureValidator>();


        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            var brokerProvider = configuration["MessageBroker:Provider"];

            if (brokerProvider == "SQS")
            {
                x.UsingAmazonSqs((context, cfg) =>
                {
                    var region = configuration["AWS:Region"] ?? "us-east-1";
                    cfg.Host(region, h =>
                    {
                        h.AccessKey(configuration["AWS:AccessKey"]);
                        h.SecretKey(configuration["AWS:SecretKey"]);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
                    var rabbitUser = configuration["RabbitMQ:User"] ?? "guest";
                    var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username(rabbitUser);
                        h.Password(rabbitPass);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Intervals(
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(15),
                            TimeSpan.FromSeconds(30));
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger? logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogWarning(
                        "HTTP Retry {RetryCount}/3 após {Delay}ms. Motivo: {Reason}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger? logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    logger?.LogError(
                        "Circuit Breaker ABERTO por {Duration}s. Motivo: {Reason}",
                        duration.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                },
                onReset: () =>
                {
                    logger?.LogInformation("Circuit Breaker FECHADO - serviço recuperado");
                },
                onHalfOpen: () =>
                {
                    logger?.LogWarning("Circuit Breaker HALF-OPEN - testando serviço");
                });
    }
}
