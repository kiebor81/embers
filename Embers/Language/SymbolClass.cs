namespace Embers.Language
{
    /// <summary>
    /// SymbolClass represents the native Symbol class in the interpreter.
    /// </summary>
    /// <seealso cref="Embers.Language.NativeClass" />
    public class SymbolClass(Machine machine) : NativeClass("Symbol", machine)
    {
    }
}
