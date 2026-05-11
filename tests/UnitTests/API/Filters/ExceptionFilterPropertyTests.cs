using API.Filters;
using Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitTests.Generators;
using UnitTests.Helpers;

namespace UnitTests.API.Filters;

/// <summary>
/// Property-Based Tests for ExceptionFilter.
///
/// **Validates: Requirements 17.1, 17.2, 17.3, 17.4, 17.5, 17.6**
/// </summary>
public class ExceptionFilterPropertyTests
{
    private static ExceptionFilter CreateSut()
    {
        var logger = Substitute.For<ILogger<ExceptionFilter>>();
        return new ExceptionFilter(logger);
    }

    // -------------------------------------------------------------------------
    // Property 17.1: ValidationException → 400 com erros de validação
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any ValidationException, ExceptionFilter SHALL set HTTP status 400
    /// and response body SHALL contain validation errors.
    ///
    /// **Validates: Requirements 17.1, 17.6**
    /// </summary>
    [Fact]
    public void ValidationException_Returns400WithErrors()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string fieldName) =>
            {
                // Arrange
                var failures = new List<ValidationFailure>
                {
                    new ValidationFailure(fieldName, $"{fieldName} é obrigatório")
                };
                var ex = new ValidationException(failures);
                var context = HttpContextTestHelper.CreateExceptionContext(ex);
                var sut = CreateSut();

                // Act
                sut.OnException(context);

                // Assert: status 400
                var result = context.Result as ObjectResult;
                var statusIs400 = result?.StatusCode == 400;

                // Assert: ExceptionHandled = true
                var handled = context.ExceptionHandled;

                // Assert: body contains errors
                var body = result?.Value;
                var bodyType = body?.GetType();
                var errorsProperty = bodyType?.GetProperty("errors");
                var hasErrors = errorsProperty != null;

                return statusIs400 && handled && hasErrors;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 17.2: DomainException → 400 com mensagem
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any DomainException, ExceptionFilter SHALL set HTTP status 400
    /// and response body SHALL contain the exception message.
    ///
    /// **Validates: Requirements 17.2, 17.6**
    /// </summary>
    [Fact]
    public void DomainException_Returns400WithMessage()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string msg) =>
            {
                // Arrange
                var ex = new DomainException(msg);
                var context = HttpContextTestHelper.CreateExceptionContext(ex);
                var sut = CreateSut();

                // Act
                sut.OnException(context);

                // Assert: status 400
                var result = context.Result as ObjectResult;
                var statusIs400 = result?.StatusCode == 400;

                // Assert: ExceptionHandled = true
                var handled = context.ExceptionHandled;

                // Assert: body contains message matching exception
                var body = result?.Value;
                var bodyType = body?.GetType();
                var messageProperty = bodyType?.GetProperty("message");
                var messageValue = messageProperty?.GetValue(body) as string;
                var messageMatches = messageValue == msg;

                return statusIs400 && handled && messageMatches;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 17.3: InvalidOperationException → 400
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any InvalidOperationException, ExceptionFilter SHALL set HTTP status 400.
    ///
    /// **Validates: Requirements 17.3, 17.6**
    /// </summary>
    [Fact]
    public void InvalidOperationException_Returns400()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string msg) =>
            {
                // Arrange
                var ex = new InvalidOperationException(msg);
                var context = HttpContextTestHelper.CreateExceptionContext(ex);
                var sut = CreateSut();

                // Act
                sut.OnException(context);

                // Assert: status 400
                var result = context.Result as ObjectResult;
                var statusIs400 = result?.StatusCode == 400;

                // Assert: ExceptionHandled = true
                var handled = context.ExceptionHandled;

                return statusIs400 && handled;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 17.4: KeyNotFoundException → 404
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any KeyNotFoundException, ExceptionFilter SHALL set HTTP status 404.
    ///
    /// **Validates: Requirements 17.4, 17.6**
    /// </summary>
    [Fact]
    public void KeyNotFoundException_Returns404()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string msg) =>
            {
                // Arrange
                var ex = new KeyNotFoundException(msg);
                var context = HttpContextTestHelper.CreateExceptionContext(ex);
                var sut = CreateSut();

                // Act
                sut.OnException(context);

                // Assert: status 404
                var result = context.Result as ObjectResult;
                var statusIs404 = result?.StatusCode == 404;

                // Assert: ExceptionHandled = true
                var handled = context.ExceptionHandled;

                return statusIs404 && handled;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 17.5: Exception genérica → 500 com "Erro interno do servidor"
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any unhandled Exception, ExceptionFilter SHALL set HTTP status 500
    /// and response body SHALL contain "Erro interno do servidor".
    ///
    /// **Validates: Requirements 17.5, 17.6**
    /// </summary>
    [Fact]
    public void GenericException_Returns500WithInternalServerErrorMessage()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidNome(),
            (string msg) =>
            {
                // Arrange — use Exception base type (not a mapped subtype)
                var ex = new Exception(msg);
                var context = HttpContextTestHelper.CreateExceptionContext(ex);
                var sut = CreateSut();

                // Act
                sut.OnException(context);

                // Assert: status 500
                var result = context.Result as ObjectResult;
                var statusIs500 = result?.StatusCode == 500;

                // Assert: ExceptionHandled = true
                var handled = context.ExceptionHandled;

                // Assert: body message = "Erro interno do servidor"
                var body = result?.Value;
                var bodyType = body?.GetType();
                var messageProperty = bodyType?.GetProperty("message");
                var messageValue = messageProperty?.GetValue(body) as string;
                var hasCorrectMessage = messageValue == "Erro interno do servidor";

                return statusIs500 && handled && hasCorrectMessage;
            });

        prop.QuickCheckThrowOnFailure();
    }

    // -------------------------------------------------------------------------
    // Property 17.6: ExceptionHandled = true para todos os tipos
    // -------------------------------------------------------------------------

    /// <summary>
    /// For all exception types handled by ExceptionFilter, ExceptionHandled SHALL be true.
    ///
    /// **Validates: Requirement 17.6**
    /// </summary>
    [Fact]
    public void AllExceptionTypes_ExceptionHandledIsTrue()
    {
        // Arrange — one representative of each mapped exception type
        var exceptions = new List<Exception>
        {
            new ValidationException(new[] { new ValidationFailure("Field", "Error") }),
            new DomainException("domain error"),
            new InvalidOperationException("invalid op"),
            new KeyNotFoundException("not found"),
            new Exception("generic")
        };

        foreach (var ex in exceptions)
        {
            var context = HttpContextTestHelper.CreateExceptionContext(ex);
            var sut = CreateSut();

            // Act
            sut.OnException(context);

            // Assert
            context.ExceptionHandled.Should().BeTrue(
                because: $"{ex.GetType().Name} should set ExceptionHandled=true");
        }
    }
}
