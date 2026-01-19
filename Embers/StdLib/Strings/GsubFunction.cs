using Embers.Annotations;
using Embers.Exceptions;
using System.Text.RegularExpressions;

namespace Embers.StdLib.Strings;

/// <summary>
/// Performs global substitution in a string using a regex pattern.
/// </summary>
[StdLib("gsub", TargetType = "String")]
public class GsubFunction : StdFunction
{
    [Comments("Performs global substitution in the string using a regex pattern.")]
    [Arguments(ParamNames = new[] { "string", "pattern", "replacement" }, ParamTypes = new[] { typeof(string), typeof(string), typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 3 || values[0] == null || values[1] == null || values[2] == null)
            throw new ArgumentError("gsub expects a string, pattern, and replacement");

        if (values[0] is not string s || values[2] is not string replacement)
            throw new TypeError("gsub expects a string, pattern, and replacement");

        if (values[1] is string pattern)
            return Regex.Replace(s, pattern, replacement);

        if (values[1] is Regexp regexp)
            return regexp.Regex.Replace(s, replacement);

        throw new TypeError("gsub expects a string, pattern, and replacement");
    }
}

