namespace DS.IHF.Services;

public class DsEnvironment
{
    public string Name            { get; }
    public string AdminUserGuid   { get; }
    public string IntegrationKey  { get; }
    public string AccountId       { get; }
    public string PrivateKeyPem   { get; }
    public string BaseUri         { get; }
    public string AuthServer      { get; }

    private DsEnvironment(string name, string adminGuid, string integrationKey,
        string accountId, string privateKeyPem, string baseUri, string authServer)
    {
        Name           = name;
        AdminUserGuid  = adminGuid;
        IntegrationKey = integrationKey;
        AccountId      = accountId;
        PrivateKeyPem  = privateKeyPem;
        BaseUri        = baseUri;
        AuthServer     = authServer;
    }

    // ─── Produktionsumgebung ──────────────────────────────────────────────────
    public static readonly DsEnvironment Production = new(
        name:           "Produktion",
        adminGuid:      "92b39023-b49b-46d6-97d9-be3e41c314f0",
        integrationKey: "{INTEGRATION_KEY_JWT_PROD}",
        accountId:      "{ACCOUNT_ID_PROD}",
        privateKeyPem:  """
            -----BEGIN RSA PRIVATE KEY-----
            {PRIVATE_KEY_PROD}
            -----END RSA PRIVATE KEY-----
            """,
        baseUri:        "https://eu.docusign.net/restapi",
        authServer:     "https://account.docusign.com"
    );

    // ─── Demoumgebung ─────────────────────────────────────────────────────────
    public static readonly DsEnvironment Demo = new(
        name:           "Demo",
        adminGuid:      "{ADMIN_GUID_DEMO}",
        integrationKey: "{INTEGRATION_KEY_JWT_DEMO}",
        accountId:      "{ACCOUNT_ID_DEMO}",
        privateKeyPem:  """
            -----BEGIN RSA PRIVATE KEY-----
            {PRIVATE_KEY_DEMO}
            -----END RSA PRIVATE KEY-----
            """,
        baseUri:        "https://demo.docusign.net/restapi",
        authServer:     "https://account-d.docusign.com"
    );

    // Aktive Umgebung — wird beim Programmstart gesetzt
    public static DsEnvironment Active { get; private set; } = Production;

    public static void SetDemo() => Active = Demo;
}
