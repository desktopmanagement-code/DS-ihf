using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DS.IHF.Models;

namespace DS.IHF.Services;

public class DocuSignAuthService
{
    private readonly AppSettings _settings;
    private readonly HttpClient  _http;

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public DocuSignAuthService(AppSettings settings, HttpClient http)
    {
        _settings = settings;
        _http     = http;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            return _cachedToken;

        await EnsureConsentAsync();

        var privateKeyPem = await File.ReadAllTextAsync(SettingsService.PrivateKeyPath);
        var jwt = BuildJwt(privateKeyPem);

        var response = await _http.PostAsync(
            $"{_settings.DOCUSIGN_AUTH_SERVER}/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"]  = jwt
            }));

        response.EnsureSuccessStatusCode();

        var json  = await response.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(json).RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("Kein access_token in der Antwort.");

        _cachedToken = token;
        _tokenExpiry = DateTime.UtcNow.AddHours(1);
        return token;
    }

    private string BuildJwt(string privateKeyPem)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var header = B64Url(JsonSerializer.Serialize(new { typ = "JWT", alg = "RS256" }));
        var payload = B64Url(JsonSerializer.Serialize(new
        {
            iss   = _settings.INTEGRATION_KEY_JWT,
            sub   = _settings.IMPERSONATION_USER_GUID,
            iat   = now,
            exp   = now + 3600,
            aud   = "account.docusign.com",
            scope = "signature impersonation"
        }));

        var unsigned = $"{header}.{payload}";

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var signatureBytes = rsa.SignData(
            Encoding.ASCII.GetBytes(unsigned),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return $"{unsigned}.{B64Url(signatureBytes)}";
    }

    private async Task EnsureConsentAsync()
    {
        if (File.Exists(SettingsService.ConsentDonePath))
            return;

        var consentUrl =
            $"{_settings.DOCUSIGN_AUTH_SERVER}/oauth/auth" +
            $"?response_type=code" +
            $"&scope=signature%20impersonation" +
            $"&client_id={_settings.INTEGRATION_KEY_JWT}" +
            $"&redirect_uri=https://www.docusign.com";

        Console.WriteLine();
        Console.WriteLine("Einmaliger Consent erforderlich.");
        Console.WriteLine("Der Browser wird geöffnet. Bitte anmelden und Zugriff erlauben.");
        Console.WriteLine(consentUrl);

        OpenBrowser(consentUrl);

        Console.WriteLine("Drücken Sie eine Taste, sobald der Consent erteilt wurde...");
        Console.ReadKey(intercept: true);

        await File.WriteAllTextAsync(SettingsService.ConsentDonePath, DateTime.UtcNow.ToString("o"));
    }

    private static string B64Url(string text) =>
        B64Url(Encoding.UTF8.GetBytes(text));

    private static string B64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static void OpenBrowser(string url)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* Browser nicht verfügbar — URL wurde bereits in Konsole ausgegeben */ }
    }
}
