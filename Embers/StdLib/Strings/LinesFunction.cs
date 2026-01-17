using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

[StdLib("lines", TargetType = "String")]
public class LinesFunction : StdFunction
{
    [Comments("Returns an array of lines in the string.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("lines expects a string argument");

        if (values[0] is string s)
            return s.Split(["\r\n", "\n", "\r"], StringSplitOptions.None).ToList();

        throw new TypeError("lines expects a string");
    }
}

