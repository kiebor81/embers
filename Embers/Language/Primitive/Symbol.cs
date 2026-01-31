namespace Embers.Language.Primitive;
/// <summary>
/// Symbols are used to represent identifiers in the language.
/// A symbol is a unique identifier that is immutable and interned.
/// </summary>
public class Symbol(string name)
{
    private static readonly int hashcode = typeof(Symbol).GetHashCode();
    private readonly string name = name;

    public string Name => name;

    public override string ToString() => ":" + name;

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is Symbol symbol)
        {
            return name == symbol.name;
        }

        return false;
    }

    public override int GetHashCode() => name.GetHashCode() + hashcode;
}

