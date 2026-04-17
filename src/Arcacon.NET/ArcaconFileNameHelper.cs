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
    /// URL에서 파일 확장자를 추론한다. 추론 불가 시 .webp를 반환한다.
    /// </summary>
    internal static string GetExtensionFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return ".webp";

        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
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
}
