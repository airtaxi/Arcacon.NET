using System.Threading.Tasks;
using Arcacon.NET.Exceptions;
using Arcacon.NET.Models;
using Arcacon.NET.Parsing;
using Xunit;

namespace Arcacon.NET.Tests.Parsing;

public class HtmlParserTests
{
    private const string ValidDetailHtml = """
        <!DOCTYPE html>
        <html>
        <body>
        <div class="emoticon-info">
            <div class="title">테스트 아카콘 패키지</div>
            <div class="author">테스트 제작자</div>
            <div class="date"><time datetime="2025-10-24T08:40:34.000Z">2025-10-24 08:40:34</time></div>
            <div class="count">Sale <span>15213</span></div>
        </div>
        <div class="emoticon-tags">
            <a href="/e/?target=tag&keyword=2D" class="tag">#2D</a>
            <a href="/e/?target=tag&keyword=스텔라소라" class="tag">#스텔라소라</a>
        </div>
        <form action="/e/123/buy" method="POST">
            <button class="btn btn-arca" type="submit">Buy with <b>Pt.100</b></button>
        </form>
        <div class="emoticons-wrapper">
            <img class="emoticon" src="//ac.namu.la/img/1.webp" data-id="101" />
            <video class="emoticon" data-src="//ac.namu.la/video/2.mp4" data-id="102"></video>
        </div>
        </body>
        </html>
        """;

    private const string DetailHtmlWithoutSellerName = """
        <!DOCTYPE html>
        <html>
        <body>
        <div class="emoticon-info">
            <div class="title">작성자 없는 패키지</div>
        </div>
        <div class="emoticons-wrapper"></div>
        </body>
        </html>
        """;

    private const string DetailHtmlWithoutTitle = """
        <!DOCTYPE html>
        <html>
        <body>
        <div class="emoticon-info">
            <div class="author">작성자만 있음</div>
        </div>
        </body>
        </html>
        """;

    private const string ValidSearchResultHtml = """
        <!DOCTYPE html>
        <html>
        <body>
        <div class="emoticon-col">
            <a href="/e/10001?p=1">
                <div class="emoticon">
                    <img src="//ac.namu.la/thumb/1.webp" />
                    <div class="title">첫 번째 아카콘</div>
                    <div class="maker">제작자A</div>
                    <div class="count">Sale <span>500</span></div>
                </div>
            </a>
        </div>
        <div class="emoticon-col">
            <a href="/e/10002?p=1">
                <div class="emoticon">
                    <img src="//ac.namu.la/thumb/2.webp" />
                    <div class="title">두 번째 아카콘</div>
                    <div class="maker">제작자B</div>
                    <div class="count">Sale <span>300</span></div>
                </div>
            </a>
        </div>
        <ul class="pagination">
            <li class="page-item"><a class="page-link" href="/e/?p=1">1</a></li>
            <li class="page-item active"><a class="page-link" href="/e/?p=2">2</a></li>
            <li class="page-item"><a class="page-link" href="/e/?p=3">3</a></li>
            <li class="page-item"><a class="page-link" href="/e/?p=3"><span class="ion-chevron-right"></span></a></li>
        </ul>
        </body>
        </html>
        """;
    private static readonly string SearchResultHtmlWithPopularSection = File.ReadAllText(GetTestDataPath("ArcaconMain.html"));
    private static readonly string SearchResultHtmlBySales = File.ReadAllText(GetTestDataPath("ArcaconMain - BySales.html"));

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesTitle()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal("테스트 아카콘 패키지", result.Title);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesSellerName()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal("테스트 제작자", result.SellerName);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_SetsPackageIndex()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal(123, result.PackageIndex);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesStickersFromHtml()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal(2, result.Stickers.Count);
        Assert.Equal(101, result.Stickers[0].Id);
        Assert.Equal("https://ac.namu.la/img/1.webp", result.Stickers[0].ImageUrl);
        Assert.Equal(102, result.Stickers[1].Id);
        Assert.Equal("https://ac.namu.la/video/2.mp4", result.Stickers[1].ImageUrl);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_SetsStickerSortNumbers()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal(1, result.Stickers[0].SortNumber);
        Assert.Equal(2, result.Stickers[1].SortNumber);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesSaleCount()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal(15213, result.SaleCount);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesRegistrationDate()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal("2025-10-24 08:40:34", result.RegistrationDate);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesRegistrationDateShort()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Matches(@"^\d{4}\.\d{2}\.\d{2}$", result.RegistrationDateShort);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesPrice()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal(100, result.Price);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_ParsesTags()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal(["2D", "스텔라소라"], result.Tags);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithValidHtml_IconCountMatchesStickerCount()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(ValidDetailHtml, 123);

        Assert.Equal(2, result.IconCount);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithoutSellerName_SetsSellerNameToEmpty()
    {
        var result = await HtmlParser.ParsePackageDetailAsync(DetailHtmlWithoutSellerName, 456);

        Assert.Equal(string.Empty, result.SellerName);
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithoutTitle_ThrowsArcaconParsingException()
    {
        await Assert.ThrowsAsync<ArcaconParsingException>(() =>
            HtmlParser.ParsePackageDetailAsync(DetailHtmlWithoutTitle, 789));
    }

    [Fact]
    public async Task ParsePackageDetailAsync_WithEmptyHtml_ThrowsArcaconParsingException()
    {
        await Assert.ThrowsAsync<ArcaconParsingException>(() =>
            HtmlParser.ParsePackageDetailAsync("", 1));
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithValidHtml_ReturnsTwoPackages()
    {
        var result = await HtmlParser.ParseSearchResultAsync(ValidSearchResultHtml, 2);

        Assert.Equal(2, result.Packages.Count);
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithValidHtml_ParsesFirstPackage()
    {
        var result = await HtmlParser.ParseSearchResultAsync(ValidSearchResultHtml, 2);
        var first = result.Packages[0];

        Assert.Equal(10001, first.PackageIndex);
        Assert.Equal("첫 번째 아카콘", first.Title);
        Assert.Equal("제작자A", first.SellerName);
        Assert.Equal("https://ac.namu.la/thumb/1.webp", first.ThumbnailUrl);
        Assert.Equal(500, first.SaleCount);
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithValidHtml_ParsesSecondPackage()
    {
        var result = await HtmlParser.ParseSearchResultAsync(ValidSearchResultHtml, 2);
        var second = result.Packages[1];

        Assert.Equal(10002, second.PackageIndex);
        Assert.Equal("두 번째 아카콘", second.Title);
        Assert.Equal("제작자B", second.SellerName);
        Assert.Equal(300, second.SaleCount);
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithValidHtml_ParsesTotalPages()
    {
        var result = await HtmlParser.ParseSearchResultAsync(ValidSearchResultHtml, 2);

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithValidHtml_SetsCurrentPage()
    {
        var result = await HtmlParser.ParseSearchResultAsync(ValidSearchResultHtml, 2);

        Assert.Equal(2, result.CurrentPage);
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithPopularSection_IgnoresPopularPackages()
    {
        var result = await HtmlParser.ParseSearchResultAsync(SearchResultHtmlWithPopularSection, 1);

        Assert.Equal([51674, 51673, 51672, 51671, 51670], result.Packages.Take(5).Select(package => package.PackageIndex));
        Assert.DoesNotContain(result.Packages.Take(5), package => package.PackageIndex is 51385 or 48724 or 50022 or 51522 or 51060);
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithBySalesHtml_UsesActualThumbnailInsteadOfStarIcon()
    {
        var result = await HtmlParser.ParseSearchResultAsync(SearchResultHtmlBySales, 1);

        Assert.Equal(37347, result.Packages[0].PackageIndex);
        Assert.Equal("https://ac-p1.namu.la/20240226sac/ea7f0d3cb63c9d4442d098fa7027f6cf213cfa90141bd6a47c08d3ab5f4a87aa.gif?expires=1777597200&key=caHUZ8ZrLM9qOjHJJUmhrQ", result.Packages[0].ThumbnailUrl);
        Assert.DoesNotContain("/static/assets/images/star-", result.Packages[0].ThumbnailUrl);
    }

    [Fact]
    public async Task ParsePopularPackagesAsync_WithSampleHtml_ReturnsDailyPopularTopFive()
    {
        var result = await HtmlParser.ParsePopularPackagesAsync(SearchResultHtmlWithPopularSection);

        Assert.Equal([51385, 48724, 50022, 51522, 51060], result.Select(package => package.PackageIndex));
    }

    [Fact]
    public async Task ParsePopularPackagesAsync_WithBySalesHtml_UsesLazyThumbnailInsteadOfStarIcon()
    {
        var result = await HtmlParser.ParsePopularPackagesAsync(SearchResultHtmlBySales);

        Assert.Equal(51385, result[0].PackageIndex);
        Assert.Equal("https://ac-p1.namu.la/20260330sac/bd225134f2b53f7c0cb4a55413a29ee9a42e2a591693247235e069ea4d5ed256.gif?expires=1777597200&key=y9_04fYXtY3rdiXsy6RaZA", result[0].ThumbnailUrl);
        Assert.DoesNotContain("/static/assets/images/star-", result[0].ThumbnailUrl);
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithEmptyHtml_ThrowsArcaconParsingException()
    {
        await Assert.ThrowsAsync<ArcaconParsingException>(() =>
            HtmlParser.ParseSearchResultAsync("", 1));
    }

    [Fact]
    public async Task ParseSearchResultAsync_WithEmptyList_ReturnsEmptyPackages()
    {
        const string emptyListHtml = """
            <!DOCTYPE html>
            <html><body><div class="emoticon-list"></div></body></html>
            """;

        var result = await HtmlParser.ParseSearchResultAsync(emptyListHtml, 1);

        Assert.Empty(result.Packages);
    }

    // ── ParseSubscribedPackagesAsync ──────────────────────────────────────────

    private const string ValidSubscribedHtml = """
        <!DOCTYPE html>
        <html>
        <body>
        <table class="table align-middle" data-action-role="emoticons.enabled">
            <thead><tr><th colspan="2">Arcacon in use</th></tr></thead>
            <tbody class="jquery-sortable emoticon-list">
                <tr>
                    <td>
                        <input type="hidden" name="customize.emoticons[]" value="38576">
                        <a href="/e/38576"><img src="/api/emoticon/38576/thumb"></a>
                    </td>
                    <td class="remove-icon"><a href="#" class="minus" data-action="emoticons.remove"></a></td>
                </tr>
            </tbody>
        </table>
        <table class="table align-middle" data-action-role="emoticons.disabled">
            <thead><tr><th colspan="3">Arcacon not in use</th></tr></thead>
            <tbody class="jquery-sortable emoticon-list">
                <tr>
                    <td>
                        <input type="hidden" name="customize.emoticons[]" value="43920" disabled>
                        <a href="/e/43920"><img src="/api/emoticon/43920/thumb"></a>
                    </td>
                    <td class="remove-icon"><a href="#" class="plus" data-action="emoticons.remove"></a></td>
                </tr>
            </tbody>
        </table>
        </body>
        </html>
        """;

    private const string SubscribedHtmlWithOnlyActive = """
        <!DOCTYPE html>
        <html>
        <body>
        <table class="table align-middle" data-action-role="emoticons.enabled">
            <tbody class="jquery-sortable emoticon-list">
                <tr><td><input type="hidden" name="customize.emoticons[]" value="11111"></td></tr>
                <tr><td><input type="hidden" name="customize.emoticons[]" value="22222"></td></tr>
            </tbody>
        </table>
        <table class="table align-middle" data-action-role="emoticons.disabled">
            <tbody class="jquery-sortable emoticon-list"></tbody>
        </table>
        </body>
        </html>
        """;

    [Fact]
    public async Task ParseSubscribedPackagesAsync_WithValidHtml_ReturnsBothPackages()
    {
        var result = await HtmlParser.ParseSubscribedPackagesAsync(ValidSubscribedHtml);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ParseSubscribedPackagesAsync_WithValidHtml_ActivePackageIsCorrect()
    {
        var result = await HtmlParser.ParseSubscribedPackagesAsync(ValidSubscribedHtml);
        var active = result.Single(package => package.IsActive);

        Assert.Equal(38576, active.PackageIndex);
        Assert.True(active.IsActive);
        Assert.Contains("38576", active.ThumbnailUrl);
    }

    [Fact]
    public async Task ParseSubscribedPackagesAsync_WithValidHtml_InactivePackageIsCorrect()
    {
        var result = await HtmlParser.ParseSubscribedPackagesAsync(ValidSubscribedHtml);
        var inactive = result.Single(package => !package.IsActive);

        Assert.Equal(43920, inactive.PackageIndex);
        Assert.False(inactive.IsActive);
        Assert.Contains("43920", inactive.ThumbnailUrl);
    }

    [Fact]
    public async Task ParseSubscribedPackagesAsync_WithValidHtml_ThumbnailUrlUsesHttps()
    {
        var result = await HtmlParser.ParseSubscribedPackagesAsync(ValidSubscribedHtml);

        Assert.All(result, package => Assert.StartsWith("https://", package.ThumbnailUrl));
    }

    [Fact]
    public async Task ParseSubscribedPackagesAsync_WithOnlyActivePackages_ReturnsOnlyActive()
    {
        var result = await HtmlParser.ParseSubscribedPackagesAsync(SubscribedHtmlWithOnlyActive);

        Assert.Equal(2, result.Count);
        Assert.All(result, package => Assert.True(package.IsActive));
    }

    [Fact]
    public async Task ParseSubscribedPackagesAsync_WithEmptyHtml_ThrowsArcaconParsingException()
    {
        await Assert.ThrowsAsync<ArcaconParsingException>(() =>
            HtmlParser.ParseSubscribedPackagesAsync(""));
    }

    [Fact]
    public async Task ParseSubscribedPackagesAsync_WithNoSubscriptions_ReturnsEmptyList()
    {
        const string noSubscriptionsHtml = """
            <!DOCTYPE html>
            <html>
            <body>
            <table class="table align-middle" data-action-role="emoticons.enabled">
                <tbody class="jquery-sortable emoticon-list"></tbody>
            </table>
            <table class="table align-middle" data-action-role="emoticons.disabled">
                <tbody class="jquery-sortable emoticon-list"></tbody>
            </table>
            </body>
            </html>
            """;

        var result = await HtmlParser.ParseSubscribedPackagesAsync(noSubscriptionsHtml);

        Assert.Empty(result);
    }

    private static string GetTestDataPath(string fileName) => Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
}
