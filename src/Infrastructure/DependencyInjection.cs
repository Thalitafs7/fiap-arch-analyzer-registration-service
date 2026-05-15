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
using Infrastructure.Settings;
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
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IDiagramaRepository, DiagramaRepository>();
        services.AddScoped<IAnaliseRepository, AnaliseRepository>();
        services.AddScoped<IRelatorioRepository, RelatorioRepository>();
        services.AddScoped<IErrorRepository, ErrorRepository>();

        services.Configure<SagaSettings>(configuration.GetSection(SagaSettings.SectionName));

        services.AddMemoryCache();

        services.AddTransient<CorrelationIdHttpMessageHandler>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileManagerService, LocalFileManagerService>();

        services.AddScoped<IRabbitMQDiagramPublisher, RabbitMQDiagramPublisher>();
        services.AddScoped<IRegistrationSettings, RegistrationSettings>();

        services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        services.AddScoped(typeof(ILogService<>), typeof(LogService<>));

        services.AddScoped<IWebhookSignatureValidator, WebhookSignatureValidator>();

        return services;
    }
}
