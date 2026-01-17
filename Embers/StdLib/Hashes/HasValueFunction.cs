using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("has_value", "has_value?", TargetTypes = new[] { "Hash" })]
public class HasValueFunction : StdFunction
{
    [Comments("Checks if the hash contains the specified value.")]
    [Arguments(ParamNames = new[] { "hash", "value" }, ParamTypes = new[] { typeof(Hash), typeof(object) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("has_value expects a hash and a value argument");

        if (values[0] is IDictionary hash)
        {
            foreach (var value in hash.Values)
                if (Equals(value, values[1]))
                    return true;
            return false;
        }

        throw new TypeError("has_value expects a hash");
    }
}
