namespace Embers.Exceptions;
/// <summary>
/// FrozenError is raised when attempting to modify a frozen object.
/// </summary>
/// <seealso cref="TypeError" />
public class FrozenError(string msg) : TypeError(msg) { }
