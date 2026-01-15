namespace Embers.Exceptions;
/// <summary>
/// NameError is a specific type of error that indicates an issue with a named type, variable, or function.
/// </summary>
/// <seealso cref="BaseError" />
public class NameError(string msg) : BaseError(msg) { }

