using System.Text.Json.Serialization;
using DS.IHF.Services;

namespace DS.IHF.Models;

public class AppSettings
{
    public string IMPERSONATION_USER_GUID { get; set; } = "";
    public string CC_EMAIL                { get; set; } = "";
    public string CC_NAME                 { get; set; } = "";

    [JsonIgnore]
    public bool NeedsUserGuidLookup =>
        string.IsNullOrWhiteSpace(IMPERSONATION_USER_GUID) || IMPERSONATION_USER_GUID.StartsWith("{");
}
