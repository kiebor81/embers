namespace Embers.Language.Primitive;
/// <summary>
/// HashClass represents the native Hash class in the runtime interpreter.
/// </summary>
/// <seealso cref="NativeClass" />
public class HashClass(Machine machine) : NativeClass("Hash", machine) { }

