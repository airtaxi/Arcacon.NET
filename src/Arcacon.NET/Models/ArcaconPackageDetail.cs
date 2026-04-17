namespace Arcacon.NET.Models;

/// <summary>
/// 아카콘 패키지 상세 정보
/// </summary>
public class ArcaconPackageDetail
{
    /// <summary>패키지 고유 번호</summary>
    public int PackageIndex { get; set; }

    /// <summary>판매(다운로드) 수</summary>
    public int SaleCount { get; set; }

    /// <summary>스티커 수 (Stickers 목록 크기에서 자동 계산)</summary>
    public int IconCount => Stickers.Count;

    /// <summary>패키지명</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>제작자 이름</summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>등록일 (긴 형식, 예: 2025-10-24 08:40:34)</summary>
    public string RegistrationDate { get; set; } = string.Empty;

    /// <summary>등록일 (짧은 형식, 예: 2025.10.24)</summary>
    public string RegistrationDateShort { get; set; } = string.Empty;

    /// <summary>구매 가격 (포인트)</summary>
    public int Price { get; set; }

    /// <summary>패키지에 포함된 스티커 목록</summary>
    public List<ArcaconSticker> Stickers { get; set; } = [];

    /// <summary>태그 목록</summary>
    public List<string> Tags { get; set; } = [];
}
