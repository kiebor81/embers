namespace Embers.Language.Primitive;
/// <summary>
/// Predicates for evaluating conditions in the runtime interpreter.
/// </summary>
public static class Predicates
{
    public static bool IsFalse(object obj) => obj == null || false.Equals(obj);

    public static bool IsTrue(object obj) => !IsFalse(obj);

    public static bool IsConstantName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return char.IsUpper(name[0]);
    }
}

