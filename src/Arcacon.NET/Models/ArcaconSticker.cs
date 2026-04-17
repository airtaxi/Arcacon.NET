namespace Arcacon.NET.Models;

/// <summary>
/// 아카콘 개별 스티커 정보
/// </summary>
public class ArcaconSticker
{
    /// <summary>스티커 고유 번호</summary>
    public int Id { get; set; }

    /// <summary>스티커 제목</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>스티커 이미지 URL</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>정렬 순번</summary>
    public int SortNumber { get; set; }

    /// <summary>파일 확장자 (mp4, webp, png, gif 등) — ImageUrl에서 자동 추론</summary>
    public string Extension => ArcaconFileNameHelper.GetExtensionFromUrl(ImageUrl);
}
