namespace Embers.Exceptions;
/// <summary>
/// Method not found error.
/// </summary>
/// <seealso cref="BaseError" />
public class NoMethodError(string mthname) : BaseError(string.Format("undefined method '{0}'", mthname)) { }

