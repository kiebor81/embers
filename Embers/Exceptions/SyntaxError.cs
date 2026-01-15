namespace Embers.Exceptions;
/// <summary>
/// SyntaxError is thrown when there is a syntax error in the code.
/// </summary>
/// <seealso cref="BaseError" />
public class SyntaxError(string message) : BaseError(message) { }

