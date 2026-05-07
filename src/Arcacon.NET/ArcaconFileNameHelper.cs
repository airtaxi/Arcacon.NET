namespace Arcacon.NET;

/// <summary>
/// 아카콘 스티커 파일명 처리 유틸리티
/// </summary>
public static class ArcaconFileNameHelper
{
    private static readonly char[] s_invalidFileNameChars = Path.GetInvalidFileNameChars();

    /// <summary>
    /// 파일명으로 사용할 수 없는 문자를 제거한다.
    /// </summary>
    public static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "unnamed";
        return string.Concat(name.Where(character => !s_invalidFileNameChars.Contains(character))).Trim();
    }

    /// <summary>
    /// 스티커의 파일명을 생성한다 (확장자 포함).
    /// </summary>
    public static string GetStickerFileName(Models.ArcaconSticker sticker) => $"{sticker.Id}{sticker.Extension}";

    /// <summary>
    /// 다운로드한 스티커 파일 시그니처를 우선해서 파일명을 생성한다 (확장자 포함).
    /// </summary>
    public static string GetStickerFileName(Models.ArcaconSticker sticker, ReadOnlySpan<byte> imageData) => $"{sticker.Id}{GetExtensionFromFileSignature(imageData) ?? sticker.Extension}";

    /// <summary>
    /// URL에서 파일 확장자를 추론한다. 추론 불가 시 .webp를 반환한다.
    /// </summary>
    internal static string GetExtensionFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return ".webp";

        try
        {
            var stickerUri = new Uri(url);
            var path = stickerUri.AbsolutePath;
            var extension = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(extension)) return extension;
        }
        catch (UriFormatException) { }

        // URL에 쿼리 파라미터가 섞인 경우 직접 탐색
        if (url.Contains(".mp4")) return ".mp4";
        if (url.Contains(".gif")) return ".gif";
        if (url.Contains(".png")) return ".png";
        if (url.Contains(".webp")) return ".webp";

        return ".webp";
    }

    internal static string? GetExtensionFromFileSignature(ReadOnlySpan<byte> imageData)
    {
        if (IsPng(imageData)) return ".png";
        if (IsWebp(imageData)) return ".webp";
        if (IsGif(imageData)) return ".gif";
        if (IsMp4(imageData)) return ".mp4";

        return null;
    }

    private static bool IsPng(ReadOnlySpan<byte> imageData) => imageData.Length >= 8
        && imageData[0] == 0x89
        && imageData[1] == (byte)'P'
        && imageData[2] == (byte)'N'
        && imageData[3] == (byte)'G'
        && imageData[4] == 0x0D
        && imageData[5] == 0x0A
        && imageData[6] == 0x1A
        && imageData[7] == 0x0A;

    private static bool IsWebp(ReadOnlySpan<byte> imageData) => imageData.Length >= 12
        && imageData[0] == (byte)'R'
        && imageData[1] == (byte)'I'
        && imageData[2] == (byte)'F'
        && imageData[3] == (byte)'F'
        && imageData[8] == (byte)'W'
        && imageData[9] == (byte)'E'
        && imageData[10] == (byte)'B'
        && imageData[11] == (byte)'P';

    private static bool IsGif(ReadOnlySpan<byte> imageData) => imageData.Length >= 6
        && imageData[0] == (byte)'G'
        && imageData[1] == (byte)'I'
        && imageData[2] == (byte)'F'
        && imageData[3] == (byte)'8'
        && (imageData[4] == (byte)'7' || imageData[4] == (byte)'9')
        && imageData[5] == (byte)'a';

    private static bool IsMp4(ReadOnlySpan<byte> imageData) => imageData.Length >= 8
        && imageData[4] == (byte)'f'
        && imageData[5] == (byte)'t'
        && imageData[6] == (byte)'y'
        && imageData[7] == (byte)'p';
}
