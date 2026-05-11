namespace Application.Common.Interfaces;


public interface ISQSManagerService : ISQSMessageService
{
    public Task Send(string message);
}
