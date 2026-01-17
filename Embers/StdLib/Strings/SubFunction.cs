using Embers.Exceptions;
using Embers.Annotations;
using System.Text.RegularExpressions;

namespace Embers.StdLib.Strings;

[StdLib("sub", TargetType = "String")]
public class SubFunction : StdFunction
{
    [Comments("Replaces the first occurrence of the pattern in the string with the replacement.")]
    [Arguments(ParamNames = new[] { "string", "pattern", "replacement" }, ParamTypes = new[] { typeof(string), typeof(string), typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 3 || values[0] == null || values[1] == null || values[2] == null)
            throw new ArgumentError("sub expects a string, pattern, and replacement");

        if (values[0] is string s && values[1] is string pattern && values[2] is string replacement)
            return Regex.Replace(s, pattern, replacement, RegexOptions.None, TimeSpan.FromMilliseconds(100)); // Replace first occurrence

        throw new TypeError("sub expects string arguments");
    }
}

