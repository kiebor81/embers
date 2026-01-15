namespace Embers.Exceptions;

/// <summary>
/// Exception for unsupported operations.
/// </summary>
/// <seealso cref="BaseError"/>
public class NotSupportedError(string msg) : BaseError(msg) { }

