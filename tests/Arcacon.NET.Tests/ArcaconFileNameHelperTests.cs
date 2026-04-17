using Arcacon.NET.Models;
using Xunit;

namespace Arcacon.NET.Tests;

public class ArcaconFileNameHelperTests
{
    [Fact]
    public void SanitizeFileName_WithValidName_ReturnsSameName()
    {
        var result = ArcaconFileNameHelper.SanitizeFileName("테스트 패키지");

        Assert.Equal("테스트 패키지", result);
    }

    [Fact]
    public void SanitizeFileName_WithInvalidChars_RemovesInvalidChars()
    {
        var result = ArcaconFileNameHelper.SanitizeFileName("test<>:\"/\\|?*file");

        Assert.Equal("testfile", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SanitizeFileName_WithEmptyInput_ReturnsUnnamed(string? input)
    {
        var result = ArcaconFileNameHelper.SanitizeFileName(input!);

        Assert.Equal("unnamed", result);
    }

    [Fact]
    public void SanitizeFileName_WithTrailingSpaces_TrimsSpaces()
    {
        var result = ArcaconFileNameHelper.SanitizeFileName("  테스트  ");

        Assert.Equal("테스트", result);
    }

    [Fact]
    public void GetStickerFileName_WithWebpUrl_ReturnsWebpExtension()
    {
        var sticker = new ArcaconSticker { Id = 42, ImageUrl = "https://example.com/images/42.webp" };

        var result = ArcaconFileNameHelper.GetStickerFileName(sticker);

        Assert.Equal("42.webp", result);
    }

    [Fact]
    public void GetStickerFileName_WithGifUrl_ReturnsGifExtension()
    {
        var sticker = new ArcaconSticker { Id = 10, ImageUrl = "https://example.com/images/10.gif" };

        var result = ArcaconFileNameHelper.GetStickerFileName(sticker);

        Assert.Equal("10.gif", result);
    }

    [Fact]
    public void GetStickerFileName_WithMp4Url_ReturnsMp4Extension()
    {
        var sticker = new ArcaconSticker { Id = 5, ImageUrl = "https://example.com/images/5.mp4" };

        var result = ArcaconFileNameHelper.GetStickerFileName(sticker);

        Assert.Equal("5.mp4", result);
    }

    [Fact]
    public void GetStickerFileName_WithPngUrl_ReturnsPngExtension()
    {
        var sticker = new ArcaconSticker { Id = 7, ImageUrl = "https://example.com/images/7.png" };

        var result = ArcaconFileNameHelper.GetStickerFileName(sticker);

        Assert.Equal("7.png", result);
    }

    [Fact]
    public void GetStickerFileName_WithEmptyUrl_ReturnsDefaultWebpExtension()
    {
        var sticker = new ArcaconSticker { Id = 1, ImageUrl = "" };

        var result = ArcaconFileNameHelper.GetStickerFileName(sticker);

        Assert.Equal("1.webp", result);
    }

    [Fact]
    public void GetStickerFileName_WithUrlContainingQueryString_ReturnsCorrectExtension()
    {
        var sticker = new ArcaconSticker { Id = 99, ImageUrl = "https://example.com/images/99.webp?v=1" };

        var result = ArcaconFileNameHelper.GetStickerFileName(sticker);

        Assert.Equal("99.webp", result);
    }

    [Fact]
    public void GetStickerFileName_WithUrlContainingWebpInQuery_ReturnsWebpExtension()
    {
        var sticker = new ArcaconSticker { Id = 3, ImageUrl = "https://example.com/image?type=.webp&id=3" };

        var result = ArcaconFileNameHelper.GetStickerFileName(sticker);

        Assert.Equal("3.webp", result);
    }
}
