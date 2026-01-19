using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

/// <summary>
/// Splits a string into an array using the given separator.
/// </summary>
[StdLib("split", TargetType = "String")]
public class SplitFunction : StdFunction
{
    [Comments("Splits the string into an array using the given separator. If no separator is provided, splits on whitespace.")]
    [Arguments(ParamNames = new[] { "string", "separator" }, ParamTypes = new[] { typeof(string), typeof(string) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("split expects a string argument");

        var s = (values[0]?.ToString()) ?? throw new TypeError("split expects a string");

        // If no separator is provided (only the string itself in values[0]), split on whitespace
        if (values.Count == 1)
            return s.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).ToList();

        // Otherwise, use the provided separator
        var separator = values[1]?.ToString() ?? "";
        return s.Split([separator], StringSplitOptions.None).ToList();
    }
}

