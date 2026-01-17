using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

/// <summary>
/// Returns the length of a string or array.
/// </summary>
[StdLib("length", "len", "size", TargetTypes = new[] { "String" })]
public class LengthFunction : StdFunction
{
    [Comments("Returns the length of the string or array.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return 0;

        var value = values[0];
        if (value is string s)
            return s.Length;
        if (value is IEnumerable<object> arr)
            return arr.Count();

        throw new ArgumentError("length expects a string or array");
    }
}

