using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Embers.ISE.Services;

public static class EmbersFormatter
{
    private const int IndentSize = 2;

    private static readonly Regex BlockStartRegex =
        new(@"^\s*(def|class|module|if|unless|while|until|for|begin|try|case)\b", RegexOptions.Compiled);

    private static readonly Regex BlockDoRegex =
        new(@"\bdo(\s*\|.*\|)?\s*$", RegexOptions.Compiled);

    private static readonly Regex BlockEndRegex =
        new(@"^\s*end\b", RegexOptions.Compiled);

    private static readonly Regex MidBlockRegex =
        new(@"^\s*(else|elsif|when|rescue|ensure)\b", RegexOptions.Compiled);

    public static string FormatText(string text, int baseIndentSpaces = 0)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = normalized.Split('\n');

        var output = new StringBuilder();
        var indentLevel = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();

            if (trimmed.Length == 0)
            {
                AppendLine(output, string.Empty, i, lines.Length);
                continue;
            }

            var stripped = StripLineComment(trimmed);
            var strippedTrimmed = stripped.Trim();

            var isCommentOnly = trimmed.StartsWith('#');
            var isEnd = BlockEndRegex.IsMatch(strippedTrimmed);
            var isMidBlock = MidBlockRegex.IsMatch(strippedTrimmed);

            if (!isCommentOnly && (isEnd || isMidBlock))
                indentLevel = Math.Max(0, indentLevel - 1);

            var indentSpaces = baseIndentSpaces + (indentLevel * IndentSize);
            var indented = new string(' ', indentSpaces) + trimmed;
            AppendLine(output, indented, i, lines.Length);

            if (isCommentOnly || strippedTrimmed.Length == 0)
                continue;

            var startsBlock = BlockStartRegex.IsMatch(strippedTrimmed) || BlockDoRegex.IsMatch(strippedTrimmed);
            if (startsBlock || isMidBlock)
                indentLevel++;
        }

        return output.ToString();
    }

    private static void AppendLine(StringBuilder output, string line, int index, int total)
    {
        output.Append(line);
        if (index < total - 1)
            output.Append(Environment.NewLine);
    }

    private static string StripLineComment(string line)
    {
        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (inSingle)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '\'')
                    inSingle = false;

                continue;
            }

            if (inDouble)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                    inDouble = false;

                continue;
            }

            if (ch == '#')
                return line[..i];

            if (ch == '\'')
            {
                inSingle = true;
                continue;
            }

            if (ch == '"')
                inDouble = true;
        }

        return line;
    }
}
