using Arcacon.NET.Exceptions;
using Arcacon.NET.Models;

namespace Arcacon.NET.Tests;

public class ArcaconClientTests
{
    [Fact]
    public void ApplyPublicStickerImageUrlsFromPublicPayload_ReplacesEveryStickerImageUrl()
    {
        var packageDetail = new ArcaconPackageDetail
        {
            Stickers =
            [
                new ArcaconSticker { Id = 101, ImageUrl = "https://example.com/old-101.webp" },
                new ArcaconSticker { Id = 102, ImageUrl = "https://example.com/old-102.mp4", PosterThumbnailUrl = "https://example.com/poster-102.webp" }
            ]
        };

        const string publicPackageStickerPayload = """
            [
              { "id": 101, "imageUrl": "//cdn.example.com/updated-101.webp" },
              { "id": 102, "imageUrl": "https://cdn.example.com/updated-102.mp4" }
            ]
            """;

        ArcaconClient.ApplyPublicStickerImageUrlsFromPublicPayload(packageDetail, publicPackageStickerPayload);

        Assert.Equal("https://cdn.example.com/updated-101.webp", packageDetail.Stickers[0].ImageUrl);
        Assert.Equal("https://cdn.example.com/updated-102.mp4", packageDetail.Stickers[1].ImageUrl);
        Assert.Equal("https://example.com/poster-102.webp", packageDetail.Stickers[1].PosterThumbnailUrl);
    }

    [Fact]
    public void ApplyPublicStickerImageUrlsFromPublicPayload_WhenStickerIsMissing_ThrowsArcaconParsingException()
    {
        var packageDetail = new ArcaconPackageDetail
        {
            Stickers =
            [
                new ArcaconSticker { Id = 101, ImageUrl = "https://example.com/old-101.webp" },
                new ArcaconSticker { Id = 102, ImageUrl = "https://example.com/old-102.mp4" }
            ]
        };

        const string publicPackageStickerPayload = """
            [
              { "id": 101, "imageUrl": "//cdn.example.com/updated-101.webp" }
            ]
            """;

        var exception = Assert.Throws<ArcaconParsingException>(() =>
            ArcaconClient.ApplyPublicStickerImageUrlsFromPublicPayload(packageDetail, publicPackageStickerPayload));

        Assert.Contains("102", exception.Message);
    }
}
