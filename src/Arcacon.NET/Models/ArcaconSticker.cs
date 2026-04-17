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

    /// <summary>비디오 스티커의 원본 비디오 URL (이미지 스티커는 null)</summary>
    public string? VideoUrl { get; set; }

    /// <summary>비디오 스티커의 poster 썸네일 URL (이미지 스티커는 null)</summary>
    public string? PosterThumbnailUrl { get; set; }

    /// <summary>정렬 순번</summary>
    public int SortNumber { get; set; }

    /// <summary>파일 확장자 (mp4, webp, png, gif 등) — ImageUrl에서 자동 추론</summary>
    public string Extension => ArcaconFileNameHelper.GetExtensionFromUrl(ImageUrl);
}
