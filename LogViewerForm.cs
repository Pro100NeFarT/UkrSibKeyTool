namespace UkrSibKeyTool;

public class LogViewerForm : Form
{
    private readonly string logPath;
    private TextBox txtLog = null!;
    private Label lblPath = null!;

    public LogViewerForm(string logPath)
    {
        this.logPath = logPath;
        InitializeComponent();
        LoadLog();
    }

    private void InitializeComponent()
    {
        Text = "Діагностичний лог";
        ClientSize = new Size(760, 520);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(500, 300);

        lblPath = new Label
        {
            Left = 12,
            Top = 10,
            Width = 736,
            AutoSize = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = logPath,
            ForeColor = Color.DimGray
        };

        txtLog = new TextBox
        {
            Left = 12,
            Top = 34,
            Width = 736,
            Height = 430,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Multiline = true,
            ReadOnly = true,
            WordWrap = false,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Consolas", 9F)
        };

        var btnRefresh = new Button
        {
            Text = "Оновити",
            Left = 12,
            Width = 120,
            Height = 30,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        btnRefresh.Click += (s, e) => LoadLog();

        var btnClear = new Button
        {
            Text = "Очистити лог",
            Left = 140,
            Width = 140,
            Height = 30,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        btnClear.Click += (s, e) =>
        {
            try { File.Delete(logPath); } catch { /* ignore */ }
            LoadLog();
        };

        var btnClose = new Button
        {
            Text = "Закрити",
            Width = 120,
            Height = 30,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnClose.Click += (s, e) => Close();

        Controls.Add(lblPath);
        Controls.Add(txtLog);
        Controls.Add(btnRefresh);
        Controls.Add(btnClear);
        Controls.Add(btnClose);

        Resize += (s, e) =>
        {
            btnRefresh.Top = ClientSize.Height - 42;
            btnClear.Top = ClientSize.Height - 42;
            btnClose.Top = ClientSize.Height - 42;
            btnClose.Left = ClientSize.Width - 12 - btnClose.Width;
        };
        btnRefresh.Top = ClientSize.Height - 42;
        btnClear.Top = ClientSize.Height - 42;
        btnClose.Top = ClientSize.Height - 42;
        btnClose.Left = ClientSize.Width - 12 - btnClose.Width;

        CancelButton = btnClose;
    }

    private void LoadLog()
    {
        try
        {
            txtLog.Text = File.Exists(logPath)
                ? File.ReadAllText(logPath)
                : "(Лог порожній — увімкніть «Вести діагностичний лог» на головному вікні й повторіть спробу конвертації.)";
        }
        catch (Exception ex)
        {
            txtLog.Text = "Не вдалося прочитати лог: " + ex.Message;
        }
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }
}
