using System.Text.Json;
using DS.IHF.Models;

namespace DS.IHF.Services;

public static class SettingsService
{
    public static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DS-Versand", "config");

    public static string SettingsFilePath => Path.Combine(ConfigDirectory, "settings.json");
    public static string ConsentDonePath  => Path.Combine(ConfigDirectory, "consent.done");

    public static AppSettings Load()
    {
        EnsureConfigDirectory();

        if (!File.Exists(SettingsFilePath))
            WriteTemplate();

        var json = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<AppSettings>(json)
            ?? throw new InvalidOperationException("settings.json konnte nicht gelesen werden.");
    }

    public static void Save(AppSettings settings)
    {
        EnsureConfigDirectory();
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true,
            // Unbekannte Felder in der JSON werden beim Laden ignoriert
        });
        File.WriteAllText(SettingsFilePath, json);
    }

    private static void EnsureConfigDirectory()
    {
        if (!Directory.Exists(ConfigDirectory))
            Directory.CreateDirectory(ConfigDirectory);
    }

    private static void WriteTemplate()
    {
        var template = new AppSettings
        {
            IMPERSONATION_USER_GUID = "",
            CC_EMAIL                = "{CC_EMAIL}",
            CC_NAME                 = "{CC_NAME}",
        };
        Save(template);
    }
}
