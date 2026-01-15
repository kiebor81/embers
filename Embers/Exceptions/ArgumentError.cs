namespace Embers.Exceptions;
/// <summary>
/// ArgumentError is used to indicate that an argument passed to a function or method is invalid.
/// </summary>
/// <seealso cref="BaseError" />
public class ArgumentError(string msg) : BaseError(msg) { }

