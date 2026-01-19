using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.RegularExpressions;

[StdLib("match", TargetType = "Regexp")]
public class RegexpMatchFunction : StdFunction
{
    [Comments("Returns the first regex match in the string, or nil if none.")]
    [Arguments(ParamNames = new[] { "regexp", "string" }, ParamTypes = new[] { typeof(Regexp), typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 1)");

        if (values[0] is not Regexp regexp)
            throw new TypeError("regexp must be a Regexp");

        if (values[1] is not string input)
            throw new TypeError("match expects a string");

        var match = regexp.Regex.Match(input);
        return match.Success ? match.Value : null;
    }
}
