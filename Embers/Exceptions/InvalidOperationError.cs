namespace Embers.Exceptions;

/// <summary>
/// Exception for invalid operations.
/// throws when an operation is invalid in the current context.
/// </summary>
/// <seealso cref="BaseError"/>
public class InvalidOperationError(string msg) : BaseError(msg) { }

