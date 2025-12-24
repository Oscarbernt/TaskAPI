using System.Diagnostics;

namespace TaskHub.API.Infrastructure.Logging
{
    public sealed class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var sw = Stopwatch.StartNew();

            try
            {
                RequestLog.IncomingRequest(_logger, context.Request.Method, context.Request.Path);

                await _next(context);

                RequestLog.OutgoingResponse(_logger, context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                RequestLog.UnhandledException(_logger, context.Request.Method, context.Request.Path, exception);
                throw;
            }
        }
    }
}
