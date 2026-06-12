using System.Windows.Forms;
using DS.IHF.Forms;
using DS.IHF.Services;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

var http     = new HttpClient();
var settings = SettingsService.Load();

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.SetHighDpiMode(HighDpiMode.SystemAware);

var thread = new Thread(() =>
{
    Application.Run(new MainForm(settings, http));
});
thread.SetApartmentState(ApartmentState.STA);
thread.Start();
thread.Join();
