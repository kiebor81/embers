namespace Embers.Exceptions
{
    /// <summary>
    /// General Embers error class.
    /// </summary>
    /// <seealso cref="Embers.Exceptions.BaseError" />
    public class EmbersError(string msg) : BaseError(msg)
    {
    }
}
