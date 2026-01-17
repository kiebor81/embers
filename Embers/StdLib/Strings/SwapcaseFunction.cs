using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

[StdLib("swapcase", TargetType = "String")]
public class SwapcaseFunction : StdFunction
{
    [Comments("Swaps the case of each character in the string.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("swapcase expects a string argument");

        if (values[0] is string s)
            return new string([.. s.Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c))]);

        throw new TypeError("swapcase expects a string");
    }
}

