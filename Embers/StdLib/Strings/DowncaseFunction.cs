using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Strings
{
    /// <summary>
    /// Returns the string in lowercase.
    /// </summary>
    [StdLib("downcase", "down", "dcase", TargetType = "String")]
    public class DowncaseFunction : StdFunction
    {
        [Comments("Returns the string in lowercase.")]
        [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
        [Returns(ReturnType = typeof(string))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                return string.Empty;

            var value = values[0]?.ToString();
            return value?.ToLowerInvariant();
        }
    }
}

