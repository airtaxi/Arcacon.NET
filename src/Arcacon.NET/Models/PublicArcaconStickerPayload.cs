namespace Arcacon.NET.Models;

/// <summary>
/// 공개 아카콘 API가 반환하는 개별 스티커 항목
/// </summary>
internal sealed class PublicArcaconStickerPayload
{
    public int Id { get; set; }

    public string? ImageUrl { get; set; }
}
