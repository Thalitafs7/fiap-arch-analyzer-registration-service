using Amazon.SQS;
using Application.Common.Interfaces;
using Domain.Interfaces;
using Infrastructure.ExternalServices;
using Infrastructure.Http;
using Infrastructure.Logging;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Services.MessageSQS;
using Infrastructure.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddScoped<IErrorRepository, ErrorRepository>();


        var awsOptions = configuration.GetAWSOptions();
        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<Amazon.S3.IAmazonS3>();
        services.AddAWSService<Amazon.SQS.IAmazonSQS>();

        services.Configure<SagaSettings>(configuration.GetSection(SagaSettings.SectionName));

        services.AddMemoryCache();

        services.AddTransient<CorrelationIdHttpMessageHandler>();

        var sqsQueueUrl = configuration["AWS:SQS:QueueUrl"] ?? "";
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileManagerService>(sp => new FileManagerService(sp.GetRequiredService<Amazon.S3.IAmazonS3>(), configuration));
        services.AddScoped<ISQSMessageService>(sp => new SQSMessageService(sp.GetRequiredService<IAmazonSQS>(), sqsQueueUrl));
        services.AddScoped<ISQSManagerService>(sp => new SQSManagerService(sp.GetRequiredService<IAmazonSQS>(), sqsQueueUrl));

        services.AddScoped<IRabbitMQDiagramPublisher, RabbitMQDiagramPublisher>();
        services.AddScoped<IRegistrationSettings, RegistrationSettings>();

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
}
