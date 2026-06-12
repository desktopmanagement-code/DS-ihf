using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DS.IHF.Models;

namespace DS.IHF.Helpers;

public static class WordDocumentReader
{
    private static readonly Regex EmailPattern   = new(@"(?<=DSEMAIL:\s*)\S+.*",   RegexOptions.Compiled);
    private static readonly Regex NamePattern    = new(@"(?<=DSASP:\s*)\S+.*",     RegexOptions.Compiled);
    private static readonly Regex SubjectPattern = new(@"(?<=DSVA:\s*)\S+.*",      RegexOptions.Compiled);

    public static DocumentMetadata ReadMetadata(string filePath)
    {
        var meta = new DocumentMetadata { FilePath = filePath };

        using var doc = WordprocessingDocument.Open(filePath, isEditable: false);
        var body = doc.MainDocumentPart?.Document?.Body
            ?? throw new InvalidOperationException($"Dokument hat keinen Body: {filePath}");

        var text = body.InnerText;

        meta.SignerEmail = Match(EmailPattern,   text).Trim();
        meta.SignerName  = Match(NamePattern,    text).Trim();
        meta.Subject     = Match(SubjectPattern, text).Trim();

        return meta;
    }

    private static string Match(Regex regex, string text)
    {
        var m = regex.Match(text);
        return m.Success ? m.Value : "";
    }
}
