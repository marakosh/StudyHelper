using UglyToad.PdfPig;
using System.Text;

namespace StudyHelperMVC.Services;

public class PdfTextExtractor
{
    public string ExtractText(Stream pdfStream)
    {
        using var doc = PdfDocument.Open(pdfStream);
        var sb = new StringBuilder();
        foreach (var page in doc.GetPages())
        {
            if (!string.IsNullOrWhiteSpace(page.Text))
                sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }
}
