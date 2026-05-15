namespace Application.Common.Interfaces;

public interface ISQSMessageService
{
    Task Send(string message, string messageGroupId = null, Dictionary<string, string> messageAttributes = null);
}
