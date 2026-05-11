using Application.Common.Behaviors;
using Domain.Entities;
using Domain.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitTests.Generators;

namespace UnitTests.Application.Behaviors;

/// <summary>
/// Property-Based Tests for ExceptionHandlingBehavior.
///
/// **Validates: Requirements 7.1, 7.2, 7.3, 7.4**
/// </summary>
public class ExceptionHandlingBehaviorPropertyTests
{
    private readonly IErrorRepository _errorRepository = Substitute.For<IErrorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<ExceptionHandlingBehavior<TestRequest, TestResponse>> _logger =
        Substitute.For<ILogger<ExceptionHandlingBehavior<TestRequest, TestResponse>>>();

    private ExceptionHandlingBehavior<TestRequest, TestResponse> CreateSut() =>
        new(_errorRepository, _unitOfWork, _logger);

    // -------------------------------------------------------------------------
    // Property 7.1 + 7.2: exception → AdicionarAsync + CommitAsync called
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any exception thrown by next delegate, ExceptionHandlingBehavior SHALL persist
    /// an Error entity via IErrorRepository.AdicionarAsync and call IUnitOfWork.CommitAsync.
    ///
    /// **Validates: Requirements 7.1, 7.2**
    /// </summary>
    [Fact]
    public void WhenNextThrows_ErrorPersistedAndCommitCalled()
    {
        var prop = Prop.ForAll(
            DomainGenerators.AnyException(),
            (Exception ex) =>
            {
                // Arrange — fresh mocks per iteration
                var errorRepo = Substitute.For<IErrorRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logger = Substitute.For<ILogger<ExceptionHandlingBehavior<TestRequest, TestResponse>>>();

                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);
                errorRepo.AdicionarAsync(Arg.Any<Error>(), Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

                var sut = new ExceptionHandlingBehavior<TestRequest, TestResponse>(
                    errorRepo, unitOfWork, logger);

                RequestHandlerDelegate<TestResponse> next = () => throw ex;

                // Act
                try
                {
                    sut.Handle(new TestRequest(), next, CancellationToken.None)
                       .GetAwaiter().GetResult();
                }
                catch
                {
                    // expected — we verify side effects below
                }

                // Assert: AdicionarAsync called once with an Error entity
                errorRepo.Received(1).AdicionarAsync(
                    Arg.Any<Error>(), Arg.Any<CancellationToken>());

                // Assert: CommitAsync called once after persisting Error
                unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());

                return true;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 7.3: original exception is re-thrown
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any exception thrown by next delegate, ExceptionHandlingBehavior SHALL re-throw
    /// the original exception (same type and message).
    ///
    /// **Validates: Requirement 7.3**
    /// </summary>
    [Fact]
    public void WhenNextThrows_OriginalExceptionIsRethrown()
    {
        var prop = Prop.ForAll(
            DomainGenerators.AnyException(),
            (Exception ex) =>
            {
                // Arrange
                var errorRepo = Substitute.For<IErrorRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logger = Substitute.For<ILogger<ExceptionHandlingBehavior<TestRequest, TestResponse>>>();

                unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);
                errorRepo.AdicionarAsync(Arg.Any<Error>(), Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

                var sut = new ExceptionHandlingBehavior<TestRequest, TestResponse>(
                    errorRepo, unitOfWork, logger);

                RequestHandlerDelegate<TestResponse> next = () => throw ex;

                Exception? caught = null;
                try
                {
                    sut.Handle(new TestRequest(), next, CancellationToken.None)
                       .GetAwaiter().GetResult();
                }
                catch (Exception thrownEx)
                {
                    caught = thrownEx;
                }

                // Assert: same exception type and message re-thrown
                return caught != null
                    && caught.GetType() == ex.GetType()
                    && caught.Message == ex.Message;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 7.4: success path — no Error persisted
    // -------------------------------------------------------------------------

    /// <summary>
    /// When next delegate completes successfully, ExceptionHandlingBehavior SHALL return
    /// the response without persisting any Error.
    ///
    /// **Validates: Requirement 7.4**
    /// </summary>
    [Fact]
    public void WhenNextSucceeds_NoErrorPersisted_ResponseReturned()
    {
        var prop = Prop.ForAll(
            Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary(),
            (Guid responseId) =>
            {
                // Arrange
                var errorRepo = Substitute.For<IErrorRepository>();
                var unitOfWork = Substitute.For<IUnitOfWork>();
                var logger = Substitute.For<ILogger<ExceptionHandlingBehavior<TestRequest, TestResponse>>>();

                var expectedResponse = new TestResponse { Id = responseId };

                var sut = new ExceptionHandlingBehavior<TestRequest, TestResponse>(
                    errorRepo, unitOfWork, logger);

                RequestHandlerDelegate<TestResponse> next =
                    () => Task.FromResult(expectedResponse);

                // Act
                var result = sut.Handle(new TestRequest(), next, CancellationToken.None)
                                .GetAwaiter().GetResult();

                // Assert: no Error persisted, no CommitAsync, response returned as-is
                errorRepo.DidNotReceive().AdicionarAsync(
                    Arg.Any<Error>(), Arg.Any<CancellationToken>());

                unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());

                return result == expectedResponse;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Helper types
    // -------------------------------------------------------------------------

    public class TestRequest : IRequest<TestResponse> { }

    public class TestResponse
    {
        public Guid Id { get; set; }
    }
}
