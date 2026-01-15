using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Compiler;

public partial class Parser
{
    /// <summary>
    /// Parses a parameter list.
    /// </summary>
    /// <param name="canhaveparens"></param>
    /// <returns></returns>
    private IList<string> ParseParameterList(bool canhaveparens = true)
    {
        IList<string> parameters = [];

        bool inparentheses = false;

        if (canhaveparens)
            inparentheses = TryParseToken(TokenType.Separator, "(");

        for (string name = TryParseName(); name != null; name = ParseName())
        {
            parameters.Add(name);
            if (!TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
            ParseToken(TokenType.Separator, ")");

        return parameters;
    }

    /// <summary>
    /// Parses a parameter list with possible block parameter (&param).
    /// </summary>
    /// <param name="canhaveparens"></param>
    /// <returns></returns>
    private (IList<string> parameters, string? blockParam) ParseParameterListWithBlock(bool canhaveparens = true)
    {
        IList<string> parameters = [];
        string? blockParam = null;

        bool inparentheses = false;

        if (canhaveparens)
            inparentheses = TryParseToken(TokenType.Separator, "(");

        while (true)
        {
            // Check for block parameter (&param)
            if (TryParseToken(TokenType.Separator, "&"))
            {
                string blockParamName = ParseName();
                blockParam = blockParamName;
                // Block parameter must be last
                break;
            }

            string name = TryParseName();
            if (name == null)
                break;

            parameters.Add(name);
            if (!TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
            ParseToken(TokenType.Separator, ")");

        return (parameters, blockParam);
    }

    /// <summary>
    /// Parses an expression list.
    /// </summary>
    /// <returns></returns>
    private IList<IExpression> ParseExpressionList()
    {
        IList<IExpression> expressions = [];

        bool inparentheses = TryParseToken(TokenType.Separator, "(");

        for (IExpression expression = ParseExpression(); expression != null; expression = ParseExpression())
        {
            expressions.Add(expression);
            if (!TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
        {
            ParseToken(TokenType.Separator, ")");
            if (TryParseName("do"))
                expressions.Add(ParseBlockExpression());
            else if (TryParseToken(TokenType.Separator, "{"))
                expressions.Add(ParseBlockExpression(true));
        }

        return expressions;
    }

    /// <summary>
    /// Parses a single expression with possible block prefix (&).
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private IExpression? ParseSingleExpressionWithBlockPrefix()
    {
        // Check for &expression (block argument)
        if (TryParseToken(TokenType.Separator, "&"))
        {
            // Check for &:symbol syntax (shorthand for &symbol.to_proc)
            if (TryParseToken(TokenType.Separator, ":"))
            {
                Token? nameToken = lexer.NextToken();
                if (nameToken == null || nameToken.Type != TokenType.Name)
                    throw new SyntaxError("Expected symbol name after &:");

                // Create :symbol.to_proc call
                IExpression symbolExpr = new ConstantExpression(new Symbol(nameToken.Value));
                IExpression toProcCall = new DotExpression(symbolExpr, "to_proc", []);
                return new BlockArgumentExpression(toProcCall);
            }

            IExpression expr = ParseExpression();
            if (expr != null)
                return new BlockArgumentExpression(expr);
            throw new SyntaxError("Expected expression after &");
        }

        return ParseExpression();
    }

    /// <summary>
    /// Parses an expression list with possible block arguments.
    /// </summary>
    /// <returns></returns>
    private IList<IExpression> ParseExpressionListWithBlockArgs()
    {
        IList<IExpression> expressions = [];

        bool inparentheses = TryParseToken(TokenType.Separator, "(");

        for (IExpression? expression = ParseSingleExpressionWithBlockPrefix(); expression != null; expression = ParseSingleExpressionWithBlockPrefix())
        {
            expressions.Add(expression);
            if (!TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
        {
            ParseToken(TokenType.Separator, ")");
            if (TryParseName("do"))
                expressions.Add(ParseBlockExpression());
            else if (TryParseToken(TokenType.Separator, "{"))
                expressions.Add(ParseBlockExpression(true));
        }

        return expressions;
    }

    /// <summary>
    /// Parses an expression list until the given separator is encountered.
    /// </summary>
    /// <param name="separator"></param>
    /// <returns></returns>
    private IList<IExpression> ParseExpressionList(string separator)
    {
        IList<IExpression> expressions = [];

        for (IExpression expression = ParseExpression(); expression != null; expression = ParseExpression())
        {
            expressions.Add(expression);
            if (!TryParseToken(TokenType.Separator, ","))
                break;
        }

        ParseToken(TokenType.Separator, separator);

        return expressions;
    }

    /// <summary>
    /// Parses a block expression.
    /// </summary>
    /// <param name="usebraces"></param>
    /// <returns></returns>
    private BlockExpression ParseBlockExpression(bool usebraces = false)
    {
        if (TryParseToken(TokenType.Separator, "|"))
        {
            IList<string> paramnames = ParseParameterList(false);
            ParseToken(TokenType.Separator, "|");
            return new BlockExpression(paramnames, ParseCommandList(usebraces));
        }

        return new BlockExpression(null, ParseCommandList(usebraces));
    }

    /// <summary>
    /// Parses a command list until "end" or "}" is encountered.
    /// </summary>
    /// <param name="usebraces"></param>
    /// <returns></returns>
    private IExpression ParseCommandList(bool usebraces = false)
    {
        Token token;
        IList<IExpression> commands = [];

        for (token = lexer.NextToken(); token != null; token = lexer.NextToken())
        {
            if (usebraces && token.Type == TokenType.Separator && token.Value == "}")
                break;
            else if (!usebraces && token.Type == TokenType.Name && token.Value == "end")
                break;

            if (IsEndOfCommand(token))
                continue;

            lexer.PushToken(token);
            commands.Add(ParseCommand());
        }

        lexer.PushToken(token);

        if (usebraces)
            ParseToken(TokenType.Separator, "}");
        else
            ParseName("end");

        if (commands.Count == 1)
            return commands[0];

        return new CompositeExpression(commands);
    }

    /// <summary>
    /// Parses a command list until one of the given names is encountered.
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    private IExpression ParseCommandList(IList<string> names)
    {
        Token token;
        IList<IExpression> commands = [];

        for (token = lexer.NextToken(); token != null && (token.Type != TokenType.Name || !names.Contains(token.Value)); token = lexer.NextToken())
        {
            if (IsEndOfCommand(token))
                continue;

            lexer.PushToken(token);
            commands.Add(ParseCommand());
        }

        lexer.PushToken(token);

        if (commands.Count == 1)
            return commands[0];

        return new CompositeExpression(commands);
    }
}
