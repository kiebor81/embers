using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

/// <summary>
/// Returns the string in uppercase.
/// </summary>
[StdLib("upcase", "up", "ucase", TargetType = "String")]
public class UpcaseFunction : StdFunction
{
    [Comments("Returns the string in uppercase.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return null;

        var value = values[0]?.ToString();
        return value?.ToUpperInvariant();
    }
}

