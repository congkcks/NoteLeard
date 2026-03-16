using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
namespace NoteLearn.Services.Pdf;
public class PdfTextExtractor
{
    public (string rawText, List<(int page, string text)> pages, int totalChars) Extract(string pdfPath)
    {
        var pages = new List<(int page, string text)>();
        var sb = new StringBuilder();
        int totalChars = 0;

        using var document = PdfDocument.Open(pdfPath);

        foreach (Page page in document.GetPages())
        {
            var text = (page.Text ?? "").Trim();
            pages.Add((page.Number, text));

            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
                sb.AppendLine();
                totalChars += text.Length;
            }
        }

        return (sb.ToString(), pages, totalChars);
    }
}
