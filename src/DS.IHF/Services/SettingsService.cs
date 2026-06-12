using System.Text.Json;
using DS.IHF.Models;

namespace DS.IHF.Services;

public static class SettingsService
{
    public static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DS-IHF", "config");

    public static string SettingsFilePath => Path.Combine(ConfigDirectory, "settings.json");
    public static string PrivateKeyPath   => Path.Combine(ConfigDirectory, "private.key");
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
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
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
            INTEGRATION_KEY_JWT     = DocuSignUserService.IntegrationKeyJwt,
            IMPERSONATION_USER_GUID = "",
            ACCOUNT_ID              = DocuSignUserService.AccountId,
            CC_EMAIL                = "{CC_EMAIL}",
            CC_NAME                 = "{CC_NAME}",
        };
        Save(template);
    }
}
