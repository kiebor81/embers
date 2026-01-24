using Embers.Exceptions;
using Embers.Expressions;

namespace Embers.Compiler.Parsing.Components;

/// <summary>
/// Parsing components for expressions
/// </summary>
/// <param name="parser"></param>
internal sealed class Expressions(Parser parser)
{
    private readonly Parser parser = parser;

    /// <summary>
    /// Parses a no-assignment expression
    /// </summary>
    /// <returns></returns>
    public IExpression? ParseNoAssignExpression()
    {
        var result = ParseBinaryExpression(0);

        if (result == null)
            return null;

        if (result is not NameExpression)
            return ApplyPostfixChain(result);

        var nexpr = (NameExpression)result;

        if (parser.TryParseToken(TokenType.Separator, "{"))
            return ApplyPostfixChain(new CallExpression(nexpr.Name, [parser.ParseBlockExpression(true)]));

        if (parser.TryParseName("do"))
            return ApplyPostfixChain(new CallExpression(nexpr.Name, [parser.ParseBlockExpression()]));

        if (!parser.NextTokenStartsExpressionListAllowSplat())
            return ApplyPostfixChain(result);

        return ApplyPostfixChain(new CallExpression(((NameExpression)result).Name, parser.ParseExpressionListWithBlockArgs()));
    }

    /// <summary>
    /// Parses a binary expression
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public IExpression? ParseBinaryExpression(int level)
    {
        if (level >= Parser.BinaryOperators.Length)
            return ParseTerm();

        IExpression expr = ParseBinaryExpression(level + 1);

        if (expr == null)
            return null;

        Token token;

        for (token = parser.NextToken(); token != null && parser.IsBinaryOperator(level, token); token = parser.NextToken())
        {
            if (token.Value == "&&" || token.Value == "and")
                expr = new AndExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "||" || token.Value == "or")
                expr = new OrExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "+")
                expr = new AddExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "-")
                expr = new SubtractExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "*")
                expr = new MultiplyExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "/")
                expr = new DivideExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "==")
                expr = new CompareExpression(expr, ParseBinaryExpression(level + 1), CompareOperator.Equal);
            if (token.Value == "===")
                throw new InvalidOperationError("=== is only supported in case matching");
            if (token.Value == "!=")
                expr = new CompareExpression(expr, ParseBinaryExpression(level + 1), CompareOperator.NotEqual);
            if (token.Value == "<=>")
                expr = new CompareThreeWayExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "<")
                expr = new CompareExpression(expr, ParseBinaryExpression(level + 1), CompareOperator.Less);
            if (token.Value == ">")
                expr = new CompareExpression(expr, ParseBinaryExpression(level + 1), CompareOperator.Greater);
            if (token.Value == "<=")
                expr = new CompareExpression(expr, ParseBinaryExpression(level + 1), CompareOperator.LessOrEqual);
            if (token.Value == ">=")
                expr = new CompareExpression(expr, ParseBinaryExpression(level + 1), CompareOperator.GreaterOrEqual);
            if (token.Value == "..")
                expr = new RangeExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "%")
                expr = new ModuloExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "**")
                expr = new PowerExpression(expr, ParseBinaryExpression(level + 1));
        }

        if (token != null)
            parser.PushToken(token);

        return expr;
    }

    /// <summary>
    /// Parses a term
    /// </summary>
    /// <returns></returns>
    public IExpression? ParseTerm()
    {
        IExpression expression = null;

        if (parser.TryParseToken(TokenType.Operator, "-"))
            expression = new NegativeExpression(ParseTerm());
        else if (parser.TryParseToken(TokenType.Operator, "+"))
            expression = ParseTerm();
        else if (parser.TryParseToken(TokenType.Operator, "!"))
            expression = new NegationExpression(ParseTerm());
        else if (parser.TryParseName("not"))
            expression = new NegationExpression(ParseTerm());
        else
            expression = parser.PrimaryParser.ParseSimpleTerm();

        if (expression == null)
            return null;

        return ApplyPostfixChain(expression);
    }

    /// <summary>
    /// Applies postfix chaining (. :: [])
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public IExpression ApplyPostfixChain(IExpression expression)
    {
        while (true)
        {
            if (parser.TryParseToken(TokenType.Separator, "."))
            {
                string name = parser.ParseName();

                if (parser.TryParseToken(TokenType.Separator, "{"))
                    expression = new DotExpression(expression, name, [parser.ParseBlockExpression(true)]);
                else if (parser.NextTokenStartsExpressionListAllowSplat())
                    expression = new DotExpression(expression, name, parser.ParseExpressionListWithBlockArgs());
                else
                    expression = new DotExpression(expression, name, []);

                continue;
            }

            if (parser.TryParseToken(TokenType.Separator, "::"))
            {
                string name = parser.ParseName();

                expression = new DoubleColonExpression(expression, name);

                continue;
            }

            if (parser.TryParseToken(TokenType.Separator, "["))
            {
                if (parser.TryParseToken(TokenType.Separator, "]"))
                {
                    expression = new IndexedExpression(expression, null);
                }
                else
                {
                    IExpression indexexpr = parser.ParseExpression();
                    parser.ParseToken(TokenType.Separator, "]");
                    expression = new IndexedExpression(expression, indexexpr);
                }

                continue;
            }

            break;
        }

        return expression;
    }

    /// <summary>
    /// Applies postfix conditional (ternary, if, unless)
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression ApplyPostfixConditional(IExpression expr)
    {
        Token? token = parser.NextToken();

        if (token != null && token.Type == TokenType.Operator && token.Value == "?")
        {
            IExpression trueExpr = ParseNoAssignExpression();
            trueExpr = ApplyPostfixesButNotTernary(trueExpr);

            Token? colonToken = parser.NextToken();
            if (colonToken == null || colonToken.Type != TokenType.Operator || colonToken.Value != ":")
                throw new SyntaxError("expected ':'");

            IExpression falseExpr = ParseNoAssignExpression();
            falseExpr = ApplyPostfixesButNotTernary(falseExpr);

            return new TernaryExpression(expr, trueExpr, falseExpr);
        }

        if (token != null)
            parser.PushToken(token);

        if (parser.TryParseName("if"))
        {
            IExpression condition = parser.ParseExpression();
            return new IfExpression(condition, expr);
        }
        else if (parser.TryParseName("unless"))
        {
            IExpression condition = parser.ParseExpression();
            return new UnlessExpression(condition, expr, null);
        }

        return expr;
    }

    /// <summary>
    /// Applies postfix ternary (? :) only.
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression ApplyPostfixTernary(IExpression expr)
    {
        Token? token = parser.NextToken();

        if (token != null && token.Type == TokenType.Operator && token.Value == "?")
        {
            IExpression trueExpr = ParseNoAssignExpression();
            trueExpr = ApplyPostfixesButNotTernary(trueExpr);

            Token? colonToken = parser.NextToken();
            if (colonToken == null || colonToken.Type != TokenType.Operator || colonToken.Value != ":")
                throw new SyntaxError("expected ':'");

            IExpression falseExpr = ParseNoAssignExpression();
            falseExpr = ApplyPostfixesButNotTernary(falseExpr);

            return new TernaryExpression(expr, trueExpr, falseExpr);
        }

        if (token != null)
            parser.PushToken(token);

        return expr;
    }

    /// <summary>
    /// Applies postfixes except for ternary
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public IExpression ApplyPostfixesButNotTernary(IExpression expr)
    {
        if (parser.TryParseName("if"))
        {
            IExpression condition = parser.ParseExpression();
            return new IfExpression(condition, expr);
        }
        else if (parser.TryParseName("unless"))
        {
            IExpression condition = parser.ParseExpression();
            return new UnlessExpression(condition, expr, null);
        }

        return expr;
    }

    /// <summary>
    /// Parses a compound assignment expression
    /// </summary>
    /// <param name="target"></param>
    /// <param name="op"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression ParseCompoundAssignment(IExpression target, string op)
    {
        IExpression rhs = parser.ParseExpression();
        string binOp = op[..^1];

        IExpression left = target switch
        {
            NameExpression name => new NameExpression(name.Name),
            DotExpression dot => new DotExpression(dot.TargetExpression, dot.Name, dot.Arguments),
            InstanceVarExpression ivar => new InstanceVarExpression(ivar.Name),
            ClassVarExpression cvar => new ClassVarExpression(cvar.Name),
            GlobalExpression gvar => new GlobalExpression(gvar.Name),
            IndexedExpression idx when idx.IndexExpression == null => throw new SyntaxError("empty index is not assignable"),
            IndexedExpression idx => new IndexedExpression(idx.Expression, idx.IndexExpression),
            _ => throw new SyntaxError("invalid compound assignment target")
        };

        IExpression binary = MakeBinaryExpression(binOp, left, rhs);

        return target switch
        {
            NameExpression name => new AssignExpression(name.Name, binary),
            DotExpression dot => new AssignDotExpressions(dot, binary),
            InstanceVarExpression ivar => new AssignInstanceVarExpression(ivar.Name, binary),
            ClassVarExpression cvar => new AssignClassVarExpression(cvar.Name, binary),
            GlobalExpression gvar => new AssignGlobalVarExpression(gvar.Name, binary),
            IndexedExpression idx when idx.IndexExpression == null => throw new SyntaxError("empty index is not assignable"),
            IndexedExpression idx => new AssignIndexedExpression(idx.Expression, idx.IndexExpression, binary),
            _ => throw new SyntaxError("invalid compound assignment target")
        };
    }

    /// <summary>
    /// Makes a binary expression for the specified operator
    /// </summary>
    /// <param name="op"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression MakeBinaryExpression(string op, IExpression left, IExpression right) => op switch
    {
        "+" => new AddExpression(left, right),
        "-" => new SubtractExpression(left, right),
        "*" => new MultiplyExpression(left, right),
        "/" => new DivideExpression(left, right),
        "%" => new ModuloExpression(left, right),
        "**" => new PowerExpression(left, right),
        _ => throw new SyntaxError($"unsupported compound assignment operator: {op}")
    };

    /// <summary>
    /// Parses a hash expression
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public HashExpression ParseHashExpression()
    {
        IList<IExpression> keyexpressions = [];
        IList<IExpression> valueexpressions = [];

        while (true)
        {
            parser.SkipEndOfLines();

            if (parser.TryParseToken(TokenType.Separator, "}"))
                break;

            if (keyexpressions.Count > 0)
            {
                parser.ParseToken(TokenType.Separator, ",");
                parser.SkipEndOfLines();
            }

            var keyexpression = parser.ParseExpression() ?? throw new SyntaxError("hash key expected");
            parser.SkipEndOfLines();

            if (parser.TryParseToken(TokenType.Operator, ":"))
            {
                if (keyexpression is NameExpression nameExpr)
                {
                    keyexpression = new ConstantExpression(new Symbol(nameExpr.Name));
                }
                else
                {
                    throw new SyntaxError("modern hash syntax (key:) only supports symbol keys");
                }
            }
            else
            {
                parser.ParseToken(TokenType.Operator, "=>");
            }

            parser.SkipEndOfLines();

            var valueexpression = parser.ParseExpression() ?? throw new SyntaxError("hash value expected");
            keyexpressions.Add(keyexpression);
            valueexpressions.Add(valueexpression);
        }

        return new HashExpression(keyexpressions, valueexpressions);
    }
}

