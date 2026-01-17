using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

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

        for (string name = parser.TryParseName(); name != null; name = parser.ParseName())
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
    public (IList<string> parameters, string? blockParam) ParseParameterListWithBlock(bool canhaveparens = true)
    {
        IList<string> parameters = [];
        string? blockParam = null;

        bool inparentheses = false;

        if (canhaveparens)
            inparentheses = parser.TryParseToken(TokenType.Separator, "(");

        while (true)
        {
            if (parser.TryParseToken(TokenType.Separator, "&"))
            {
                string blockParamName = parser.ParseName();
                blockParam = blockParamName;
                break;
            }

            string name = parser.TryParseName();
            if (name == null)
                break;

            parameters.Add(name);
            if (!parser.TryParseToken(TokenType.Separator, ","))
                break;
        }

        if (inparentheses)
            parser.ParseToken(TokenType.Separator, ")");

        return (parameters, blockParam);
    }

    /// <summary>
    /// Parses a list of expressions
    /// </summary>
    /// <returns></returns>
    public IList<IExpression> ParseExpressionList()
    {
        IList<IExpression> expressions = [];

        bool inparentheses = parser.TryParseToken(TokenType.Separator, "(");

        for (IExpression expression = parser.ParseExpression(); expression != null; expression = parser.ParseExpression())
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
        if (parser.TryParseToken(TokenType.Separator, "&"))
        {
            if (parser.TryParseToken(TokenType.Separator, ":"))
            {
                Token? nameToken = parser.Lexer.NextToken();
                if (nameToken == null || nameToken.Type != TokenType.Name)
                    throw new SyntaxError("Expected symbol name after &:");

                IExpression symbolExpr = new ConstantExpression(new Symbol(nameToken.Value));
                IExpression toProcCall = new DotExpression(symbolExpr, "to_proc", []);
                return new BlockArgumentExpression(toProcCall);
            }

            IExpression expr = parser.ParseExpression();
            if (expr != null)
                return new BlockArgumentExpression(expr);
            throw new SyntaxError("Expected expression after &");
        }

        return parser.ParseExpression();
    }

    /// <summary>
    /// Parses a list of expressions with support for block arguments
    /// </summary>
    /// <returns></returns>
    public IList<IExpression> ParseExpressionListWithBlockArgs()
    {
        IList<IExpression> expressions = [];

        bool inparentheses = parser.TryParseToken(TokenType.Separator, "(");

        for (IExpression? expression = ParseSingleExpressionWithBlockPrefix(); expression != null; expression = ParseSingleExpressionWithBlockPrefix())
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

        for (IExpression expression = parser.ParseExpression(); expression != null; expression = parser.ParseExpression())
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

        for (token = parser.Lexer.NextToken(); token != null; token = parser.Lexer.NextToken())
        {
            if (usebraces && token.Type == TokenType.Separator && token.Value == "}")
                break;
            else if (!usebraces && token.Type == TokenType.Name && token.Value == "end")
                break;

            if (parser.IsEndOfCommand(token))
                continue;

            parser.Lexer.PushToken(token);
            commands.Add(parser.ParseCommand());
        }

        parser.Lexer.PushToken(token);

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

        for (token = parser.Lexer.NextToken(); token != null && (token.Type != TokenType.Name || !names.Contains(token.Value)); token = parser.Lexer.NextToken())
        {
            if (parser.IsEndOfCommand(token))
                continue;

            parser.Lexer.PushToken(token);
            commands.Add(parser.ParseCommand());
        }

        parser.Lexer.PushToken(token);

        if (commands.Count == 1)
            return commands[0];

        return new CompositeExpression(commands);
    }
}
