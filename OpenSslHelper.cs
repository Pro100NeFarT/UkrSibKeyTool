using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace UkrSibKeyTool;

internal static class OpenSslHelper
{
    public static readonly string DebugLogPath = Path.Combine(Path.GetTempPath(), "PemToXmlTool_debug.log");
    public static bool LoggingEnabled = false;

    public static void AppendLog(string line)
    {
        if (!LoggingEnabled) return;
        try
        {
            File.AppendAllText(DebugLogPath, $"[{DateTime.Now:HH:mm:ss.fff}] {line}\r\n");
        }
        catch
        {
            // logging must never break the actual operation
        }
    }

    public static string Sha256Hex(string s)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(hash);
    }

    public static string FindOpenSsl()
    {
        string[] candidates =
        {
            "openssl",
            @"C:\Program Files\Git\usr\bin\openssl.exe",
            @"C:\Program Files\Git\mingw64\bin\openssl.exe"
        };
        foreach (var c in candidates)
        {
            try
            {
                var psi = new ProcessStartInfo(c, "version")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(2000);
                if (p != null && p.ExitCode == 0) return c;
            }
            catch
            {
                // try next candidate
            }
        }
        throw new FileNotFoundException("openssl.exe не знайдено. Встановіть Git for Windows або OpenSSL.");
    }

    public static string DecryptLegacyPem(string pemText, string password)
    {
        // Key content and password are streamed via stdin rather than passed as a
        // command-line/file-path argument, so a Cyrillic (or otherwise non-ASCII)
        // folder/file name can never be mangled by the child process's argv codepage.
        AppendLog("---- DecryptLegacyPem start ----");
        AppendLog($"pemText length={pemText.Length}, sha256={Sha256Hex(pemText)}");
        AppendLog($"password length={password.Length}, sha256={Sha256Hex(password)}");

        string opensslPath = FindOpenSsl();
        AppendLog("Resolved openssl path: " + opensslPath);
        LogOpenSslVersion(opensslPath);

        var psi = new ProcessStartInfo
        {
            FileName = opensslPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.ArgumentList.Add("rsa");
        psi.ArgumentList.Add("-passin");
        psi.ArgumentList.Add("stdin");

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Не вдалося запустити openssl.");

        // Write raw ASCII bytes directly to the pipe's base stream instead of going
        // through StandardInput's TextWriter: in a console-less GUI process that writer's
        // encoding resolves to a bogus "codepage 0" fallback, which can corrupt what's
        // actually sent even though the source strings are pure ASCII. Bypassing it removes
        // all ambiguity about encoding/newline translation.
        byte[] payload = Encoding.ASCII.GetBytes(password + "\n" + pemText);
        AppendLog($"Writing {payload.Length} raw bytes to stdin. First 16 bytes: {Convert.ToHexString(payload, 0, Math.Min(16, payload.Length))}, last 16 bytes: {Convert.ToHexString(payload, Math.Max(0, payload.Length - 16), Math.Min(16, payload.Length))}");
        proc.StandardInput.BaseStream.Write(payload, 0, payload.Length);
        proc.StandardInput.BaseStream.Flush();
        proc.StandardInput.Close();
        AppendLog("Finished writing to stdin and closed it.");

        string stdout = proc.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        AppendLog($"exit code={proc.ExitCode}, stdout length={stdout.Length}, stderr=\"{stderr.Trim()}\"");
        AppendLog("---- DecryptLegacyPem end ----");

        if (proc.ExitCode != 0 || string.IsNullOrWhiteSpace(stdout))
        {
            string hint = LoggingEnabled
                ? "\n\n[Діагностика записана — натисніть «Переглянути лог»]"
                : "\n\n[Увімкніть «Вести діагностичний лог» і спробуйте ще раз, щоб побачити деталі]";
            throw new InvalidOperationException("openssl не зміг розшифрувати ключ (перевірте пароль): " + stderr.Trim() + hint);
        }
        return stdout;
    }

    private static void LogOpenSslVersion(string opensslPath)
    {
        try
        {
            var vpsi = new ProcessStartInfo(opensslPath, "version")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var vproc = Process.Start(vpsi)!;
            string vOut = vproc.StandardOutput.ReadToEnd();
            vproc.WaitForExit(2000);
            AppendLog("openssl version output: " + vOut.Trim());
        }
        catch (Exception ex)
        {
            AppendLog("Failed to query openssl version: " + ex.Message);
        }
    }

    /// <summary>
    /// Generates a new RSA key pair matching what UkrSibBank's own openssl instructions
    /// produce (4096-bit, exponent 65537, unencrypted PKCS8 private key + SubjectPublicKeyInfo
    /// public key), using .NET's own RSA implementation instead of shelling out to openssl.
    /// This keeps key generation free of any external dependency, so a client machine with
    /// no Git/OpenSSL installed can still generate a key with just this one .exe.
    /// </summary>
    public static void GenerateRsaKeyPair(string privateKeyPath, string publicKeyPath, int bits = 4096)
    {
        AppendLog("---- GenerateRsaKeyPair start (pure .NET RSA, no external tools) ----");
        using var rsa = RSA.Create(bits);
        string privatePem = rsa.ExportPkcs8PrivateKeyPem() + "\n";
        string publicPem = rsa.ExportSubjectPublicKeyInfoPem() + "\n";
        File.WriteAllText(privateKeyPath, privatePem);
        AppendLog("Private key (PKCS8, unencrypted) written to: " + privateKeyPath);
        File.WriteAllText(publicKeyPath, publicPem);
        AppendLog("Public key written to: " + publicKeyPath);
        AppendLog("---- GenerateRsaKeyPair end ----");
    }
}
