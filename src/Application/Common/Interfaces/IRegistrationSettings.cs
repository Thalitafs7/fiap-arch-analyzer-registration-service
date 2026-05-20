namespace Application.Common.Interfaces;

public interface IRegistrationSettings
{
    string CallbackBaseUrl { get; }
    string ProcessingServiceBaseUrl { get; }
}
