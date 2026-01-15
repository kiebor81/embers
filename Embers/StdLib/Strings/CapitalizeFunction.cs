using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

/// <summary>
/// Capitalizes the first character of a string.
/// </summary>
[StdLib("capitalize", TargetType = "String")]
public class CapitalizeFunction : StdFunction
{
    [Comments("Capitalizes the first character of the string.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return "";

        if (values[0] is string s && s.Length > 0)
            return char.ToUpper(s[0]) + s[1..];

        throw new TypeError("capitalize expects a string");
    }
}

