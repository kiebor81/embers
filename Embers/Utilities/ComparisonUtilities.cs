using Microsoft.VisualBasic.CompilerServices;

namespace Embers.Utilities;

/// <summary>
/// Provides utility methods for comparing objects.
/// </summary>
internal static class ComparisonUtilities
{
    /// <summary>
    /// Tries to compare two objects and returns the comparison result.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <param name="result">The result of the comparison: -1 if left is less than right, 1 if left is greater than right, 0 if they are equal.</param>
    /// <returns>True if the comparison was successful; otherwise, false.</returns>
    public static bool TryCompare(object? left, object? right, out int result)
    {
        if (left == null && right == null)
        {
            result = 0;
            return true;
        }

        if (left == null || right == null)
        {
            result = 0;
            return false;
        }

        try
        {
            if (Operators.ConditionalCompareObjectLess(left, right, false))
            {
                result = -1;
                return true;
            }

            if (Operators.ConditionalCompareObjectGreater(left, right, false))
            {
                result = 1;
                return true;
            }

            if (Operators.ConditionalCompareObjectEqual(left, right, false))
            {
                result = 0;
                return true;
            }
        }
        catch
        {
        }

        result = 0;
        return false;
    }
}
