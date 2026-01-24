using Embers.Exceptions;
using Embers.Expressions;

namespace Embers.Compiler.Parsing.Components;

/// <summary>
/// Parsing components for blocks and parameter lists
/// </summary>
/// <param name="parser"></param>
internal sealed class Blocks(Parser parser)
{
    /// <summary>
    /// Parses a parameter list
    /// </summary>
    private readonly Parser parser = parser;

    /// <summary>
    /// Parses a parameter list
    /// </summary>
    /// <param name="canhaveparens"></param>
    /// <returns></returns>
    public IList<string> ParseParameterList(bool canhaveparens = true)
    {
        IList<string> parameters = [];

        bool inparentheses = false;

        if (canhaveparens)
            inparentheses = parser.TryParseToken(TokenType.Separator, "(");

        for (string? name = parser.TryParseName(); name != null; name = parser.ParseName())
        {
            parameters.Add(name);
            if (!parser.TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
            parser.ParseToken(TokenType.Separator, ")");

        return parameters;
    }

    /// <summary>
    /// Parses a parameter list with optional block parameter
    /// </summary>
    /// <param name="canhaveparens"></param>
    /// <returns></returns>
    public (IList<string> parameters, string? splatParam, string? kwParam, string? blockParam) ParseParameterListWithBlock(bool canhaveparens = true)
    {
        IList<string> parameters = [];
        string? splatParam = null;
        string? kwParam = null;
        string? blockParam = null;

        bool inparentheses = false;

        if (canhaveparens)
            inparentheses = parser.TryParseToken(TokenType.Separator, "(");

        while (true)
        {
            if (parser.TryParseToken(TokenType.Operator, "**"))
            {
                if (kwParam != null)
                    throw new SyntaxError("duplicate keyword splat parameter");

                string kwName = parser.ParseName();
                kwParam = kwName;

                if (!parser.TryParseToken(TokenType.Separator, ","))
                    break;

                continue;
            }

            if (parser.TryParseToken(TokenType.Operator, "*"))
            {
                if (splatParam != null)
                    throw new SyntaxError("duplicate splat parameter");

                if (kwParam != null)
                    throw new SyntaxError("splat parameter must precede keyword splat");

                string splatName = parser.ParseName();
                splatParam = splatName;

                if (!parser.TryParseToken(TokenType.Separator, ","))
                    break;

                continue;
            }

            if (parser.TryParseToken(TokenType.Separator, "&"))
            {
                string blockParamName = parser.ParseName();
                blockParam = blockParamName;
                break;
            }

            string? name = parser.TryParseName();
            if (name == null)
                break;

            if (splatParam != null || kwParam != null)
                throw new SyntaxError("positional parameters must precede splat or kwargs");

            parameters.Add(name);
            if (!parser.TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
            parser.ParseToken(TokenType.Separator, ")");

        return (parameters, splatParam, kwParam, blockParam);
    }

    /// <summary>
    /// Parses a list of expressions
    /// </summary>
    /// <returns></returns>
    public IList<IExpression> ParseExpressionList()
    {
        IList<IExpression> expressions = [];

        bool inparentheses = parser.TryParseToken(TokenType.Separator, "(");

        for (IExpression? expression = parser.ParseExpression(); expression != null; expression = parser.ParseExpression())
        {
            expressions.Add(expression);
            if (!parser.TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
        {
            parser.ParseToken(TokenType.Separator, ")");
            if (parser.TryParseName("do"))
                expressions.Add(parser.ParseBlockExpression());
            else if (parser.TryParseToken(TokenType.Separator, "{"))
                expressions.Add(parser.ParseBlockExpression(true));
        }

        return expressions;
    }

    /// <summary>
    /// Parses a single expression, supporting block argument prefix '&'
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression? ParseSingleExpressionWithBlockPrefix()
    {
        return ParseSingleExpressionWithBlockPrefix(true);
    }

    /// <summary>
    /// Parses a single expression, supporting block argument prefix '&'
    /// </summary>
    /// <param name="allowPostfixConditional"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private IExpression? ParseSingleExpressionWithBlockPrefix(bool allowPostfixConditional)
    {
        if (parser.TryParseToken(TokenType.Operator, "**"))
        {
            IExpression? expr = allowPostfixConditional ? parser.ParseExpression() : ParseCommandArgExpression();
            if (expr != null)
                return new KeywordSplatExpression(expr);
            throw new SyntaxError("Expected expression after **");
        }

        if (parser.TryParseToken(TokenType.Operator, "*"))
        {
            IExpression? expr = allowPostfixConditional ? parser.ParseExpression() : ParseCommandArgExpression();
            if (expr != null)
                return new SplatExpression(expr);
            throw new SyntaxError("Expected expression after *");
        }

        if (parser.TryParseToken(TokenType.Separator, "&"))
        {
            if (parser.TryParseToken(TokenType.Separator, ":"))
            {
                Token? nameToken = parser.NextToken();
                if (nameToken == null || nameToken.Type != TokenType.Name)
                    throw new SyntaxError("Expected symbol name after &:");

                IExpression symbolExpr = new ConstantExpression(new Symbol(nameToken.Value));
                IExpression toProcCall = new DotExpression(symbolExpr, "to_proc", []);
                return new BlockArgumentExpression(toProcCall);
            }

            IExpression? expr = allowPostfixConditional ? parser.ParseExpression() : ParseCommandArgExpression();
            if (expr != null)
                return new BlockArgumentExpression(expr);
            throw new SyntaxError("Expected expression after &");
        }

        return allowPostfixConditional ? parser.ParseExpression() : ParseCommandArgExpression();
    }

    /// <summary>
    /// Parses a command argument expression (no assignment)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private IExpression? ParseCommandArgExpression()
    {
        IExpression? expr = parser.ParseNoAssignExpression();
        if (expr == null)
            return null;
        return parser.ApplyPostfixTernary(expr);
    }

    /// <summary>
    /// Parses a list of expressions with support for block arguments
    /// </summary>
    /// <returns></returns>
    public IList<IExpression> ParseExpressionListWithBlockArgs()
    {
        IList<IExpression> expressions = [];

        bool inparentheses = parser.TryParseToken(TokenType.Separator, "(");

        for (IExpression? expression = ParseSingleExpressionWithBlockPrefix(inparentheses); expression != null; expression = ParseSingleExpressionWithBlockPrefix(inparentheses))
        {
            expressions.Add(expression);
            if (!parser.TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
        {
            parser.ParseToken(TokenType.Separator, ")");
            if (parser.TryParseName("do"))
                expressions.Add(parser.ParseBlockExpression());
            else if (parser.TryParseToken(TokenType.Separator, "{"))
                expressions.Add(parser.ParseBlockExpression(true));
        }

        return expressions;
    }

    /// <summary>
    /// Parses a list of expressions until the specified separator is encountered
    /// </summary>
    /// <param name="separator"></param>
    /// <returns></returns>
    public IList<IExpression> ParseExpressionList(string separator)
    {
        IList<IExpression> expressions = [];

        parser.SkipEndOfLines();

        for (IExpression? expression = parser.ParseExpression(); expression != null; expression = parser.ParseExpression())
        {
            expressions.Add(expression);
            if (!parser.TryParseToken(TokenType.Separator, ","))
                break;
            parser.SkipEndOfLines();
        }

        parser.SkipEndOfLines();
        parser.ParseToken(TokenType.Separator, separator);

        return expressions;
    }

    /// <summary>
    /// Parses a block expression
    /// </summary>
    /// <param name="usebraces"></param>
    /// <returns></returns>
    public BlockExpression ParseBlockExpression(bool usebraces = false)
    {
        if (parser.TryParseToken(TokenType.Separator, "|"))
        {
            IList<string> paramnames = ParseParameterList(false);
            parser.ParseToken(TokenType.Separator, "|");
            return new BlockExpression(paramnames, ParseCommandList(usebraces));
        }

        return new BlockExpression(null, ParseCommandList(usebraces));
    }

    /// <summary>
    /// Parses a command list until 'end' or '}' is encountered
    /// </summary>
    /// <param name="usebraces"></param>
    /// <returns></returns>
    public IExpression ParseCommandList(bool usebraces = false)
    {
        Token token;
        IList<IExpression> commands = [];

        for (token = parser.NextToken(); token != null; token = parser.NextToken())
        {
            if (usebraces && token.Type == TokenType.Separator && token.Value == "}")
                break;
            else if (!usebraces && token.Type == TokenType.Name && token.Value == "end")
                break;

            if (parser.IsEndOfCommand(token))
                continue;

            parser.PushToken(token);
            commands.Add(parser.ParseCommand());
        }

        parser.PushToken(token);

        if (usebraces)
            parser.ParseToken(TokenType.Separator, "}");
        else
            parser.ParseName("end");

        if (commands.Count == 1)
            return commands[0];

        return new CompositeExpression(commands);
    }

    /// <summary>
    /// Parses a command list until one of the specified names is encountered
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    public IExpression ParseCommandList(IList<string> names)
    {
        Token token;
        IList<IExpression> commands = [];

        for (token = parser.NextToken(); token != null && (token.Type != TokenType.Name || !names.Contains(token.Value)); token = parser.NextToken())
        {
            if (parser.IsEndOfCommand(token))
                continue;

            parser.PushToken(token);
            commands.Add(parser.ParseCommand());
        }

        parser.PushToken(token);

        if (commands.Count == 1)
            return commands[0];

        return new CompositeExpression(commands);
    }
}

