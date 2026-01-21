namespace Embers.Functions;

internal sealed class KeywordArguments(DynamicHash values)
{
    public DynamicHash Values { get; } = values;
}
