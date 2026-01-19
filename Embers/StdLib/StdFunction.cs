using System.Reflection;
using Embers.Annotations;
using Embers.Functions;

namespace Embers.StdLib;
/// <summary>
/// Standard function base class for all functions in the standard library.
/// Implements from <see cref="IFunction"/> and provides a common interface for applying functions.
/// Clases derived from StdFunction will automatically be included in the standard library and can be used as functions in the language.
/// </summary>
/// <seealso cref="IFunction" />
[ScannerIgnore]
public abstract class StdFunction : IFunction
{
    private static readonly FieldInfo? BlockField = typeof(Context)
        .GetField("<Block>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? typeof(Context).GetField("block", BindingFlags.Instance | BindingFlags.NonPublic);

    public abstract object? Apply(DynamicObject self, Context context, IList<object> values);

    public virtual object? ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction block)
    {
        var previous = context.Block;

        try
        {
            // Assign the block temporarily
            BlockField?.SetValue(context, block);

            return Apply(self, context, values);
        }
        finally
        {
            BlockField?.SetValue(context, previous);
        }
    }

}
