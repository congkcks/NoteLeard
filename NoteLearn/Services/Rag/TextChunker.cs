using System.Text.RegularExpressions;

namespace NoteLearn.Services.Rag;

public static class TextChunker
{
    public static List<string> Chunk(string text, int maxChunkLength = 1200, int overlap = 150)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        text = Normalize(text);

        var paragraphs = Regex
            .Split(text, @"\n\s*\n")
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        var chunks = new List<string>();
        var current = "";

        foreach (var p in paragraphs)
        {
            // đoạn dài thì tách theo câu
            if (p.Length > maxChunkLength)
            {
                foreach (var sentence in SplitSentences(p))
                {
                    // +1 để chừa khoảng trắng
                    if (current.Length + sentence.Length + 1 > maxChunkLength)
                        Flush(chunks, ref current, overlap);

                    current = AppendWithSpace(current, sentence);
                }

                // sau khi xử lý đoạn dài, thêm xuống dòng để không dính đoạn kế
                current = AppendWithNewParagraph(current);
            }
            else
            {
                // +2 vì "\n\n"
                if (current.Length + p.Length + 2 > maxChunkLength)
                    Flush(chunks, ref current, overlap);

                current = AppendWithParagraph(current, p);
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
            chunks.Add(current.Trim());

        return chunks;
    }

    private static void Flush(List<string> chunks, ref string current, int overlap)
    {
        var chunk = current.Trim();
        if (!string.IsNullOrWhiteSpace(chunk))
            chunks.Add(chunk);

        current = MakeOverlap(chunk, overlap);
    }

    private static string MakeOverlap(string chunk, int overlap)
    {
        if (string.IsNullOrWhiteSpace(chunk) || overlap <= 0)
            return "";

        if (chunk.Length <= overlap)
            return chunk;

        var tail = chunk.Substring(chunk.Length - overlap);

        // Ưu tiên cắt overlap theo ranh giới câu (.,!,?)
        int boundary = LastSentenceBoundary(tail);
        if (boundary >= 0 && boundary + 1 < tail.Length)
        {
            var after = tail.Substring(boundary + 1).TrimStart();
            return after;
        }

        // fallback: cắt theo khoảng trắng để không cắt giữa từ
        int firstSpace = tail.IndexOf(' ');
        if (firstSpace > 0 && firstSpace + 1 < tail.Length)
            tail = tail.Substring(firstSpace + 1);

        return tail.TrimStart();
    }

    private static int LastSentenceBoundary(string s)
    {
        // tìm dấu kết câu gần cuối nhất
        int dot = s.LastIndexOf('.');
        int ex = s.LastIndexOf('!');
        int q = s.LastIndexOf('?');
        return Math.Max(dot, Math.Max(ex, q));
    }

    private static string AppendWithParagraph(string current, string paragraph)
    {
        if (string.IsNullOrWhiteSpace(current))
            return paragraph;

        return current.TrimEnd() + "\n\n" + paragraph;
    }

    private static string AppendWithNewParagraph(string current)
    {
        if (string.IsNullOrWhiteSpace(current))
            return "";

        return current.TrimEnd() + "\n\n";
    }

    private static string AppendWithSpace(string current, string sentence)
    {
        if (string.IsNullOrWhiteSpace(current))
            return sentence.Trim();

        return current.TrimEnd() + " " + sentence.Trim();
    }

    private static string Normalize(string text)
    {
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");
        // giữ newline nhưng dọn space
        text = Regex.Replace(text, @"[ \t]+", " ");
        // nén nhiều newline về 2 newline (để nhận diện đoạn)
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        // đảm bảo sau dấu chấm có khoảng trắng nếu bị dính "AI.Hệ"
        text = Regex.Replace(text, @"([\.!\?])([^\s\n])", "$1 $2");
        return text.Trim();
    }

    private static List<string> SplitSentences(string text)
    {
        // tách câu: dấu kết câu + khoảng trắng
        return Regex
            .Split(text.Trim(), @"(?<=[\.!\?])\s+")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }
}