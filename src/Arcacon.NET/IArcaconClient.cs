using Arcacon.NET.Models;
using Microsoft.Web.WebView2.Core;

namespace Arcacon.NET;

/// <summary>
/// 아카콘 클라이언트 인터페이스
/// </summary>
public interface IArcaconClient
{
    /// <summary>
    /// 현재 로그인 상태
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// 제공된 WebView2를 통해 arca.live 로그인 페이지로 이동한 뒤,
    /// 사용자가 직접 로그인할 때까지 대기한다.
    /// 이미 로그인된 상태라면 즉시 완료된다.
    /// </summary>
    /// <param name="webView">로그인에 사용할 <see cref="CoreWebView2"/> 인스턴스</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task LoginAsync(CoreWebView2 webView, CancellationToken cancellationToken = default);

    /// <summary>
    /// 아카콘을 검색한다 (로그인 필요).
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="searchType">검색 유형 (제목, 닉네임, 태그)</param>
    /// <param name="sort">정렬 방식 (최신순, 판매순)</param>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<ArcaconSearchResult> SearchAsync(
        string query,
        ArcaconSearchType searchType = ArcaconSearchType.Title,
        ArcaconSearchSort sort = ArcaconSearchSort.Hot,
        int page = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 판매순(인기) 아카콘 목록을 가져온다 (로그인 필요).
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<ArcaconSearchResult> GetHotListAsync(int page = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 최신 아카콘 목록을 가져온다 (로그인 필요).
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<ArcaconSearchResult> GetNewListAsync(int page = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 아카콘 패키지 상세 정보를 가져온다 (로그인 필요).
    /// </summary>
    /// <param name="packageIndex">패키지 고유 번호</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<ArcaconPackageDetail> GetPackageDetailAsync(int packageIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// 스티커 이미지를 바이트 배열로 다운로드한다.
    /// </summary>
    /// <param name="sticker">다운로드할 스티커</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<byte[]> DownloadStickerAsync(ArcaconSticker sticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// 스티커 이미지를 스트림으로 다운로드한다.
    /// </summary>
    /// <param name="sticker">다운로드할 스티커</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<Stream> DownloadStickerStreamAsync(ArcaconSticker sticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// 내 구독(보유) 아카콘 목록을 가져온다 (로그인 필요).
    /// </summary>
    /// <param name="includeInactive">
    /// <c>true</c>이면 사용 중이지 않은 아카콘도 포함한다.
    /// <c>false</c>(기본값)이면 사용 중인 아카콘만 반환한다.
    /// </param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<IReadOnlyList<ArcaconSubscribedPackage>> GetSubscribedPackagesAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 패키지 내 모든 스티커를 지정 폴더에 다운로드한다 (로그인 필요).
    /// </summary>
    /// <param name="packageIndex">패키지 고유 번호</param>
    /// <param name="outputDirectory">저장할 폴더 경로</param>
    /// <param name="progress">진행 상태 콜백 (현재 완료 수 / 전체 수)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task DownloadPackageAsync(
        int packageIndex,
        string outputDirectory,
        IProgress<(int Completed, int Total)>? progress = null,
        CancellationToken cancellationToken = default);
}
