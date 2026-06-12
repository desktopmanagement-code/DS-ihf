using System.Net.Http.Headers;
using System.Text.Json;
using DS.IHF.Models;

namespace DS.IHF.Services;

public class DocuSignUserService
{
    public const string AdminUserGuid      = "92b39023-b49b-46d6-97d9-be3e41c314f0";
    public const string IntegrationKeyJwt  = "geheim";
    public const string AccountId          = "66637";

    private readonly AppSettings     _settings;
    private readonly DocuSignAuthService _auth;
    private readonly HttpClient      _http;

    public DocuSignUserService(AppSettings settings, DocuSignAuthService auth, HttpClient http)
    {
        _settings = settings;
        _auth     = auth;
        _http     = http;
    }

    /// <summary>
    /// Sucht die DocuSign User-GUID anhand der E-Mail-Adresse.
    /// Verwendet den Admin-Account für die Abfrage.
    /// </summary>
    public async Task<string> LookupUserGuidByEmailAsync(string email)
    {
        var token = await _auth.GetAccessTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_settings.DOCUSIGN_BASE_URI}/v2.1/accounts/{_settings.ACCOUNT_ID}/users?email={Uri.EscapeDataString(email)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        var body     = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Benutzersuche fehlgeschlagen: HTTP {(int)response.StatusCode}\n{body}");

        var users = JsonDocument.Parse(body).RootElement
            .GetProperty("users");

        if (users.GetArrayLength() == 0)
            throw new InvalidOperationException(
                $"Kein DocuSign-Benutzer mit der E-Mail '{email}' gefunden.");

        var guid = users[0].GetProperty("userId").GetString()
            ?? throw new InvalidOperationException("userId fehlt in der API-Antwort.");

        return guid;
    }

    public static string AdminGuid => AdminUserGuid;
}
