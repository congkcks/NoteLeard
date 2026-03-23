using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace NoteLearn.Services.Pdf;

public class PdfTextExtractor
{
    public (string rawText, List<(int page, string text)> pages, int totalChars) Extract(string pdfPath)
    {
        var pages = new List<(int page, string text)>();
        var sbGlobal = new StringBuilder();
        int totalChars = 0;

        using var document = PdfDocument.Open(pdfPath);

        foreach (Page page in document.GetPages())
        {
            // 💡 Cải tiến: Thay vì dùng page.Text, chúng ta lấy danh sách các từ.
            // Điều này giúp PdfPig nhận diện các ký tự tiếng Việt có dấu tốt hơn 
            // trong các PDF được tạo từ Office/Canvas.
            var words = page.GetWords();
            var pageContent = string.Join(" ", words.Select(w => w.Text));

            // Làm sạch văn bản trích xuất được
            var cleanText = CleanExtractedText(pageContent);

            pages.Add((page.Number, cleanText));

            if (!string.IsNullOrWhiteSpace(cleanText))
            {
                sbGlobal.AppendLine(cleanText);
                sbGlobal.AppendLine();
                totalChars += cleanText.Length;
            }
        }

        return (sbGlobal.ToString(), pages, totalChars);
    }

    private string CleanExtractedText(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Một số PDF bị lỗi dính chữ hoặc lỗi ký tự điều khiển (control characters)
        // Chúng ta loại bỏ các ký tự rác không in được (non-printable)
        var result = new StringBuilder();
        foreach (var c in input)
        {
            if (c >= 32 || c == '\n' || c == '\r' || c == '\t')
            {
                result.Append(c);
            }
        }

        return result.ToString().Trim();
    }
}