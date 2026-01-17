using System.Collections;
using Embers.Exceptions;
using Embers.StdLib.Numeric;

namespace Embers.Language.Primitive;
/// <summary>
/// Represents a range of numeric values from <c>from</c> to <c>to</c>.
/// </summary>
public class Range(object from, object to) : IEnumerable
{
    private const double DoubleEpsilon = 1e-9;

    private readonly object from = from;
    private readonly object to = to;

    public object From => from;
    public object To => to;

    public bool TryGetIntBounds(out int start, out int end)
    {
        if (NumericCoercion.TryGetLong(from, out var fromLong)
            && NumericCoercion.TryGetLong(to, out var toLong)
            && fromLong >= int.MinValue && fromLong <= int.MaxValue
            && toLong >= int.MinValue && toLong <= int.MaxValue)
        {
            start = (int)fromLong;
            end = (int)toLong;
            return true;
        }

        start = 0;
        end = 0;
        return false;
    }

    public bool TryGetLongBounds(out long start, out long end)
    {
        if (NumericCoercion.TryGetLong(from, out var fromLong)
            && NumericCoercion.TryGetLong(to, out var toLong))
        {
            start = fromLong;
            end = toLong;
            return true;
        }

        start = 0;
        end = 0;
        return false;
    }

    public bool TryGetDoubleBounds(out double start, out double end)
    {
        if (NumericCoercion.TryGetDouble(from, out var fromDouble)
            && NumericCoercion.TryGetDouble(to, out var toDouble))
        {
            start = fromDouble;
            end = toDouble;
            return true;
        }

        start = 0;
        end = 0;
        return false;
    }

    public IEnumerable Enumerate()
    {
        if (TryGetIntBounds(out var intStart, out var intEnd))
        {
            for (var value = intStart; value <= intEnd; value++)
                yield return value;
            yield break;
        }

        if (TryGetLongBounds(out var longStart, out var longEnd))
        {
            for (var value = longStart; value <= longEnd; value++)
                yield return value;
            yield break;
        }

        if (TryGetDoubleBounds(out var doubleStart, out var doubleEnd))
        {
            for (var value = doubleStart; value <= doubleEnd + DoubleEpsilon; value += 1.0)
                yield return value;
            yield break;
        }

        throw new TypeError("range must be numeric");
    }

    public IEnumerator GetEnumerator() => Enumerate().GetEnumerator();

    public override string ToString() => string.Format("{0}..{1}", from, to);
}
