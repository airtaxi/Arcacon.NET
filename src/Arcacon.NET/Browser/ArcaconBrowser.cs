using Microsoft.Web.WebView2.Core;

namespace Arcacon.NET.Browser;

/// <summary>
/// WebView2 기반 로그인 처리 클래스.
/// 호출자가 제공한 <see cref="CoreWebView2"/>를 통해 arca.live 로그인 페이지로 이동하고,
/// 사용자가 직접 로그인할 때까지 대기한 뒤 쿠키를 추출한다.
/// </summary>
internal static class ArcaconBrowser
{
    /// <summary>
    /// 제공된 WebView2를 통해 arca.live 로그인 페이지로 이동한 뒤,
    /// 사용자가 로그인을 완료할 때까지 대기한다.
    /// 이미 로그인된 상태라면 즉시 완료된다.
    /// </summary>
    /// <param name="webView">로그인에 사용할 <see cref="CoreWebView2"/> 인스턴스</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>로그인 후 추출한 arca.live 쿠키 목록</returns>
    public static async Task<IReadOnlyList<CoreWebView2Cookie>> LoginAsync(
        CoreWebView2 webView,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // NavigationCompleted는 매 페이지 로드마다 발생한다.
        // arca.live 도메인 내에서 /u/login을 벗어난 경우만 로그인 완료로 판단한다.
        // 소셜 로그인(Google 등) 중간 페이지(외부 도메인)에서 조기 완료되는 것을 방지한다.
        void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            var source = webView.Source;
            if (source.StartsWith("https://arca.live") && !source.Contains("/u/login"))
            {
                webView.NavigationCompleted -= OnNavigationCompleted;
                tcs.TrySetResult(true);
            }
        }

        webView.NavigationCompleted += OnNavigationCompleted;

        try
        {
            webView.Navigate("https://arca.live/u/login");

            // useSynchronizationContext: true — 취소 콜백을 현재 SynchronizationContext(UI 스레드)에서
            // 실행하도록 강제한다. CoreWebView2 이벤트 조작은 UI 스레드에서만 안전하다.
            using var registration = cancellationToken.Register(() =>
            {
                webView.NavigationCompleted -= OnNavigationCompleted;
                tcs.TrySetCanceled(cancellationToken);
            }, useSynchronizationContext: true);

            // ConfigureAwait(false) 미사용 — CoreWebView2는 UI 스레드 친화적이므로
            // await 이후에도 호출자의 SynchronizationContext(UI 스레드)를 유지한다.
            await tcs.Task;
        }
        finally
        {
            // 정상 완료·취소·예외 모든 경우에 이벤트 핸들러 해제 (중복 해제는 무해함)
            webView.NavigationCompleted -= OnNavigationCompleted;
        }

        return await webView.CookieManager.GetCookiesAsync("https://arca.live");
    }
}
