using System.Windows.Forms;
using DS.IHF.Helpers;
using DS.IHF.Models;
using DS.IHF.Services;

namespace DS.IHF.Forms;

public class MainForm : Form
{
    private readonly AppSettings         _settings;
    private readonly DocuSignAuthService _auth;
    private readonly DocuSignSendService _sender;
    private readonly DocuSignUserService _userService;
    private readonly HttpClient          _http;

    private TextBox     _folderPath    = null!;
    private Button      _btnBrowse     = null!;
    private Button      _btnStart      = null!;
    private RichTextBox _log           = null!;
    private Label       _lblStatus     = null!;
    private ProgressBar _progress      = null!;

    public MainForm(AppSettings settings, HttpClient http)
    {
        _settings    = settings;
        _http        = http;
        _auth        = new DocuSignAuthService(settings, http);
        _userService = new DocuSignUserService(settings, _auth, http);
        _sender      = new DocuSignSendService(settings, _auth, http);

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        var env = DsEnvironment.Active.Name == "Demo" ? " [DEMO]" : "";
        Text    = $"DS-IHF — DocuSign Serienbrief-Upload{env}";
        Size            = new Size(700, 520);
        MinimumSize     = new Size(600, 480);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;

        var panel = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            Padding     = new Padding(12),
            RowCount    = 5,
            ColumnCount = 1,
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Ordnerauswahl
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Start-Button
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Status
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Log
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // Progress

        // Ordnerauswahl
        var folderGroup = new GroupBox
        {
            Text   = "Ordner mit Serienbriefen (.docx)",
            Dock   = DockStyle.Fill,
            Height = 60,
            Padding = new Padding(8, 4, 8, 4),
        };
        var folderRow = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 1,
            Padding     = new Padding(0),
        };
        folderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        folderRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _folderPath = new TextBox
        {
            Dock      = DockStyle.Fill,
            ReadOnly  = true,
            BackColor = SystemColors.Window,
        };
        _btnBrowse = new Button
        {
            Text      = "Durchsuchen...",
            Width     = 110,
            Dock      = DockStyle.Fill,
        };
        _btnBrowse.Click += OnBrowseClick;

        folderRow.Controls.Add(_folderPath, 0, 0);
        folderRow.Controls.Add(_btnBrowse, 1, 0);
        folderGroup.Controls.Add(folderRow);
        panel.Controls.Add(folderGroup, 0, 0);

        // Start-Button
        _btnStart = new Button
        {
            Text    = "Dokumente senden",
            Height  = 36,
            Dock    = DockStyle.Fill,
            Enabled = false,
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font    = new Font(Font.FontFamily, 10, FontStyle.Bold),
        };
        _btnStart.FlatAppearance.BorderSize = 0;
        _btnStart.Click += OnStartClick;
        panel.Controls.Add(_btnStart, 0, 1);

        // Status
        _lblStatus = new Label
        {
            Text      = "Bereit.",
            Dock      = DockStyle.Fill,
            Height    = 22,
            ForeColor = Color.Gray,
        };
        panel.Controls.Add(_lblStatus, 0, 2);

        // Log
        var logGroup = new GroupBox
        {
            Text    = "Ergebnis",
            Dock    = DockStyle.Fill,
            Padding = new Padding(8),
        };
        _log = new RichTextBox
        {
            Dock      = DockStyle.Fill,
            ReadOnly  = true,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.LightGreen,
            Font      = new Font("Consolas", 9),
            ScrollBars = RichTextBoxScrollBars.Vertical,
        };
        logGroup.Controls.Add(_log);
        panel.Controls.Add(logGroup, 0, 3);

        // Progress
        _progress = new ProgressBar
        {
            Dock    = DockStyle.Fill,
            Height  = 14,
            Visible = false,
        };
        panel.Controls.Add(_progress, 0, 4);

        Controls.Add(panel);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await RunAuthAsync();
    }

    private async Task RunAuthAsync()
    {
        SetStatus("Authentifizierung bei DocuSign...", Color.DarkOrange);
        _btnBrowse.Enabled = false;
        _btnStart.Enabled  = false;

        try
        {
            if (_settings.NeedsUserGuidLookup)
            {
                var email = PromptEmail();
                if (email == null) { Close(); return; }

                SetStatus("Suche Benutzer-GUID...", Color.DarkOrange);
                _auth.UseAdminGuidForNextToken();
                var guid = await _userService.LookupUserGuidByEmailAsync(email);
                _auth.ClearGuidOverride();
                _settings.IMPERSONATION_USER_GUID = guid;
                SettingsService.Save(_settings);
                AppendLog($"Benutzer-GUID gespeichert: {guid}", Color.Cyan);
            }

            await _auth.GetAccessTokenAsync();
            SetStatus("Authentifiziert. Bitte Ordner wählen.", Color.Green);
            _btnBrowse.Enabled = true;
            AppendLog("Token erfolgreich erhalten.", Color.LightGreen);
        }
        catch (Exception ex)
        {
            SetStatus("Authentifizierung fehlgeschlagen.", Color.Red);
            AppendLog($"FEHLER: {ex.Message}", Color.Red);
        }
    }

    private void OnBrowseClick(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description         = "Ordner mit .docx Serienbriefen wählen",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
            AutoUpgradeEnabled  = true,
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _folderPath.Text   = dialog.SelectedPath;
            _btnStart.Enabled  = true;
            var count = Directory.GetFiles(dialog.SelectedPath, "*.docx").Length;
            SetStatus($"{count} .docx-Datei(en) gefunden.", count > 0 ? Color.Green : Color.OrangeRed);
        }
    }

    private async void OnStartClick(object? sender, EventArgs e)
    {
        var files = Directory.GetFiles(_folderPath.Text, "*.docx");
        if (files.Length == 0) { SetStatus("Keine .docx-Dateien gefunden.", Color.OrangeRed); return; }

        _btnStart.Enabled  = false;
        _btnBrowse.Enabled = false;
        _log.Clear();
        _progress.Maximum = files.Length;
        _progress.Value   = 0;
        _progress.Visible = true;

        int ok = 0, fehler = 0;

        foreach (var file in files)
        {
            SetStatus($"Sende {Path.GetFileName(file)}...", Color.DarkOrange);

            DocumentMetadata meta;
            try
            {
                meta = WordDocumentReader.ReadMetadata(file);
            }
            catch (Exception ex)
            {
                AppendLog($"✗ {Path.GetFileName(file)} — Lesefehler: {ex.Message}", Color.Red);
                fehler++;
                _progress.Value++;
                continue;
            }

            var missing = meta.GetMissingFields().ToList();
            if (missing.Any())
            {
                AppendLog($"⚠ {Path.GetFileName(file)} — fehlende Tags: {string.Join(", ", missing)}", Color.Yellow);
                fehler++;
                _progress.Value++;
                continue;
            }

            try
            {
                await _sender.SendEnvelopeAsync(meta);
                AppendLog($"✓ {Path.GetFileName(file)} → {meta.SignerEmail}", Color.LightGreen);
                ok++;
            }
            catch (Exception ex)
            {
                AppendLog($"✗ {Path.GetFileName(file)} — {ex.Message}", Color.Red);
                fehler++;
            }

            _progress.Value++;
        }

        _progress.Visible  = false;
        _btnBrowse.Enabled = true;
        _btnStart.Enabled  = true;

        var summary = $"Fertig: {ok} gesendet, {fehler} Fehler.";
        SetStatus(summary, fehler == 0 ? Color.Green : Color.OrangeRed);
        AppendLog($"\n{summary}", fehler == 0 ? Color.LightGreen : Color.OrangeRed);
    }

    private string? PromptEmail()
    {
        var form = new Form
        {
            Text            = "DocuSign E-Mail",
            AutoSize        = true,
            AutoSizeMode    = AutoSizeMode.GrowAndShrink,
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            MinimizeBox     = false,
            Padding         = new Padding(12),
        };

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            AutoSize    = true,
            ColumnCount = 1,
            RowCount    = 3,
            Padding     = new Padding(0),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 376));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lbl = new Label
        {
            Text     = "Ihre DocuSign E-Mail-Adresse:",
            Dock     = DockStyle.Fill,
            AutoSize = true,
            Margin   = new Padding(0, 0, 0, 4),
        };
        var txt = new TextBox
        {
            Dock   = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8),
        };
        var btn = new Button
        {
            Text         = "OK",
            Dock         = DockStyle.Right,
            Width        = 80,
            DialogResult = DialogResult.OK,
        };

        layout.Controls.Add(lbl, 0, 0);
        layout.Controls.Add(txt, 0, 1);
        layout.Controls.Add(btn, 0, 2);
        form.Controls.Add(layout);
        form.AcceptButton = btn;

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            var result = txt.Text.Trim();
            return string.IsNullOrEmpty(result) ? null : result;
        }
        return null;
    }

    private void SetStatus(string text, Color color)
    {
        _lblStatus.ForeColor = color;
        _lblStatus.Text      = text;
    }

    private void AppendLog(string text, Color color)
    {
        _log.SelectionStart  = _log.TextLength;
        _log.SelectionLength = 0;
        _log.SelectionColor  = color;
        _log.AppendText(text + "\n");
        _log.ScrollToCaret();
    }
}
