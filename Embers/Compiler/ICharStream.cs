namespace Embers.Compiler;
/// <summary>
/// Represents a character stream abstraction for use by the lexer.
/// Provides methods to read the next character and to move the stream position backwards.
/// </summary>
public interface ICharStream
{
    /// <summary>
    /// Moves the stream position back by one character.
    /// Used to "unread" a character after peeking or reading ahead.
    /// </summary>
    void BackChar();

    /// <summary>
    /// Reads and returns the next character from the stream.
    /// Returns -1 if the end of the stream is reached.
    /// </summary>
    /// <returns>The next character as an integer, or -1 if at end of stream.</returns>
    int NextChar();
}

