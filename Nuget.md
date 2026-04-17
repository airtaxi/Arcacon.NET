# NuGet 패키지 배포 가이드

## 패키지 구성

| 패키지 | 대상 | `WebView2EnableCsWinRTProjection` |
|---|---|---|
| `Arcacon.NET` | WPF / WinForms / Console 앱 | `false` |
| `Arcacon.NET.WinRT` | WinUI 3 앱 | `true` |

두 패키지는 동일한 소스를 공유하며 (`Arcacon.NET.WinRT`가 `Arcacon.NET` 소스를 링크로 참조),  
공통 속성은 `src/Directory.Build.props`에서 관리합니다.

---

## 버전 업데이트

`src/Directory.Build.props`의 `<Version>` 값을 변경합니다.

```xml
<Version>0.2.0</Version>
```

---

## 패킹

```bash
dotnet pack src/Arcacon.NET/Arcacon.NET.csproj             -c Release -o nupkg/
dotnet pack src/Arcacon.NET.WinRT/Arcacon.NET.WinRT.csproj -c Release -o nupkg/
```

---

## NuGet 배포

```bash
dotnet nuget push nupkg/Arcacon.NET.<version>.nupkg           --api-key <API_KEY> --source https://api.nuget.org/v3/index.json
dotnet nuget push nupkg/Arcacon.NET.WinRT.<version>.nupkg     --api-key <API_KEY> --source https://api.nuget.org/v3/index.json
```

> API Key는 [NuGet.org](https://www.nuget.org/account/apikeys)에서 발급합니다.

---

## 한 번에 배포 (순서대로 실행)

```bash
dotnet pack src/Arcacon.NET/Arcacon.NET.csproj             -c Release -o nupkg/
dotnet pack src/Arcacon.NET.WinRT/Arcacon.NET.WinRT.csproj -c Release -o nupkg/
dotnet nuget push nupkg/Arcacon.NET.<version>.nupkg           --api-key <API_KEY> --source https://api.nuget.org/v3/index.json
dotnet nuget push nupkg/Arcacon.NET.WinRT.<version>.nupkg     --api-key <API_KEY> --source https://api.nuget.org/v3/index.json
```
