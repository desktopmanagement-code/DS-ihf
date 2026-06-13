using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DS.IHF.Models;

namespace DS.IHF.Services;

public class DocuSignAuthService
{
    private const string RedirectUri = "http://localhost:8080/authorization-code/callback";

    private readonly AppSettings _settings;
    private readonly HttpClient  _http;

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    // Wird für den initialen User-Lookup mit dem Admin-GUID überschrieben
    private string? _overrideUserGuid;

    public DocuSignAuthService(AppSettings settings, HttpClient http)
    {
        _settings = settings;
        _http     = http;
    }

    public void UseAdminGuidForNextToken() =>
        _overrideUserGuid = DocuSignUserService.AdminGuid;

    public void ClearGuidOverride()
    {
        _overrideUserGuid = null;
        _cachedToken      = null;
        _tokenExpiry      = DateTime.MinValue;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            return _cachedToken;

        await EnsureConsentAsync();

        var jwt = BuildJwt(DocuSignUserService.PrivateKeyPem);

        var response = await _http.PostAsync(
            $"{_settings.DOCUSIGN_AUTH_SERVER}/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"]  = jwt
            }));

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Token-Anfrage fehlgeschlagen: HTTP {(int)response.StatusCode}\n" +
                $"DocuSign Antwort: {errorBody}\n\n" +
                $"Mögliche Ursachen:\n" +
                $"  1. Consent noch nicht erteilt → '{SettingsService.ConsentDonePath}' löschen und neu starten\n" +
                $"  2. INTEGRATION_KEY_JWT oder IMPERSONATION_USER_GUID falsch in settings.json\n" +
                $"  3. private.key Format ungültig (muss mit -----BEGIN RSA PRIVATE KEY----- beginnen)");
        }

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
        var userGuid = _overrideUserGuid ?? _settings.IMPERSONATION_USER_GUID;
        var payload = B64Url(JsonSerializer.Serialize(new
        {
            iss   = DocuSignUserService.IntegrationKeyJwt,
            sub   = userGuid,
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

        var state      = Convert.ToString(Random.Shared.NextInt64(0, 1_000_000_000), 16);
        var consentUrl =
            $"{_settings.DOCUSIGN_AUTH_SERVER}/oauth/auth" +
            $"?response_type=code" +
            $"&scope=signature%20impersonation" +
            $"&client_id={DocuSignUserService.IntegrationKeyJwt}" +
            $"&state={state}" +
            $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}";

        Console.WriteLine();
        Console.WriteLine("Einmaliger Consent erforderlich — Browser wird geöffnet...");

        var listener = new HttpListener();
        listener.Prefixes.Add(RedirectUri + "/");

        try
        {
            listener.Start();
        }
        catch (HttpListenerException)
        {
            throw new InvalidOperationException(
                "Port 8080 ist bereits belegt. Bitte das andere Programm beenden und neu starten.");
        }

        OpenBrowser(consentUrl);

        // Auf Callback warten
        var context = await listener.GetContextAsync();

        // Erfolgsseite an Browser senden
        var html = Encoding.UTF8.GetBytes("""
            <html><head><meta charset="utf-8"><title>DS-IHF</title>
            <style>body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;background:#f0f4f8;}
            .box{text-align:center;padding:40px;border-radius:8px;background:white;box-shadow:0 2px 12px rgba(0,0,0,.1);}
            h2{color:#0a7c42;} p{color:#555;}</style></head>
            <body><div class="box">
            <h2>✓ Anmeldung erfolgreich</h2>
            <p>Sie können diesen Tab jetzt schließen und zur Anwendung zurückkehren.</p>
            </div></body></html>
            """);
        context.Response.ContentLength64 = html.Length;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.OutputStream.WriteAsync(html);
        context.Response.OutputStream.Close();

        listener.Stop();

        await File.WriteAllTextAsync(SettingsService.ConsentDonePath, DateTime.UtcNow.ToString("o"));
        Console.WriteLine("Consent erfolgreich erteilt.");
    }

    private static string B64Url(string text) =>
        B64Url(Encoding.UTF8.GetBytes(text));

    private static string B64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static void OpenBrowser(string url)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { Console.WriteLine($"Browser konnte nicht geöffnet werden. Bitte manuell aufrufen:\n{url}"); }
    }
}
