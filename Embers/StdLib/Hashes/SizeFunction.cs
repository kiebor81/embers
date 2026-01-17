using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("size", "length", TargetTypes = new[] { "Hash" })]
public class SizeFunction : StdFunction
{
    [Comments("Returns the number of key-value pairs in the hash.")]
    [Arguments(ParamNames = new[] { "hash" }, ParamTypes = new[] { typeof(Hash) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("size expects a hash argument");

        if (values[0] is IDictionary hash)
            return hash.Count;

        throw new TypeError("size expects a hash");
    }
}
