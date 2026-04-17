namespace Arcacon.NET.Exceptions;

/// <summary>
/// 아카콘 라이브러리의 기본 예외
/// </summary>
public class ArcaconException : Exception
{
    /// <inheritdoc />
    public ArcaconException()
    {
    }

    /// <inheritdoc />
    public ArcaconException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArcaconException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
