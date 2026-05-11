using Amazon.SQS;
using Amazon.SQS.Model;
using FsCheck;
using FsCheck.Xunit;
using Infrastructure.Services.MessageSQS;
using NSubstitute;
using System.Net;
using UnitTests.Generators;

namespace UnitTests.Infrastructure.Services;

/// <summary>
/// Property-Based Tests for SQSMessageService and SQSManagerService.
///
/// **Validates: Requirements 12.1, 12.2, 12.3, 12.4**
/// </summary>
public class SQSMessageServicePropertyTests
{
    private const string QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/test-queue";

    private static (SQSMessageService sut, IAmazonSQS sqsClient) CreateSut()
    {
        var sqsClient = Substitute.For<IAmazonSQS>();
        var sut = new SQSMessageService(sqsClient, QueueUrl);
        return (sut, sqsClient);
    }

    private static (SQSManagerService sut, IAmazonSQS sqsClient) CreateManagerSut()
    {
        var sqsClient = Substitute.For<IAmazonSQS>();
        var sut = new SQSManagerService(sqsClient, QueueUrl);
        return (sut, sqsClient);
    }

    // -------------------------------------------------------------------------
    // Property 9 (12.1): Send invokes IAmazonSQS.SendMessageAsync with queue URL and message
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any non-empty message string, calling Send SHALL invoke the SQS client
    /// SendMessageAsync with the configured queue URL and the message content.
    ///
    /// **Validates: Requirements 12.1**
    /// </summary>
    [Fact]
    public void Send_AnyNonEmptyMessage_InvokesSqsClientWithQueueUrlAndMessage()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string message) =>
            {
                // Arrange
                var (sut, sqsClient) = CreateSut();

                sqsClient
                    .SendMessageAsync(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>())
                    .Returns(new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK });

                // Act
                sut.Send(message).GetAwaiter().GetResult();

                // Assert: SendMessageAsync called with correct QueueUrl and MessageBody
                sqsClient.Received(1).SendMessageAsync(
                    Arg.Is<SendMessageRequest>(r =>
                        r.QueueUrl == QueueUrl &&
                        r.MessageBody == message),
                    Arg.Any<CancellationToken>());

                return true;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 9 (12.2): Delete with HTTP 200 does not throw
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any valid SQS Message, when the SQS client returns HTTP 200,
    /// Delete SHALL complete without throwing an exception.
    ///
    /// **Validates: Requirements 12.2**
    /// </summary>
    [Fact]
    public void Delete_Http200Response_DoesNotThrow()
    {
        var prop = Prop.ForAll(
            Arb.Generate<NonEmptyString>().Select(s => s.Get).ToArbitrary(),
            (string receiptHandle) =>
            {
                // Arrange
                var (sut, sqsClient) = CreateSut();
                var message = new Message { ReceiptHandle = receiptHandle };

                sqsClient
                    .DeleteMessageAsync(Arg.Any<DeleteMessageRequest>(), Arg.Any<CancellationToken>())
                    .Returns(new DeleteMessageResponse { HttpStatusCode = HttpStatusCode.OK });

                // Act & Assert: no exception thrown
                var exception = Record.ExceptionAsync(() => sut.Delete(message)).GetAwaiter().GetResult();
                return exception == null;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 9 (12.3): Delete with non-200 throws Exception
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any non-200 HTTP status code returned by the SQS client,
    /// Delete SHALL throw an Exception.
    ///
    /// **Validates: Requirements 12.3**
    /// </summary>
    [Fact]
    public void Delete_NonHttp200Response_ThrowsException()
    {
        var nonOkStatuses = new[]
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound
        };

        var arb = (from receiptHandle in Arb.Generate<NonEmptyString>().Select(s => s.Get)
                   from statusIndex in Gen.Choose(0, nonOkStatuses.Length - 1)
                   select (receiptHandle, nonOkStatuses[statusIndex])).ToArbitrary();

        var prop = Prop.ForAll(arb, tuple =>
        {
            var (receiptHandle, statusCode) = tuple;

            // Arrange
            var (sut, sqsClient) = CreateSut();
            var message = new Message { ReceiptHandle = receiptHandle };

            sqsClient
                .DeleteMessageAsync(Arg.Any<DeleteMessageRequest>(), Arg.Any<CancellationToken>())
                .Returns(new DeleteMessageResponse { HttpStatusCode = statusCode });

            // Act & Assert: exception must be thrown
            var exception = Record.ExceptionAsync(() => sut.Delete(message)).GetAwaiter().GetResult();
            return exception != null;
        });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 9 (12.4): SQSManagerService.Send delegates to base Send
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any non-empty message string, SQSManagerService.Send SHALL invoke
    /// the base class Send method, which calls IAmazonSQS.SendMessageAsync
    /// with the configured queue URL and the message content.
    ///
    /// **Validates: Requirements 12.4**
    /// </summary>
    [Fact]
    public void SQSManagerService_Send_DelegatesToBaseSend()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string message) =>
            {
                // Arrange
                var (sut, sqsClient) = CreateManagerSut();

                sqsClient
                    .SendMessageAsync(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>())
                    .Returns(new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK });

                // Act
                sut.Send(message).GetAwaiter().GetResult();

                // Assert: base Send invoked → SendMessageAsync called with correct params
                sqsClient.Received(1).SendMessageAsync(
                    Arg.Is<SendMessageRequest>(r =>
                        r.QueueUrl == QueueUrl &&
                        r.MessageBody == message),
                    Arg.Any<CancellationToken>());

                return true;
            });

        prop.QuickCheckThrowOnFailure();
    }
}
