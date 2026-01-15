using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Ranges;

[StdLib("map", TargetType = "Range")]
public class MapFunction : StdFunction
{
    [Comments("Applies the given block to each element of the range and returns an array of the results.")]
    [Arguments(ParamNames = new[] { "range", "block" }, ParamTypes = new[] { typeof(Language.Range), typeof(Block) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        var range = values[0] as IEnumerable<int>;
        if (range == null)
            throw new Exceptions.TypeError("range must be a Range");

        if (context.Block == null)
            throw new Exceptions.ArgumentError("no block given");

        var result = new DynamicArray();
        foreach (var value in range)
        {
            var blockResult = context.Block.Apply(self, context, [value]);
            result.Add(blockResult);
        }

        return result;
    }
}
