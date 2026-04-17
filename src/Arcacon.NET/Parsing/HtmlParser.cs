using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Arcacon.NET.Exceptions;
using Arcacon.NET.Models;

namespace Arcacon.NET.Parsing;

/// <summary>
/// arca.live HTML 페이지 파싱 유틸리티
/// </summary>
internal static class HtmlParser
{
    private const string HttpsPrefix = "https:";

    /// <summary>
    /// 아카콘 패키지 상세 페이지 HTML에서 전체 정보(메타 + 스티커)를 파싱한다.
    /// </summary>
    /// <param name="html">페이지 HTML 문자열</param>
    /// <param name="packageId">패키지 번호</param>
    public static async Task<ArcaconPackageDetail> ParsePackageDetailAsync(string html, int packageId)
    {
        var parser = new AngleSharp.Html.Parser.HtmlParser();
        using var document = await parser.ParseDocumentAsync(html).ConfigureAwait(false);

        var titleElement = document.QuerySelector(".emoticon-info .title");
        if (titleElement is null) throw new ArcaconParsingException("패키지 상세 페이지에서 제목 요소를 찾을 수 없습니다.");

        var sellerNameElement = document.QuerySelector(".emoticon-info .author");
        var dateElement = document.QuerySelector(".emoticon-info .date time");
        var saleCountElement = document.QuerySelector(".emoticon-info .count span");
        var priceElement = document.QuerySelector("form[action*='/buy'] button b");
        var tagElements = document.QuerySelectorAll(".emoticon-tags .tag");

        var registrationDate = dateElement?.TextContent.Trim() ?? string.Empty;
        var registrationDateShort = ParseRegistrationDateShort(dateElement?.GetAttribute("datetime"));
        var saleCount = int.TryParse(
            saleCountElement?.TextContent.Trim().Replace(",", ""), out var parsedSaleCount)
            ? parsedSaleCount : 0;
        var price = ParsePrice(priceElement?.TextContent.Trim());
        var tags = tagElements
            .Select(element => element.TextContent.Trim().TrimStart('#'))
            .Where(tag => !string.IsNullOrEmpty(tag))
            .ToList();

        // 스티커: .emoticons-wrapper 내 class="emoticon" 인 video/img 요소
        var stickerElements = document.QuerySelectorAll(".emoticons-wrapper .emoticon");
        var stickers = new List<ArcaconSticker>(stickerElements.Length);
        for (var index = 0; index < stickerElements.Length; index++)
        {
            var element = stickerElements[index];
            var rawUrl = element.GetAttribute("data-src") ?? element.GetAttribute("src") ?? string.Empty;
            if (string.IsNullOrEmpty(rawUrl)) continue;

            var imageUrl = NormalizeMediaUrl(rawUrl) ?? string.Empty;
            var isVideoElement = element.LocalName.Equals("video", StringComparison.OrdinalIgnoreCase);
            var videoUrl = isVideoElement ? imageUrl : null;
            var posterThumbnailUrl = isVideoElement ? NormalizeMediaUrl(element.GetAttribute("poster")) : null;

            _ = int.TryParse(element.GetAttribute("data-id"), out var stickerId);
            stickers.Add(new ArcaconSticker
            {
                Id = stickerId,
                ImageUrl = imageUrl,
                VideoUrl = videoUrl,
                PosterThumbnailUrl = posterThumbnailUrl,
                SortNumber = index + 1
            });
        }

        return new ArcaconPackageDetail
        {
            PackageIndex = packageId,
            Title = titleElement.TextContent.Trim(),
            SellerName = sellerNameElement?.TextContent.Trim() ?? string.Empty,
            RegistrationDate = registrationDate,
            RegistrationDateShort = registrationDateShort,
            SaleCount = saleCount,
            Price = price,
            Tags = tags,
            Stickers = stickers
        };
    }

    /// <summary>
    /// 아카콘 목록/검색 페이지 HTML에서 검색 결과를 파싱한다.
    /// </summary>
    /// <param name="html">페이지 HTML 문자열</param>
    /// <param name="currentPage">현재 페이지 번호</param>
    public static async Task<ArcaconSearchResult> ParseSearchResultAsync(string html, int currentPage)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw new ArcaconParsingException("파싱할 HTML이 비어있습니다.");

        try
        {
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            using var document = await parser.ParseDocumentAsync(html).ConfigureAwait(false);

            var result = new ArcaconSearchResult
            {
                CurrentPage = currentPage,
                Packages = []
            };

            var packageListContainer = FindPackageListContainer(document);
            var packageAnchors = packageListContainer?.QuerySelectorAll(".emoticon-col a[href^='/e/']")
                ?? document.QuerySelectorAll(".emoticon-col a[href^='/e/']");
            foreach (var packageAnchor in packageAnchors)
            {
                var packageSummary = CreatePackageSummaryFromPackageAnchor(packageAnchor);
                if (packageSummary is null) continue;

                result.Packages.Add(packageSummary);
            }

            // 페이지네이션: .pagination .page-link[href] 에서 p= 파라미터 최댓값
            var pageLinks = document.QuerySelectorAll(".pagination .page-link[href]");
            var maxPage = 0;
            foreach (var pageLink in pageLinks)
            {
                var href = pageLink.GetAttribute("href") ?? string.Empty;
                var pageNumber = ExtractPageNumberFromUrl(href);
                if (pageNumber > maxPage) maxPage = pageNumber;
            }

            result.TotalPages = maxPage > 0 ? maxPage : (result.Packages.Count > 0 ? 1 : 0);

            return result;
        }
        catch (ArcaconParsingException) { throw; }
        catch (Exception exception) { throw new ArcaconParsingException("HTML 파싱 중 오류가 발생했습니다.", exception); }
    }

    /// <summary>
    /// 아카콘 목록/검색 페이지 HTML에서 상단 인기 아카콘 5개를 파싱한다.
    /// </summary>
    /// <param name="html">페이지 HTML 문자열</param>
    public static async Task<IReadOnlyList<ArcaconPackageSummary>> ParsePopularPackagesAsync(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) throw new ArcaconParsingException("파싱할 HTML이 비어있습니다.");

        try
        {
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            using var document = await parser.ParseDocumentAsync(html).ConfigureAwait(false);

            var popularPackageListContainer = FindPopularPackageListContainer(document);
            var popularPackageAnchors = popularPackageListContainer?.QuerySelectorAll(".emoticon-col a[href^='/e/']").ToArray() ?? [];
            var popularPackages = new List<ArcaconPackageSummary>(popularPackageAnchors.Length);

            foreach (var popularPackageAnchor in popularPackageAnchors.Take(5))
            {
                var popularPackage = CreatePackageSummaryFromPackageAnchor(popularPackageAnchor);
                if (popularPackage is null) continue;

                popularPackages.Add(popularPackage);
            }

            return popularPackages;
        }
        catch (ArcaconParsingException) { throw; }
        catch (Exception exception) { throw new ArcaconParsingException("HTML 파싱 중 오류가 발생했습니다.", exception); }
    }

    /// <summary>
    /// 아카콘 설정 페이지 HTML에서 구독(보유) 중인 패키지 목록을 파싱한다.
    /// </summary>
    /// <param name="html">설정 페이지 HTML 문자열</param>
    public static async Task<IReadOnlyList<ArcaconSubscribedPackage>> ParseSubscribedPackagesAsync(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) throw new ArcaconParsingException("파싱할 HTML이 비어있습니다.");

        try
        {
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            using var document = await parser.ParseDocumentAsync(html).ConfigureAwait(false);

            var result = new List<ArcaconSubscribedPackage>();

            // 사용 중인 아카콘
            var activeRows = document.QuerySelectorAll("table[data-action-role='emoticons.enabled'] tbody.emoticon-list tr");
            foreach (var row in activeRows)
            {
                var input = row.QuerySelector("input[type='hidden'][name]");
                if (input is null) continue;
                if (!int.TryParse(input.GetAttribute("value"), out var packageIndex) || packageIndex <= 0) continue;

                result.Add(new ArcaconSubscribedPackage
                {
                    PackageIndex = packageIndex,
                    ThumbnailUrl = BuildPackageThumbnailUrl(packageIndex),
                    IsActive = true
                });
            }

            // 사용하지 않는 아카콘
            var inactiveRows = document.QuerySelectorAll("table[data-action-role='emoticons.disabled'] tbody.emoticon-list tr");
            foreach (var row in inactiveRows)
            {
                var input = row.QuerySelector("input[type='hidden'][name]");
                if (input is null) continue;
                if (!int.TryParse(input.GetAttribute("value"), out var packageIndex) || packageIndex <= 0) continue;

                result.Add(new ArcaconSubscribedPackage
                {
                    PackageIndex = packageIndex,
                    ThumbnailUrl = BuildPackageThumbnailUrl(packageIndex),
                    IsActive = false
                });
            }

            return result;
        }
        catch (ArcaconParsingException) { throw; }
        catch (Exception exception) { throw new ArcaconParsingException("HTML 파싱 중 오류가 발생했습니다.", exception); }
    }

    private static string ParseRegistrationDateShort(string? datetimeAttribute)
    {
        if (string.IsNullOrEmpty(datetimeAttribute)) return string.Empty;

        if (DateTimeOffset.TryParse(datetimeAttribute, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var dateTimeOffset))
            return dateTimeOffset.ToLocalTime().ToString("yyyy.MM.dd", CultureInfo.InvariantCulture);

        return string.Empty;
    }

    private static int ParsePrice(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var match = Regex.Match(text, @"\d+");
        return match.Success && int.TryParse(match.Value, out var price) ? price : 0;
    }

    private static int ExtractPackageIndexFromHref(string href)
    {
        // href 형식: /e/{packageIndex}?p=1
        var pathMatch = Regex.Match(href, @"^/e/(\d+)");
        if (pathMatch.Success && int.TryParse(pathMatch.Groups[1].Value, out var index)) return index;

        return 0;
    }

    private static int ExtractPageNumberFromUrl(string href)
    {
        // href 형식: /e/?p=3 또는 /e/?sort=rank&p=5
        var match = Regex.Match(href, @"[?&]p=(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var page)) return page;

        return 0;
    }

    private static ArcaconPackageSummary? CreatePackageSummaryFromPackageAnchor(IElement packageAnchor)
    {
        var href = packageAnchor.GetAttribute("href") ?? string.Empty;
        var packageIndex = ExtractPackageIndexFromHref(href);
        if (packageIndex <= 0) return null;

        var titleElement = packageAnchor.QuerySelector(".title");
        var makerElement = packageAnchor.QuerySelector(".maker");
        var saleCountElement = packageAnchor.QuerySelector(".count span");

        _ = int.TryParse(saleCountElement?.TextContent.Trim().Replace(",", ""), out var saleCount);

        return new ArcaconPackageSummary
        {
            PackageIndex = packageIndex,
            Title = titleElement?.TextContent.Trim() ?? string.Empty,
            SellerName = makerElement?.TextContent.Trim() ?? string.Empty,
            ThumbnailUrl = BuildPackageThumbnailUrl(packageIndex),
            SaleCount = saleCount
        };
    }

    private static string BuildPackageThumbnailUrl(int packageIndex) =>
        $"{HttpsPrefix}//arca.live/api/emoticon/{packageIndex}/thumb";

    private static string? NormalizeMediaUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return null;

        return rawUrl.StartsWith("//", StringComparison.Ordinal)
            ? HttpsPrefix + rawUrl
            : rawUrl;
    }

    private static IElement? FindPopularPackageListContainer(IParentNode documentRoot) => documentRoot
        .QuerySelectorAll(".emoticon-list")
        .FirstOrDefault(container =>
            container.QuerySelector(".rank") is not null
            && container.QuerySelector(".emoticon-col a[href^='/e/']") is not null);

    private static IElement? FindPackageListContainer(IParentNode documentRoot)
    {
        var packageListContainers = documentRoot
            .QuerySelectorAll(".emoticon-list")
            .Where(container => container.QuerySelector(".emoticon-col a[href^='/e/']") is not null)
            .ToList();

        return packageListContainers.FirstOrDefault(container => container.QuerySelector(".sort") is not null)
            ?? packageListContainers.LastOrDefault();
    }
}
