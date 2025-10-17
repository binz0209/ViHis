
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ViHis.Application.Utils;
public static class SlugHelper
{
    public static string ToSlug(string input)
    {
        string normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        var s = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        s = s.Replace("đ", "d");
        s = Regex.Replace(s, @"\s", "-");
        s = Regex.Replace(s, "-+", "-");
        return s;
    }
}
