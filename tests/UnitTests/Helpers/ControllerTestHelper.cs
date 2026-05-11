using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.Helpers;

public static class ControllerTestHelper
{
    /// <summary>
    /// Cria instância de controller injetando IMediator e, se o construtor exigir, ILogger&lt;T&gt;.
    /// Suporta construtores com assinatura (IMediator) ou (IMediator, ILogger&lt;T&gt;).
    /// </summary>
    public static T CreateController<T>(IMediator mediator) where T : ControllerBase
    {
        var type = typeof(T);
        var loggerType = typeof(ILogger<T>);

        // Tenta construtor (IMediator, ILogger<T>)
        var ctorWithLogger = type.GetConstructor(new[] { typeof(IMediator), loggerType });
        if (ctorWithLogger is not null)
        {
            var logger = Substitute.For<ILogger<T>>();
            return (T)ctorWithLogger.Invoke(new object[] { mediator, logger });
        }

        // Tenta construtor (IMediator)
        var ctorMediator = type.GetConstructor(new[] { typeof(IMediator) });
        if (ctorMediator is not null)
        {
            return (T)ctorMediator.Invoke(new object[] { mediator });
        }

        throw new InvalidOperationException(
            $"Nenhum construtor compatível encontrado em {type.Name}. " +
            "Esperado: (IMediator) ou (IMediator, ILogger<T>).");
    }
}
