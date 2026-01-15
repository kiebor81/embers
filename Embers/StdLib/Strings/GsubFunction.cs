using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
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

        if (values[0] is string s && values[1] is string pattern && values[2] is string replacement)
            return Regex.Replace(s, pattern, replacement);

        throw new TypeError("gsub expects string arguments");
    }
}

