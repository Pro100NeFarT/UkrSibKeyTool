using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace UkrSibKeyTool;

internal sealed class UpdateInfo
{
    public required Version Version { get; init; }
    public required string TagName { get; init; }
    public required string ReleaseUrl { get; init; }
    public string? DownloadUrl { get; init; }
    public string? ReleaseNotes { get; init; }
}

internal static class UpdateHelper
{
    public const string RepoOwner = "Pro100NeFarT";
    public const string RepoName = "UkrSibKeyTool";
    public static readonly string RepoUrl = $"https://github.com/{RepoOwner}/{RepoName}";

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

    /// <summary>
    /// Asks the GitHub Releases API for the latest release and returns it when its
    /// version is strictly newer than the currently running one; null when up to date.
    /// Throws on network/parse failure so the caller can show a clear error.
    /// </summary>
    public static async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(RepoName, CurrentVersion.ToString()));
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        http.Timeout = TimeSpan.FromSeconds(15);

        string url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
        using var response = await http.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // no releases published yet
        }
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        string tagName = root.GetProperty("tag_name").GetString() ?? "";
        string versionText = tagName.TrimStart('v', 'V');
        if (!Version.TryParse(versionText, out var latestVersion))
        {
            throw new InvalidOperationException($"Не вдалося розпізнати версію релізу: \"{tagName}\"");
        }

        string releaseUrl = root.TryGetProperty("html_url", out var h) ? h.GetString() ?? RepoUrl : RepoUrl;
        string? notes = root.TryGetProperty("body", out var b) ? b.GetString() : null;

        string? downloadUrl = null;
        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                string name = asset.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    downloadUrl = asset.TryGetProperty("browser_download_url", out var u) ? u.GetString() : null;
                    break;
                }
            }
        }

        if (latestVersion <= CurrentVersion) return null;

        return new UpdateInfo
        {
            Version = latestVersion,
            TagName = tagName,
            ReleaseUrl = releaseUrl,
            DownloadUrl = downloadUrl,
            ReleaseNotes = notes
        };
    }

    /// <summary>
    /// Downloads the new .exe and replaces the currently running one in place, then
    /// relaunches it. Because a running .exe can't overwrite itself on Windows, the
    /// swap is done by a tiny detached helper script that waits for this process to
    /// exit first. Call this last — it terminates the current process.
    /// </summary>
    public static async Task DownloadAndInstallAsync(string downloadUrl)
    {
        string currentExe = Environment.ProcessPath ?? Application.ExecutablePath;
        string tempNewExe = Path.Combine(Path.GetTempPath(), $"UkrSibKeyTool_update_{Guid.NewGuid():N}.exe");

        using (var http = new HttpClient())
        {
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(RepoName, CurrentVersion.ToString()));
            http.Timeout = TimeSpan.FromMinutes(5);
            await using var fs = new FileStream(tempNewExe, FileMode.Create, FileAccess.Write);
            using var resp = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();
            await resp.Content.CopyToAsync(fs);
        }

        string scriptPath = Path.Combine(Path.GetTempPath(), $"UkrSibKeyTool_update_{Guid.NewGuid():N}.cmd");
        int pid = Environment.ProcessId;
        string script =
            "@echo off\r\n" +
            $":wait\r\n" +
            $"tasklist /FI \"PID eq {pid}\" | find \"{pid}\" >nul\r\n" +
            "if not errorlevel 1 (\r\n" +
            "  timeout /t 1 /nobreak >nul\r\n" +
            "  goto wait\r\n" +
            ")\r\n" +
            $"move /y \"{tempNewExe}\" \"{currentExe}\" >nul\r\n" +
            $"start \"\" \"{currentExe}\"\r\n" +
            "del \"%~f0\"\r\n";
        File.WriteAllText(scriptPath, script);

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            ArgumentList = { "/c", scriptPath },
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        Process.Start(psi);

        Environment.Exit(0);
    }
}
