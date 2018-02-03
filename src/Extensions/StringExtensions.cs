using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtensions
    {
        public static string PlainTextTruncate(this string input, int length)
        {
            string text = HtmlToPlainText(input);
            if (text.Length < length) {
                return text;
            }

            char[] terminators = { '.', ',', ';', ':', '?', '!' };
            int end = text.LastIndexOfAny(terminators, length);
            if (end == -1)
            {
                end = text.LastIndexOf(" ", length);
                return text.Substring(0, end) + "...";
            }
            return text.Substring(0, end + 1);
        }

        //From https://stackoverflow.com/a/16407272/5
        //TODO: Use a proper sanitizer, perhaps https://github.com/atifaziz/High5
        public static string HtmlToPlainText(this string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
    }
}
