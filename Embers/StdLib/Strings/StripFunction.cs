using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

/// <summary>
/// Removes leading and trailing whitespace from a string.
/// </summary>
[StdLib("strip", TargetType = "String")]
public class StripFunction : StdFunction
{
    [Comments("Removes leading and trailing whitespace from the string.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("strip expects a string argument");

        if (values[0] is string s)
            return s.Trim();

        throw new TypeError("strip expects a string");
    }
}

