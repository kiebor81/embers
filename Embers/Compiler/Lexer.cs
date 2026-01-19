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
    private const string Separators = ";()[],.|{}&";
    private static readonly string[] operators = ["?", ":", "+", "-", "*", "/", "%", "**", "=", "<", ">", "!", "==", "===", "<=", ">=", "!=", "<=>", "=>", "->", "..", "&&", "||", "+=", "-=", "*=", "/=", "%=", "**="];

    /// <summary>
    /// the character stream
    /// </summary>
    private readonly ICharStream stream;

    /// <summary>
    /// the pushed-back tokens stack
    /// </summary>
    private readonly Stack<Token> tokens = new();

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
            return tokens.Pop();

        int ich = NextFirstChar();

        if (ich == -1)
            return null;

        char ch = (char)ich;

        if (ch == EndOfLine)
            return new Token(TokenType.EndOfLine, "\n");

        if (ch == Quote)
            return NextString(Quote);

        if (ch == DoubleQuote)
            return NextDoubleQuotedString();

        //if (ch == Colon)
        //    return NextSymbol();

        if (ch == Colon)
        {
            int peek = PeekChar();

            // If followed by a valid symbol name character, it's a symbol
            if (char.IsLetterOrDigit((char)peek) || peek == '_' || peek == '@')
                return NextSymbol();

            // If followed by another colon, it's a namespace separator
            if (peek == ':')
            {
                NextChar();  // consume second ':'
                return new Token(TokenType.Separator, "::");
            }

            // Else, it's the colon in a ternary operator
            return new Token(TokenType.Operator, ":");
        }

        if (ch == Variable)
            return NextInstanceVariableName();

        if (ch == GlobalVariable)
            return NextGlobalVariableName();

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
                                return new Token(TokenType.Operator, value3);

                            BackChar();
                        }
                    }

                    if (operators.Contains(value2))
                        return new Token(TokenType.Operator, value2);

                    BackChar();
                }
            }

            if (operators.Contains(value1))
                return new Token(TokenType.Operator, value1);
        }

        if (Separators.Contains(ch))
            return new Token(TokenType.Separator, ch.ToString());

        if (char.IsDigit(ch))
            return NextInteger(ch);

        if (char.IsLetter(ch) || ch == '_')
            return NextName(ch);

        throw new SyntaxError(string.Format("unexpected '{0}'", ch));
    }

    /// <summary>
    /// Pushes a token back onto the stream.
    /// </summary>
    /// <param name="token"></param>
    public void PushToken(Token token) => tokens.Push(token);

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
    private int NextChar() => stream.NextChar();

    /// <summary>
    /// Moves back one character in the stream.
    /// </summary>
    private void BackChar() => stream.BackChar();

    /// <summary>
    /// Peeks at the next character without consuming it.
    /// </summary>
    /// <returns></returns>
    private int PeekChar()
    {
        int ich = stream.NextChar();
        if (ich >= 0)
            stream.BackChar();
        return ich;
    }

}

