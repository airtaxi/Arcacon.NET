using System.Collections.ObjectModel;
using System.Linq;
using Arcacon.NET;
using Arcacon.NET.Models;
using Arcacon.NET.Sample.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Arcacon.NET.Sample;

public sealed partial class MainWindow : Window
{
    private readonly ArcaconClient _client = new();
    private readonly ObservableCollection<LogViewModel> _logItems = [];

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        LogItemsRepeater.ItemsSource = _logItems;
    }

    private void AppendLog(string message)
    {
        _logItems.Add(new LogViewModel(DateTime.Now, message));
        LogScrollViewer.UpdateLayout();
        LogScrollViewer.ChangeView(null, double.MaxValue, null);
    }

    private async void OnLoginButtonClicked(object sender, RoutedEventArgs e)
    {
        LoginButton.IsEnabled = false;
        WebView2Control.Visibility = Visibility.Visible;
        AppendLog("WebView2를 초기화합니다...");

        try
        {
            await WebView2Control.EnsureCoreWebView2Async();
            AppendLog("arca.live 로그인 페이지로 이동합니다. 직접 로그인해 주세요.");
            await _client.LoginAsync(WebView2Control.CoreWebView2);
            AppendLog("로그인 성공!");
            GetNewListButton.IsEnabled = true;
            GetHotListButton.IsEnabled = true;
            GetDailyPopularButton.IsEnabled = true;
            GetWeeklyPopularButton.IsEnabled = true;
            GetMonthlyPopularButton.IsEnabled = true;
            SearchButton.IsEnabled = true;
            GetPackageDetailButton.IsEnabled = true;
            GetSubscribedPackagesButton.IsEnabled = true;
        }
        catch (OperationCanceledException)
        {
            AppendLog("로그인이 취소되었습니다.");
        }
        catch (Exception exception)
        {
            AppendLog($"로그인 오류: {exception.Message}");
        }
        finally
        {
            WebView2Control.Visibility = Visibility.Collapsed;
            LoginButton.IsEnabled = true;
        }
    }

    private async void OnGetNewListButtonClicked(object sender, RoutedEventArgs e)
    {
        GetNewListButton.IsEnabled = false;
        AppendLog("최신 아카콘 목록을 조회합니다...");

        try
        {
            var result = await _client.GetNewListAsync();
            AppendLog($"최신 목록 (페이지 {result.CurrentPage}/{result.TotalPages}) — {result.Packages.Count}개:");
            foreach (var package in result.Packages)
                AppendLog($"  [{package.PackageIndex}] {package.Title} by {package.SellerName} (판매: {package.SaleCount:N0}, 썸네일: {package.ThumbnailUrl})");
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            GetNewListButton.IsEnabled = true;
        }
    }

    private async void OnGetHotListButtonClicked(object sender, RoutedEventArgs e)
    {
        GetHotListButton.IsEnabled = false;
        AppendLog("인기 아카콘 목록을 조회합니다...");

        try
        {
            var result = await _client.GetHotListAsync();
            AppendLog($"인기 목록 (페이지 {result.CurrentPage}/{result.TotalPages}) — {result.Packages.Count}개:");
            foreach (var package in result.Packages)
                AppendLog($"  [{package.PackageIndex}] {package.Title} by {package.SellerName} (판매: {package.SaleCount:N0}, 썸네일: {package.ThumbnailUrl})");
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            GetHotListButton.IsEnabled = true;
        }
    }

    private async void OnGetDailyPopularButtonClicked(object sender, RoutedEventArgs e)
    {
        GetDailyPopularButton.IsEnabled = false;
        AppendLog("일간 인기 아카콘 5개를 조회합니다...");

        try
        {
            var popularPackages = await _client.GetDailyPopularAsync();
            AppendPopularPackages("일간 인기 5개", popularPackages);
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            GetDailyPopularButton.IsEnabled = true;
        }
    }

    private async void OnGetWeeklyPopularButtonClicked(object sender, RoutedEventArgs e)
    {
        GetWeeklyPopularButton.IsEnabled = false;
        AppendLog("주간 인기 아카콘 5개를 조회합니다...");

        try
        {
            var popularPackages = await _client.GetWeeklyPopularAsync();
            AppendPopularPackages("주간 인기 5개", popularPackages);
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            GetWeeklyPopularButton.IsEnabled = true;
        }
    }

    private async void OnGetMonthlyPopularButtonClicked(object sender, RoutedEventArgs e)
    {
        GetMonthlyPopularButton.IsEnabled = false;
        AppendLog("월간 인기 아카콘 5개를 조회합니다...");

        try
        {
            var popularPackages = await _client.GetMonthlyPopularAsync();
            AppendPopularPackages("월간 인기 5개", popularPackages);
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            GetMonthlyPopularButton.IsEnabled = true;
        }
    }

    private async void OnSearchButtonClicked(object sender, RoutedEventArgs e)
    {
        var query = SearchQueryTextBox.Text.Trim();
        if (string.IsNullOrEmpty(query))
        {
            AppendLog("검색어를 입력해 주세요.");
            return;
        }

        SearchButton.IsEnabled = false;
        var searchType = SearchTypeComboBox.SelectedIndex switch
        {
            1 => ArcaconSearchType.NickName,
            2 => ArcaconSearchType.Tags,
            _ => ArcaconSearchType.Title
        };
        var searchSort = SearchSortComboBox.SelectedIndex switch
        {
            1 => ArcaconSearchSort.New,
            _ => ArcaconSearchSort.Hot
        };
        AppendLog($"'{query}' 검색 중 (유형: {searchType}, 정렬: {searchSort})...");

        try
        {
            var result = await _client.SearchAsync(query, searchType, searchSort);
            AppendLog($"검색 결과 (페이지 {result.CurrentPage}/{result.TotalPages}) — {result.Packages.Count}개:");
            foreach (var package in result.Packages)
                AppendLog($"  [{package.PackageIndex}] {package.Title} by {package.SellerName} (판매: {package.SaleCount:N0})");
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            SearchButton.IsEnabled = true;
        }
    }

    private void OnClearLogButtonClicked(object sender, RoutedEventArgs e) => _logItems.Clear();

    private void AppendPopularPackages(string title, IReadOnlyList<ArcaconPackageSummary> popularPackages)
    {
        AppendLog($"{title} — {popularPackages.Count}개:");
        foreach (var popularPackage in popularPackages)
            AppendLog($"  [{popularPackage.PackageIndex}] {popularPackage.Title} by {popularPackage.SellerName} (판매: {popularPackage.SaleCount:N0}, 썸네일: {popularPackage.ThumbnailUrl})");
    }

    private async void OnGetSubscribedPackagesButtonClicked(object sender, RoutedEventArgs e)
    {
        GetSubscribedPackagesButton.IsEnabled = false;
        AppendLog("구독 중인 아카콘 목록을 조회합니다...");

        try
        {
            var packages = await _client.GetSubscribedPackagesAsync(includeInactive: true);
            var activePackages = packages.Where(package => package.IsActive).ToList();
            var inactivePackages = packages.Where(package => !package.IsActive).ToList();

            AppendLog($"구독 목록 — 총 {packages.Count}개 (사용 중: {activePackages.Count}, 미사용: {inactivePackages.Count}):");
            foreach (var package in activePackages)
                AppendLog($"  [사용 중] [{package.PackageIndex}] {package.ThumbnailUrl}");
            foreach (var package in inactivePackages)
                AppendLog($"  [미사용]  [{package.PackageIndex}] {package.ThumbnailUrl}");
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            GetSubscribedPackagesButton.IsEnabled = true;
        }
    }

    private async void OnGetPackageDetailButtonClicked(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PackageIdTextBox.Text, out var packageId))
        {
            AppendLog("올바른 패키지 ID를 입력해 주세요.");
            return;
        }

        GetPackageDetailButton.IsEnabled = false;
        AppendLog($"패키지 {packageId} 상세를 조회합니다...");

        try
        {
            var detail = await _client.GetPackageDetailAsync(packageId);
            AppendLog($"제목: {detail.Title}");
            AppendLog($"제작자: {detail.SellerName}");
            AppendLog($"등록일: {detail.RegistrationDate}");
            AppendLog($"판매 수: {detail.SaleCount:N0}");
            AppendLog($"가격: {detail.Price}pt");
            AppendLog($"태그: {string.Join(", ", detail.Tags)}");
            AppendLog($"스티커 수: {detail.IconCount}개");
            foreach (var sticker in detail.Stickers)
                AppendLog($"  [{sticker.Id}] media={sticker.ImageUrl} video={sticker.VideoUrl ?? "null"} poster={sticker.PosterThumbnailUrl ?? "null"}");
        }
        catch (Exception exception)
        {
            AppendLog($"오류: {exception.Message}");
        }
        finally
        {
            GetPackageDetailButton.IsEnabled = true;
        }
    }
}
