using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

[StdLib("casecmp", TargetTypes = new[] { "String" })]
public class CasecmpFunction : StdFunction
{
    [Comments("Compares two strings case-insensitively and returns -1, 0, or 1.")]
    [Arguments(ParamNames = new[] { "string", "other" }, ParamTypes = new[] { typeof(string), typeof(string) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("casecmp expects two string arguments");

        if (values[0] is string s && values[1] is string other)
        {
            var result = string.Compare(s, other, StringComparison.OrdinalIgnoreCase);
            return result < 0 ? -1 : result > 0 ? 1 : 0;
        }

        throw new TypeError("casecmp expects string arguments");
    }
}
