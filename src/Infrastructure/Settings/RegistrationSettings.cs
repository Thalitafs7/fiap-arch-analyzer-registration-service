using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Settings;

public class RegistrationSettings : IRegistrationSettings
{
    public string CallbackBaseUrl { get; }
    public string ProcessingServiceBaseUrl { get; }

    public RegistrationSettings(IConfiguration configuration)
    {
        CallbackBaseUrl = configuration["Services:Registration:BaseUrl"]
            ?? "http://registration-service:5002";
        ProcessingServiceBaseUrl = configuration["Services:Processing:BaseUrl"]
            ?? "http://processing-service:8000";
    }
}
