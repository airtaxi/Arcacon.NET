using System.Net;
using Arcacon.NET.Exceptions;
using Microsoft.Web.WebView2.Core;

namespace Arcacon.NET.Http;

/// <summary>
/// arca.live와의 HTTP 통신을 담당하는 클래스
/// </summary>
internal class ArcaconHttpClient(HttpClient httpClient)
{
    private const string BaseUrl = "https://arca.live";

    /// <summary>
    /// WebView2 쿠키를 Cookie 요청 헤더에 적용한다.
    /// </summary>
    public void SetSessionCookies(IReadOnlyList<CoreWebView2Cookie> cookies)
    {
        var cookieHeader = string.Join("; ", cookies.Select(cookie => $"{cookie.Name}={cookie.Value}"));
        httpClient.DefaultRequestHeaders.Remove("Cookie");
        httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
    }

    /// <summary>
    /// 아카콘 목록/검색 페이지 HTML을 가져온다 (로그인 필요).
    /// </summary>
    public async Task<string> GetListPageHtmlAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + relativeUrl);
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0");
        request.Headers.Add("Priority", "u=0, i");
        request.Headers.Add("Referer", $"{BaseUrl}/e");

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var finalUrl = response.RequestMessage?.RequestUri?.AbsoluteUri ?? string.Empty;
        if (finalUrl.Contains("/u/login") || response.StatusCode == HttpStatusCode.Unauthorized) throw new ArcaconLoginException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 패키지 상세 페이지 HTML을 가져온다 (로그인 필요).
    /// </summary>
    public async Task<string> GetPackagePageHtmlAsync(int packageId, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/e/{packageId}");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0");
        request.Headers.Add("Priority", "u=0, i");
        request.Headers.Add("Referer", $"{BaseUrl}/e");

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var finalUrl = response.RequestMessage?.RequestUri?.AbsoluteUri ?? string.Empty;
        if (finalUrl.Contains("/u/login") || response.StatusCode == HttpStatusCode.Unauthorized) throw new ArcaconLoginException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        if (response.StatusCode == HttpStatusCode.Forbidden) throw new ArcaconLoginException("접근이 거부되었습니다. 세션이 만료되었거나 이 패키지에 접근 권한이 없습니다.");

        if (response.StatusCode == HttpStatusCode.NotFound) throw new ArcaconNotFoundException($"패키지를 찾을 수 없습니다: {packageId}");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 공개 아카콘 API에서 패키지 스티커 목록을 가져온다.
    /// </summary>
    public async Task<string> GetPublicPackageStickerPayloadAsync(int packageId, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/emoticon/{packageId}");
        request.Headers.Add("Accept", "application/json, text/plain, */*");
        request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0");
        request.Headers.Add("Priority", "u=0, i");
        request.Headers.Add("Referer", $"{BaseUrl}/e/{packageId}");

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound) throw new ArcaconNotFoundException($"패키지를 찾을 수 없습니다: {packageId}");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 아카콘 설정 페이지 HTML을 가져온다 (로그인 필요).
    /// </summary>
    public async Task<string> GetSettingsPageHtmlAsync(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/settings/emoticons");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0");
        request.Headers.Add("Priority", "u=0, i");
        request.Headers.Add("Referer", $"{BaseUrl}/settings");

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var finalUrl = response.RequestMessage?.RequestUri?.AbsoluteUri ?? string.Empty;
        if (finalUrl.Contains("/u/login") || response.StatusCode == HttpStatusCode.Unauthorized) throw new ArcaconLoginException("로그인이 필요합니다. LoginAsync()를 먼저 호출해 주세요.");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 이미지 URL에서 바이트 배열로 다운로드한다.
    /// </summary>
    public async Task<byte[]> DownloadImageBytesAsync(string imageUrl, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
        request.Headers.Add("Referer", BaseUrl);

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 이미지 URL에서 스트림으로 다운로드한다.
    /// </summary>
    public async Task<Stream> DownloadImageStreamAsync(string imageUrl, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
        request.Headers.Add("Referer", BaseUrl);

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
    }
}
