namespace Embers.Exceptions
{
    /// <summary>
    /// TypeAccessError is a specific type of error that occurs when there is an attempt to access a type that is not allowed by the current security policy.
    /// </summary>
    /// <seealso cref="Embers.Exceptions.BaseError" />
    public class TypeAccessError(string msg) : BaseError(msg)
    {
    }
}
