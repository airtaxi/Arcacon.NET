namespace Arcacon.NET.Exceptions;

/// <summary>
/// HTML/JSON 파싱 중 오류 발생 시 던지는 예외
/// </summary>
public class ArcaconParsingException : ArcaconException
{
    /// <inheritdoc />
    public ArcaconParsingException()
    {
    }

    /// <inheritdoc />
    public ArcaconParsingException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArcaconParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
