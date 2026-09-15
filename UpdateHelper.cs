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

        // Look for the installer specifically (not the portable .zip) — the auto-update
        // flow silently runs it, which is the only asset that knows how to update the
        // folder-based install correctly.
        string? downloadUrl = null;
        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                string name = asset.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                    name.Contains("setup", StringComparison.OrdinalIgnoreCase))
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
    /// Downloads the release's installer and runs it silently. The app is deployed as a
    /// folder of ~250 files (not a single exe), so a hand-rolled file swap can't safely
    /// update it — the installer already knows how to do that correctly. Its
    /// "CloseApplications=yes" setting closes this running instance via Windows Restart
    /// Manager before overwriting files, and its [Run] entry relaunches the app afterward
    /// (including in silent mode). Call this last — it terminates the current process.
    /// </summary>
    public static async Task DownloadAndInstallAsync(string downloadUrl)
    {
        string tempInstaller = Path.Combine(Path.GetTempPath(), $"UkrSibKeyTool_Setup_{Guid.NewGuid():N}.exe");

        using (var http = new HttpClient())
        {
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(RepoName, CurrentVersion.ToString()));
            http.Timeout = TimeSpan.FromMinutes(5);
            await using var fs = new FileStream(tempInstaller, FileMode.Create, FileAccess.Write);
            using var resp = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();
            await resp.Content.CopyToAsync(fs);
        }

        var psi = new ProcessStartInfo
        {
            FileName = tempInstaller,
            ArgumentList = { "/CURRENTUSER", "/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART" },
            UseShellExecute = true
        };
        Process.Start(psi);

        Environment.Exit(0);
    }
}
