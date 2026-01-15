using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Compiler;

/// <summary>
/// the parser for the Embers language
/// </summary>
public partial class Parser
{
    /// <summary>
    /// the binary operators by precedence level
    /// </summary>
    private static string[][] binaryoperators = [
        ["&&", "||"],
            ["..", "==", "!=", "<", ">", "<=", ">="],
            ["+", "-"],
            ["*", "/", "%"],
            ["**"]
    ];

    /// <summary>
    /// the lexer for the Embers language
    /// </summary>
    private readonly Lexer lexer;

    /// <summary>
    /// the parser for the Embers language
    /// </summary>
    /// <param name="text"></param>
    public Parser(string text)
    {
        lexer = new Lexer(text);
    }

    /// <summary>
    /// the parser for the Embers language
    /// </summary>
    /// <param name="reader"></param>
    public Parser(TextReader reader)
    {
        lexer = new Lexer(reader);
    }

    /// <summary>
    /// Parses an expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression? ParseExpression()
    {
        IExpression expr = ApplyPostfixConditional(ParseNoAssignExpression());

        if (expr == null)
            return null;

        // Only variables or assignable targets can be followed by assignment
        bool isAssignable = expr is NameExpression
            || expr is ClassVarExpression
            || expr is InstanceVarExpression
            || expr is DotExpression
            || expr is IndexedExpression;

        if (!isAssignable)
            return ApplyPostfixConditional(expr);

        Token token = lexer.NextToken();

        if (token == null)
            return ApplyPostfixConditional(expr);

        // Handle regular assignment
        if (token.Type == TokenType.Operator && token.Value == "=")
        {
            IExpression assignexpr = expr switch
            {
                NameExpression name => new AssignExpression(name.Name, ParseExpression()),
                DotExpression dot => new AssignDotExpressions(dot, ParseExpression()),
                InstanceVarExpression ivar => new AssignInstanceVarExpression(ivar.Name, ParseExpression()),
                ClassVarExpression cvar => new AssignClassVarExpression(cvar.Name, ParseExpression()),
                IndexedExpression idx => new AssignIndexedExpression(idx.Expression, idx.IndexExpression, ParseExpression()),
                _ => throw new SyntaxError("invalid assignment target")
            };

            return ApplyPostfixConditional(assignexpr);
        }

        // Handle compound assignment: +=, -=, etc.
        if (token.Type == TokenType.Operator && (
            token.Value == "+=" || token.Value == "-=" ||
            token.Value == "*=" || token.Value == "/=" ||
            token.Value == "%=" || token.Value == "**="))
        {
            return ApplyPostfixConditional(ParseCompoundAssignment(expr, token.Value));
        }

        // Not an assignment at all
        lexer.PushToken(token);
        return ApplyPostfixConditional(expr);
    }

    /// <summary>
    /// Parses a command.
    /// </summary>
    /// <returns></returns>
    public IExpression? ParseCommand()
    {
        Token token = lexer.NextToken();

        while (token != null && IsEndOfCommand(token))
            token = lexer.NextToken();

        if (token == null)
            return null;

        lexer.PushToken(token);

        IExpression expr = ParseExpression();
        ParseEndOfCommand();
        return expr;
    }

    /// <summary>
    /// Parses an expression without assignment.
    /// </summary>
    /// <returns></returns>
    private IExpression? ParseNoAssignExpression()
    {
        var result = ParseBinaryExpression(0);

        if (result == null)
            return null;

        if (result is not NameExpression)
            return ApplyPostfixChain(result);

        var nexpr = (NameExpression)result;

        if (TryParseToken(TokenType.Separator, "{"))
            return ApplyPostfixChain(new CallExpression(nexpr.Name, [ParseBlockExpression(true)]));

        if (TryParseName("do"))
            return ApplyPostfixChain(new CallExpression(nexpr.Name, [ParseBlockExpression()]));

        if (!NextTokenStartsExpressionList())
            return ApplyPostfixChain(result);

        return ApplyPostfixChain(new CallExpression(((NameExpression)result).Name, ParseExpressionListWithBlockArgs()));
    }
}
