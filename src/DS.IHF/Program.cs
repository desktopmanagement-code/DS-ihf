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

var auth   = new DocuSignAuthService(settings, http);
var sender = new DocuSignSendService(settings, auth, http);

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

// Ordner wählen
Application.EnableVisualStyles();
using var dialog = new FolderBrowserDialog
{
    Description         = "Ordner mit .docx Serienbriefen wählen",
    ShowNewFolderButton = false
};

if (dialog.ShowDialog() != DialogResult.OK)
{
    Console.WriteLine("Kein Ordner gewählt. Programm beendet.");
    return;
}

var files = Directory.GetFiles(dialog.SelectedPath, "*.docx");
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
