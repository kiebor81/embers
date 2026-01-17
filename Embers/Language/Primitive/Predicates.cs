namespace Embers.Language.Primitive;
/// <summary>
/// Predicates for evaluating conditions in the runtime interpreter.
/// </summary>
public static class Predicates
{
    /// <summary>
    /// Determines if a given object is considered false.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsFalse(object obj) => obj == null || false.Equals(obj);

    /// <summary>
    /// Determines if a given object is considered true.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsTrue(object obj) => !IsFalse(obj);

    /// <summary>
    /// Determines if a given name is a constant name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool IsConstantName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return char.IsUpper(name[0]);
    }
}

