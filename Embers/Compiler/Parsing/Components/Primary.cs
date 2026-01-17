using Embers.Exceptions;
using Embers.Expressions;

namespace Embers.Compiler.Parsing.Components;

/// <summary>
/// Parsing components for primary expressions
/// </summary>
/// <param name="parser"></param>
internal sealed class Primary(Parser parser)
{
    private readonly Parser parser = parser;

    /// <summary>
    /// Parses a simple term
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression? ParseSimpleTerm()
    {
        Token token = parser.Lexer.NextToken();

        if (token == null)
            return null;

        if (token.Type == TokenType.Integer)
            return new ConstantExpression(long.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture));

        if (token.Type == TokenType.Real)
            return new ConstantExpression(double.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture));

        if (token.Type == TokenType.String)
            return new ConstantExpression(token.Value);

        if (token.Type == TokenType.InterpolatedString)
            return ParseInterpolatedString(token.Value);

        if (token.Type == TokenType.Name)
        {
            if (token.Value == "false")
                return new ConstantExpression(false);

            if (token.Value == "true")
                return new ConstantExpression(true);

            if (token.Value == "self")
                return new SelfExpression();

            if (token.Value == "nil")
                return new ConstantExpression(null);

            if (token.Value == "do")
                return parser.ParseBlockExpression();

            if (token.Value == "if")
                return parser.ParseIfExpression();

            if (token.Value == "while")
                return parser.ParseWhileExpression();

            if (token.Value == "until")
                return parser.ParseUntilExpression();

            if (token.Value == "for")
                return parser.ParseForInExpression();

            if (token.Value == "def")
                return parser.ParseDefExpression();

            if (token.Value == "class")
                return parser.ParseClassExpression();

            if (token.Value == "module")
                return parser.ParseModuleExpression();

            if (token.Value == "begin")
                return parser.ParseTryExpression();

            if (token.Value == "raise")
                return parser.ParseRaiseExpression();

            if (token.Value == "unless")
                return parser.ParseUnlessExpression();

            if (token.Value == "break")
            {
                if (parser.NextTokenStartsExpressionList())
                    return new BreakExpression(parser.ParseExpression());
                return new BreakExpression(null);
            }

            if (token.Value == "return")
            {
                if (parser.NextTokenStartsExpressionList())
                    return new ReturnExpression(parser.ParseExpression());
                return new ReturnExpression(null);
            }

            if (token.Value == "next")
                return new NextExpression();

            if (token.Value == "redo")
                return new RedoExpression();

            if (token.Value == "defined?")
            {
                if (parser.TryParseToken(TokenType.Separator, "("))
                {
                    IExpression inner = parser.ParseExpression();
                    parser.ParseToken(TokenType.Separator, ")");
                    return new DefinedExpression(inner);
                }

                return new DefinedExpression(ParseSimpleTerm());
            }

            if (token.Value == "yield")
            {
                IList<IExpression> args = [];

                if (parser.NextTokenStartsExpressionList())
                {
                    args = parser.ParseExpressionListWithBlockArgs();
                }

                return new YieldExpression(args);
            }

            if (token.Value == "lambda")
            {
                BlockExpression block;
                if (parser.TryParseToken(TokenType.Separator, "{"))
                    block = parser.ParseBlockExpression(true);
                else if (parser.TryParseName("do"))
                    block = parser.ParseBlockExpression();
                else
                    throw new SyntaxError("lambda requires a block");

                return new LambdaExpression(block);
            }

            if (token.Value == "proc")
            {
                BlockExpression block;
                if (parser.TryParseToken(TokenType.Separator, "{"))
                    block = parser.ParseBlockExpression(true);
                else if (parser.TryParseName("do"))
                    block = parser.ParseBlockExpression();
                else
                    throw new SyntaxError("proc requires a block");

                return new LambdaExpression(block);
            }

            return new NameExpression(token.Value);
        }

        if (token.Type == TokenType.Operator && token.Value == "->")
        {
            IList<string> parameters = [];

            if (parser.TryParseToken(TokenType.Separator, "("))
            {
                parameters = parser.ParseParameterList(false);
                parser.ParseToken(TokenType.Separator, ")");
            }

            BlockExpression block;
            if (parser.TryParseToken(TokenType.Separator, "{"))
                block = new BlockExpression(parameters, parser.ParseCommandList(true));
            else if (parser.TryParseName("do"))
                block = new BlockExpression(parameters, parser.ParseCommandList());
            else
                throw new SyntaxError("stabby lambda requires a block");

            return new LambdaExpression(block);
        }

        if (token.Type == TokenType.InstanceVarName)
            return new InstanceVarExpression(token.Value);

        if (token.Type == TokenType.ClassVarName)
            return new ClassVarExpression(token.Value);

        if (token.Type == TokenType.GlobalVarName)
            return new GlobalExpression(token.Value);

        if (token.Type == TokenType.Symbol)
            return new ConstantExpression(new Symbol(token.Value));

        if (token.Type == TokenType.Separator && token.Value == "(")
        {
            IExpression expr = parser.ParseExpression();
            parser.ParseToken(TokenType.Separator, ")");
            return expr;
        }

        if (token.Type == TokenType.Separator && token.Value == "{")
            return parser.ParseHashExpression();

        if (token.Type == TokenType.Separator && token.Value == "[")
        {
            IList<IExpression> expressions = parser.ParseExpressionList("]");
            return new ArrayExpression(expressions);
        }

        parser.Lexer.PushToken(token);

        return null;
    }

    /// <summary>
    /// Parses an interpolated string into its component expressions
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    public IExpression ParseInterpolatedString(string raw)
    {
        var parts = new List<IExpression>();

        int i = 0;
        while (i < raw.Length)
        {
            int start = i;
            while (i < raw.Length && !(raw[i] == '#' && i + 1 < raw.Length && raw[i + 1] == '{'))
                i++;

            if (i > start)
            {
                parts.Add(new ConstantExpression(raw[start..i]));
            }

            if (i < raw.Length && raw[i] == '#' && raw[i + 1] == '{')
            {
                i += 2;
                int braceCount = 1;
                int exprStart = i;
                while (i < raw.Length && braceCount > 0)
                {
                    if (raw[i] == '{') braceCount++;
                    else if (raw[i] == '}') braceCount--;
                    i++;
                }
                int exprEnd = i - 1;
                string exprText = raw[exprStart..exprEnd];
                var exprParser = new Parser(exprText);
                var expr = exprParser.ParseExpression();
                parts.Add(expr);
            }
        }

        return new InterpolatedStringExpression(parts);
    }
}
