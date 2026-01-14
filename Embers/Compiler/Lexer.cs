using Embers.Exceptions;

namespace Embers.Compiler
{
    public class Lexer
    {
        private const char Quote = '\'';
        private const char DoubleQuote = '"';
        private const char Colon = ':';
        private const char StartComment = '#';
        private const char EndOfLine = '\n';
        private const char Variable = '@';

        private const string Separators = ";()[],.|{}&";

        private static readonly string[] operators = ["?", ":", "+", "-", "*", "/", "%", "**", "=", "<", ">", "!", "==", "<=", ">=", "!=", "=>", "->", "..", "&&", "||", "+=", "-=", "*=", "/=", "%=", "**="];

        private readonly ICharStream stream;
        private readonly Stack<Token> tokens = new();

        public Lexer(string text)
        {
            stream = new TextCharStream(text);
        }

        public Lexer(TextReader reader)
        {
            stream = new TextReaderCharStream(reader);
        }

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
                if (char.IsLetterOrDigit((char)peek) || peek == '_')
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

            if (operators.Contains(ch.ToString()))
            {
                string value = ch.ToString();
                ich = NextChar();

                if (ich >= 0)
                {
                    value += (char)ich;
                    if (operators.Contains(value))
                        return new Token(TokenType.Operator, value);

                    BackChar();
                }

                return new Token(TokenType.Operator, ch.ToString());
            }
            else if (operators.Any(op => op.StartsWith(ch.ToString())))
            {
                string value = ch.ToString();
                ich = NextChar();

                if (ich >= 0)
                {
                    value += (char)ich;
                    if (operators.Contains(value))
                        return new Token(TokenType.Operator, value);

                    BackChar();
                }
            }

            if (Separators.Contains(ch))
                return new Token(TokenType.Separator, ch.ToString());

            if (char.IsDigit(ch))
                return NextInteger(ch);

            if (char.IsLetter(ch) || ch == '_')
                return NextName(ch);

            throw new SyntaxError(string.Format("unexpected '{0}'", ch));
        }

        public void PushToken(Token token) => tokens.Push(token);

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

        private Token NextSymbol()
        {
            string value = string.Empty;
            int ich;

            for (ich = NextChar(); ich >= 0 && ((char)ich == '_' || char.IsLetterOrDigit((char)ich)); ich = NextChar())
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

        private int NextChar() => stream.NextChar();

        private void BackChar() => stream.BackChar();

        private int PeekChar()
        {
            int ich = stream.NextChar();
            if (ich >= 0)
                stream.BackChar();
            return ich;
        }

    }
}
