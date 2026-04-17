namespace Arcacon.NET.Sample.ViewModels;

public sealed class LogViewModel(DateTime timestamp, string message)
{
    public DateTime Timestamp { get; } = timestamp;
    public string TimestampText { get; } = $"[{timestamp:HH:mm:ss}]";
    public string Message { get; } = message;
}
