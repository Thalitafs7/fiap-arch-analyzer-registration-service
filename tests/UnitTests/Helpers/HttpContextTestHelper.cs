using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace UnitTests.Helpers;

public static class HttpContextTestHelper
{
    /// <summary>
    /// Cria DefaultHttpContext com headers pré-populados.
    /// </summary>
    public static DefaultHttpContext CreateWithHeaders(Dictionary<string, string> headers)
    {
        var context = new DefaultHttpContext();
        foreach (var (key, value) in headers)
            context.Request.Headers[key] = value;
        return context;
    }

    /// <summary>
    /// Cria ExceptionContext com a exceção fornecida, usando DefaultHttpContext vazio.
    /// </summary>
    public static ExceptionContext CreateExceptionContext(Exception ex)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };
    }
}
