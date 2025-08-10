using Microsoft.Extensions.Logging;

namespace Natsume.Utils;

public sealed class TimedOperationScope<T>(
    ILogger<T> logger,
    TimeProvider timeProvider,
    string operation,
    int warningThresholdMilliseconds
) : IDisposable
{
    private readonly long _operationStartTime = timeProvider.GetTimestamp();
    private bool _disposed;
    private bool _failed;

    public void MarkFailed() => _failed = true;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        var elapsed = timeProvider.GetElapsedTime(_operationStartTime);
        var logLevel = (_failed, warningThresholdMilliseconds) switch
        {
            (_failed: true, _) => LogLevel.Error,
            (_, warningThresholdMilliseconds: 0) => LogLevel.Information,
            (_, _) when elapsed.TotalMilliseconds > warningThresholdMilliseconds => LogLevel.Warning,
            _ => LogLevel.Information,
        };

        logger.Log(
            logLevel: logLevel,
            message: "⏱️ {ElapsedTotalMilliseconds}ms {Operation}",
            elapsed.TotalMilliseconds.ToString(format: "N0"),
            operation
        );
    }
}

public static class TimedOperationScopeLoggerExtensions
{
    public static IDisposable BeginTimedOperationScope<T>(
        this ILogger<T> logger,
        TimeProvider timeProvider,
        string operation,
        int warningThresholdMilliseconds = 0
    )
    {
        return new TimedOperationScope<T>(logger, timeProvider, operation, warningThresholdMilliseconds);
    }
}