using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

/// <summary>
/// Joins array elements into a string, separated by the given separator.
/// </summary>
[StdLib("join", TargetType = "Array")]
public class JoinFunction : StdFunction
{
    [Comments("Joins array elements into a string, separated by the given separator.")]
    [Arguments(ParamNames = new[] { "array_data", "separator" }, ParamTypes = new[] { typeof(Array), typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("join expects an array argument");

        var separator = values.Count > 1 ? values[1]?.ToString() ?? "" : "";

        if (values[0] is IEnumerable arr)
            return string.Join(separator, arr.Cast<object>().Select(x => x?.ToString()));

        throw new TypeError("join expects an array");
    }
}

