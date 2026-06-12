namespace DS.IHF.Models;

public class DocumentMetadata
{
    public string FilePath { get; set; } = "";
    public string SignerEmail { get; set; } = "";
    public string SignerName { get; set; } = "";
    public string Subject { get; set; } = "";

    public IEnumerable<string> GetMissingFields()
    {
        if (string.IsNullOrWhiteSpace(SignerEmail)) yield return "DSEMAIL";
        if (string.IsNullOrWhiteSpace(SignerName))  yield return "DSASP";
        if (string.IsNullOrWhiteSpace(Subject))     yield return "DSVA";
    }
}
