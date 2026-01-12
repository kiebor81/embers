using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes
{
    [StdLib("has_key?", TargetType = "Hash")]
    public class HasKeyFunction : StdFunction
    {
        [Comments("Checks if the hash contains the specified key.")]
        [Arguments(ParamNames = new[] { "hash", "key" }, ParamTypes = new[] { typeof(Hash), typeof(object) })]
        [Returns(ReturnType = typeof(Boolean))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null)
                throw new ArgumentError("has_key? expects a hash and a key argument");

            if (values[0] is IDictionary hash)
                return hash.Contains(values[1]);

            throw new TypeError("has_key? expects a hash");
        }
    }
}
