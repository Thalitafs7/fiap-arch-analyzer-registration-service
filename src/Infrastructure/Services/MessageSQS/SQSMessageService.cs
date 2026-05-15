using Application.Common.Interfaces;

namespace Infrastructure.Services.MessageSQS;

public class SQSMessageService : ISQSMessageService
{
    public Task Send(string message, string messageGroupId = null, Dictionary<string, string> messageAttributes = null)
        => Task.CompletedTask;
}
