using Embers.Language;

namespace Embers.StdLib.Strings
{
    /// <summary>
    /// Returns the string in lowercase.
    /// </summary>
    [StdLib("downcase", "down", "dcase", TargetType = "String")]
    public class DowncaseFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                return null;

            var value = values[0]?.ToString();
            return value?.ToLowerInvariant();
        }
    }
}

