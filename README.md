# Arcacon.NET

비공식 아카라이브 아카콘(Arcacon) .NET 라이브러리

[![NuGet](https://img.shields.io/nuget/v/Arcacon.NET.svg)](https://www.nuget.org/packages/Arcacon.NET)
[![NuGet (WinRT)](https://img.shields.io/nuget/v/Arcacon.NET.WinRT.svg?label=NuGet%20(WinRT))](https://www.nuget.org/packages/Arcacon.NET.WinRT)
[![Windows](https://img.shields.io/badge/platform-Windows-blue.svg)](https://www.microsoft.com/windows)

## 소개

Arcacon.NET은 아카라이브의 아카콘(Arcacon) 스티커를 프로그래밍적으로 검색, 조회, 다운로드할 수 있는 .NET 라이브러리입니다.

> ⚠️ **주의**: 이 라이브러리는 비공식이며, 아카라이브의 사이트 구조 변경 시 동작하지 않을 수 있습니다. 과도한 자동 요청은 IP 차단의 원인이 될 수 있으니 주의하세요.

> 🪟 **Windows 전용**: WebView2 기반 로그인을 사용하므로 Windows에서만 동작합니다.

## 기능

- 🔍 **아카콘 검색** — 제목, 닉네임, 태그로 검색 (인기순/최신순 정렬)
- 📋 **목록 조회** — 인기 아카콘 / 최신 아카콘 목록
- 📦 **패키지 상세** — 패키지 정보, 스티커 목록, 태그 조회
- 🔖 **구독 목록** — 내가 구독(보유) 중인 아카콘 목록 조회
- ⬇️ **이미지 다운로드** — 개별 스티커 또는 패키지 전체 일괄 다운로드
- ⚡ **병렬 다운로드** — 패키지 전체 다운로드 시 병렬 처리 + 진행 상태 콜백
- 🔐 **WebView2 로그인** — WebView2 기반 arca.live 로그인 (로그인 필요 API에 사용)

## 설치

사용 환경에 맞는 패키지를 설치하세요.

| 환경 | 패키지 |
|---|---|
| WinUI 3 앱 | `Arcacon.NET.WinRT` |
| WPF / WinForms / Console 앱 | `Arcacon.NET` |

```bash
# WinUI 3 앱
dotnet add package Arcacon.NET.WinRT

# WPF / WinForms / Console 앱
dotnet add package Arcacon.NET
```

또는 NuGet Package Manager에서 `Arcacon.NET` 또는 `Arcacon.NET.WinRT`를 검색하세요.

## 로그인

> 🔒 스티커 다운로드를 제외한 **모든 API는 로그인이 필요합니다**.

로그인은 WebView2 기반으로 동작합니다. 앱에서 `CoreWebView2` 인스턴스를 전달하면, 사용자가 직접 arca.live에 로그인할 때까지 대기한 뒤 세션 쿠키를 자동으로 추출합니다.

```csharp
// WebView2 초기화 (WinUI 3 예시)
await webView.EnsureCoreWebView2Async();
await client.LoginAsync(webView.CoreWebView2);
```

## 사용법

### 기본 사용

```csharp
using Arcacon.NET;
using Arcacon.NET.Models;

await using var client = new ArcaconClient();

// 로그인 (CoreWebView2 인스턴스 필요)
await client.LoginAsync(webView.CoreWebView2);

// 아카콘 검색
var result = await client.SearchAsync("스텔라소라");
foreach (var package in result.Packages)
    Console.WriteLine($"[{package.PackageIndex}] {package.Title} - {package.SellerName}");
```

### 인기/최신 목록 조회

```csharp
// 인기 아카콘
var hotList = await client.GetHotListAsync(page: 1);

// 최신 아카콘
var newList = await client.GetNewListAsync(page: 1);
```

### 검색 옵션

```csharp
// 태그로 검색, 최신순 정렬, 2페이지
var result = await client.SearchAsync(
    query: "고양이",
    searchType: ArcaconSearchType.Tags,
    sort: ArcaconSearchSort.New,
    page: 2);
```

### 패키지 상세 조회

```csharp
var detail = await client.GetPackageDetailAsync(packageIndex: 38576);

Console.WriteLine($"제목: {detail.Title}");
Console.WriteLine($"제작자: {detail.SellerName}");
Console.WriteLine($"스티커 수: {detail.Stickers.Count}개");
Console.WriteLine($"태그: {string.Join(", ", detail.Tags)}");
```

### 구독 목록 조회

```csharp
// 사용 중인 아카콘만
var activePackages = await client.GetSubscribedPackagesAsync();

// 미사용 아카콘 포함
var allPackages = await client.GetSubscribedPackagesAsync(includeInactive: true);

foreach (var package in allPackages)
    Console.WriteLine($"[{package.PackageIndex}] 사용 중: {package.IsActive}");
```

### 스티커 다운로드

```csharp
// 개별 스티커 다운로드 (byte[]) — 로그인 불필요
var sticker = detail.Stickers[0];
byte[] imageData = await client.DownloadStickerAsync(sticker);

// 스트림으로 다운로드
using var stream = await client.DownloadStickerStreamAsync(sticker);
```

### 패키지 전체 다운로드

```csharp
var progress = new Progress<(int Completed, int Total)>(report =>
    Console.WriteLine($"다운로드 중... {report.Completed}/{report.Total}"));

await client.DownloadPackageAsync(
    packageIndex: 38576,
    outputDirectory: @"C:\Downloads",
    progress: progress);
```

### 다운로드 파일명 예측

`ArcaconFileNameHelper`를 사용하면 다운로드 시 사용되는 파일명을 사전에 알 수 있습니다:

```csharp
var detail = await client.GetPackageDetailAsync(packageIndex: 38576);

foreach (var sticker in detail.Stickers)
{
    // DownloadPackageAsync에서 저장되는 파일명과 동일
    string fileName = ArcaconFileNameHelper.GetStickerFileName(sticker);
    Console.WriteLine(fileName);
}

// 파일명 안전 변환만 필요한 경우
string safeName = ArcaconFileNameHelper.SanitizeFileName("잘못된/파일:명");
```

### HttpClient 주입 (DI 패턴)

```csharp
// IHttpClientFactory 패턴과 호환
var httpClient = httpClientFactory.CreateClient();
await using var client = new ArcaconClient(httpClient);
```

### CancellationToken 지원

모든 비동기 메서드에서 `CancellationToken`을 지원합니다:

```csharp
using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var result = await client.SearchAsync("스텔라소라", cancellationToken: cancellationTokenSource.Token);
```

## API 참조

### `IArcaconClient` 인터페이스

| 메서드 | 로그인 필요 | 설명 |
|--------|:-----------:|------|
| `LoginAsync` | — | WebView2를 통해 arca.live 로그인 |
| `SearchAsync` | ✅ | 아카콘 검색 (제목/닉네임/태그, 인기순/최신순) |
| `GetHotListAsync` | ✅ | 인기 아카콘 목록 조회 |
| `GetNewListAsync` | ✅ | 최신 아카콘 목록 조회 |
| `GetPackageDetailAsync` | ✅ | 패키지 상세 정보 조회 |
| `GetSubscribedPackagesAsync` | ✅ | 구독(보유) 아카콘 목록 조회 |
| `DownloadStickerAsync` | ❌ | 스티커 이미지 byte[] 다운로드 |
| `DownloadStickerStreamAsync` | ❌ | 스티커 이미지 Stream 다운로드 |
| `DownloadPackageAsync` | ✅ | 패키지 전체 일괄 다운로드 |

### 유틸리티

| 클래스 | 메서드 | 설명 |
|--------|--------|------|
| `ArcaconFileNameHelper` | `SanitizeFileName` | 파일명에 사용할 수 없는 문자를 제거 |
| `ArcaconFileNameHelper` | `GetStickerFileName` | 스티커 다운로드 시 사용되는 파일명 반환 |

### 모델

| 클래스 | 설명 |
|--------|------|
| `ArcaconSearchResult` | 검색 결과 (패키지 목록 + 페이지네이션) |
| `ArcaconPackageSummary` | 패키지 요약 (검색 결과 항목) |
| `ArcaconPackageDetail` | 패키지 상세 (스티커 목록 + 태그 포함) |
| `ArcaconSticker` | 개별 스티커 정보 |
| `ArcaconSubscribedPackage` | 구독(보유) 아카콘 패키지 정보 |
| `ArcaconSearchSort` | 정렬 방식 (Hot, New) |
| `ArcaconSearchType` | 검색 유형 (Title, NickName, Tags) |

## 요구 사항

- .NET 10.0 이상 (Windows 10.0.26100.0 이상)
- Windows 전용 (WebView2 의존성)

## 의존성

- [AngleSharp](https://anglesharp.github.io/) — HTML 파싱
- [Microsoft.Web.WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) — 로그인 처리
- [System.Net.Http.WinHttpHandler](https://learn.microsoft.com/dotnet/api/system.net.http.winhttphandler) — HTTP 통신

## 라이선스

MIT License

## 작성자

**이호원**

## 감사의 말

이 프로젝트는 [GitHub Copilot](https://github.com/features/copilot)의 도움을 받아 작성되었습니다.
