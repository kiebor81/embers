namespace Embers.Compiler;

/// <summary>
/// Token representation for the Embers compiler.
/// This record holds the type and value of a token.
/// </summary>
public class Token(TokenType type, string value)
{
    private readonly string value = value;
    private readonly TokenType type = type;

    /// <summary>
    /// Gets the value of the token.
    /// </summary>
    public string Value { get { return value; } }

    /// <summary>
    /// Gets the type of the token.
    /// </summary>
    public TokenType Type { get { return type; } }
}
