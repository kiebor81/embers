namespace Embers.Language.Primitive;
/// <summary>
/// SymbolClass represents the native Symbol class in the interpreter.
/// </summary>
/// <seealso cref="NativeClass" />
public class SymbolClass(Machine machine) : NativeClass("Symbol", machine) { }
