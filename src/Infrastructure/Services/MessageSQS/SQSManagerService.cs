using Application.Common.Interfaces;

namespace Infrastructure.Services.MessageSQS;

public class SQSManagerService : ISQSManagerService
{
    public Task Send(string message) => Task.CompletedTask;
}
