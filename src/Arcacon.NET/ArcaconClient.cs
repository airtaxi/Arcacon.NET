using System.Text.Json;
using Arcacon.NET.Browser;
using Arcacon.NET.Exceptions;
using Arcacon.NET.Http;
using Arcacon.NET.Models;
using Arcacon.NET.Parsing;
using Microsoft.Web.WebView2.Core;

namespace Arcacon.NET;

/// <summary>
/// 비공식 아카콘 클라이언트 구현체
/// </summary>
public class ArcaconClient : IArcaconClient, IAsyncDisposable
{
    private static readonly JsonSerializerOptions s_javaScriptObjectNotationSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ArcaconHttpClient _httpClient;
    private readonly HttpClient? _innerHttpClient;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>
    /// 내부 HttpClient를 생성하는 기본 생성자
    /// </summary>
    public ArcaconClient()
    {
        _innerHttpClient = CreateDefaultHttpClient();
        _httpClient = new ArcaconHttpClient(_innerHttpClient);
        _ownsHttpClient = true;
    }

    /// <summary>
    /// 외부 HttpClient를 주입받는 생성자 (IHttpClientFactory 패턴 호환)
    /// </summary>
    /// <param name="httpClient">사용할 HttpClient 인스턴스</param>
    public ArcaconClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = new ArcaconHttpClient(httpClient);
        _ownsHttpClient = false;
    }

    /// <inheritdoc />
    public bool IsLoggedIn { get; private set; }

    /// <inheritdoc />
    public async Task LoginAsync(CoreWebView2 webView, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webView);
        var cookies = await ArcaconBrowser.LoginAsync(webView, cancellationToken);
        _httpClient.SetSessionCookies(cookies);
        IsLoggedIn = true;
    }

    /// <inheritdoc />
    public async Task<ArcaconSearchResult> SearchAsync(
        string query,
        ArcaconSearchType searchType = ArcaconSearchType.Title,
        ArcaconSearchSort sort = ArcaconSearchSort.Hot,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("검색어가 비어있습니다.", nameof(query));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");
        if (!IsLoggedIn) throw new InvalidOperationException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        var target = ConvertSearchTypeToTarget(searchType);
        var encodedQuery = Uri.EscapeDataString(query);
        var sortParam = sort == ArcaconSearchSort.Hot ? "&sort=rank" : string.Empty;
        var url = $"/e/?target={target}&keyword={encodedQuery}{sortParam}&p={page}";

        var html = await _httpClient.GetListPageHtmlAsync(url, cancellationToken).ConfigureAwait(false);
        return await HtmlParser.ParseSearchResultAsync(html, page).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ArcaconSearchResult> GetHotListAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");
        if (!IsLoggedIn) throw new InvalidOperationException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        var html = await _httpClient.GetListPageHtmlAsync($"/e/?sort=rank&p={page}", cancellationToken).ConfigureAwait(false);
        return await HtmlParser.ParseSearchResultAsync(html, page).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ArcaconSearchResult> GetNewListAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");
        if (!IsLoggedIn) throw new InvalidOperationException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        var html = await _httpClient.GetListPageHtmlAsync($"/e/?p={page}", cancellationToken).ConfigureAwait(false);
        return await HtmlParser.ParseSearchResultAsync(html, page).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ArcaconPackageSummary>> GetDailyPopularAsync(CancellationToken cancellationToken = default) => GetPopularPackagesAsync("/e/?", cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<ArcaconPackageSummary>> GetWeeklyPopularAsync(CancellationToken cancellationToken = default) => GetPopularPackagesAsync("/e/?rank=weekly", cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<ArcaconPackageSummary>> GetMonthlyPopularAsync(CancellationToken cancellationToken = default) => GetPopularPackagesAsync("/e/?rank=monthly", cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArcaconSubscribedPackage>> GetSubscribedPackagesAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        if (!IsLoggedIn) throw new InvalidOperationException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        var html = await _httpClient.GetSettingsPageHtmlAsync(cancellationToken).ConfigureAwait(false);
        var packages = await HtmlParser.ParseSubscribedPackagesAsync(html).ConfigureAwait(false);

        return includeInactive ? packages : packages.Where(package => package.IsActive).ToList();
    }

    /// <inheritdoc />
    public async Task<ArcaconPackageDetail> GetPackageDetailAsync(
        int packageIndex,
        CancellationToken cancellationToken = default)
    {
        if (packageIndex <= 0) throw new ArgumentOutOfRangeException(nameof(packageIndex), "패키지 번호는 양수여야 합니다.");

        if (!IsLoggedIn) throw new InvalidOperationException("패키지 상세 조회는 로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        var html = await _httpClient.GetPackagePageHtmlAsync(packageIndex, cancellationToken).ConfigureAwait(false);
        var publicPackageStickerPayload = await _httpClient.GetPublicPackageStickerPayloadAsync(packageIndex, cancellationToken).ConfigureAwait(false);
        var packageDetail = await HtmlParser.ParsePackageDetailAsync(html, packageIndex).ConfigureAwait(false);

        ApplyPublicStickerImageUrlsFromPublicPayload(packageDetail, publicPackageStickerPayload);
        return packageDetail;
    }

    /// <inheritdoc />
    public async Task<byte[]> DownloadStickerAsync(
        ArcaconSticker sticker,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sticker);
        if (string.IsNullOrEmpty(sticker.ImageUrl)) throw new ArgumentException("스티커의 ImageUrl이 비어있습니다.", nameof(sticker));

        return await _httpClient.DownloadImageBytesAsync(sticker.ImageUrl, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadStickerStreamAsync(
        ArcaconSticker sticker,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sticker);
        if (string.IsNullOrEmpty(sticker.ImageUrl)) throw new ArgumentException("스티커의 ImageUrl이 비어있습니다.", nameof(sticker));

        return await _httpClient.DownloadImageStreamAsync(sticker.ImageUrl, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DownloadPackageAsync(
        int packageIndex,
        string outputDirectory,
        IProgress<(int Completed, int Total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (packageIndex <= 0) throw new ArgumentOutOfRangeException(nameof(packageIndex), "패키지 번호는 양수여야 합니다.");
        if (string.IsNullOrWhiteSpace(outputDirectory)) throw new ArgumentException("출력 디렉토리가 비어있습니다.", nameof(outputDirectory));

        var packageDetail = await GetPackageDetailAsync(packageIndex, cancellationToken).ConfigureAwait(false);

        var safeDirectoryName = ArcaconFileNameHelper.SanitizeFileName(packageDetail.Title);
        var packageDirectory = Path.Combine(outputDirectory, safeDirectoryName);
        Directory.CreateDirectory(packageDirectory);

        var total = packageDetail.Stickers.Count;
        var completed = 0;

        // 병렬 다운로드 (최대 4개 동시)
        var semaphore = new SemaphoreSlim(4);
        var tasks = packageDetail.Stickers.Select(async sticker =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var filePath = Path.Combine(packageDirectory, ArcaconFileNameHelper.GetStickerFileName(sticker));
                var imageData = await _httpClient.DownloadImageBytesAsync(sticker.ImageUrl, cancellationToken).ConfigureAwait(false);
                await File.WriteAllBytesAsync(filePath, imageData, cancellationToken).ConfigureAwait(false);

                var currentCompleted = Interlocked.Increment(ref completed);
                progress?.Report((currentCompleted, total));
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static string ConvertSearchTypeToTarget(ArcaconSearchType searchType) => searchType switch
    {
        ArcaconSearchType.Title => "title",
        ArcaconSearchType.NickName => "nickname",
        ArcaconSearchType.Tags => "tag",
        _ => "title"
    };

    private async Task<IReadOnlyList<ArcaconPackageSummary>> GetPopularPackagesAsync(
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        if (!IsLoggedIn) throw new InvalidOperationException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        var html = await _httpClient.GetListPageHtmlAsync(relativeUrl, cancellationToken).ConfigureAwait(false);
        return await HtmlParser.ParsePopularPackagesAsync(html).ConfigureAwait(false);
    }

    internal static void ApplyPublicStickerImageUrlsFromPublicPayload(
        ArcaconPackageDetail packageDetail,
        string publicPackageStickerPayload)
    {
        ArgumentNullException.ThrowIfNull(packageDetail);
        if (string.IsNullOrWhiteSpace(publicPackageStickerPayload))
            throw new ArcaconParsingException("공개 아카콘 API 응답이 비어있습니다.");

        var publicArcaconStickers = JsonSerializer.Deserialize<List<PublicArcaconStickerPayload>>(
            publicPackageStickerPayload,
            s_javaScriptObjectNotationSerializerOptions)
            ?? throw new ArcaconParsingException("공개 아카콘 API 응답을 해석할 수 없습니다.");

        var imageUrlByStickerId = publicArcaconStickers
            .Where(publicArcaconSticker => publicArcaconSticker.Id > 0 && !string.IsNullOrWhiteSpace(publicArcaconSticker.ImageUrl))
            .ToDictionary(
                publicArcaconSticker => publicArcaconSticker.Id,
                publicArcaconSticker => NormalizePublicStickerImageUrl(publicArcaconSticker.ImageUrl!));

        var missingStickerIds = packageDetail.Stickers
            .Where(sticker => !imageUrlByStickerId.ContainsKey(sticker.Id))
            .Select(sticker => sticker.Id)
            .ToArray();

        if (missingStickerIds.Length > 0)
            throw new ArcaconParsingException($"공개 아카콘 API 응답에 일부 스티커가 없습니다: {string.Join(", ", missingStickerIds)}");

        foreach (var sticker in packageDetail.Stickers)
            sticker.ImageUrl = imageUrlByStickerId[sticker.Id];
    }

    private static string NormalizePublicStickerImageUrl(string imageUrl) => imageUrl.StartsWith("//", StringComparison.Ordinal)
        ? $"https:{imageUrl}"
        : imageUrl;

    private static HttpClient CreateDefaultHttpClient()
    {
        var handler = new WinHttpHandler { AutomaticDecompression = System.Net.DecompressionMethods.All };
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
        return client;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_ownsHttpClient) _innerHttpClient?.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private sealed class PublicArcaconStickerPayload
    {
        public int Id { get; set; }

        public string? ImageUrl { get; set; }
    }
}
