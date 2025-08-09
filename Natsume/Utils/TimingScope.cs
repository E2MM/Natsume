namespace Natsume.Utils;

public sealed class TimingScope(
    TimeProvider tp,
    // ILogger log, 
    string name
) : IDisposable
{
    //private readonly ILogger _log;
    private readonly long _start = tp.GetTimestamp();

    //_log = log;

    public void Dispose()
    {
        var elapsed = tp.GetElapsedTime(_start);
        Console.WriteLine($"{name} done: {elapsed.TotalMilliseconds}ms");
        
        // if (elapsed > TimeSpan.FromMilliseconds(200)) // soglia “lento”
        //     _log.LogInformation("{Op} slow: {ElapsedMs}ms", name, elapsed.TotalMilliseconds);
        // else
        //     _log.LogDebug("{Op} done: {ElapsedMs}ms", name, elapsed.TotalMilliseconds);
    }
}