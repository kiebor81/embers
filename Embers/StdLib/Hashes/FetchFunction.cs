using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("fetch", TargetTypes = new[] { "Hash" })]
public class FetchFunction : StdFunction
{
    [Comments("Returns the value for the key, or a default if provided.")]
    [Arguments(ParamNames = new[] { "hash", "key", "default" }, ParamTypes = new[] { typeof(Hash), typeof(object), typeof(object) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("fetch expects a hash and a key");

        if (values[0] is IDictionary hash)
        {
            var key = values[1];
            if (hash.Contains(key))
                return hash[key];

            if (values.Count > 2)
                return values[2];

            throw new ArgumentError("fetch key not found");
        }

        throw new TypeError("fetch expects a hash");
    }
}
