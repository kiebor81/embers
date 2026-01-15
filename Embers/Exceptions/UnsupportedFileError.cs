namespace Embers.Exceptions;
/// <summary>
/// UnsupportedFileError is used to indicate that a file type is not supported by the application.
/// </summary>
/// <seealso cref="BaseError" />
public class UnsupportedFileError(string msg) : BaseError(msg) { }

