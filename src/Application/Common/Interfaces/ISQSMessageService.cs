using Amazon.SQS.Model;


namespace Application.Common.Interfaces;


public interface ISQSMessageService
{
    Task Delete(Message message);
    Task Send(string message, string messageGroupId = null, Dictionary<string, MessageAttributeValue> messageAtributes = null, bool addTracekey = false, string customQueue = null);
}
