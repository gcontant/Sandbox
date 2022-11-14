using Microsoft.Extensions.Logging;

namespace CustomerAPI.EndpointFilters;

public class RequestAuditorFilter : IEndpointFilter
{
    private readonly ILogger<RequestAuditorFilter> _logger;

    public RequestAuditorFilter(ILogger<RequestAuditorFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext handlerContext, EndpointFilterDelegate next)
    {
        _logger.LogInformation($"[⚙️] Received a request for: {handlerContext.HttpContext.Request.Path}");
        return await next(handlerContext);
    }
}
