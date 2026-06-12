using DS.IHF.Services;

namespace DS.IHF.Models;

public class AppSettings
{
    public string IMPERSONATION_USER_GUID { get; set; } = "";
    public string CC_EMAIL                { get; set; } = "";
    public string CC_NAME                 { get; set; } = "";
    public string DOCUSIGN_BASE_URI       { get; set; } = "https://eu.docusign.net/restapi";
    public string DOCUSIGN_AUTH_SERVER    { get; set; } = "https://account.docusign.com";

    public bool NeedsUserGuidLookup =>
        string.IsNullOrWhiteSpace(IMPERSONATION_USER_GUID) || IMPERSONATION_USER_GUID.StartsWith("{");
}
