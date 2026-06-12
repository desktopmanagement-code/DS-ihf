using System.Windows.Forms;
using DS.IHF.Helpers;
using DS.IHF.Models;
using DS.IHF.Services;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

var http     = new HttpClient();
var settings = SettingsService.Load();

// Pflichtfelder prüfen
var missing = settings.GetMissingFields().ToList();
if (missing.Any())
{
    Console.WriteLine("Die folgenden Felder fehlen in der settings.json:");
    Console.WriteLine($"  {SettingsService.SettingsFilePath}");
    Console.WriteLine();
    foreach (var field in missing)
        Console.WriteLine($"  - {field}");
    Console.WriteLine();
    Console.WriteLine("Bitte die Datei öffnen, die Werte eintragen und das Programm erneut starten.");
    Console.WriteLine("Drücken Sie eine Taste zum Beenden...");
    Console.ReadKey();
    return;
}

// Private Key prüfen
if (!File.Exists(SettingsService.PrivateKeyPath))
{
    Console.WriteLine($"Fehlende Datei: {SettingsService.PrivateKeyPath}");
    Console.WriteLine("Bitte den RSA Private Key dort ablegen.");
    Console.WriteLine("Drücken Sie eine Taste zum Beenden...");
    Console.ReadKey();
    return;
}

var auth        = new DocuSignAuthService(settings, http);
var userService = new DocuSignUserService(settings, auth, http);
var sender      = new DocuSignSendService(settings, auth, http);

// IMPERSONATION_USER_GUID per E-Mail-Abfrage holen falls noch nicht gesetzt
if (settings.NeedsUserGuidLookup)
{
    Console.WriteLine();
    Console.Write("Bitte Ihre DocuSign E-Mail-Adresse eingeben: ");
    var email = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(email))
    {
        Console.WriteLine("Keine E-Mail eingegeben. Programm beendet.");
        Console.ReadKey();
        return;
    }

    Console.WriteLine("Suche Benutzer-GUID bei DocuSign...");
    try
    {
        auth.UseAdminGuidForNextToken();
        var guid = await userService.LookupUserGuidByEmailAsync(email);
        auth.ClearGuidOverride();

        settings.IMPERSONATION_USER_GUID = guid;
        SettingsService.Save(settings);
        Console.WriteLine($"Benutzer-GUID gefunden und gespeichert: {guid}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Benutzersuche fehlgeschlagen: {ex.Message}");
        Console.ReadKey();
        return;
    }
}

// Token vorab holen (prüft auch Consent)
Console.WriteLine("Authentifizierung bei DocuSign...");
try
{
    await auth.GetAccessTokenAsync();
    Console.WriteLine("Token erfolgreich erhalten.");
}
catch (Exception ex)
{
    Console.WriteLine($"Authentifizierung fehlgeschlagen: {ex.Message}");
    Console.ReadKey();
    return;
}

// Ordner-Dialog auf STA-Thread ausführen (Windows Forms Voraussetzung)
string? selectedFolder = null;
var staThread = new Thread(() =>
{
    Application.EnableVisualStyles();
    using var dialog = new FolderBrowserDialog
    {
        Description         = "Ordner mit .docx Serienbriefen wählen",
        ShowNewFolderButton = false
    };
    if (dialog.ShowDialog() == DialogResult.OK)
        selectedFolder = dialog.SelectedPath;
});
staThread.SetApartmentState(ApartmentState.STA);
staThread.Start();
staThread.Join();

if (selectedFolder is null)
{
    Console.WriteLine("Kein Ordner gewählt. Programm beendet.");
    return;
}

var files = Directory.GetFiles(selectedFolder, "*.docx");
if (files.Length == 0)
{
    Console.WriteLine("Keine .docx-Dateien im gewählten Ordner gefunden.");
    Console.ReadKey();
    return;
}

Console.WriteLine($"{files.Length} Dokument(e) gefunden. Senden wird gestartet...");
Console.WriteLine();

int ok = 0, fehler = 0;

foreach (var file in files)
{
    Console.Write($"  {Path.GetFileName(file)} ... ");

    DocumentMetadata meta;
    try
    {
        meta = WordDocumentReader.ReadMetadata(file);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FEHLER beim Lesen: {ex.Message}");
        fehler++;
        continue;
    }

    var metaMissing = meta.GetMissingFields().ToList();
    if (metaMissing.Any())
    {
        Console.WriteLine($"ÜBERSPRUNGEN — fehlende Tags: {string.Join(", ", metaMissing)}");
        fehler++;
        continue;
    }

    try
    {
        await sender.SendEnvelopeAsync(meta);
        Console.WriteLine("OK");
        ok++;
    }
    catch
    {
        Console.WriteLine("FEHLER (siehe DS-IHF.log)");
        fehler++;
    }
}

Console.WriteLine();
Console.WriteLine($"Fertig. {ok} gesendet, {fehler} Fehler.");
Console.WriteLine("Drücken Sie eine Taste zum Beenden...");
Console.ReadKey();
