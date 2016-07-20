namespace ReamQuery.Helpers
{
    using System;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    public static class StringHelpers
    {
        public static string InsertTextAt(this string text, string newText, int line, int column)
        {
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            lines[line] = lines[line].Substring(0, column) + newText + lines[line].Substring(column);
            return string.Join(Environment.NewLine, lines);
        }

        static Regex nlRegex = new Regex(@"(\n|\r\n)", RegexOptions.Multiline);

        public static string NormalizeNewlines(this string text)
        {
            return nlRegex.Replace(text, Environment.NewLine);
        }

        public static string ReplaceToken(this string text, string token, string replacement, out LinePosition position)
        {
            var colOffset = -1;
            var lineOffset = -1;
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            for(var i = lines.Length - 1; i > 0; i--) {
                // Console.WriteLine("{0}:{1}", i, lines[i]);
                if (lines[i].Contains(token)) {
                    lineOffset = i;
                    colOffset = lines[i].IndexOf(token);
                    lines[i] = lines[i].Replace(token, replacement);
                    break;
                }
            }
            position = new LinePosition(lineOffset, colOffset);
            return string.Join(Environment.NewLine, lines);
        }
    }
}
