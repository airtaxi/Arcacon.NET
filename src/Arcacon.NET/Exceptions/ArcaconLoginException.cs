namespace Arcacon.NET.Exceptions;

/// <summary>
/// 로그인 실패 시 발생하는 예외
/// </summary>
public class ArcaconLoginException : ArcaconException
{
    /// <inheritdoc />
    public ArcaconLoginException()
    {
    }

    /// <inheritdoc />
    public ArcaconLoginException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArcaconLoginException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
