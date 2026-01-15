using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Compiler;

public partial class Parser
{
    /// <summary>
    /// Parses the end of a command.
    /// </summary>
    /// <exception cref="SyntaxError"></exception>
    private void ParseEndOfCommand()
    {
        Token token = lexer.NextToken();

        if (token != null && token.Type == TokenType.Name && token.Value == "end")
        {
            lexer.PushToken(token);
            return;
        }

        if (token != null && token.Type == TokenType.Separator && token.Value == "}")
        {
            lexer.PushToken(token);
            return;
        }

        if (!IsEndOfCommand(token))
            throw new SyntaxError("end of command expected");
    }

    /// <summary>
    /// Determines if the next token starts an expression list.
    /// </summary>
    /// <returns></returns>
    private bool NextTokenStartsExpressionList()
    {
        Token token = lexer.NextToken();
        lexer.PushToken(token);

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

        return true;
    }

    /// <summary>
    /// Determines if a token is an end of command.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private bool IsEndOfCommand(Token token)
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
    /// Parses a name token with the given value.
    /// </summary>
    /// <param name="name"></param>
    private void ParseName(string name) => ParseToken(TokenType.Name, name);

    /// <summary>
    /// Parses a token of the given type and value.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <exception cref="SyntaxError"></exception>
    private void ParseToken(TokenType type, string value)
    {
        Token token = lexer.NextToken();

        if (token == null || token.Type != type || token.Value != value)
            throw new SyntaxError(string.Format("expected '{0}'", value));
    }

    /// <summary>
    /// Parses a name token.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private string ParseName()
    {
        Token token = lexer.NextToken();

        if (token == null || token.Type != TokenType.Name)
            throw new SyntaxError("name expected");

        return token.Value;
    }

    /// <summary>
    /// Tries to parse a name token with the given value.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private bool TryParseName(string name) => TryParseToken(TokenType.Name, name);

    /// <summary>
    /// Tries to parse a token of the given type and value.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool TryParseToken(TokenType type, string value)
    {
        Token token = lexer.NextToken();

        if (token != null && token.Type == type && token.Value == value)
            return true;

        lexer.PushToken(token);

        return false;
    }

    /// <summary>
    /// Tries to parse a name token.
    /// </summary>
    /// <returns></returns>
    private string? TryParseName()
    {
        Token token = lexer.NextToken();

        if (token != null && token.Type == TokenType.Name)
            return token.Value;

        lexer.PushToken(token);

        return null;
    }

    /// <summary>
    /// Tries to parse an end-of-line token.
    /// </summary>
    /// <returns></returns>
    private bool TryParseEndOfLine()
    {
        Token token = lexer.NextToken();

        if (token != null && token.Type == TokenType.EndOfLine && token.Value == "\n")
            return true;

        lexer.PushToken(token);

        return false;
    }

    /// <summary>
    /// Determines if the given token is a binary operator at the specified precedence level.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private bool IsBinaryOperator(int level, Token token) => token.Type == TokenType.Operator && binaryoperators[level].Contains(token.Value);
}
