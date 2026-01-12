using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings
{
    [StdLib("replace", TargetType = "String")]
    public class ReplaceFunction : StdFunction
    {
        [Comments("Replaces the string with the specified replacement string.")]
        [Arguments(ParamNames = new[] { "string", "replacement" }, ParamTypes = new[] { typeof(string), typeof(string) })]
        [Returns(ReturnType = typeof(string))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("replace expects two string arguments");

            if (values[0] is string s && values[1] is string replacement)
                return replacement;

            throw new TypeError("replace expects string arguments");
        }
    }
}

