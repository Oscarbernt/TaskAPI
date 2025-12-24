namespace TaskHub.API.Infrastructure.Logging
{
    internal static partial class RequestLog
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Incoming Request: {Method} {Path}")]
        public static partial void IncomingRequest(ILogger logger, string method, string path);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Completed HTTP {Method} {Path} with {StatusCode} in {ElapsedMs}ms")]
        public static partial void OutgoingResponse(ILogger logger, string method, string path, int statusCode, long elapsedMs);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Unhandled exception while processing {Method} {Path}")]
        public static partial void UnhandledException(ILogger logger, string method, string path, Exception exception);
    }
}
