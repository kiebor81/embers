using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

[StdLib("rindex", TargetType = "String")]
public class RindexFunction : StdFunction
{
    [Comments("Returns the last index of the substring in the string.")]
    [Arguments(ParamNames = new[] { "string", "substring" }, ParamTypes = new[] { typeof(string), typeof(string) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
            throw new ArgumentError("rindex expects a string and a substring");

        if (values[0] is string s && values[1] is string sub)
            return s.LastIndexOf(sub, StringComparison.Ordinal);

        throw new TypeError("rindex expects string arguments");
    }
}

