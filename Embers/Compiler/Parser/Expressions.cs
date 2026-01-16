using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Compiler;

public partial class Parser
{
    /// <summary>
    /// Parses a binary expression at the given precedence level.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private IExpression? ParseBinaryExpression(int level)
    {
        if (level >= binaryoperators.Length)
            return ParseTerm();

        IExpression expr = ParseBinaryExpression(level + 1);

        if (expr == null)
            return null;

        Token token;

        for (token = lexer.NextToken(); token != null && IsBinaryOperator(level, token); token = lexer.NextToken())
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
            if (token.Value == "!=")
                expr = new CompareExpression(expr, ParseBinaryExpression(level + 1), CompareOperator.NotEqual);
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
            lexer.PushToken(token);

        return expr;
    }

    /// <summary>
    /// Determines if a token is a binary operator at the given precedence level.
    /// </summary>
    /// <returns></returns>
    private IExpression? ParseTerm()
    {
        IExpression expression = null;

        if (TryParseToken(TokenType.Operator, "-"))
            expression = new NegativeExpression(ParseTerm());
        else if (TryParseToken(TokenType.Operator, "+"))
            expression = ParseTerm();
        else if (TryParseToken(TokenType.Operator, "!"))
            expression = new NegationExpression(ParseTerm());
        else if (TryParseName("not"))
            expression = new NegationExpression(ParseTerm());
        else
            expression = ParseSimpleTerm();

        if (expression == null)
            return null;

        return ApplyPostfixChain(expression);
    }

    /// <summary>
    /// Applies postfix chains (., ::, []) to an expression.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private IExpression ApplyPostfixChain(IExpression expression)
    {
        while (true)
        {
            if (TryParseToken(TokenType.Separator, "."))
            {
                string name = ParseName();

                if (TryParseToken(TokenType.Separator, "{"))
                    expression = new DotExpression(expression, name, [ParseBlockExpression(true)]);
                else if (NextTokenStartsExpressionList())
                    expression = new DotExpression(expression, name, ParseExpressionListWithBlockArgs());
                else
                    expression = new DotExpression(expression, name, []);

                continue;
            }

            if (TryParseToken(TokenType.Separator, "::"))
            {
                string name = ParseName();

                expression = new DoubleColonExpression(expression, name);

                continue;
            }

            if (TryParseToken(TokenType.Separator, "["))
            {
                IExpression indexexpr = ParseExpression();
                ParseToken(TokenType.Separator, "]");
                expression = new IndexedExpression(expression, indexexpr);

                continue;
            }

            break;
        }

        return expression;
    }

    /// <summary>
    /// Parses a simple term.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private IExpression? ParseSimpleTerm()
    {
        Token token = lexer.NextToken();

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
                return ParseBlockExpression();

            if (token.Value == "if")
                return ParseIfExpression();

            if (token.Value == "while")
                return ParseWhileExpression();

            if (token.Value == "until")
                return ParseUntilExpression();

            if (token.Value == "for")
                return ParseForInExpression();

            if (token.Value == "def")
                return ParseDefExpression();

            if (token.Value == "class")
                return ParseClassExpression();

            if (token.Value == "module")
                return ParseModuleExpression();

            if (token.Value == "begin")
                return ParseTryExpression();

            if (token.Value == "raise")
                return ParseRaiseExpression();

            if (token.Value == "unless")
                return ParseUnlessExpression();

            if (token.Value == "break")
            {
                if (NextTokenStartsExpressionList())
                    return new BreakExpression(ParseExpression());
                return new BreakExpression(null);
            }

            if (token.Value == "return")
            {
                if (NextTokenStartsExpressionList())
                    return new ReturnExpression(ParseExpression());
                return new ReturnExpression(null);
            }

            if (token.Value == "next")
                return new NextExpression();

            if (token.Value == "redo")
                return new RedoExpression();

            if (token.Value == "defined?")
            {
                if (TryParseToken(TokenType.Separator, "("))
                {
                    IExpression inner = ParseExpression();
                    ParseToken(TokenType.Separator, ")");
                    return new DefinedExpression(inner);
                }

                return new DefinedExpression(ParseSimpleTerm());
            }

            //if (token.Value == "alias")
            //{
            //    string newName = ParseName();
            //    string oldName = ParseName();
            //    return new AliasExpression(newName, oldName);
            //}

            if (token.Value == "yield")
            {
                IList<IExpression> args = [];

                if (NextTokenStartsExpressionList())
                {
                    args = ParseExpressionListWithBlockArgs();
                }

                return new YieldExpression(args);
            }

            if (token.Value == "lambda")
            {
                BlockExpression block;
                if (TryParseToken(TokenType.Separator, "{"))
                    block = ParseBlockExpression(true);
                else if (TryParseName("do"))
                    block = ParseBlockExpression();
                else
                    throw new SyntaxError("lambda requires a block");

                return new LambdaExpression(block);
            }

            if (token.Value == "proc")
            {
                BlockExpression block;
                if (TryParseToken(TokenType.Separator, "{"))
                    block = ParseBlockExpression(true);
                else if (TryParseName("do"))
                    block = ParseBlockExpression();
                else
                    throw new SyntaxError("proc requires a block");

                return new LambdaExpression(block);
            }

            return new NameExpression(token.Value);
        }

        // Stabby lambda syntax: ->(params) { body } or -> { body }
        if (token.Type == TokenType.Operator && token.Value == "->")
        {
            IList<string> parameters = [];

            // Check for parameters in parentheses
            if (TryParseToken(TokenType.Separator, "("))
            {
                parameters = ParseParameterList(false);
                ParseToken(TokenType.Separator, ")");
            }

            // Parse the block body
            BlockExpression block;
            if (TryParseToken(TokenType.Separator, "{"))
                block = new BlockExpression(parameters, ParseCommandList(true));
            else if (TryParseName("do"))
                block = new BlockExpression(parameters, ParseCommandList());
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
            IExpression expr = ParseExpression();
            ParseToken(TokenType.Separator, ")");
            return expr;
        }

        if (token.Type == TokenType.Separator && token.Value == "{")
            return ParseHashExpression();

        if (token.Type == TokenType.Separator && token.Value == "[")
        {
            IList<IExpression> expressions = ParseExpressionList("]");
            return new ArrayExpression(expressions);
        }

        lexer.PushToken(token);

        return null;
    }

    /// <summary>
    /// Parses an interpolated string.
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    private IExpression ParseInterpolatedString(string raw)
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
                // Add literal part
                parts.Add(new ConstantExpression(raw[start..i]));
            }

            if (i < raw.Length && raw[i] == '#' && raw[i + 1] == '{')
            {
                i += 2; // skip #{
                int braceCount = 1;
                int exprStart = i;
                while (i < raw.Length && braceCount > 0)
                {
                    if (raw[i] == '{') braceCount++;
                    else if (raw[i] == '}') braceCount--;
                    i++;
                }
                int exprEnd = i - 1; // position of closing }
                string exprText = raw[exprStart..exprEnd];
                // Parse the embedded expression using a new Parser instance
                var exprParser = new Parser(exprText);
                var expr = exprParser.ParseExpression();
                parts.Add(expr);
            }
        }

        return new InterpolatedStringExpression(parts);
    }

    /// <summary>
    /// Applies postfix conditional expressions (ternary, if, unless).
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    private IExpression ApplyPostfixConditional(IExpression expr)
    {
        // --- Ternary Operator ---
        Token? token = lexer.NextToken();

        if (token != null && token.Type == TokenType.Operator && token.Value == "?")
        {
            // Parse the 'true' branch without triggering recursive ternary
            IExpression trueExpr = ParseNoAssignExpression();
            trueExpr = ApplyPostfixesButNotTernary(trueExpr);

            // Expect a colon token
            Token? colonToken = lexer.NextToken();
            if (colonToken == null || colonToken.Type != TokenType.Operator || colonToken.Value != ":")
                throw new SyntaxError("expected ':'");

            // Parse the 'false' branch similarly
            IExpression falseExpr = ParseNoAssignExpression();
            falseExpr = ApplyPostfixesButNotTernary(falseExpr);

            return new TernaryExpression(expr, trueExpr, falseExpr);
        }

        // Push back token if not a ternary
        if (token != null)
            lexer.PushToken(token);

        // --- Postfix conditionals ---
        if (TryParseName("if"))
        {
            IExpression condition = ParseExpression();
            return new IfExpression(condition, expr);
        }
        else if (TryParseName("unless"))
        {
            IExpression condition = ParseExpression();
            return new UnlessExpression(condition, expr, null);
        }

        return expr;
    }

    /// <summary>
    /// Applies postfixes except for the ternary operator.
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    private IExpression ApplyPostfixesButNotTernary(IExpression expr)
    {
        if (TryParseName("if"))
        {
            IExpression condition = ParseExpression();
            return new IfExpression(condition, expr);
        }
        else if (TryParseName("unless"))
        {
            IExpression condition = ParseExpression();
            return new UnlessExpression(condition, expr, null);
        }

        return expr;
    }

    /// <summary>
    /// Parses a compound assignment expression.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="op"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private IExpression ParseCompoundAssignment(IExpression target, string op)
    {
        IExpression rhs = ParseExpression();
        string binOp = op[..^1];

        IExpression left = target switch
        {
            NameExpression name => new NameExpression(name.Name),
            DotExpression dot => new DotExpression(dot.TargetExpression, dot.Name, dot.Arguments),
            InstanceVarExpression ivar => new InstanceVarExpression(ivar.Name),
            ClassVarExpression cvar => new ClassVarExpression(cvar.Name),
            GlobalExpression gvar => new GlobalExpression(gvar.Name),
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
            IndexedExpression idx => new AssignIndexedExpression(idx.Expression, idx.IndexExpression, binary),
            _ => throw new SyntaxError("invalid compound assignment target")
        };
    }

    /// <summary>
    /// Creates a binary expression for the given operator and operands.
    /// </summary>
    /// <param name="op"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private IExpression MakeBinaryExpression(string op, IExpression left, IExpression right) => op switch
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
    /// Parses a hash expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private HashExpression ParseHashExpression()
    {
        IList<IExpression> keyexpressions = [];
        IList<IExpression> valueexpressions = [];

        while (true)
        {
            SkipEndOfLines();

            if (TryParseToken(TokenType.Separator, "}"))
                break;

            if (keyexpressions.Count > 0)
            {
                ParseToken(TokenType.Separator, ",");
                SkipEndOfLines();
            }

            var keyexpression = ParseExpression();
            if (keyexpression == null)
                throw new SyntaxError("hash key expected");

            SkipEndOfLines();

            // Check for modern syntax (name: value) or old syntax (key => value)
            if (TryParseToken(TokenType.Operator, ":"))
            {
                // Modern syntax - convert name to symbol
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
                // Old syntax - requires =>
                ParseToken(TokenType.Operator, "=>");
            }

            SkipEndOfLines();

            var valueexpression = ParseExpression();
            if (valueexpression == null)
                throw new SyntaxError("hash value expected");

            keyexpressions.Add(keyexpression);
            valueexpressions.Add(valueexpression);
        }

        return new HashExpression(keyexpressions, valueexpressions);
    }
}
