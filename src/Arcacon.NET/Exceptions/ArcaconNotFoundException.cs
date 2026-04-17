namespace Arcacon.NET.Exceptions;

/// <summary>
/// 요청한 아카콘 패키지를 찾을 수 없을 때 발생하는 예외
/// </summary>
public class ArcaconNotFoundException : ArcaconException
{
    /// <inheritdoc />
    public ArcaconNotFoundException()
    {
    }

    /// <inheritdoc />
    public ArcaconNotFoundException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArcaconNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
