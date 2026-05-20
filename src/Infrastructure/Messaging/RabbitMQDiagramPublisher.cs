using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging;

public class RabbitMQDiagramPublisher : IRabbitMQDiagramPublisher
{
    private const string Exchange = "reports.events";
    private const string RoutingKey = "diagram.uploaded";

    private readonly string _rabbitUrl;
    private readonly ILogger<RabbitMQDiagramPublisher> _logger;

    public RabbitMQDiagramPublisher(IConfiguration configuration, ILogger<RabbitMQDiagramPublisher> logger)
    {
        var host = configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = configuration["RabbitMQ:User"] ?? "hackathon";
        var pass = configuration["RabbitMQ:Password"] ?? "hackathon123";
        var port = configuration["RabbitMQ:Port"] ?? "5672";
        _rabbitUrl = $"amqp://{user}:{pass}@{host}:{port}/";
        _logger = logger;
    }

    public Task PublishAsync(DiagramMessage message)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_rabbitUrl) };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(Exchange, "topic", durable: true);

        var payload = new
        {
            job_id = message.JobId,
            file_name = message.FileName,
            file_b64 = message.FileB64,
            content_type = message.ContentType,
            soat_analysis_id = message.SoatAnalysisId,
            callback_url = message.CallbackUrl,
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        var props = channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2; // persistent

        channel.BasicPublish(Exchange, RoutingKey, props, body);

        _logger.LogInformation(
            "RabbitMQ: diagrama publicado. job_id={JobId}, file={File}, exchange={Exchange}",
            message.JobId, message.FileName, Exchange);

        return Task.CompletedTask;
    }
}
