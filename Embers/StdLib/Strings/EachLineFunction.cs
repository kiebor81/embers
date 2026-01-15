using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings;

[StdLib("each_line", TargetType = "String")]
public class EachLineFunction : StdFunction
{
    [Comments("Iterates over each line in the string, executing the provided block for each line.")]
    [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(void))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("each_line expects a string argument");

        if (context.Block == null)
            throw new ArgumentError("each_line expects a block");

        if (values[0] is string s)
        {
            var lines = s.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
            foreach (var line in lines)
            {
                context.Block.Apply(self, context, [line]);
            }
            return null;
        }

        throw new TypeError("each_line expects a string");
    }
}

