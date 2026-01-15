using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Compiler;

/// <summary>
/// the parser for the Embers language
/// </summary>
public class Parser
{
    private static string[][] binaryoperators = [
        ["&&", "||"],
            ["..", "==", "!=", "<", ">", "<=", ">="],
            ["+", "-"],
            ["*", "/", "%"],
            ["**"]
    ];
    private readonly Lexer lexer;

    public Parser(string text)
    {
        lexer = new Lexer(text);
    }

    public Parser(TextReader reader)
    {
        lexer = new Lexer(reader);
    }

    public IExpression ParseExpression()
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


    public IExpression ParseCommand()
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

    private IExpression ParseNoAssignExpression()
    {
        var result = ParseBinaryExpression(0);

        if (result is not NameExpression)
            return result;

        var nexpr = (NameExpression)result;

        if (TryParseToken(TokenType.Separator, "{"))
            return new CallExpression(nexpr.Name, [ParseBlockExpression(true)]);

        if (TryParseName("do"))
            return new CallExpression(nexpr.Name, [ParseBlockExpression()]);

        if (!NextTokenStartsExpressionList())
            return result;

        return new CallExpression(((NameExpression)result).Name, ParseExpressionListWithBlockArgs());
    }

    private IfExpression ParseIfExpression()
    {
        IExpression condition = ParseExpression();
        if (TryParseName("then"))
            TryParseEndOfLine();
        else
            ParseEndOfCommand();

        IExpression thencommand = ParseCommandList(["end", "elsif", "elif", "else"]);
        IExpression elsecommand = null;

        if (TryParseName("elsif"))
            elsecommand = ParseIfExpression();
        else if (TryParseName("elif"))
            elsecommand = ParseIfExpression();
        else if (TryParseName("else"))
            elsecommand = ParseCommandList();
        else
            ParseName("end");

        return new IfExpression(condition, thencommand, elsecommand);
    }

    private ForInExpression ParseForInExpression()
    {
        string name = ParseName();
        ParseName("in");
        IExpression expression = ParseExpression();
        if (TryParseName("do"))
            TryParseEndOfLine();
        else
            ParseEndOfCommand();
        IExpression command = ParseCommandList();
        return new ForInExpression(name, expression, command);
    }

    private WhileExpression ParseWhileExpression()
    {
        IExpression condition = ParseExpression();
        if (TryParseName("do"))
            TryParseEndOfLine();
        else
            ParseEndOfCommand();
        IExpression command = ParseCommandList();
        return new WhileExpression(condition, command);
    }

    private UntilExpression ParseUntilExpression()
    {
        IExpression condition = ParseExpression();
        if (TryParseName("do"))
            TryParseEndOfLine();
        else
            ParseEndOfCommand();
        IExpression command = ParseCommandList();
        return new UntilExpression(condition, command);
    }

    private IExpression ParseDefExpression()
    {
        string name = ParseName();
        INamedExpression named = null;

        if (name == "self")
            named = new SelfExpression();
        else
            named = new NameExpression(name);

        while (true)
        {
            if (TryParseToken(TokenType.Separator, "."))
            {
                string newname = ParseName();
                named = new DotExpression(named, newname, []);
                continue;
            }

            if (TryParseToken(TokenType.Separator, "::"))
            {
                string newname = ParseName();
                named = new DoubleColonExpression(named, newname);
                continue;
            }

            break;
        }

        var (parameters, blockParam) = ParseParameterListWithBlock();
        IExpression body = ParseCommandList();

        return new DefExpression(named, parameters, body, blockParam);
    }

    private ClassExpression ParseClassExpression()
    {
        string name = ParseName();
        INamedExpression named = null;

        if (name == "self")
            named = new SelfExpression();
        else
            named = new NameExpression(name);

        while (true)
        {
            if (TryParseToken(TokenType.Separator, "::"))
            {
                string newname = ParseName();
                named = new DoubleColonExpression(named, newname);
                continue;
            }

            break;
        }

        ParseEndOfCommand();
        IExpression body = ParseCommandList();

        //IExpression body = raw is CompositeExpression ? raw : new CompositeExpression([raw]);

        if (!Predicates.IsConstantName(named.Name))
            throw new SyntaxError("class/module name must be a CONSTANT");

        return new ClassExpression(named, body);
    }

    private ModuleExpression ParseModuleExpression()
    {
        string name = ParseName();
        IExpression body = ParseCommandList();

        if (!Predicates.IsConstantName(name))
            throw new SyntaxError("class/module name must be a CONSTANT");

        return new ModuleExpression(name, body);
    }

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

    private IExpression ParseBinaryExpression(int level)
    {
        if (level >= binaryoperators.Length)
            return ParseTerm();

        IExpression expr = ParseBinaryExpression(level + 1);

        if (expr == null)
            return null;

        Token token;

        for (token = lexer.NextToken(); token != null && IsBinaryOperator(level, token); token = lexer.NextToken())
        {
            if (token.Value == "&&")
                expr = new AndExpression(expr, ParseBinaryExpression(level + 1));
            if (token.Value == "||")
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

    private IExpression ParseTerm()
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

        while (true)
        {
            if (TryParseToken(TokenType.Separator, "."))
            {
                string name = ParseName();

                //IList<IExpression> args = [];

                //if (NextTokenStartsExpressionList())
                //    args = ParseExpressionList();

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

    private IExpression ParseSimpleTerm()
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

    private UnlessExpression ParseUnlessExpression()
    {
        IExpression condition = ParseExpression();
        if (TryParseName("then"))
            TryParseEndOfLine();
        else
            ParseEndOfCommand();

        IExpression thenBlock = ParseCommandList(["end", "else"]);
        IExpression? elseBlock = null;

        if (TryParseName("else"))
            elseBlock = ParseCommandList();
        else
            ParseName("end");

        return new UnlessExpression(condition, thenBlock, elseBlock);
    }

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

    private IExpression ParseTryExpression()
    {
        // Parse the main try block
        IExpression tryBlock = ParseCommandList(["rescue", "ensure", "end"]);

        // Parse zero or more rescue blocks
        var rescueBlocks = new List<(IExpression? ExceptionType, string VarName, IExpression Handler)>();
        while (TryParseName("rescue"))
        {
            IExpression? exceptionType = null;
            string varName = "error";

            // Try to parse an exception type (e.g., rescue ArgumentError)
            Token next = lexer.NextToken();
            if (next != null && next.Type == TokenType.Name && next.Value != "ensure" && next.Value != "rescue" && next.Value != "end")
            {
                lexer.PushToken(next);
                exceptionType = ParseExpression();

                // Try to parse '=>' for variable name (e.g., rescue ArgumentError => ex)
                if (TryParseToken(TokenType.Operator, "=>"))
                    varName = ParseName();
                else
                {
                    // Or just a variable name (legacy: rescue ex)
                    string? possibleName = TryParseName();
                    if (possibleName != null)
                        varName = possibleName;
                }
            }
            else
            {
                lexer.PushToken(next);
                // Try to parse just a variable name (legacy: rescue ex)
                string? possibleName = TryParseName();
                if (possibleName != null)
                    varName = possibleName;
            }

            IExpression handler = ParseCommandList(["rescue", "ensure", "end"]);
            rescueBlocks.Add((exceptionType, varName, handler));
        }

        // Optionally parse ensure block
        IExpression? ensureBlock = null;
        if (TryParseName("ensure"))
        {
            ensureBlock = ParseCommandList(["end"]);
        }

        ParseName("end");

        return new TryExpression(tryBlock, rescueBlocks, ensureBlock);

    }

    private IExpression ParseRaiseExpression()
    {
        // Parse the first argument (exception type or message)
        var first = ParseExpression();
        // Optionally parse a comma and a second argument (message)
        if (TryParseToken(TokenType.Separator, ","))
        {
            var second = ParseExpression();
            return new RaiseExpression(first, second);
        }
        return new RaiseExpression(first, null);
    }

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

    private IExpression ParseCompoundAssignment(IExpression target, string op)
    {
        IExpression rhs = ParseExpression();
        string binOp = op[..^1]; // "+=" → "+"

        IExpression left = target switch
        {
            NameExpression name => new NameExpression(name.Name),
            DotExpression dot => new DotExpression(dot.TargetExpression, dot.Name, dot.Arguments),
            InstanceVarExpression ivar => new InstanceVarExpression(ivar.Name),
            ClassVarExpression cvar => new ClassVarExpression(cvar.Name),
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
            IndexedExpression idx => new AssignIndexedExpression(idx.Expression, idx.IndexExpression, binary),
            _ => throw new SyntaxError("invalid compound assignment target")
        };
    }

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

    private HashExpression ParseHashExpression()
    {
        IList<IExpression> keyexpressions = [];
        IList<IExpression> valueexpressions = [];

        while (!TryParseToken(TokenType.Separator, "}"))
        {
            if (keyexpressions.Count > 0)
                ParseToken(TokenType.Separator, ",");

            var keyexpression = ParseExpression();

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

            var valueexpression = ParseExpression();

            keyexpressions.Add(keyexpression);
            valueexpressions.Add(valueexpression);
        }

        return new HashExpression(keyexpressions, valueexpressions);
    }

    private void ParseName(string name) => ParseToken(TokenType.Name, name);

    private void ParseToken(TokenType type, string value)
    {
        Token token = lexer.NextToken();

        if (token == null || token.Type != type || token.Value != value)
            throw new SyntaxError(string.Format("expected '{0}'", value));
    }

    private string ParseName()
    {
        Token token = lexer.NextToken();

        if (token == null || token.Type != TokenType.Name)
            throw new SyntaxError("name expected");

        return token.Value;
    }

    private bool TryParseName(string name) => TryParseToken(TokenType.Name, name);

    private bool TryParseToken(TokenType type, string value)
    {
        Token token = lexer.NextToken();

        if (token != null && token.Type == type && token.Value == value)
            return true;

        lexer.PushToken(token);

        return false;
    }

    private string TryParseName()
    {
        Token token = lexer.NextToken();

        if (token != null && token.Type == TokenType.Name)
            return token.Value;

        lexer.PushToken(token);

        return null;
    }

    private bool TryParseEndOfLine()
    {
        Token token = lexer.NextToken();

        if (token != null && token.Type == TokenType.EndOfLine && token.Value == "\n")
            return true;

        lexer.PushToken(token);

        return false;
    }

    private bool IsBinaryOperator(int level, Token token) => token.Type == TokenType.Operator && binaryoperators[level].Contains(token.Value);
}

