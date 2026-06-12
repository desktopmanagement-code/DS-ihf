namespace DS.IHF.Models;

public class AppSettings
{
    public string INTEGRATION_KEY_JWT { get; set; } = "";
    public string IMPERSONATION_USER_GUID { get; set; } = "";
    public string ACCOUNT_ID { get; set; } = "";
    public string CC_EMAIL { get; set; } = "";
    public string CC_NAME { get; set; } = "";
    public string DOCUSIGN_BASE_URI { get; set; } = "https://eu.docusign.net/restapi";
    public string DOCUSIGN_AUTH_SERVER { get; set; } = "https://account.docusign.com";

    public IEnumerable<string> GetMissingFields()
    {
        if (string.IsNullOrWhiteSpace(INTEGRATION_KEY_JWT) || INTEGRATION_KEY_JWT.StartsWith("{"))
            yield return nameof(INTEGRATION_KEY_JWT);
        if (string.IsNullOrWhiteSpace(ACCOUNT_ID) || ACCOUNT_ID.StartsWith("{"))
            yield return nameof(ACCOUNT_ID);
    }

    public bool NeedsUserGuidLookup =>
        string.IsNullOrWhiteSpace(IMPERSONATION_USER_GUID) || IMPERSONATION_USER_GUID.StartsWith("{");
}
