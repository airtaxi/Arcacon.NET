namespace Arcacon.NET.Models;

/// <summary>
/// 아카콘 패키지 목록에서 표시되는 요약 정보
/// </summary>
public class ArcaconPackageSummary
{
    /// <summary>패키지 고유 번호</summary>
    public int PackageIndex { get; set; }

    /// <summary>패키지명</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>제작자 이름</summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>썸네일 이미지 URL</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>판매(다운로드) 수</summary>
    public int SaleCount { get; set; }
}
