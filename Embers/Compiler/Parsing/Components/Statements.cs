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
        string name = parser.ParseName();
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

        var (parameters, blockParam) = parser.ParseParameterListWithBlock();
        IExpression body = parser.ParseCommandList();

        return new DefExpression(named, parameters, body, blockParam);
    }

    /// <summary>
    /// Parses a class expression
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public ClassExpression ParseClassExpression()
    {
        string name = parser.ParseName();
        INamedExpression named = null;

        if (name == "self")
            named = new SelfExpression();
        else
            named = new NameExpression(name);

        while (true)
        {
            if (parser.TryParseToken(TokenType.Separator, "::"))
            {
                string newname = parser.ParseName();
                named = new DoubleColonExpression(named, newname);
                continue;
            }

            break;
        }

        parser.ParseEndOfCommand();
        IExpression body = parser.ParseCommandList();

        if (!Predicates.IsConstantName(named.Name))
            throw new SyntaxError("class/module name must be a CONSTANT");

        return new ClassExpression(named, body);
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
