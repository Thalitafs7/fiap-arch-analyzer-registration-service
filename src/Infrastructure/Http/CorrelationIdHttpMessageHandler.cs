using Application.Common.Interfaces;
using System.Diagnostics;

namespace Infrastructure.Http;


public class CorrelationIdHttpMessageHandler : DelegatingHandler
{
    private readonly ICorrelationIdService _correlationIdService;

    public CorrelationIdHttpMessageHandler(ICorrelationIdService correlationIdService)
    {
        _correlationIdService = correlationIdService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {

        var correlationId = _correlationIdService.GetCorrelationId();
        if (!request.Headers.Contains("X-Correlation-ID"))
        {
            request.Headers.Add("X-Correlation-ID", correlationId.ToString());
        }

        var activity = Activity.Current;
        if (activity != null && !request.Headers.Contains("traceparent"))
        {
            request.Headers.Add("traceparent", activity.Id);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
