using Embers.Exceptions;
using Embers.Annotations;
using System.Text.RegularExpressions;

namespace Embers.StdLib.Strings;

[StdLib("match", TargetTypes = new[] { "String" })]
public class MatchFunction : StdFunction
{
    [Comments("Returns the first regex match in the string, or nil if none.")]
    [Arguments(ParamNames = new[] { "string", "pattern" }, ParamTypes = new[] { typeof(string), typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("match expects a string and a pattern");

        if (values[0] is string s && values[1] is string pattern)
        {
            var match = Regex.Match(s, pattern);
            return match.Success ? match.Value : null;
        }

        throw new TypeError("match expects string arguments");
    }
}
