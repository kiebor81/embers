namespace Embers.StdLib;

/// <summary>
/// Utility methods for inspecting values in a human-readable format.
/// </summary>
internal static class InspectUtils
{
    /// <summary>
    /// Returns a human-readable string representation of the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Inspect(object? value)
    {
        if (value == null) return "nil";

        if (value is Symbol symbol)
            return ":" + symbol.Name;

        // Strings: quote like Ruby
        if (value is string str)
            return $"\"{Escape(str)}\"";

        // Lists: [a, b]
        if (value is IEnumerable<object> list && value is not string)
            return "[" + string.Join(", ", list.Select(Inspect)) + "]";

        // Dictionaries: {k=>v}
        if (value is IDictionary<object, object> dict)
            return "{" + string.Join(", ", dict.Select(kv => $"{Inspect(kv.Key)}=>{Inspect(kv.Value)}")) + "}";

        return value.ToString() ?? value.GetType().Name;
    }

    /// <summary>
    /// Escapes special characters in a string for inspection.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string Escape(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
}
