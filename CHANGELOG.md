# 변경 이력

## 0.3.0

- 목록/검색/인기 목록 썸네일을 `https://arca.live/api/emoticon/{packageIndex}/thumb` 기준으로 통일했습니다.
- `GetDailyPopularAsync`, `GetWeeklyPopularAsync`, `GetMonthlyPopularAsync`를 추가해 상단 인기 5개를 별도 조회할 수 있습니다.
- `GetPackageDetailAsync()`가 공개 API(`/api/emoticon/{packageIndex}`)를 함께 사용해 스티커 `ImageUrl`을 공개 API 기준 URL로 보정합니다.
- 비디오 스티커 메타데이터를 확장해 `VideoUrl`, `PosterThumbnailUrl`을 함께 제공합니다.
- 공개 API 역직렬화를 소스 생성 컨텍스트 기반으로 변경해 NativeAOT 환경을 지원합니다.
- 샘플 앱 버튼 레이아웃과 Any CPU 설정을 정리했습니다.
