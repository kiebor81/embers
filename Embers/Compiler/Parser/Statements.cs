using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Compiler;

public partial class Parser
{
    /// <summary>
    /// Parses an if expression.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Parses a for-in expression.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Parses a while expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
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

    /// <summary>
    /// Parses an until expression.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Parses a def expression.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Parses a class expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
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

        if (!Predicates.IsConstantName(named.Name))
            throw new SyntaxError("class/module name must be a CONSTANT");

        return new ClassExpression(named, body);
    }

    /// <summary>
    /// Parses a module expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ModuleExpression ParseModuleExpression()
    {
        string name = ParseName();
        IExpression body = ParseCommandList();

        if (!Predicates.IsConstantName(name))
            throw new SyntaxError("class/module name must be a CONSTANT");

        return new ModuleExpression(name, body);
    }

    /// <summary>
    /// Parses an unless expression.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Parses a try expression.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Parses a raise expression.
    /// </summary>
    /// <returns></returns>
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
}
