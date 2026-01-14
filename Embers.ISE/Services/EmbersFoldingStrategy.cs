using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Embers.ISE.Services;

public sealed class EmbersFoldingStrategy
{
    private static readonly Regex BlockStartRegex =
        new(@"^\s*(def|class|module|if|unless|while|until|for|begin|try|case)\b", RegexOptions.Compiled);

    private static readonly Regex BlockDoRegex =
        new(@"\bdo\s*$", RegexOptions.Compiled);

    private static readonly Regex BlockEndRegex =
        new(@"^\s*end\b", RegexOptions.Compiled);

    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
        var newFoldings = new List<NewFolding>();
        AddKeywordFoldings(document, newFoldings);
        AddBraceFoldings(document, newFoldings);

        var ordered = newFoldings
            .Where(f => f.StartOffset < f.EndOffset)
            .OrderBy(f => f.StartOffset)
            .ThenBy(f => f.EndOffset)
            .ToList();

        manager.UpdateFoldings(ordered, -1);
    }

    private static void AddKeywordFoldings(TextDocument document, List<NewFolding> foldings)
    {
        var startLines = new Stack<int>();

        foreach (var line in document.Lines)
        {
            var text = document.GetText(line);
            var stripped = StripLineComment(text);
            if (string.IsNullOrWhiteSpace(stripped))
                continue;

            if (BlockStartRegex.IsMatch(stripped) || BlockDoRegex.IsMatch(stripped))
            {
                startLines.Push(line.LineNumber);
                continue;
            }

            if (!BlockEndRegex.IsMatch(stripped))
                continue;

            if (startLines.Count == 0)
                continue;

            var startLineNumber = startLines.Pop();
            if (startLineNumber >= line.LineNumber)
                continue;

            var startLine = document.GetLineByNumber(startLineNumber);
            var endLine = line;
            var startOffset = startLine.EndOffset;
            var endOffset = endLine.EndOffset;

            if (startOffset < endOffset)
                foldings.Add(new NewFolding(startOffset, endOffset));
        }
    }

    private static void AddBraceFoldings(TextDocument document, List<NewFolding> foldings)
    {
        var stack = new Stack<int>();
        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;
        bool inComment = false;

        for (int i = 0; i < document.TextLength; i++)
        {
            char ch = document.GetCharAt(i);

            if (inComment)
            {
                if (ch == '\n')
                    inComment = false;
                continue;
            }

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
            {
                inComment = true;
                continue;
            }

            if (ch == '\'')
            {
                inSingle = true;
                continue;
            }

            if (ch == '"')
            {
                inDouble = true;
                continue;
            }

            if (ch == '{')
            {
                stack.Push(i);
                continue;
            }

            if (ch != '}' || stack.Count == 0)
                continue;

            var openOffset = stack.Pop();
            var startLine = document.GetLineByOffset(openOffset);
            var endLine = document.GetLineByOffset(i);
            if (startLine.LineNumber == endLine.LineNumber)
                continue;

            var startOffset = startLine.EndOffset;
            var endOffset = endLine.EndOffset;

            if (startOffset < endOffset)
                foldings.Add(new NewFolding(startOffset, endOffset));
        }
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
            {
                inDouble = true;
            }
        }

        return line;
    }
}
