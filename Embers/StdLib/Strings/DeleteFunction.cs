using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings
{
    [StdLib("delete", TargetType = "String")]
    public class DeleteFunction : StdFunction
    {
        [Comments("Deletes all occurrences of a substring from the string.")]
        [Arguments(ParamNames = new[] { "string", "substring" }, ParamTypes = new[] { typeof(string), typeof(string) })]
        [Returns(ReturnType = typeof(string))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("delete expects a string and a substring");

            if (values[0] is string s && values[1] is string sub)
                return s.Replace(sub, "");

            throw new TypeError("delete expects string arguments");
        }
    }
}

