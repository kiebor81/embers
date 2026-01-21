namespace Embers.Functions;

/// <summary>
/// Represents keyword arguments passed to a function.
/// </summary>
internal sealed class KeywordArguments(DynamicHash values)
{
    public DynamicHash Values { get; } = values;
}
