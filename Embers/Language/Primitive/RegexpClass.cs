namespace Embers.Language.Primitive;

/// <summary>
/// RegexpClass represents the Regexp type in the runtime interpreter.
/// </summary>
/// <seealso cref="NativeClass" />
public sealed class RegexpClass : NativeClass
{
    public RegexpClass(Machine machine)
        : base("Regexp", machine)
    {
    }
}
