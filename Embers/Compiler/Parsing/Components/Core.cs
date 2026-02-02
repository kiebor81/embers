using Embers.Exceptions;

namespace Embers.Compiler.Parsing.Components;

/// <summary>
/// Core parsing components
/// </summary>
/// <param name="parser"></param>
/// <param name="binaryoperators"></param>
internal sealed class Core(Parser parser, string[][] binaryoperators)
{
    private readonly Parser parser = parser;
    private readonly string[][] binaryoperators = binaryoperators;

    /// <summary>
    /// Parses an end-of-line token
    /// </summary>
    /// <exception cref="SyntaxError"></exception>
    public void ParseEndOfCommand()
    {
        Token token = parser.NextToken();

        if (token != null && token.Type == TokenType.Name && token.Value == "end")
        {
            parser.PushToken(token);
            return;
        }

        if (token != null && token.Type == TokenType.Separator && token.Value == "}")
        {
            parser.PushToken(token);
            return;
        }

        if (!IsEndOfCommand(token))
            throw new SyntaxError("end of command expected");
    }

    /// <summary>
    /// Determines if the next token starts an expression list
    /// </summary>
    /// <returns></returns>
    public bool NextTokenStartsExpressionList()
    {
        Token token = parser.NextToken();
        parser.PushToken(token);

        if (token == null)
            return false;

        if (IsEndOfCommand(token))
            return false;

        if (token.Type == TokenType.Operator)
            return false;

        if (token.Type == TokenType.Separator)
            return token.Value == "(";

        if (token.Type == TokenType.Name && token.Value == "end")
            return false;

        if (token.Type == TokenType.Name && (token.Value == "if" || token.Value == "unless" || token.Value == "then" || token.Value == "when" || token.Value == "else" || token.Value == "in"))
            return false;

        if (token.Type == TokenType.Name && (token.Value == "and" || token.Value == "or" || token.Value == "not"))
            return false;

        return true;
    }

    /// <summary>
    /// Determines if the next token starts an expression list, allowing splat/kwargs prefixes.
    /// </summary>
    /// <returns></returns>
    public bool NextTokenStartsExpressionListAllowSplat()
    {
        Token token = parser.NextToken();
        parser.PushToken(token);

        if (token == null)
            return false;

        if (IsEndOfCommand(token))
            return false;

        if (token.Type == TokenType.Operator)
            return token.Value == "*" || token.Value == "**";

        if (token.Type == TokenType.Separator)
            return token.Value == "(";

        if (token.Type == TokenType.Name && token.Value == "end")
            return false;

        if (token.Type == TokenType.Name && (token.Value == "if" || token.Value == "unless" || token.Value == "then" || token.Value == "when" || token.Value == "else" || token.Value == "in"))
            return false;

        if (token.Type == TokenType.Name && (token.Value == "and" || token.Value == "or" || token.Value == "not"))
            return false;

        return true;
    }

    /// <summary>
    /// Determines if the specified token indicates the end of a command
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool IsEndOfCommand(Token token)
    {
        if (token == null)
            return true;

        if (token.Type == TokenType.EndOfLine)
            return true;

        if (token.Type == TokenType.Separator && token.Value == ";")
            return true;

        return false;
    }

    /// <summary>
    /// Parses a name token with the specified value
    /// </summary>
    /// <param name="name"></param>
    public void ParseName(string name) => ParseToken(TokenType.Name, name);

    /// <summary>
    /// Parses a token of the specified type and value
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <exception cref="SyntaxError"></exception>
    public void ParseToken(TokenType type, string value)
    {
        Token token = parser.NextToken();

        if (token == null || token.Type != type || token.Value != value)
            throw new SyntaxError(string.Format("expected '{0}'", value));
    }

    /// <summary>
    /// Parses a name token
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public string ParseName()
    {
        Token token = parser.NextToken();

        if (token == null || token.Type != TokenType.Name)
            throw new SyntaxError("name expected");

        return token.Value;
    }

    /// <summary>
    /// Tries to parse a name token with the specified value
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool TryParseName(string name) => TryParseToken(TokenType.Name, name);

    /// <summary>
    /// Tries to parse a token of the specified type and value
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryParseToken(TokenType type, string value)
    {
        Token token = parser.NextToken();

        if (token != null && token.Type == type && token.Value == value)
            return true;

        parser.PushToken(token);

        return false;
    }

    /// <summary>
    /// Tries to parse a name token
    /// </summary>
    /// <returns></returns>
    public string? TryParseName()
    {
        Token token = parser.NextToken();

        if (token != null && token.Type == TokenType.Name)
            return token.Value;

        parser.PushToken(token);

        return null;
    }

    /// <summary>
    /// Tries to parse an end-of-line token
    /// </summary>
    /// <returns></returns>
    public bool TryParseEndOfLine()
    {
        Token token = parser.NextToken();

        if (token != null && token.Type == TokenType.EndOfLine && token.Value == "\n")
            return true;

        parser.PushToken(token);

        return false;
    }

    /// <summary>
    /// Skips all consecutive end-of-line tokens
    /// </summary>
    public void SkipEndOfLines()
    {
        while (TryParseEndOfLine())
        {
        }
    }

    /// <summary>
    /// Parses primary expressions
    /// </summary>
    /// <param name="level"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool IsBinaryOperator(int level, Token token)
    {
        if (token.Type == TokenType.Operator && binaryoperators[level].Contains(token.Value))
            return true;

        return token.Type == TokenType.Name
            && (token.Value == "and" || token.Value == "or")
            && binaryoperators[level].Contains(token.Value);
    }
}


