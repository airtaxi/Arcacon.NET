using System.Linq;
using System.Threading.Tasks;
using Arcacon.NET.Models;
using Xunit;

namespace Arcacon.NET.Tests.Integration;

/// <summary>
/// 실제 Arcacon API를 호출하는 통합 테스트.
/// 로그인이 필요한 테스트는 WebView2 UI 환경이 필요하므로 Skip 처리되어 있다.
/// </summary>
[Trait("Category", "Integration")]
public class ArcaconClientIntegrationTests
{
    [Fact]
    public async Task SearchAsync_WithoutLogin_ThrowsInvalidOperationException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.SearchAsync("테스트"));
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ThrowsArgumentException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<ArgumentException>(() => client.SearchAsync(""));
    }

    [Fact]
    public async Task SearchAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SearchAsync("테스트", page: 0));
    }

    [Fact]
    public async Task GetHotListAsync_WithoutLogin_ThrowsInvalidOperationException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetHotListAsync());
    }

    [Fact]
    public async Task GetHotListAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetHotListAsync(0));
    }

    [Fact]
    public async Task GetNewListAsync_WithoutLogin_ThrowsInvalidOperationException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetNewListAsync());
    }

    [Fact]
    public async Task GetNewListAsync_WithInvalidPage_ThrowsArgumentOutOfRangeException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetNewListAsync(0));
    }

    [Fact(Skip = "WebView2 UI 환경에서 직접 실행해야 합니다.")]
    public async Task GetNewListAsync_AfterLogin_ReturnsNonEmptyList()
    {
        await using var client = new ArcaconClient();
        var result = await client.GetNewListAsync();

        Assert.NotEmpty(result.Packages);
    }

    [Fact(Skip = "WebView2 UI 환경에서 직접 실행해야 합니다.")]
    public async Task GetNewListAsync_AfterLogin_ReturnsValidPackageSummaries()
    {
        await using var client = new ArcaconClient();
        var result = await client.GetNewListAsync();

        Assert.All(result.Packages, package =>
        {
            Assert.True(package.PackageIndex > 0);
            Assert.False(string.IsNullOrEmpty(package.Title));
            Assert.False(string.IsNullOrEmpty(package.ThumbnailUrl));
        });
    }

    [Fact(Skip = "WebView2 UI 환경에서 직접 실행해야 합니다.")]
    public async Task SearchAsync_AfterLogin_WithTitleSearch_ReturnsResults()
    {
        await using var client = new ArcaconClient();
        var result = await client.SearchAsync("아카콘", ArcaconSearchType.Title);

        Assert.NotEmpty(result.Packages);
    }

    [Fact]
    public async Task GetPackageDetailAsync_WithoutLogin_ThrowsInvalidOperationException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetPackageDetailAsync(1));
    }

    [Fact]
    public async Task GetPackageDetailAsync_WithInvalidPackageId_ThrowsArgumentOutOfRangeException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetPackageDetailAsync(0));
    }

    [Fact]
    public async Task DownloadStickerAsync_WithNullSticker_ThrowsArgumentNullException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.DownloadStickerAsync(null!));
    }

    [Fact]
    public async Task DownloadStickerStreamAsync_WithNullSticker_ThrowsArgumentNullException()
    {
        await using var client = new ArcaconClient();

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.DownloadStickerStreamAsync(null!));
    }

    [Fact]
    public async Task IsLoggedIn_BeforeLogin_ReturnsFalse()
    {
        await using var client = new ArcaconClient();

        Assert.False(client.IsLoggedIn);
    }
}
