using Embers.Exceptions;

namespace Embers.Compiler;

/// <summary>
/// tokenizes Embers source code
/// </summary>
public class Lexer
{
    private const char Quote = '\'';
    private const char DoubleQuote = '"';
    private const char Colon = ':';
    private const char StartComment = '#';
    private const char EndOfLine = '\n';
    private const char Variable = '@';
    private const char GlobalVariable = '$';
    private const char Backtick = '`';
    private const string Separators = ";()[],.|{}&";
    private static readonly string[] operators = ["?", ":", "+", "-", "*", "/", "%", "**", "=", "<", ">", "!", "==", "===", "<=", ">=", "!=", "<=>", "=>", "->", "..", "&&", "||", "+=", "-=", "*=", "/=", "%=", "**="];

    /// <summary>
    /// the character stream
    /// </summary>
    private readonly ICharStream stream;

    /// <summary>
    /// the pushed-back tokens stack
    /// </summary>
    private readonly Stack<(Token Token, TokenSpan? Span)> tokens = new();
    internal TokenSpan? LastTokenSpan { get; private set; }
    private int offset;
    private int lastCharOffset;
    private int tokenStartOffset;

    /// <summary>
    /// the lexer for the Embers language
    /// </summary>
    /// <param name="text"></param>
    public Lexer(string text)
    {
        stream = new TextCharStream(text);
    }

    /// <summary>
    /// the lexer for the Embers language
    /// </summary>
    /// <param name="reader"></param>
    public Lexer(TextReader reader)
    {
        stream = new TextReaderCharStream(reader);
    }

    /// <summary>
    /// Gets the next token from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public Token? NextToken()
    {
        if (tokens.Count > 0)
        {
            var (token, span) = tokens.Pop();
            LastTokenSpan = span;
            return token;
        }

        int ich = NextFirstChar();

        if (ich == -1)
        {
            LastTokenSpan = null;
            return null;
        }

        char ch = (char)ich;
        tokenStartOffset = lastCharOffset;

        if (ch == EndOfLine)
            return FinishToken(new Token(TokenType.EndOfLine, "\n"));

        if (ch == Quote)
            return FinishToken(NextString(Quote));

        if (ch == DoubleQuote)
            return FinishToken(NextDoubleQuotedString());

        //if (ch == Colon)
        //    return NextSymbol();

        if (ch == Colon)
        {
            int peek = PeekChar();

            // If followed by a valid symbol name character, it's a symbol
            if (char.IsLetterOrDigit((char)peek) || peek == '_' || peek == '@')
                return FinishToken(NextSymbol());

            // If followed by another colon, it's a namespace separator
            if (peek == ':')
            {
                NextChar();  // consume second ':'
                return FinishToken(new Token(TokenType.Separator, "::"));
            }

            // Else, it's the colon in a ternary operator
            return FinishToken(new Token(TokenType.Operator, ":"));
        }

        if (ch == Variable)
            return FinishToken(NextInstanceVariableName());

        if (ch == GlobalVariable)
            return FinishToken(NextGlobalVariableName());

        if (ch == '<' && PeekChar() == '<')
        {
            NextChar(); // consume second '<'
            return FinishToken(NextHeredoc());
        }

        string value1 = ch.ToString();
        if (operators.Any(op => op.StartsWith(value1)))
        {
            if (operators.Any(op => op.Length > 1 && op.StartsWith(value1)))
            {
                int ich1 = NextChar();
                if (ich1 >= 0)
                {
                    char ch1 = (char)ich1;
                    string value2 = value1 + ch1;

                    if (operators.Any(op => op.Length > 2 && op.StartsWith(value2)))
                    {
                        int ich2 = NextChar();
                        if (ich2 >= 0)
                        {
                            char ch2 = (char)ich2;
                            string value3 = value2 + ch2;

                            if (operators.Contains(value3))
                                return FinishToken(new Token(TokenType.Operator, value3));

                            BackChar();
                        }
                    }

                    if (operators.Contains(value2))
                        return FinishToken(new Token(TokenType.Operator, value2));

                    BackChar();
                }
            }

            if (operators.Contains(value1))
                return FinishToken(new Token(TokenType.Operator, value1));
        }

        if (Separators.Contains(ch))
            return FinishToken(new Token(TokenType.Separator, ch.ToString()));

        if (char.IsDigit(ch))
            return FinishToken(NextInteger(ch));

        if (char.IsLetter(ch) || ch == '_')
            return FinishToken(NextName(ch));

        throw new SyntaxError(string.Format("unexpected '{0}'", ch));
    }

    /// <summary>
    /// Pushes a token back onto the stream.
    /// </summary>
    /// <param name="token"></param>
    public void PushToken(Token? token, TokenSpan? span = null)
    {
        if (token == null)
            return;

        tokens.Push((token, span));
    }

    /// <summary>
    /// Gets the next name token from the stream.
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    private Token NextName(char ch)
    {
        string value = ch.ToString();
        int ich;

        for (ich = NextChar(); ich >= 0 && ((char)ich == '_' || char.IsLetterOrDigit((char)ich)); ich = NextChar())
            value += (char)ich;

        //if (ich >= 0)
        //    BackChar();

        // Handle trailing '?' or '!' as part of Ruby-like method names (e.g., defined?, nil?, even?)
        if (ich >= 0 && ((char)ich == '?' || (char)ich == '!'))
        {
            value += (char)ich;
        }
        else if (ich >= 0)
        {
            BackChar();
        }

        return new Token(TokenType.Name, value);
    }

    /// <summary>
    /// Gets the next instance variable name token from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private Token NextInstanceVariableName()
    {
        string value = string.Empty;
        int ich;

        for (ich = NextChar(); ich >= 0 && ((char)ich == '_' || char.IsLetterOrDigit((char)ich)); ich = NextChar())
            value += (char)ich;

        if (ich >= 0)
        {
            if (string.IsNullOrEmpty(value) && (char)ich == Variable)
                return NextClassVariableName();

            BackChar();
        }

        if (string.IsNullOrEmpty(value) || char.IsDigit(value[0]))
            throw new SyntaxError("invalid instance variable name");

        return new Token(TokenType.InstanceVarName, value);
    }

    /// <summary>
    /// Gets the next class variable name token from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private Token NextClassVariableName()
    {
        string value = string.Empty;
        int ich;

        for (ich = NextChar(); ich >= 0 && ((char)ich == '_' || char.IsLetterOrDigit((char)ich)); ich = NextChar())
            value += (char)ich;

        if (ich >= 0)
            BackChar();

        if (string.IsNullOrEmpty(value) || char.IsDigit(value[0]))
            throw new SyntaxError("invalid class variable name");

        return new Token(TokenType.ClassVarName, value);
    }

    /// <summary>
    /// Gets the next global variable name token from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private Token NextGlobalVariableName()
    {
        string value = string.Empty;
        int ich;

        for (ich = NextChar(); ich >= 0 && ((char)ich == '_' || char.IsLetterOrDigit((char)ich)); ich = NextChar())
            value += (char)ich;

        if (ich >= 0)
            BackChar();

        if (string.IsNullOrEmpty(value) || char.IsDigit(value[0]))
            throw new SyntaxError("invalid global variable name");

        return new Token(TokenType.GlobalVarName, value);
    }

    /// <summary>
    /// Gets the next symbol token from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private Token NextSymbol()
    {
        string value = string.Empty;
        int ich;

        for (ich = NextChar(); ich >= 0 && ((char)ich == '_' || char.IsLetterOrDigit((char)ich) || (char)ich == '@'); ich = NextChar())
        {
            char ch = (char)ich;

            if (char.IsDigit(ch) && string.IsNullOrEmpty(value))
                throw new SyntaxError("unexpected integer");

            value += ch;
        }

        if (ich >= 0)
        {
            char ch = (char)ich;

            if (ch == ':' && string.IsNullOrEmpty(value))
                return new Token(TokenType.Separator, "::");

            BackChar();
        }

        return new Token(TokenType.Symbol, value);
    }

    /// <summary>
    /// Gets the next string token from the stream.
    /// </summary>
    /// <param name="init"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private Token NextString(char init)
    {
        string value = string.Empty;
        int ich;

        for (ich = NextChar(); ich >= 0 && (char)ich != init; ich = NextChar())
        {
            char ch = (char)ich;

            if (ch == '\\')
            {
                int ich2 = NextChar();

                if (ich2 > 0)
                {
                    char ch2 = (char)ich2;

                    if (ch2 == 't')
                    {
                        value += '\t';
                        continue;
                    }

                    if (ch2 == 'r')
                    {
                        value += '\r';
                        continue;
                    }

                    if (ch2 == 'n')
                    {
                        value += '\n';
                        continue;
                    }

                    value += ch2;
                    continue;
                }
            }

            value += (char)ich;
        }

        if (ich < 0)
            throw new SyntaxError("unclosed string");

        return new Token(TokenType.String, value);
    }

    /// <summary>
    /// Parses a heredoc and returns a string token.
    /// </summary>
    /// <returns></returns>
    private Token NextHeredoc()
    {
        bool allowIndent = false;
        bool stripIndent = false;

        int ich = NextChar();
        if (ich < 0)
            throw new SyntaxError("unexpected end of file after heredoc");

        char ch = (char)ich;
        if (ch == '-')
        {
            allowIndent = true;
            ich = NextChar();
            if (ich < 0)
                throw new SyntaxError("unexpected end of file after heredoc");
            ch = (char)ich;
        }
        else if (ch == '~')
        {
            allowIndent = true;
            stripIndent = true;
            ich = NextChar();
            if (ich < 0)
                throw new SyntaxError("unexpected end of file after heredoc");
            ch = (char)ich;
        }

        // Skip optional spaces/tabs before the delimiter
        while (ch == ' ' || ch == '\t')
        {
            ich = NextChar();
            if (ich < 0)
                throw new SyntaxError("unexpected end of file after heredoc");
            ch = (char)ich;
        }

        if (ch == EndOfLine)
            throw new SyntaxError("heredoc missing delimiter");

        if (ch == Backtick)
            throw new SyntaxError("heredoc backtick delimiters are not supported");

        bool allowInterpolation;
        bool allowEscapes;
        string delimiter;

        if (ch == Quote || ch == DoubleQuote)
        {
            char quote = ch;
            delimiter = ReadQuotedDelimiter(quote);
            allowInterpolation = quote == DoubleQuote;
            allowEscapes = quote == DoubleQuote;
        }
        else
        {
            delimiter = ReadBareDelimiter(ch);
            allowInterpolation = true;
            allowEscapes = true;
        }

        // Require heredoc header to end the line (only whitespace or comments allowed).
        if (!ConsumeHeaderToEndOfLine())
            throw new SyntaxError("heredoc header must end the line");

        string value = ReadHeredocBody(delimiter, allowIndent, stripIndent, allowInterpolation, allowEscapes, out bool interpolated);
        return interpolated ? new Token(TokenType.InterpolatedString, value) : new Token(TokenType.String, value);
    }

    private string ReadQuotedDelimiter(char quote)
    {
        string value = string.Empty;
        int ich;

        for (ich = NextChar(); ich >= 0 && (char)ich != quote; ich = NextChar())
            value += (char)ich;

        if (ich < 0)
            throw new SyntaxError("unclosed heredoc delimiter");

        if (string.IsNullOrEmpty(value))
            throw new SyntaxError("heredoc missing delimiter");

        return value;
    }

    private string ReadBareDelimiter(char first)
    {
        string value = string.Empty;
        int ich = first;

        while (ich >= 0 && ((char)ich == '_' || char.IsLetterOrDigit((char)ich)))
        {
            value += (char)ich;
            ich = NextChar();
        }

        if (ich >= 0)
            BackChar();

        if (string.IsNullOrEmpty(value))
            throw new SyntaxError("heredoc missing delimiter");

        return value;
    }

    private bool ConsumeHeaderToEndOfLine()
    {
        int ich = NextChar();

        while (ich >= 0 && (char)ich != EndOfLine)
        {
            char ch = (char)ich;
            if (ch == StartComment)
            {
                for (ich = NextChar(); ich >= 0 && (char)ich != EndOfLine; ich = NextChar())
                {
                }
                break;
            }

            if (ch != ' ' && ch != '\t' && ch != '\r')
                return false;

            ich = NextChar();
        }

        if (ich < 0)
            throw new SyntaxError("unclosed heredoc");

        return true;
    }

    private string ReadHeredocBody(
        string delimiter,
        bool allowIndent,
        bool stripIndent,
        bool allowInterpolation,
        bool allowEscapes,
        out bool interpolated)
    {
        var lines = new List<(string Content, bool HasNewline)>();

        while (true)
        {
            string line = ReadLine(out bool hasNewline);
            if (!hasNewline && line.Length == 0)
                throw new SyntaxError("unclosed heredoc");

            string lineForMatch = TrimTrailingCarriageReturn(line);
            string candidate = allowIndent ? TrimLeadingWhitespace(lineForMatch) : lineForMatch;

            if (IsTerminatorLine(candidate, delimiter))
                break;

            lines.Add((line, hasNewline));
        }

        int stripCount = stripIndent ? GetMinimumIndent(lines) : 0;

        var builder = new System.Text.StringBuilder();
        foreach (var (content, hasNewline) in lines)
        {
            string trimmed = TrimTrailingCarriageReturn(content);
            bool hadCarriageReturn = content.EndsWith("\r", StringComparison.Ordinal);

            if (stripCount > 0)
                trimmed = RemoveLeadingIndent(trimmed, stripCount);

            if (hadCarriageReturn)
                trimmed += "\r";

            if (hasNewline)
                trimmed += "\n";

            builder.Append(trimmed);
        }

        string value = builder.ToString();
        if (allowEscapes)
            value = ProcessEscapes(value);

        interpolated = allowInterpolation && value.Contains("#{", StringComparison.Ordinal);
        return value;
    }

    private string ReadLine(out bool hasNewline)
    {
        var builder = new System.Text.StringBuilder();
        int ich;

        for (ich = NextChar(); ich >= 0 && (char)ich != EndOfLine; ich = NextChar())
            builder.Append((char)ich);

        hasNewline = ich >= 0 && (char)ich == EndOfLine;
        return builder.ToString();
    }

    private static string TrimTrailingCarriageReturn(string line)
    {
        if (line.EndsWith("\r", StringComparison.Ordinal))
            return line[..^1];
        return line;
    }

    private static string TrimLeadingWhitespace(string line)
    {
        int i = 0;
        while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
            i++;
        return line[i..];
    }

    private static bool IsTerminatorLine(string candidate, string delimiter)
    {
        if (!candidate.StartsWith(delimiter, StringComparison.Ordinal))
            return false;

        string rest = candidate[delimiter.Length..];
        return rest.Length == 0 || rest.Trim().Length == 0;
    }

    private static int GetMinimumIndent(List<(string Content, bool HasNewline)> lines)
    {
        int minIndent = int.MaxValue;
        foreach (var (content, _) in lines)
        {
            string line = TrimTrailingCarriageReturn(content);
            if (line.Length == 0 || line.Trim().Length == 0)
                continue;

            int count = 0;
            while (count < line.Length && (line[count] == ' ' || line[count] == '\t'))
                count++;

            if (count < minIndent)
                minIndent = count;
        }

        return minIndent == int.MaxValue ? 0 : minIndent;
    }

    private static string RemoveLeadingIndent(string line, int count)
    {
        int i = 0;
        while (i < line.Length && i < count && (line[i] == ' ' || line[i] == '\t'))
            i++;
        return line[i..];
    }

    private static string ProcessEscapes(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            char ch = value[i];
            if (ch == '\\' && i + 1 < value.Length)
            {
                char next = value[i + 1];
                if (next == 't') { builder.Append('\t'); i++; continue; }
                if (next == 'r') { builder.Append('\r'); i++; continue; }
                if (next == 'n') { builder.Append('\n'); i++; continue; }
                builder.Append(next);
                i++;
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Reads a regex literal after the leading '/' has already been consumed.
    /// </summary>
    /// <returns>Pattern and option flags.</returns>
    /// <exception cref="SyntaxError"></exception>
    internal (string Pattern, string Options) ReadRegexLiteral()
    {
        string pattern = string.Empty;
        bool escaped = false;
        int ich;

        for (ich = NextChar(); ich >= 0; ich = NextChar())
        {
            char ch = (char)ich;

            if (!escaped && ch == '/')
                break;

            if (!escaped && ch == '\n')
                throw new SyntaxError("unclosed regex");

            if (!escaped && ch == '\\')
            {
                escaped = true;
                pattern += ch;
                continue;
            }

            escaped = false;
            pattern += ch;
        }

        if (ich < 0)
            throw new SyntaxError("unclosed regex");

        string options = string.Empty;
        int ichOption = NextChar();
        while (ichOption >= 0)
        {
            char opt = (char)ichOption;
            if (!char.IsLetter(opt))
            {
                BackChar();
                break;
            }

            options += opt;
            ichOption = NextChar();
        }

        return (pattern, options);
    }

    /// <summary>
    /// Gets the next double-quoted string token from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private Token NextDoubleQuotedString()
    {
        string value = string.Empty;
        int ich;
        bool isInterpolated = false;

        for (ich = NextChar(); ich >= 0 && (char)ich != DoubleQuote; ich = NextChar())
        {
            char ch = (char)ich;

            if (ch == '\\')
            {
                int ich2 = NextChar();
                if (ich2 > 0)
                {
                    char ch2 = (char)ich2;
                    if (ch2 == 't') { value += '\t'; continue; }
                    if (ch2 == 'r') { value += '\r'; continue; }
                    if (ch2 == 'n') { value += '\n'; continue; }
                    value += ch2;
                    continue;
                }
            }

            if (ch == '#' && PeekChar() == '{')
            {
                isInterpolated = true;
            }

            value += ch;
        }

        if (ich < 0)
            throw new SyntaxError("unclosed string");

        if (isInterpolated)
            return new Token(TokenType.InterpolatedString, value);
        else
            return new Token(TokenType.String, value);
    }

    /// <summary>
    /// Gets the next integer token from the stream.
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    private Token NextInteger(char ch)
    {
        string value = ch.ToString();
        int ich;

        for (ich = NextChar(); ich >= 0 && char.IsDigit((char)ich); ich = NextChar())
            value += (char)ich;

        if (ich >= 0 && (char)ich == '.')
            return NextReal(value);

        if (ich >= 0)
            BackChar();

        return new Token(TokenType.Integer, value);
    }

    /// <summary>
    /// Gets the next real (floating-point) number token from the stream.
    /// </summary>
    /// <param name="ivalue"></param>
    /// <returns></returns>
    private Token NextReal(string ivalue)
    {
        string value = ivalue + ".";
        int ich;

        for (ich = NextChar(); ich >= 0 && char.IsDigit((char)ich); ich = NextChar())
            value += (char)ich;

        if (ich >= 0)
            BackChar();

        if (value.EndsWith("."))
        {
            BackChar();
            return new Token(TokenType.Integer, ivalue);
        }

        return new Token(TokenType.Real, value);
    }

    /// <summary>
    /// Gets the next non-whitespace, non-comment character from the stream.
    /// </summary>
    /// <returns></returns>
    private int NextFirstChar()
    {
        int ich = NextChar();

        while (true)
        {
            while (ich > 0 && (char)ich != '\n' && char.IsWhiteSpace((char)ich))
                ich = NextChar();

            if (ich > 0 && (char)ich == StartComment)
            {
                for (ich = stream.NextChar(); ich >= 0 && (char)ich != '\n';)
                    ich = stream.NextChar();

                if (ich < 0)
                    return -1;

                continue;
            }

            break;
        }

        return ich;
    }

    /// <summary>
    /// Gets the next character from the stream.
    /// </summary>
    /// <returns></returns>
    private int NextChar()
    {
        int ich = stream.NextChar();
        if (ich >= 0)
        {
            lastCharOffset = offset;
            offset++;
        }
        return ich;
    }

    /// <summary>
    /// Moves back one character in the stream.
    /// </summary>
    private void BackChar()
    {
        if (offset > 0)
            offset--;
        stream.BackChar();
    }

    /// <summary>
    /// Peeks at the next character without consuming it.
    /// </summary>
    /// <returns></returns>
    private int PeekChar()
    {
        int ich = NextChar();
        if (ich >= 0)
            BackChar();
        return ich;
    }

    /// <summary>
    /// Finishes the token by setting its span.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private Token FinishToken(Token token)
    {
        LastTokenSpan = new TokenSpan(tokenStartOffset, offset);
        return token;
    }

}

