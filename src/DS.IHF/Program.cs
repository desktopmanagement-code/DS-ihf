using System.Windows.Forms;
using DS.IHF.Forms;
using DS.IHF.Services;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

// /? Hilfe anzeigen
if (args.Any(a => a is "/?" or "-?" or "--help" or "/help"))
{
    var logDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DS-Versand", "logs");

    MessageBox.Show(
        "DS-Versand — DocuSign Serienbrief-Upload\n\n" +
        "Aufrufoptionen:\n\n" +
        "  DS.IHF.exe\n" +
        "    Normaler Start (Produktionsumgebung)\n\n" +
        "  DS.IHF.exe /Demo\n" +
        "    Start mit DocuSign-Demoumgebung (demo.docusign.net)\n" +
        "    Nützlich zum Testen ohne echte Signaturen zu versenden.\n\n" +
        "  DS.IHF.exe /?\n" +
        "    Diese Hilfe anzeigen\n\n" +
        "Protokolldateien:\n" +
        $"  {logDir}",
        "DS-Versand Hilfe",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    return;
}

// /Demo Schalter prüfen
if (args.Any(a => a.Equals("/Demo", StringComparison.OrdinalIgnoreCase)))
    DsEnvironment.SetDemo();

var http     = new HttpClient();
var settings = SettingsService.Load();

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

var thread = new Thread(() =>
{
    Application.Run(new MainForm(settings, http));
});
thread.SetApartmentState(ApartmentState.STA);
thread.Start();
thread.Join();
