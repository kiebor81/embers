namespace Embers.Compiler;

/// <summary>
/// Token representation for the Embers compiler.
/// This record holds the type and value of a token.
/// </summary>
public class Token(TokenType type, string value)
{
    private readonly string value = value;
    private readonly TokenType type = type;

    public string Value { get { return value; } }

    public TokenType Type { get { return type; } }
}
