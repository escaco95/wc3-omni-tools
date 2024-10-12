using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NonWPF.Network
{
    public static class UpdateCheckUtils
    {
        public static async Task<UpdateCheckResult> CheckForUpdates(string userName, string repoName, string currentVersion)
        {
            string githubApiUrl = $"https://api.github.com/repos/{userName}/{repoName}/releases/latest";

            try
            {
                using HttpClient client = new();

                // GitHub API 요청 헤더 추가 (GitHub API 요구사항)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

                // 최신 릴리스 정보 가져오기
                var response = await client.GetStringAsync(githubApiUrl);

                // JSON 데이터 파싱
                var jsonDocument = JsonDocument.Parse(response);
                if (jsonDocument.RootElement.GetProperty("tag_name").GetString() is not string latestVersion)
                    throw new Exception("Failed to get the latest version.");

                if (jsonDocument.RootElement.GetProperty("body").GetString() is not string latestVersionLog)
                    throw new Exception("Failed to get the latest version log.");

                // 버전 비교
                if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
                {
                    // 업데이트가 필요한 경우, 업데이트 페이지 URL 반환
                    var latestVersionWebUrl = $"https://github.com/{userName}/{repoName}/releases/latest";

                    return new UpdateCheckResult(latestVersion, latestVersionWebUrl, latestVersionLog, null); // 업데이트 필요
                }
                else
                {
                    return new UpdateCheckResult(string.Empty, string.Empty, string.Empty, null); // 업데이트가 필요하지 않음
                }
            }
            catch (Exception ex)
            {
                // 오류가 발생한 경우, 예외와 함께 결과 반환
                return new UpdateCheckResult(string.Empty, string.Empty, string.Empty, ex);
            }
        }
    }

    public class UpdateCheckResult(string LatestVersionTagName, string LatestVersionWebUrl, string LatestVersionLog, Exception? Error)
    {
        public bool IsUpdateRequired { get; private set; } = !string.IsNullOrEmpty(LatestVersionWebUrl);

        public string LatestVersionTagName { get; private set; } = LatestVersionTagName;

        public string LatestVersionWebUrl { get; private set; } = LatestVersionWebUrl;

        public string LatestVersionLog { get; private set; } = LatestVersionLog;

        public Exception? Error { get; private set; } = Error;
    }
}
