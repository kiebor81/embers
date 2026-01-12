using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Strings
{
    [StdLib("insert", TargetType = "String")]
    public class InsertFunction : StdFunction
    {
        [Comments("Inserts a substring into the string at the specified index.")]
        [Arguments(ParamNames = new[] { "string", "index", "substring" }, ParamTypes = new[] { typeof(string), typeof(int), typeof(string) })]
        [Returns(ReturnType = typeof(string))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 3 || values[0] == null || values[1] == null || values[2] == null)
                throw new ArgumentError("insert expects a string, index, and substring");

            if (values[0] is string s && values[1] is int index && values[2] is string sub)
            {
                if (index < 0 || index > s.Length)
                    throw new ArgumentError("insert: index out of bounds");
                return s[..index] + sub + s[index..];
            }

            throw new TypeError("insert expects a string, integer, and string");
        }
    }
}

