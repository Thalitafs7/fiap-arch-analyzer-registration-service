namespace Application.Common.Interfaces;

public interface ISQSManagerService
{
    Task Send(string message);
}
