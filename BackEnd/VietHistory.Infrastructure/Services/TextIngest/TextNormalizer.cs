using System.Text;
using System.Text.RegularExpressions;

namespace VietHistory.Infrastructure.Services.TextIngest
{
    public static class TextNormalizer
    {
        // "Việt-\nNam" -> "Việt Nam"
        static readonly Regex HyphenLineBreak = new(@"(\p{L})-\n(\p{L})", RegexOptions.Multiline);
        // "Việt\nNam" -> "Việt Nam" (không áp dụng khi là xuống đoạn đôi)
        static readonly Regex SoftLineBreak = new(@"(?<!\.)\n(?!\n)", RegexOptions.Multiline);
        // Gom khoảng trắng thừa
        static readonly Regex MultiSpace = new(@"\s{2,}", RegexOptions.Multiline);

        // Một số PDF tách T e x t  T r a c k i n g -> gộp lại
        // Chiến lược: với chuỗi toàn chữ cái, nếu có >= 6 "token 1 ký tự" liên tiếp -> gộp lại.
        public static string CondenseSpacedLetters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var sb = new StringBuilder(input.Length);
            var parts = input.Split('\n');
            foreach (var line in parts)
            {
                var tokens = line.Split(' ');
                int i = 0;
                while (i < tokens.Length)
                {
                    // Nhóm chuỗi gồm toàn token 1 ký tự là chữ (có dấu)
                    int j = i;
                    while (j < tokens.Length && tokens[j].Length == 1 && char.IsLetter(tokens[j][0]))
                        j++;

                    int runLen = j - i;
                    if (runLen >= 6) // ngưỡng an toàn: 6 ký tự trở lên mới gộp
                    {
                        // Gộp thành một từ
                        var word = string.Concat(tokens[i..j]);
                        sb.Append(word);
                        if (j < tokens.Length) sb.Append(' ');
                        i = j;
                    }
                    else
                    {
                        sb.Append(tokens[i]);
                        if (i < tokens.Length - 1) sb.Append(' ');
                        i++;
                    }
                }
                sb.Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        public static string CleanRaw(string text)
        {
            var t = text.Replace("\r\n", "\n");
            t = HyphenLineBreak.Replace(t, "$1$2");   // bỏ gạch nối cuối dòng
            t = CondenseSpacedLetters(t);             // gộp chữ bị tách
            t = SoftLineBreak.Replace(t, " ");        // gộp dòng mềm
            t = MultiSpace.Replace(t, " ");           // gom khoảng trắng
            return t.Trim();
        }
    }
}
