using System.Globalization;
using System.Text;
using Embers.Annotations;
using Embers;
using Embers.Exceptions;
using Embers.Host;
using Embers.Language.Dynamic;

namespace MyLib.DSL;

[HostFunction("note")]
internal class NoteFunction : HostFunction
{
    [Comments("Writes a tagged message to the host console.")]
    [Arguments(ParamNames = new[] { "message" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(void))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        var message = values != null && values.Count > 0 ? values[0]?.ToString() ?? string.Empty : string.Empty;
        Console.WriteLine($"[DSL] {message}");
        return null;
    }
}

[HostFunction("stamp", "timestamp")]
internal class TimestampFunction : HostFunction
{
    [Comments("Returns a UTC timestamp string using an optional .NET format string.")]
    [Arguments(ParamNames = new[] { "format" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var format = values != null && values.Count > 0 && values[0] != null
            ? values[0].ToString()
            : "o";

        return DateTime.UtcNow.ToString(format, CultureInfo.InvariantCulture);
    }
}

[HostFunction("slug")]
internal class SlugFunction : HostFunction
{
    [Comments("Creates a simple lowercase slug with hyphen separators.")]
    [Arguments(ParamNames = new[] { "text" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("slug expects a text argument");

        var input = values[0].ToString();
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var builder = new StringBuilder(input.Length);
        var previousDash = false;

        foreach (var ch in input.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                previousDash = false;
                continue;
            }

            if ((ch == ' ' || ch == '-' || ch == '_') && !previousDash && builder.Length > 0)
            {
                builder.Append('-');
                previousDash = true;
            }
        }

        if (builder.Length > 0 && builder[^1] == '-')
            builder.Length--;

        return builder.ToString();
    }
}

[HostFunction("host_is_colour?", "host_is_color?")]
internal class HostIsColourFunction : HostFunction
{
    [Comments("Returns true when the input string is 'green'.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(string) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var value = values != null && values.Count > 0 ? values[0]?.ToString() : null;
        return string.Equals(value, "green", StringComparison.OrdinalIgnoreCase);
    }
}

[HostFunction("host_has_item?")]
internal class HostHasItemFunction : HostFunction
{
    [Comments("Returns true when key is 'KEY1' and count is greater than 3.")]
    [Arguments(ParamNames = new[] { "key", "count" }, ParamTypes = new[] { typeof(string), typeof(long) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2)
            return false;

        var key = values[0]?.ToString();
        var count = values[1] is IConvertible
            ? Convert.ToInt64(values[1], CultureInfo.InvariantCulture)
            : 0;

        return string.Equals(key, "KEY1", StringComparison.OrdinalIgnoreCase) && count > 3;
    }
}
