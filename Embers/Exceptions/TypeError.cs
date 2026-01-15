namespace Embers.Exceptions;
/// <summary>
/// TypeError is a specific type of error that occurs when there is a type mismatch or an invalid type operation.
/// </summary>
/// <seealso cref="BaseError" />
public class TypeError(string msg) : BaseError(msg) { }

