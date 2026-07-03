using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DS.IHF.Models;

namespace DS.IHF.Helpers;

public static class WordDocumentReader
{
    private static readonly Regex EmailPattern   = new(@"(?<=DSEMAIL:\s*)\S.*",    RegexOptions.Compiled);
    private static readonly Regex NamePattern    = new(@"(?<=DSASP:\s*)\S.*",   RegexOptions.Compiled);
    private static readonly Regex SubjectPattern = new(@"(?<=DSVA:\s*)\S.*",    RegexOptions.Compiled);

    public static DocumentMetadata ReadMetadata(string filePath)
    {
        var meta = new DocumentMetadata { FilePath = filePath };

        using var doc = WordprocessingDocument.Open(filePath, isEditable: false);
        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException($"Dokument hat keinen Body: {filePath}");

        // Paragraph für Paragraph auslesen — wie PS1 zeilenweise
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            var line = paragraph.InnerText.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (string.IsNullOrEmpty(meta.SignerEmail))
                meta.SignerEmail = MatchLine(EmailPattern, line, removeSpaces: true);
            if (string.IsNullOrEmpty(meta.SignerName))
                meta.SignerName = MatchLine(NamePattern, line);
            if (string.IsNullOrEmpty(meta.Subject))
                meta.Subject = MatchLine(SubjectPattern, line);

            if (!string.IsNullOrEmpty(meta.SignerEmail) &&
                !string.IsNullOrEmpty(meta.SignerName)  &&
                !string.IsNullOrEmpty(meta.Subject))
                break;
        }

        return meta;
    }

    private static string MatchLine(Regex regex, string line, bool removeSpaces = false)
    {
        var m = regex.Match(line);
        if (!m.Success) return "";
        var value = m.Value.Trim();
        return removeSpaces ? value.Replace(" ", "") : value;
    }
}
