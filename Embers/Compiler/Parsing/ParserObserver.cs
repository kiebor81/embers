namespace Embers.Compiler.Parsing;

/// <summary>
/// Observer interface for monitoring parsing events.
/// </summary>
public interface IParserObserver
{
    void OnTokenRead(ParserTokenInfo token);
    void OnCommandEnter(ParserCommandInfo command);
    void OnCommandExit(ParserCommandInfo command);
    void OnParseError(ParserErrorInfo error);
}

/// <summary>
/// Information about a parsed token.
/// </summary>
/// <param name="Type"></param>
/// <param name="Value"></param>
/// <param name="Span"></param>
public readonly record struct ParserTokenInfo(TokenType Type, string Value, TokenSpan? Span);

/// <summary>
/// Information about a parser command.
/// </summary>
/// <param name="CommandIndex"></param>
/// <param name="Span"></param>
public readonly record struct ParserCommandInfo(int CommandIndex, TokenSpan? Span);

/// <summary>
/// Information about a parsing error.
/// </summary>
/// <param name="Message"></param>
/// <param name="CommandIndex"></param>
/// <param name="Span"></param>
public sealed record ParserErrorInfo(string Message, int? CommandIndex, TokenSpan? Span);
