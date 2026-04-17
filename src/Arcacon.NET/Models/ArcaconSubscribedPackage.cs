namespace Arcacon.NET.Models;

/// <summary>
/// 구독(보유) 중인 아카콘 패키지 정보
/// </summary>
public class ArcaconSubscribedPackage
{
    /// <summary>패키지 고유 번호</summary>
    public int PackageIndex { get; set; }

    /// <summary>썸네일 이미지 URL</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>현재 사용 중 여부 (설정 페이지의 "사용 중인 아카콘" 목록에 포함된 경우 true)</summary>
    public bool IsActive { get; set; }
}
