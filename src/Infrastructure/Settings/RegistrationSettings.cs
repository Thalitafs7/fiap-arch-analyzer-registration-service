using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Settings;

public class RegistrationSettings : IRegistrationSettings
{
    public string CallbackBaseUrl { get; }

    public RegistrationSettings(IConfiguration configuration)
    {
        CallbackBaseUrl = configuration["Services:Registration:BaseUrl"]
            ?? "http://registration-service:5002";
    }
}
