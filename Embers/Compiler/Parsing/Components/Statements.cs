using Embers.Exceptions;
using Embers.Expressions;

namespace Embers.Compiler.Parsing.Components;

/// <summary>
/// Parsing components for statements
/// </summary>
/// <param name="parser"></param>
internal sealed class Statements(Parser parser)
{
    private readonly Parser parser = parser;

    /// <summary>
    /// Parses an if expression
    /// </summary>
    /// <returns></returns>
    public IfExpression ParseIfExpression()
    {
        IExpression condition = parser.ParseExpression();
        if (parser.TryParseName("then"))
            parser.TryParseEndOfLine();
        else
            parser.ParseEndOfCommand();

        IExpression thencommand = parser.ParseCommandList(["end", "elsif", "elif", "else"]);
        IExpression elsecommand = null;

        if (parser.TryParseName("elsif"))
            elsecommand = ParseIfExpression();
        else if (parser.TryParseName("elif"))
            elsecommand = ParseIfExpression();
        else if (parser.TryParseName("else"))
            elsecommand = parser.ParseCommandList();
        else
            parser.ParseName("end");

        return new IfExpression(condition, thencommand, elsecommand);
    }

    /// <summary>
    /// Parses a for-in expression
    /// </summary>
    /// <returns></returns>
    public ForInExpression ParseForInExpression()
    {
        string name = parser.ParseName();
        parser.ParseName("in");
        IExpression expression = parser.ParseExpression();
        if (parser.TryParseName("do"))
            parser.TryParseEndOfLine();
        else
            parser.ParseEndOfCommand();
        IExpression command = parser.ParseCommandList();
        return new ForInExpression(name, expression, command);
    }

    /// <summary>
    /// Parses a while expression
    /// </summary>
    /// <returns></returns>
    public WhileExpression ParseWhileExpression()
    {
        IExpression condition = parser.ParseExpression();
        if (parser.TryParseName("do"))
            parser.TryParseEndOfLine();
        else
            parser.ParseEndOfCommand();
        IExpression command = parser.ParseCommandList();
        return new WhileExpression(condition, command);
    }

    /// <summary>
    /// Parses an until expression
    /// </summary>
    /// <returns></returns>
    public UntilExpression ParseUntilExpression()
    {
        IExpression condition = parser.ParseExpression();
        if (parser.TryParseName("do"))
            parser.TryParseEndOfLine();
        else
            parser.ParseEndOfCommand();
        IExpression command = parser.ParseCommandList();
        return new UntilExpression(condition, command);
    }

    /// <summary>
    /// Parses a def expression
    /// </summary>
    /// <returns></returns>
    public IExpression ParseDefExpression()
    {
        string name = ParseDefName();
        INamedExpression named = null;

        if (name == "self")
            named = new SelfExpression();
        else
            named = new NameExpression(name);

        while (true)
        {
            if (parser.TryParseToken(TokenType.Separator, "."))
            {
                string newname = parser.ParseName();
                named = new DotExpression(named, newname, []);
                continue;
            }

            if (parser.TryParseToken(TokenType.Separator, "::"))
            {
                string newname = parser.ParseName();
                named = new DoubleColonExpression(named, newname);
                continue;
            }

            break;
        }

        var (parameters, splatParam, kwParam, blockParam) = parser.ParseParameterListWithBlock();
        IExpression body = parser.ParseCommandList();

        return new DefExpression(named, parameters, splatParam, kwParam, body, blockParam);
    }

    /// <summary>
    /// Parses a def name
    /// </summary>
    private string ParseDefName()
    {
        Token token = parser.Lexer.NextToken() ?? throw new SyntaxError("name expected");
        if (token.Type == TokenType.Name || token.Type == TokenType.Operator)
            return token.Value;

        throw new SyntaxError("name expected");
    }

    /// <summary>
    /// Parses a class expression
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public ClassExpression ParseClassExpression()
    {
        INamedExpression named = ParseNamedExpression();
        INamedExpression? superClass = null;

        if (parser.TryParseToken(TokenType.Operator, "<"))
            superClass = ParseNamedExpression();

        parser.ParseEndOfCommand();
        IExpression body = parser.ParseCommandList();

        if (!Predicates.IsConstantName(named.Name))
            throw new SyntaxError("class/module name must be a CONSTANT");

        if (superClass != null && !Predicates.IsConstantName(superClass.Name))
            throw new SyntaxError("superclass name must be a CONSTANT");

        return new ClassExpression(named, body, superClass);
    }

    /// <summary>
    /// Parses a module expression
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public ModuleExpression ParseModuleExpression()
    {
        string name = parser.ParseName();
        IExpression body = parser.ParseCommandList();

        if (!Predicates.IsConstantName(name))
            throw new SyntaxError("class/module name must be a CONSTANT");

        return new ModuleExpression(name, body);
    }

    private INamedExpression ParseNamedExpression()
    {
        string name = parser.ParseName();
        INamedExpression named = name == "self"
            ? new SelfExpression()
            : new NameExpression(name);

        while (true)
        {
            if (!parser.TryParseToken(TokenType.Separator, "::"))
                break;

            string newname = parser.ParseName();
            named = new DoubleColonExpression(named, newname);
        }

        return named;
    }

    /// <summary>
    /// Parses an unless expression
    /// </summary>
    /// <returns></returns>
    public UnlessExpression ParseUnlessExpression()
    {
        IExpression condition = parser.ParseExpression();
        if (parser.TryParseName("then"))
            parser.TryParseEndOfLine();
        else
            parser.ParseEndOfCommand();

        IExpression thenBlock = parser.ParseCommandList(["end", "else"]);
        IExpression? elseBlock = null;

        if (parser.TryParseName("else"))
            elseBlock = parser.ParseCommandList();
        else
            parser.ParseName("end");

        return new UnlessExpression(condition, thenBlock, elseBlock);
    }

    /// <summary>
    /// Parses a case expression.
    /// </summary>
    public CaseExpression ParseCaseExpression()
    {
        IExpression? subject = null;
        Token token = parser.Lexer.NextToken() ?? throw new SyntaxError("unexpected end of input");
        bool hasSubject = true;

        if (parser.IsEndOfCommand(token))
        {
            hasSubject = false;
        }
        else if (token.Type == TokenType.Name && token.Value == "when")
        {
            hasSubject = false;
            parser.Lexer.PushToken(token);
        }
        else
        {
            parser.Lexer.PushToken(token);
        }

        if (hasSubject)
        {
            subject = parser.ParseExpression();
            parser.ParseEndOfCommand();
        }

        var clauses = new List<ICaseClause>();
        bool? isInClause = null;

        while (true)
        {
            if (parser.TryParseName("when"))
            {
                if (isInClause == true)
                    throw new SyntaxError("case cannot mix when and in clauses");

                isInClause = false;

                var patterns = new List<ICasePattern>();

                var first = parser.ParseExpression() ?? throw new SyntaxError("expression expected");
                patterns.Add(new ExpressionPattern(first));

                while (parser.TryParseToken(TokenType.Separator, ","))
                {
                    var next = parser.ParseExpression() ?? throw new SyntaxError("expression expected");
                    patterns.Add(new ExpressionPattern(next));
                }

                if (parser.TryParseName("then"))
                    parser.TryParseEndOfLine();
                else
                    parser.ParseEndOfCommand();

                var body = parser.ParseCommandList(["when", "else", "end"]);
                clauses.Add(new CaseWhenClause(patterns, body));
                continue;
            }

            if (parser.TryParseName("in"))
            {
                if (isInClause == false)
                    throw new SyntaxError("case cannot mix when and in clauses");

                isInClause = true;

                var pattern = ParseCasePattern();

                if (parser.TryParseName("then"))
                    parser.TryParseEndOfLine();
                else
                    parser.ParseEndOfCommand();

                var body = parser.ParseCommandList(["in", "else", "end"]);
                clauses.Add(new CaseInClause(pattern, body));
                continue;
            }

            break;
        }

        if (clauses.Count == 0)
            throw new SyntaxError("case requires at least one when or in clause");

        IExpression? elseExpression = null;
        if (parser.TryParseName("else"))
            elseExpression = parser.ParseCommandList(["end"]);

        parser.ParseName("end");

        return new CaseExpression(subject, clauses, elseExpression);
    }

    /// <summary>
    /// Parses a case pattern.
    /// </summary>
    private ICasePattern ParseCasePattern()
    {
        Token token = parser.Lexer.NextToken() ?? throw new SyntaxError("pattern expected");
        if (token.Type == TokenType.Separator && token.Value == "{")
            return ParseHashPattern(true);

        if (token.Type == TokenType.Name)
        {
            Token next = parser.Lexer.NextToken();
            if (next != null && next.Type == TokenType.Operator && next.Value == ":")
            {
                parser.Lexer.PushToken(next);
                parser.Lexer.PushToken(token);
                return ParseHashPattern(false);
            }

            parser.Lexer.PushToken(next);
            parser.Lexer.PushToken(token);
        }
        else
        {
            parser.Lexer.PushToken(token);
        }

        var expr = parser.ParseExpression() ?? throw new SyntaxError("pattern expected");
        return new ExpressionPattern(expr);
    }

    /// <summary>
    /// Parses a hash pattern.
    /// </summary>
    private HashPattern ParseHashPattern(bool hasBraces)
    {
        var entries = new List<HashPatternEntry>();

        while (true)
        {
            parser.SkipEndOfLines();

            if (hasBraces && parser.TryParseToken(TokenType.Separator, "}"))
                break;

            if (!hasBraces)
            {
                Token? peek = parser.Lexer.NextToken();
                if (IsPatternTerminator(peek))
                {
                    parser.Lexer.PushToken(peek);
                    break;
                }
                parser.Lexer.PushToken(peek);
            }

            entries.Add(ParseHashPatternEntry());
            parser.SkipEndOfLines();

            if (hasBraces)
            {
                if (parser.TryParseToken(TokenType.Separator, "}"))
                    break;

                parser.ParseToken(TokenType.Separator, ",");
            }
            else
            {
                if (!parser.TryParseToken(TokenType.Separator, ","))
                {
                    Token? peek = parser.Lexer.NextToken();
                    if (IsPatternTerminator(peek))
                    {
                        parser.Lexer.PushToken(peek);
                        break;
                    }

                    parser.Lexer.PushToken(peek);
                    throw new SyntaxError("expected ','");
                }
            }
        }

        return new HashPattern(entries);
    }

    /// <summary>
    /// Parses a hash pattern entry.
    /// </summary>
    private HashPatternEntry ParseHashPatternEntry()
    {
        Token token = parser.Lexer.NextToken() ?? throw new SyntaxError("pattern key expected");
        string? name = token.Type switch
        {
            TokenType.Name => token.Value,
            TokenType.Symbol => token.Value,
            _ => null
        } ?? throw new SyntaxError("pattern key expected");
        IExpression key = new ConstantExpression(new Symbol(name));

        if (parser.TryParseToken(TokenType.Operator, ":"))
        {
            var valuePattern = ParseHashValuePattern(name);
            return new HashPatternEntry(key, valuePattern);
        }

        parser.ParseToken(TokenType.Operator, "=>");
        var pattern = ParseCasePattern();
        return new HashPatternEntry(key, pattern);
    }

    /// <summary>
    /// Parses a hash value pattern
    /// </summary>
    private ICasePattern ParseHashValuePattern(string bindingName)
    {
        Token? token = parser.Lexer.NextToken();
        if (token == null || IsPatternValueTerminator(token))
        {
            if (token != null)
                parser.Lexer.PushToken(token);
            return new BindingPattern(bindingName);
        }

        if (token.Type == TokenType.Separator && token.Value == "{")
            return ParseHashPattern(true);

        if (token.Type == TokenType.Name)
        {
            Token? next = parser.Lexer.NextToken();
            if (next != null && next.Type == TokenType.Operator && next.Value == ":")
            {
                parser.Lexer.PushToken(next);
                parser.Lexer.PushToken(token);
                return ParseHashPattern(false);
            }

            parser.Lexer.PushToken(next);
        }

        parser.Lexer.PushToken(token);
        var expr = parser.ParseExpression() ?? throw new SyntaxError("pattern expected");
        return new ExpressionPattern(expr);
    }

    /// <summary>
    /// Determines if a token is a pattern terminator
    /// </summary>
    private bool IsPatternTerminator(Token? token)
    {
        if (token == null)
            return true;

        if (parser.IsEndOfCommand(token))
            return true;

        return token.Type == TokenType.Name
            && (token.Value == "then" || token.Value == "else" || token.Value == "end");
    }

    /// <summary>
    /// Determines if a token is a pattern value terminator
    /// </summary>
    private bool IsPatternValueTerminator(Token token)
    {
        if (IsPatternTerminator(token))
            return true;

        return token.Type == TokenType.Separator
            && (token.Value == "," || token.Value == "}");
    }

    /// <summary>
    /// Parses a try expression
    /// </summary>
    /// <returns></returns>
    public IExpression ParseTryExpression()
    {
        IExpression tryBlock = parser.ParseCommandList(["rescue", "ensure", "end"]);

        var rescueBlocks = new List<(IExpression? ExceptionType, string VarName, IExpression Handler)>();
        while (parser.TryParseName("rescue"))
        {
            IExpression? exceptionType = null;
            string varName = "error";

            Token next = parser.Lexer.NextToken();
            if (next != null && next.Type == TokenType.Name && next.Value != "ensure" && next.Value != "rescue" && next.Value != "end")
            {
                parser.Lexer.PushToken(next);
                exceptionType = parser.ParseExpression();

                if (parser.TryParseToken(TokenType.Operator, "=>"))
                    varName = parser.ParseName();
                else
                {
                    string? possibleName = parser.TryParseName();
                    if (possibleName != null)
                        varName = possibleName;
                }
            }
            else
            {
                parser.Lexer.PushToken(next);
                string? possibleName = parser.TryParseName();
                if (possibleName != null)
                    varName = possibleName;
            }

            IExpression handler = parser.ParseCommandList(["rescue", "ensure", "end"]);
            rescueBlocks.Add((exceptionType, varName, handler));
        }

        IExpression? ensureBlock = null;
        if (parser.TryParseName("ensure"))
        {
            ensureBlock = parser.ParseCommandList(["end"]);
        }

        parser.ParseName("end");

        return new TryExpression(tryBlock, rescueBlocks, ensureBlock);
    }

    /// <summary>
    /// Parses a raise expression
    /// </summary>
    /// <returns></returns>
    public IExpression ParseRaiseExpression()
    {
        var first = parser.ParseExpression();
        if (parser.TryParseToken(TokenType.Separator, ","))
        {
            var second = parser.ParseExpression();
            return new RaiseExpression(first, second);
        }
        return new RaiseExpression(first, null);
    }
}
