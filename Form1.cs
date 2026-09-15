using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;

namespace UkrSibKeyTool;

public partial class Form1 : Form
{
    private TextBox txtPemPath = null!;
    private Button btnBrowse = null!;
    private CheckBox chkEncrypted = null!;
    private TextBox txtPassword = null!;
    private CheckBox chkShowPassword = null!;
    private Button btnConvert = null!;
    private TextBox txtOutput = null!;
    private Button btnCopy = null!;
    private Button btnSave = null!;
    private CheckBox chkEnableLog = null!;
    private Button btnViewLog = null!;
    private Button btnGenerateKey = null!;
    private Button btnVersion = null!;
    private Button btnGitHub = null!;
    private Label lblStatus = null!;

    public Form1()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "UkrSibKeyTool \u2014 RSA-\u043a\u043b\u044e\u0447\u0456 \u0434\u043b\u044f 1\u0421";
        ClientSize = new Size(760, 646);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        var lblFile = new Label { Text = "\u0424\u0430\u0439\u043b \u043f\u0440\u0438\u0432\u0430\u0442\u043d\u043e\u0433\u043e \u043a\u043b\u044e\u0447\u0430 (.pem):", Left = 12, Top = 15, Width = 300, AutoSize = true };
        txtPemPath = new TextBox { Left = 12, Top = 38, Width = 600, ReadOnly = true };
        btnBrowse = new Button { Text = "\u041e\u0431\u0440\u0430\u0442\u0438 \u0444\u0430\u0439\u043b...", Left = 620, Top = 36, Width = 128 };
        btnBrowse.Click += BtnBrowse_Click;

        btnGenerateKey = new Button { Text = "\u0417\u0433\u0435\u043d\u0435\u0440\u0443\u0432\u0430\u0442\u0438 \u043d\u043e\u0432\u0438\u0439 \u043a\u043b\u044e\u0447...", Left = 480, Top = 10, Width = 268, Height = 22 };
        btnGenerateKey.Click += BtnGenerateKey_Click;

        chkEncrypted = new CheckBox { Text = "\u041a\u043b\u044e\u0447 \u0437\u0430\u0448\u0438\u0444\u0440\u043e\u0432\u0430\u043d\u0438\u0439 \u043f\u0430\u0440\u043e\u043b\u0435\u043c", Left = 12, Top = 72, Width = 260, AutoSize = true };
        chkEncrypted.CheckedChanged += (s, e) => txtPassword.Enabled = chkEncrypted.Checked;

        var lblPass = new Label { Text = "\u041f\u0430\u0440\u043e\u043b\u044c:", Left = 300, Top = 74, Width = 60, AutoSize = true };
        txtPassword = new TextBox { Left = 350, Top = 71, Width = 200, PasswordChar = '*', Enabled = false };

        chkShowPassword = new CheckBox { Text = "\u041f\u043e\u043a\u0430\u0437\u0430\u0442\u0438 \u043f\u0430\u0440\u043e\u043b\u044c", Left = 560, Top = 74, Width = 150, AutoSize = true };
        chkShowPassword.CheckedChanged += (s, e) => txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';

        btnConvert = new Button { Text = "\u041a\u043e\u043d\u0432\u0435\u0440\u0442\u0443\u0432\u0430\u0442\u0438 \u0432 XML", Left = 12, Top = 105, Width = 200, Height = 32 };
        btnConvert.Click += BtnConvert_Click;

        var lblOut = new Label { Text = "\u0420\u0435\u0437\u0443\u043b\u044c\u0442\u0430\u0442 (\u0432\u0441\u0442\u0430\u0432\u0442\u0435 \u0443 \u043c\u041f\u0440\u0438\u0432\u0430\u0442\u043d\u044b\u0439\u041a\u043b\u044e\u0447XML):", Left = 12, Top = 150, Width = 500, AutoSize = true };
        txtOutput = new TextBox
        {
            Left = 12,
            Top = 173,
            Width = 718,
            Height = 300,
            Multiline = true,
            ReadOnly = true,
            WordWrap = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9F)
        };

        btnCopy = new Button { Text = "\u041a\u043e\u043f\u0456\u044e\u0432\u0430\u0442\u0438 \u0432 \u0431\u0443\u0444\u0435\u0440 \u043e\u0431\u043c\u0456\u043d\u0443", Left = 12, Top = 485, Width = 220, Height = 30 };
        btnCopy.Click += BtnCopy_Click;

        btnSave = new Button { Text = "\u0417\u0431\u0435\u0440\u0435\u0433\u0442\u0438 \u044f\u043a .xml...", Left = 240, Top = 485, Width = 180, Height = 30 };
        btnSave.Click += BtnSave_Click;

        btnViewLog = new Button { Text = "\u041f\u0435\u0440\u0435\u0433\u043b\u044f\u043d\u0443\u0442\u0438 \u043b\u043e\u0433", Left = 12, Top = 520, Width = 180, Height = 30 };
        btnViewLog.Click += BtnViewLog_Click;

        chkEnableLog = new CheckBox { Text = "\u0412\u0435\u0441\u0442\u0438 \u0434\u0456\u0430\u0433\u043d\u043e\u0441\u0442\u0438\u0447\u043d\u0438\u0439 \u043b\u043e\u0433 (\u0434\u043b\u044f \u0443\u0441\u0443\u043d\u0435\u043d\u043d\u044f \u043f\u0440\u043e\u0431\u043b\u0435\u043c)", Left = 200, Top = 526, AutoSize = true, Checked = false };
        chkEnableLog.CheckedChanged += (s, e) => OpenSslHelper.LoggingEnabled = chkEnableLog.Checked;

        lblStatus = new Label { Left = 12, Top = 560, Width = 718, Height = 44, ForeColor = Color.Black };

        btnVersion = new Button
        {
            Text = $"v{UpdateHelper.CurrentVersion.ToString(3)} — перевірити оновлення",
            Left = 12,
            Top = 612,
            Width = 220,
            Height = 26,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnVersion.FlatAppearance.BorderSize = 0;
        btnVersion.Click += BtnVersion_Click;

        btnGitHub = new Button
        {
            Text = "🔗",
            Left = 238,
            Top = 612,
            Width = 34,
            Height = 26,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnGitHub.FlatAppearance.BorderSize = 0;
        var githubTip = new ToolTip();
        githubTip.SetToolTip(btnGitHub, "Відкрити репозиторій на GitHub");
        btnGitHub.Click += (s, e) =>
        {
            try { Process.Start(new ProcessStartInfo(UpdateHelper.RepoUrl) { UseShellExecute = true }); }
            catch (Exception ex) { SetStatus("Не вдалося відкрити посилання: " + ex.Message, true); }
        };

        Controls.Add(lblFile);
        Controls.Add(txtPemPath);
        Controls.Add(btnBrowse);
        Controls.Add(btnGenerateKey);
        Controls.Add(chkEncrypted);
        Controls.Add(lblPass);
        Controls.Add(txtPassword);
        Controls.Add(chkShowPassword);
        Controls.Add(btnConvert);
        Controls.Add(lblOut);
        Controls.Add(txtOutput);
        Controls.Add(btnCopy);
        Controls.Add(btnSave);
        Controls.Add(btnViewLog);
        Controls.Add(chkEnableLog);
        Controls.Add(lblStatus);
        Controls.Add(btnVersion);
        Controls.Add(btnGitHub);

        AcceptButton = btnConvert;
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "PEM \u0444\u0430\u0439\u043b\u0438 (*.pem;*.key)|*.pem;*.key|\u0423\u0441\u0456 \u0444\u0430\u0439\u043b\u0438 (*.*)|*.*",
            Title = "\u041e\u0431\u0435\u0440\u0456\u0442\u044c \u043f\u0440\u0438\u0432\u0430\u0442\u043d\u0438\u0439 \u043a\u043b\u044e\u0447"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            txtPemPath.Text = dlg.FileName;
            txtOutput.Text = "";
            string content;
            try { content = File.ReadAllText(dlg.FileName); }
            catch { content = string.Empty; }
            chkEncrypted.Checked = content.Contains("ENCRYPTED");
            SetStatus(string.Empty, false);
        }
    }

    private void BtnConvert_Click(object? sender, EventArgs e)
    {
        SetStatus(string.Empty, false);
        txtOutput.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(txtPemPath.Text) || !File.Exists(txtPemPath.Text))
        {
            SetStatus("\u041e\u0431\u0435\u0440\u0456\u0442\u044c \u043a\u043e\u0440\u0435\u043a\u0442\u043d\u0438\u0439 \u0444\u0430\u0439\u043b \u043a\u043b\u044e\u0447\u0430.", true);
            return;
        }

        string pemText;
        try { pemText = File.ReadAllText(txtPemPath.Text); }
        catch (Exception ex)
        {
            SetStatus("\u041d\u0435 \u0432\u0434\u0430\u043b\u043e\u0441\u044f \u043f\u0440\u043e\u0447\u0438\u0442\u0430\u0442\u0438 \u0444\u0430\u0439\u043b: " + ex.Message, true);
            return;
        }

        bool isLegacyEncrypted = pemText.Contains("Proc-Type: 4,ENCRYPTED");
        string password = txtPassword.Text.Trim();

        if (chkEncrypted.Checked && password.Length > 0)
        {
            string? nonAsciiWarning = FindNonAsciiWarning(password);
            if (nonAsciiWarning != null)
            {
                SetStatus(nonAsciiWarning, true);
                return;
            }
        }

        try
        {
            using var rsa = RSA.Create();

            if (isLegacyEncrypted)
            {
                if (!chkEncrypted.Checked || string.IsNullOrEmpty(password))
                {
                    SetStatus("\u0426\u0435 \u0441\u0442\u0430\u0440\u0438\u0439 \u0444\u043e\u0440\u043c\u0430\u0442 \u0437\u0430\u0448\u0438\u0444\u0440\u043e\u0432\u0430\u043d\u043e\u0433\u043e \u043a\u043b\u044e\u0447\u0430 (Proc-Type: 4,ENCRYPTED). \u0412\u043a\u0430\u0436\u0456\u0442\u044c \u043f\u0430\u0440\u043e\u043b\u044c \u0456 \u043f\u043e\u0432\u0442\u043e\u0440\u0456\u0442\u044c \u0441\u043f\u0440\u043e\u0431\u0443.", true);
                    return;
                }
                string decrypted = OpenSslHelper.DecryptLegacyPem(pemText, password);
                rsa.ImportFromPem(decrypted);
            }
            else if (chkEncrypted.Checked)
            {
                rsa.ImportFromEncryptedPem(pemText, password);
            }
            else
            {
                rsa.ImportFromPem(pemText);
            }

            string xml = rsa.ToXmlString(true);
            txtOutput.Text = xml;
            SetStatus($"\u0413\u043e\u0442\u043e\u0432\u043e. XML \u0441\u0444\u043e\u0440\u043c\u043e\u0432\u0430\u043d\u043e ({xml.Length} \u0441\u0438\u043c\u0432\u043e\u043b\u0456\u0432).", false, success: true);
        }
        catch (CryptographicException ex)
        {
            SetStatus("\u041f\u043e\u043c\u0438\u043b\u043a\u0430 \u0440\u043e\u0437\u0448\u0438\u0444\u0440\u0443\u0432\u0430\u043d\u043d\u044f/\u0440\u043e\u0437\u0431\u043e\u0440\u0443 \u043a\u043b\u044e\u0447\u0430 (\u043c\u043e\u0436\u043b\u0438\u0432\u043e, \u043d\u0435\u0432\u0456\u0440\u043d\u0438\u0439 \u043f\u0430\u0440\u043e\u043b\u044c): " + ex.Message, true);
        }
        catch (Exception ex)
        {
            SetStatus("\u041f\u043e\u043c\u0438\u043b\u043a\u0430: " + ex.Message, true);
        }
    }

    private static string? FindNonAsciiWarning(string password)
    {
        var bad = new List<string>();
        for (int i = 0; i < password.Length; i++)
        {
            char c = password[i];
            if (c > 127)
            {
                bad.Add($"'{c}' (U+{(int)c:X4}, позиція {i + 1})");
            }
        }
        if (bad.Count == 0) return null;

        return "Пароль містить не-ASCII символи, схожі на латинські (ймовірно, була активна кирилична розкладка клавіатури): "
            + string.Join(", ", bad)
            + ". Перемкніть розкладку на англійську (US) і введіть пароль ще раз.";
    }

    private void BtnCopy_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtOutput.Text))
        {
            SetStatus("\u041d\u0435\u043c\u0430\u0454 \u0449\u043e \u043a\u043e\u043f\u0456\u044e\u0432\u0430\u0442\u0438 \u2014 \u0441\u043f\u043e\u0447\u0430\u0442\u043a\u0443 \u0432\u0438\u043a\u043e\u043d\u0430\u0439\u0442\u0435 \u043a\u043e\u043d\u0432\u0435\u0440\u0442\u0430\u0446\u0456\u044e.", true);
            return;
        }
        Clipboard.SetText(txtOutput.Text);
        SetStatus("\u0421\u043a\u043e\u043f\u0456\u0439\u043e\u0432\u0430\u043d\u043e \u0432 \u0431\u0443\u0444\u0435\u0440 \u043e\u0431\u043c\u0456\u043d\u0443.", false, success: true);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtOutput.Text))
        {
            SetStatus("\u041d\u0435\u043c\u0430\u0454 \u0449\u043e \u0437\u0431\u0435\u0440\u0456\u0433\u0430\u0442\u0438 \u2014 \u0441\u043f\u043e\u0447\u0430\u0442\u043a\u0443 \u0432\u0438\u043a\u043e\u043d\u0430\u0439\u0442\u0435 \u043a\u043e\u043d\u0432\u0435\u0440\u0442\u0430\u0446\u0456\u044e.", true);
            return;
        }
        using var dlg = new SaveFileDialog
        {
            Filter = "XML \u0444\u0430\u0439\u043b\u0438 (*.xml)|*.xml|\u0423\u0441\u0456 \u0444\u0430\u0439\u043b\u0438 (*.*)|*.*",
            FileName = "ukrsibbankRsaPrivateKey.xml"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, txtOutput.Text);
            SetStatus("\u0417\u0431\u0435\u0440\u0435\u0436\u0435\u043d\u043e: " + dlg.FileName, false, success: true);
        }
    }

    private void BtnViewLog_Click(object? sender, EventArgs e)
    {
        using var viewer = new LogViewerForm(OpenSslHelper.DebugLogPath);
        viewer.ShowDialog(this);
    }

    private void BtnGenerateKey_Click(object? sender, EventArgs e)
    {
        using var gen = new GenerateKeyForm();
        if (gen.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(gen.GeneratedPrivateKeyPath))
        {
            LoadGeneratedPrivateKey(gen.GeneratedPrivateKeyPath);
        }
    }

    private void LoadGeneratedPrivateKey(string privateKeyPath)
    {
        txtPemPath.Text = privateKeyPath;
        txtOutput.Text = string.Empty;
        chkEncrypted.Checked = false; // generated per the bank's instructions as unencrypted PKCS8
        txtPassword.Text = string.Empty;
        SetStatus("Завантажено щойно згенерований ключ. Натисніть «Конвертувати в XML».", false, success: true);
    }

    private async void BtnVersion_Click(object? sender, EventArgs e)
    {
        btnVersion.Enabled = false;
        SetStatus("Перевірка оновлень...", false);
        try
        {
            var update = await UpdateHelper.CheckForUpdateAsync();
            if (update == null)
            {
                SetStatus($"У вас найновіша версія (v{UpdateHelper.CurrentVersion.ToString(3)}).", false, success: true);
                return;
            }

            string notes = string.IsNullOrWhiteSpace(update.ReleaseNotes) ? "" : "\n\n" + update.ReleaseNotes;
            var result = MessageBox.Show(
                this,
                $"Доступна нова версія {update.TagName} (у вас v{UpdateHelper.CurrentVersion.ToString(3)}).{notes}\n\nОновити зараз?",
                "Доступне оновлення",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result != DialogResult.Yes) return;

            if (string.IsNullOrEmpty(update.DownloadUrl))
            {
                Process.Start(new ProcessStartInfo(update.ReleaseUrl) { UseShellExecute = true });
                SetStatus("Відкрито сторінку релізу — завантажте файл вручну.", false);
                return;
            }

            SetStatus("Завантаження оновлення...", false);
            await UpdateHelper.DownloadAndInstallAsync(update.DownloadUrl);
            // DownloadAndInstallAsync exits the process on success; execution won't reach here.
        }
        catch (Exception ex)
        {
            SetStatus("Не вдалося перевірити/встановити оновлення: " + ex.Message, true);
        }
        finally
        {
            btnVersion.Enabled = true;
        }
    }

    private void SetStatus(string text, bool isError, bool success = false)
    {
        lblStatus.Text = text;
        lblStatus.ForeColor = isError ? Color.DarkRed : (success ? Color.DarkGreen : Color.Black);
    }
}
