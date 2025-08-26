using UglyToad.PdfPig;
using System.Text;

namespace StudyHelperMVC.Services;

public class PdfTextExtractor
{
    public string ExtractText(Stream pdfStream)
    {
        using var ms = new MemoryStream();
        pdfStream.CopyTo(ms);
        ms.Position = 0;

        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(ms.ToArray());
        foreach (var page in doc.GetPages())
            sb.AppendLine(page.Text);

        return sb.ToString();
    }
}
