using Amazon.SQS;
using Amazon.SQS.Model;

namespace Infrastructure.Services.MessageSQS;

public static class SQSExtension
{

    public static async Task<DeleteMessageResponse> DeleteMessage(this IAmazonSQS sqsClient, string queueUrl, Message message)
    {

        var deleteMessageRequest = new DeleteMessageRequest
        {
            QueueUrl = queueUrl,
            ReceiptHandle = message.ReceiptHandle
        };
        return await sqsClient.DeleteMessageAsync(deleteMessageRequest);
    }


    public static async Task<List<Message>> ReceiveMessages(this IAmazonSQS sqsClient, string queueUrl, int maxMessages = 10)
    {
        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = maxMessages
        };
        var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest);
        return response.Messages;
    }

    public static async Task<SendMessageResponse> SendSQSMessage(this IAmazonSQS sqsClient, string queueUrl, string message, string messageGroupId = null, Dictionary<string, MessageAttributeValue> messageAtribute = null, bool addTraceKey = true)
    {
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = message,
            MessageGroupId = messageGroupId,
            MessageAttributes = messageAtribute ?? new Dictionary<string, MessageAttributeValue>()
        };
        return await sqsClient.SendMessageAsync(sendMessageRequest);
    }
}
