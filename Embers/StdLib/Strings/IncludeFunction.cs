using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Strings;

[StdLib("include?", TargetTypes = new[] { "String", "Array" })]
public class IncludeFunction : StdFunction
{
    [Comments("Checks if the string or array includes the specified item.")]
    [Arguments(ParamNames = new[] { "data", "item" }, ParamTypes = new[] { typeof(object), typeof(object) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("include? expects a collection and a value");

        var collection = values[0];
        var item = values[1];

        if (collection is string s)
            return item != null && s.Contains(item.ToString());
        if (collection is IList list)
            return list.Contains(item);
        if (collection is IEnumerable enumerable)
        {
            foreach (var entry in enumerable)
                if (Equals(entry, item))
                    return true;
            return false;
        }

        throw new TypeError("include? expects a string or array as first argument");
    }
}

