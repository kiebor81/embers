namespace Embers.Exceptions;
/// <summary>
/// ValueError is a specific type of error that indicates an issue with a value or assingment.
/// </summary>
/// <seealso cref="BaseError" />
public class ValueError(string msg) : BaseError(msg) { }

