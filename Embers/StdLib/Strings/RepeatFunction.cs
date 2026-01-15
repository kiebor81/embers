using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Text;

namespace Embers.StdLib.Strings;

[StdLib("repeat")]
internal class RepeatFunction : StdFunction
{
    [Comments("Repeats a string or character a given number of times.")]
    [Arguments(ParamNames = new[] { "text", "count" }, ParamTypes = new[] { typeof(string), typeof(long) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2)
            throw new ArgumentError("repeat expects text and count arguments");

        if (!long.TryParse(values[1]?.ToString(), out var count))
            throw new ArgumentError("repeat expects count to be a number");

        var text = values[0]?.ToString() ?? string.Empty;

        if (count < 0)
            throw new ArgumentError("repeat expects a non-negative count");

        if (count == 0 || text.Length == 0)
            return string.Empty;

        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
            builder.Append(text);

        return builder.ToString();
    }
}
