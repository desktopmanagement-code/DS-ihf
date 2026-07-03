using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DS.IHF.Models;

namespace DS.IHF.Services;

public class DocuSignSendService
{
    private readonly AppSettings        _settings;
    private readonly DocuSignAuthService _auth;
    private readonly HttpClient          _http;
    public DocuSignSendService(AppSettings settings, DocuSignAuthService auth, HttpClient http)
    {
        _settings = settings;
        _auth     = auth;
        _http     = http;
    }

    public async Task SendEnvelopeAsync(DocumentMetadata meta)
    {
        var token     = await _auth.GetAccessTokenAsync();
        var docBytes  = await File.ReadAllBytesAsync(meta.FilePath);
        var boundary  = "multipartboundary_ihf";

        var envelopeJson = BuildEnvelopeJson(meta);
        var body         = BuildMultipartBody(boundary, envelopeJson, docBytes, meta.Subject);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{DsEnvironment.Active.BaseUri}/v2.1/accounts/{DsEnvironment.Active.AccountId}/envelopes");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new ByteArrayContent(body);
        request.Content.Headers.ContentType =
            MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");

        try
        {
            var response = await _http.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var envelopeId = JsonDocument.Parse(responseText)
                    .RootElement.TryGetProperty("envelopeId", out var id) ? id.GetString() : "?";
                Log($"OK | {Path.GetFileName(meta.FilePath)} | {meta.SignerEmail} | EnvelopeId={envelopeId}");
            }
            else
            {
                Log($"FEHLER | {Path.GetFileName(meta.FilePath)} | HTTP {(int)response.StatusCode} | {responseText}");
                throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {responseText}");
            }
        }
        catch (Exception ex)
        {
            Log($"EXCEPTION | {Path.GetFileName(meta.FilePath)} | {ex.Message}");
            throw;
        }
    }

    private string BuildEnvelopeJson(DocumentMetadata meta)
    {
        var envelope = new
        {
            emailSubject = Truncate($"Bitte unterzeichnen Sie dieses Dokument ({meta.Subject})", 100),
            documents = new[]
            {
                new { name = meta.Subject, fileExtension = "docx", documentId = "1" }
            },
            recipients = new
            {
                signers = new[]
                {
                    new
                    {
                        email        = meta.SignerEmail,
                        name         = meta.SignerName,
                        recipientId  = "1",
                        routingOrder = "1",
                        tabs = new
                        {
                            signHereTabs = new[]
                            {
                                new { anchorString = "**signature_1**", anchorYOffset = "-10", anchorUnits = "pixels", anchorXOffset = "20" },
                                new { anchorString = "/sn1/",           anchorYOffset = "-10", anchorUnits = "pixels", anchorXOffset = "20" }
                            },
                            dateSignedTabs = new[]
                            {
                                new { anchorString = "/d1/", font = "Arial", fontsize = "Size11" }
                            },
                            textTabs = new[]
                            {
                                new { anchorString = "/L1/", anchorYOffset = "-3", anchorUnits = "pixels",
                                      value = "               ", tooltip = "hier bitte den Ort eintragen",
                                      font = "Arial", fontsize = "Size11", width = "40" }
                            }
                        }
                    }
                }
            },
            status = "sent"
        };

        return JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = false });
    }

    private static byte[] BuildMultipartBody(string boundary, string json, byte[] docBytes, string docName)
    {
        using var ms = new MemoryStream();

        void WriteText(string text) { var b = Encoding.UTF8.GetBytes(text); ms.Write(b); }
        void WriteBytes(byte[] b)   => ms.Write(b);
        void CRLF()                 => WriteText("\r\n");

        // JSON part
        WriteText($"--{boundary}"); CRLF();
        WriteText("Content-Type: application/json"); CRLF();
        WriteText("Content-Disposition: form-data"); CRLF();
        CRLF();
        WriteText(json); CRLF();

        // Document part
        WriteText($"--{boundary}"); CRLF();
        WriteText("Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document"); CRLF();
        WriteText($"Content-Disposition: file; filename=\"{docName}\";documentid=1"); CRLF();
        CRLF();
        WriteBytes(docBytes); CRLF();

        // Closing boundary
        WriteText($"--{boundary}--"); CRLF();

        return ms.ToArray();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static void Log(string message) => LogService.Write(message);
}
