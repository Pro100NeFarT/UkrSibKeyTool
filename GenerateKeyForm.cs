namespace UkrSibKeyTool;

public class GenerateKeyForm : Form
{
    public string? GeneratedPrivateKeyPath { get; private set; }

    private TextBox txtPrivatePath = null!;
    private TextBox txtPublicPath = null!;
    private Button btnChoosePrivate = null!;
    private Button btnChoosePublic = null!;
    private Button btnGenerate = null!;
    private Button btnUse = null!;
    private Button btnClose = null!;
    private Label lblStatus = null!;

    private TextBox txtSystemName = null!;
    private Button btnFormatLetter = null!;
    private TextBox txtSubject = null!;
    private Button btnCopySubject = null!;
    private TextBox txtBody = null!;
    private Button btnCopyBody = null!;
    private Label lblAttachmentHint = null!;

    public GenerateKeyForm()
    {
        InitializeComponent();
        FormatLetter();
    }

    private void InitializeComponent()
    {
        Text = "Генерація нового RSA-ключа (UkrSib business API)";
        ClientSize = new Size(700, 650);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblIntro = new Label
        {
            Left = 12,
            Top = 12,
            Width = 676,
            Height = 36,
            Text = "Буде згенеровано пару RSA-4096 ключів так само, як у інструкції банку: " +
                   "приватний — PKCS8 без пароля, відкритий — окремим файлом для надсилання банку."
        };

        var lblPriv = new Label { Left = 12, Top = 54, Width = 300, Text = "Приватний ключ буде збережено як:" };
        txtPrivatePath = new TextBox { Left = 12, Top = 76, Width = 560, ReadOnly = true };
        btnChoosePrivate = new Button { Left = 580, Top = 74, Width = 108, Text = "Обрати..." };
        btnChoosePrivate.Click += BtnChoosePrivate_Click;

        var lblPub = new Label { Left = 12, Top = 108, Width = 400, Text = "Відкритий ключ (для банку) буде збережено як:" };
        txtPublicPath = new TextBox { Left = 12, Top = 130, Width = 560, ReadOnly = true };
        btnChoosePublic = new Button { Left = 580, Top = 128, Width = 108, Text = "Обрати..." };
        btnChoosePublic.Click += BtnChoosePublic_Click;

        btnGenerate = new Button { Left = 12, Top = 166, Width = 260, Height = 34, Text = "Згенерувати RSA-4096 ключ" };
        btnGenerate.Click += BtnGenerate_Click;

        lblStatus = new Label { Left = 12, Top = 206, Width = 676, Height = 32, ForeColor = Color.Black };

        var lblNextStep = new Label
        {
            Left = 12,
            Top = 244,
            Width = 676,
            Height = 18,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Наступний крок: надішліть відкритий ключ банку листом (UKRSIB business)"
        };

        var lblSystemName = new Label { Left = 12, Top = 270, Width = 100, Text = "Назва системи:" };
        txtSystemName = new TextBox { Left = 116, Top = 267, Width = 200, Text = "BAF" };
        btnFormatLetter = new Button { Left = 328, Top = 265, Width = 220, Height = 26, Text = "Сформувати текст листа" };
        btnFormatLetter.Click += (s, e) => FormatLetter();

        var lblSubject = new Label { Left = 12, Top = 304, Width = 90, Text = "Тема листа:" };
        txtSubject = new TextBox { Left = 116, Top = 301, Width = 452, ReadOnly = true };
        btnCopySubject = new Button { Left = 580, Top = 299, Width = 108, Height = 26, Text = "Копіювати" };
        btnCopySubject.Click += (s, e) =>
        {
            Clipboard.SetText(txtSubject.Text);
            SetStatus("Тему листа скопійовано в буфер обміну.", false, success: true);
        };

        var lblBody = new Label { Left = 12, Top = 336, Width = 300, Text = "Текст листа (можна відредагувати):" };
        txtBody = new TextBox
        {
            Left = 12,
            Top = 358,
            Width = 676,
            Height = 150,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 9F)
        };

        lblAttachmentHint = new Label
        {
            Left = 12,
            Top = 514,
            Width = 676,
            Height = 34,
            ForeColor = Color.DimGray,
            Text = "Не забудьте прикріпити файл відкритого ключа до листа."
        };

        btnCopyBody = new Button { Left = 12, Top = 552, Width = 220, Height = 28, Text = "Копіювати текст листа" };
        btnCopyBody.Click += (s, e) =>
        {
            Clipboard.SetText(txtBody.Text);
            SetStatus("Текст листа скопійовано в буфер обміну.", false, success: true);
        };

        btnUse = new Button { Left = 12, Top = 602, Width = 300, Height = 34, Text = "Використати цей ключ у головному вікні", Enabled = false };
        btnUse.Click += BtnUse_Click;

        btnClose = new Button { Left = 588, Top = 602, Width = 100, Height = 34, Text = "Закрити" };
        btnClose.Click += (s, e) => Close();

        Controls.Add(lblIntro);
        Controls.Add(lblPriv);
        Controls.Add(txtPrivatePath);
        Controls.Add(btnChoosePrivate);
        Controls.Add(lblPub);
        Controls.Add(txtPublicPath);
        Controls.Add(btnChoosePublic);
        Controls.Add(btnGenerate);
        Controls.Add(lblStatus);
        Controls.Add(lblNextStep);
        Controls.Add(lblSystemName);
        Controls.Add(txtSystemName);
        Controls.Add(btnFormatLetter);
        Controls.Add(lblSubject);
        Controls.Add(txtSubject);
        Controls.Add(btnCopySubject);
        Controls.Add(lblBody);
        Controls.Add(txtBody);
        Controls.Add(lblAttachmentHint);
        Controls.Add(btnCopyBody);
        Controls.Add(btnUse);
        Controls.Add(btnClose);

        CancelButton = btnClose;
    }

    private void FormatLetter()
    {
        string sysName = string.IsNullOrWhiteSpace(txtSystemName.Text) ? "(вкажіть назву системи)" : txtSystemName.Text.Trim();
        txtSubject.Text = "Підключення до API";
        txtBody.Text =
            "Добрий день.\r\n" +
            "Для отримання доступу до налаштувань UKRSIB business API надаємо наступні дані:\r\n" +
            $"1) Назва системи – {sysName}\r\n" +
            "2) Відкрита частина RSA-ключа (publicKey) у прикріпленому файлі";
        UpdateAttachmentHint();
    }

    private void UpdateAttachmentHint()
    {
        lblAttachmentHint.Text = string.IsNullOrEmpty(txtPublicPath.Text)
            ? "Не забудьте прикріпити файл відкритого ключа до листа (оберіть його вище)."
            : "Не забудьте прикріпити до листа файл: " + txtPublicPath.Text;
    }

    private void BtnChoosePrivate_Click(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog
        {
            Title = "Куди зберегти приватний ключ",
            Filter = "PEM файли (*.pem)|*.pem|Усі файли (*.*)|*.*",
            FileName = "ukrsibbankRsaPrivateKey.pem"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            txtPrivatePath.Text = dlg.FileName;
            if (string.IsNullOrEmpty(txtPublicPath.Text))
            {
                string? dir = Path.GetDirectoryName(dlg.FileName);
                if (dir != null)
                {
                    txtPublicPath.Text = Path.Combine(dir, "ukrsibbankRsaPublicKey.pem");
                    UpdateAttachmentHint();
                }
            }
            SetStatus(string.Empty, false);
        }
    }

    private void BtnChoosePublic_Click(object? sender, EventArgs e)
    {
        string? initialDir = string.IsNullOrEmpty(txtPrivatePath.Text) ? null : Path.GetDirectoryName(txtPrivatePath.Text);
        using var dlg = new SaveFileDialog
        {
            Title = "Куди зберегти відкритий ключ (для банку)",
            Filter = "PEM файли (*.pem)|*.pem|Усі файли (*.*)|*.*",
            FileName = "ukrsibbankRsaPublicKey.pem",
            InitialDirectory = initialDir ?? string.Empty
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            txtPublicPath.Text = dlg.FileName;
            UpdateAttachmentHint();
            SetStatus(string.Empty, false);
        }
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPrivatePath.Text) || string.IsNullOrWhiteSpace(txtPublicPath.Text))
        {
            SetStatus("Оберіть, куди зберегти обидва файли — приватний і відкритий ключ.", true);
            return;
        }
        if (string.Equals(txtPrivatePath.Text, txtPublicPath.Text, StringComparison.OrdinalIgnoreCase))
        {
            SetStatus("Приватний і відкритий ключ мають бути різними файлами.", true);
            return;
        }

        var existing = new List<string>();
        if (File.Exists(txtPrivatePath.Text)) existing.Add(txtPrivatePath.Text);
        if (File.Exists(txtPublicPath.Text)) existing.Add(txtPublicPath.Text);
        if (existing.Count > 0)
        {
            var result = MessageBox.Show(
                this,
                "Файл(и) вже існують і будуть перезаписані:\n\n" + string.Join("\n", existing) +
                "\n\nЯкщо це вже робочий ключ банку — перезапис зробить його недійсним!\n\nПродовжити?",
                "Підтвердження перезапису",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;
        }

        btnGenerate.Enabled = false;
        btnChoosePrivate.Enabled = false;
        btnChoosePublic.Enabled = false;
        btnUse.Enabled = false;
        SetStatus("Генерується RSA-4096 ключ... (кілька секунд)", false);

        string privatePath = txtPrivatePath.Text;
        string publicPath = txtPublicPath.Text;

        try
        {
            await System.Threading.Tasks.Task.Run(() => OpenSslHelper.GenerateRsaKeyPair(privatePath, publicPath));
            GeneratedPrivateKeyPath = privatePath;
            btnUse.Enabled = true;
            UpdateAttachmentHint();
            SetStatus("Готово! Ключі збережено. Тепер надішліть відкритий ключ банку листом (текст нижче).", false, success: true);
        }
        catch (Exception ex)
        {
            SetStatus("Помилка генерації: " + ex.Message, true);
        }
        finally
        {
            btnGenerate.Enabled = true;
            btnChoosePrivate.Enabled = true;
            btnChoosePublic.Enabled = true;
        }
    }

    private void BtnUse_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void SetStatus(string text, bool isError, bool success = false)
    {
        lblStatus.Text = text;
        lblStatus.ForeColor = isError ? Color.DarkRed : (success ? Color.DarkGreen : Color.Black);
    }
}
