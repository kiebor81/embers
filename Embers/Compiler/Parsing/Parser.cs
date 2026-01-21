//using Embers.Compiler.Parsing.Components;
using Embers.Exceptions;
using Embers.Expressions;

namespace Embers.Compiler.Parsing;

/// <summary>
/// the parser for the Embers language
/// </summary>
public class Parser
{
    /// <summary>
    /// the binary operators by precedence level
    /// </summary>
    private static string[][] binaryoperators = [
        ["&&", "||", "and", "or"],
            ["..", "==", "===", "!=", "<", ">", "<=", ">=", "<=>"],
            ["+", "-"],
            ["*", "/", "%"],
            ["**"]
    ];
    internal static string[][] BinaryOperators => binaryoperators;

    /// <summary>
    /// the lexer for the Embers language
    /// </summary>
    private readonly Lexer lexer;
    private readonly Components.Core core;
    private readonly Components.Primary primaryParser;
    private readonly Components.Expressions expressionParser;
    private readonly Components.Statements statementParser;
    private readonly Components.Blocks blockParser;
    internal Lexer Lexer => lexer;
    internal Components.Primary PrimaryParser => primaryParser;

    /// <summary>
    /// the parser for the Embers language
    /// </summary>
    /// <param name="text"></param>
    public Parser(string text)
    {
        lexer = new Lexer(text);
        core = new Components.Core(lexer, binaryoperators);
        primaryParser = new Components.Primary(this);
        expressionParser = new Components.Expressions(this);
        statementParser = new Components.Statements(this);
        blockParser = new Components.Blocks(this);
    }

    /// <summary>
    /// the parser for the Embers language
    /// </summary>
    /// <param name="reader"></param>
    public Parser(TextReader reader)
    {
        lexer = new Lexer(reader);
        core = new Components.Core(lexer, binaryoperators);
        primaryParser = new Components.Primary(this);
        expressionParser = new Components.Expressions(this);
        statementParser = new Components.Statements(this);
        blockParser = new Components.Blocks(this);
    }

    /// <summary>
    /// Parses an expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public IExpression? ParseExpression()
    {
        IExpression expr = ApplyPostfixConditional(ParseNoAssignExpression());

        if (expr == null)
            return null;

        // Only variables or assignable targets can be followed by assignment
        bool isAssignable = expr is NameExpression
            || expr is ClassVarExpression
            || expr is InstanceVarExpression
            || expr is GlobalExpression
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
                GlobalExpression gvar => new AssignGlobalVarExpression(gvar.Name, ParseExpression()),
                IndexedExpression idx when idx.IndexExpression == null => throw new SyntaxError("empty index is not assignable"),
                IndexedExpression idx => new AssignIndexedExpression(idx.Expression, idx.IndexExpression!, ParseExpression()),
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

    /// <summary>
    /// Parses a command.
    /// </summary>
    /// <returns></returns>
    public IExpression? ParseCommand()
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

    /// <summary>
    /// Parses an expression without assignment.
    /// </summary>
    /// <returns></returns>
    internal IExpression? ParseNoAssignExpression()
    {
        return expressionParser.ParseNoAssignExpression();
    }

    /// <summary>
    /// Parses the end of a command.
    /// </summary>
    /// <exception cref="SyntaxError"></exception>
    internal void ParseEndOfCommand() => core.ParseEndOfCommand();

    /// <summary>
    /// Determines if the next token starts an expression list.
    /// </summary>
    /// <returns></returns>
    internal bool NextTokenStartsExpressionList() => core.NextTokenStartsExpressionList();

    /// <summary>
    /// Determines if a token is an end of command.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    internal bool IsEndOfCommand(Token token) => core.IsEndOfCommand(token);

    /// <summary>
    /// Parses a name token with the given value.
    /// </summary>
    /// <param name="name"></param>
    internal void ParseName(string name) => core.ParseName(name);

    /// <summary>
    /// Parses a token of the given type and value.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <exception cref="SyntaxError"></exception>
    internal void ParseToken(TokenType type, string value) => core.ParseToken(type, value);

    /// <summary>
    /// Parses a name token.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal string ParseName() => core.ParseName();

    /// <summary>
    /// Tries to parse a name token with the given value.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    internal bool TryParseName(string name) => core.TryParseName(name);

    /// <summary>
    /// Tries to parse a token of the given type and value.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal bool TryParseToken(TokenType type, string value) => core.TryParseToken(type, value);
    internal bool NextTokenStartsExpressionListAllowSplat() => core.NextTokenStartsExpressionListAllowSplat();

    /// <summary>
    /// Tries to parse a name token.
    /// </summary>
    /// <returns></returns>
    internal string? TryParseName() => core.TryParseName();

    /// <summary>
    /// Tries to parse an end-of-line token.
    /// </summary>
    /// <returns></returns>
    internal bool TryParseEndOfLine() => core.TryParseEndOfLine();

    internal void SkipEndOfLines() => core.SkipEndOfLines();

    /// <summary>
    /// Determines if the given token is a binary operator at the specified precedence level.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal bool IsBinaryOperator(int level, Token token) => core.IsBinaryOperator(level, token);

    /// <summary>
    /// Parses a parameter list.
    /// </summary>
    /// <param name="canhaveparens"></param>
    /// <returns></returns>
    internal IList<string> ParseParameterList(bool canhaveparens = true)
        => blockParser.ParseParameterList(canhaveparens);

    /// <summary>
    /// Parses a parameter list with possible block parameter (&param).
    /// </summary>
    /// <param name="canhaveparens"></param>
    /// <returns></returns>
    internal (IList<string> parameters, string? splatParam, string? kwParam, string? blockParam) ParseParameterListWithBlock(bool canhaveparens = true)
        => blockParser.ParseParameterListWithBlock(canhaveparens);

    /// <summary>
    /// Parses an expression list.
    /// </summary>
    /// <returns></returns>
    internal IList<IExpression> ParseExpressionList()
        => blockParser.ParseExpressionList();

    /// <summary>
    /// Parses a single expression with possible block prefix (&).
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal IExpression? ParseSingleExpressionWithBlockPrefix()
        => blockParser.ParseSingleExpressionWithBlockPrefix();

    /// <summary>
    /// Parses an expression list with possible block arguments.
    /// </summary>
    /// <returns></returns>
    internal IList<IExpression> ParseExpressionListWithBlockArgs()
        => blockParser.ParseExpressionListWithBlockArgs();

    /// <summary>
    /// Parses an expression list until the given separator is encountered.
    /// </summary>
    /// <param name="separator"></param>
    /// <returns></returns>
    internal IList<IExpression> ParseExpressionList(string separator)
        => blockParser.ParseExpressionList(separator);

    /// <summary>
    /// Parses a block expression.
    /// </summary>
    /// <param name="usebraces"></param>
    /// <returns></returns>
    internal BlockExpression ParseBlockExpression(bool usebraces = false)
        => blockParser.ParseBlockExpression(usebraces);

    /// <summary>
    /// Parses a command list until "end" or "}" is encountered.
    /// </summary>
    /// <param name="usebraces"></param>
    /// <returns></returns>
    internal IExpression ParseCommandList(bool usebraces = false)
        => blockParser.ParseCommandList(usebraces);

    /// <summary>
    /// Parses a command list until one of the given names is encountered.
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    internal IExpression ParseCommandList(IList<string> names)
        => blockParser.ParseCommandList(names);

    /// <summary>
    /// Parses an if expression.
    /// </summary>
    /// <returns></returns>
    internal IfExpression ParseIfExpression()
        => statementParser.ParseIfExpression();

    /// <summary>
    /// Parses a for-in expression.
    /// </summary>
    /// <returns></returns>
    internal ForInExpression ParseForInExpression()
        => statementParser.ParseForInExpression();

    /// <summary>
    /// Parses a while expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal WhileExpression ParseWhileExpression()
        => statementParser.ParseWhileExpression();

    /// <summary>
    /// Parses an until expression.
    /// </summary>
    /// <returns></returns>
    internal UntilExpression ParseUntilExpression()
        => statementParser.ParseUntilExpression();

    /// <summary>
    /// Parses a def expression.
    /// </summary>
    /// <returns></returns>
    internal IExpression ParseDefExpression()
        => statementParser.ParseDefExpression();

    /// <summary>
    /// Parses a class expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal ClassExpression ParseClassExpression()
        => statementParser.ParseClassExpression();

    /// <summary>
    /// Parses a module expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal ModuleExpression ParseModuleExpression()
        => statementParser.ParseModuleExpression();

    /// <summary>
    /// Parses an unless expression.
    /// </summary>
    /// <returns></returns>
    internal UnlessExpression ParseUnlessExpression()
        => statementParser.ParseUnlessExpression();

    /// <summary>
    /// Parses a case expression.
    /// </summary>
    internal CaseExpression ParseCaseExpression()
        => statementParser.ParseCaseExpression();

    /// <summary>
    /// Parses a try expression.
    /// </summary>
    /// <returns></returns>
    internal IExpression ParseTryExpression()
        => statementParser.ParseTryExpression();

    /// <summary>
    /// Parses a raise expression.
    /// </summary>
    /// <returns></returns>
    internal IExpression ParseRaiseExpression()
        => statementParser.ParseRaiseExpression();

    /// <summary>
    /// Parses a binary expression at the given precedence level.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    internal IExpression? ParseBinaryExpression(int level)
        => expressionParser.ParseBinaryExpression(level);

    /// <summary>
    /// Determines if a token is a binary operator at the given precedence level.
    /// </summary>
    /// <returns></returns>
    internal IExpression? ParseTerm() => expressionParser.ParseTerm();

    /// <summary>
    /// Applies postfix chains (., ::, []) to an expression.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    internal IExpression ApplyPostfixChain(IExpression expression)
        => expressionParser.ApplyPostfixChain(expression);

    /// <summary>
    /// Parses a simple term.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal IExpression? ParseSimpleTerm() => primaryParser.ParseSimpleTerm();

    /// <summary>
    /// Parses an interpolated string.
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    internal IExpression ParseInterpolatedString(string raw) => primaryParser.ParseInterpolatedString(raw);

    /// <summary>
    /// Applies postfix conditional expressions (ternary, if, unless).
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    internal IExpression ApplyPostfixConditional(IExpression expr)
        => expressionParser.ApplyPostfixConditional(expr);

    /// <summary>
    /// Applies postfix ternary expressions (? :).
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    internal IExpression ApplyPostfixTernary(IExpression expr)
        => expressionParser.ApplyPostfixTernary(expr);

    /// <summary>
    /// Applies postfixes except for the ternary operator.
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    internal IExpression ApplyPostfixesButNotTernary(IExpression expr)
        => expressionParser.ApplyPostfixesButNotTernary(expr);

    /// <summary>
    /// Parses a compound assignment expression.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="op"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal IExpression ParseCompoundAssignment(IExpression target, string op)
        => expressionParser.ParseCompoundAssignment(target, op);

    /// <summary>
    /// Creates a binary expression for the given operator and operands.
    /// </summary>
    /// <param name="op"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal IExpression MakeBinaryExpression(string op, IExpression left, IExpression right)
        => expressionParser.MakeBinaryExpression(op, left, right);

    /// <summary>
    /// Parses a hash expression.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    internal HashExpression ParseHashExpression() => expressionParser.ParseHashExpression();
}
