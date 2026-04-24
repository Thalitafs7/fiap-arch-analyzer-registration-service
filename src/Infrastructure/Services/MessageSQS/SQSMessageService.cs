using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.MessageSQS;

public class SQSMessageService : ISQSMessageService
{
    private readonly IAmazonSQS _sQSClient;
    private readonly string _queueUrl;


    public SQSMessageService(IAmazonSQS sQSClient, string queueUrl)
    {
        _sQSClient = sQSClient;
        _queueUrl = queueUrl;
    }


    public async Task Delete(Message message)
    {
        if ((await _sQSClient.DeleteMessage(_queueUrl, message)).HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("Erro ao deletar mensagem da fila SQS");
        }
    }

    public async Task Send(string message, string messageGroupId = null, Dictionary<string, MessageAttributeValue> messageAtributes = null, bool addTracekey = false, string customQueue = null)
    {
        await _sQSClient.SendSQSMessage(customQueue ?? _queueUrl, message, messageGroupId, messageAtributes, addTracekey);
    }
}
