using Amazon.SQS;
using Application.Common.Interfaces;

namespace Infrastructure.Services.MessageSQS;

public class SQSManagerService : SQSMessageService, ISQSManagerService
{

    public SQSManagerService(IAmazonSQS amazonSQS, string queueUrl) : base(amazonSQS, queueUrl)
    {
    }

    public Task Send(string message) => base.Send(message);

}
