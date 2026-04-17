namespace Arcacon.NET.Models;

/// <summary>
/// 아카콘 패키지 목록 조회 결과 (페이지네이션 포함)
/// </summary>
public class ArcaconSearchResult
{
    /// <summary>총 패키지 수</summary>
    public int TotalCount { get; set; }

    /// <summary>현재 페이지 번호</summary>
    public int CurrentPage { get; set; }

    /// <summary>총 페이지 수</summary>
    public int TotalPages { get; set; }

    /// <summary>아카콘 패키지 요약 목록</summary>
    public List<ArcaconPackageSummary> Packages { get; set; } = [];
}
