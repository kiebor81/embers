using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

[StdLib("empty?", TargetTypes = new[] { "String" })]
public class EmptyFunction : StdFunction
{
    [Comments("Returns true if the string is empty.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("empty? expects a string argument");

        if (values[0] is string s)
            return s.Length == 0;

        throw new TypeError("empty? expects a string");
    }
}
