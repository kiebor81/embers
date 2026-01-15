namespace Embers.Exceptions;
/// <summary>
/// Base class for all errors in the Embers project.
/// This class is used to represent errors that occur during the execution of the Embers language.
/// It inherits from System.Exception to provide a standard error handling mechanism aligned with .NET
/// Errors derived from this class are automatically caught by the Embers runtime.
/// Error types derived from BaseError are auto-registered in the <see cref="Machine"/>
/// </summary>
public abstract class BaseError(string msg) : Exception(msg) { }

