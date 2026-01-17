using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("delete", TargetTypes = new[] { "Hash" })]
public class DeleteFunction : StdFunction
{
    [Comments("Removes the key from the hash and returns its value.")]
    [Arguments(ParamNames = new[] { "hash", "key" }, ParamTypes = new[] { typeof(Hash), typeof(object) })]
    [Returns(ReturnType = typeof(object))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("delete expects a hash and a key");

        if (values[0] is IDictionary hash)
        {
            var key = values[1];
            if (!hash.Contains(key))
                return null;

            var value = hash[key];
            hash.Remove(key);
            return value;
        }

        throw new TypeError("delete expects a hash");
    }
}
