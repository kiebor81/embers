namespace Embers.Expressions;
/// <summary>
/// TryExpression represents a try-catch-finally block in the Embers language.
/// </summary>
/// <seealso cref="BaseExpression" />
public class TryExpression(
    IExpression tryBlock,
    IList<(IExpression? ExceptionType, string VarName, IExpression Handler)> rescueBlocks,
    IExpression? ensureBlock) : BaseExpression
{
    public IExpression TryBlock { get; } = tryBlock;
    // public IList<(string VarName, IExpression Handler)> RescueBlocks { get; }
    public IExpression? EnsureBlock { get; } = ensureBlock;

    public IList<(IExpression? ExceptionType, string VarName, IExpression Handler)> RescueBlocks { get; } = rescueBlocks;

    public override object? Evaluate(Context context)
    {
        object? result = null;
        try
        {
            result = TryBlock.Evaluate(context);
        }
        catch (Exception ex)
        {
            bool handled = false;
            foreach (var (exceptionType, varName, handler) in RescueBlocks)
            {
                bool typeMatches = true;
                if (exceptionType != null)
                {
                    var typeObj = exceptionType.Evaluate(context);
                    if (typeObj is Type t)
                        typeMatches = t.IsInstanceOfType(ex);
                    else
                        typeMatches = false;
                }
                if (typeMatches)
                {
                    var rescueContext = new BlockContext(context);
                    rescueContext.SetLocalValue(varName, ex);
                    result = handler.Evaluate(rescueContext);
                    handled = true;
                    break;
                }
            }
            if (!handled)
                throw;
        }
        finally
        {
            //Console.WriteLine("Ensure block about to run: " + (EnsureBlock != null));
            EnsureBlock?.Evaluate(context);
        }
        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TryExpression other) return false;
        return Equals(TryBlock, other.TryBlock)
            && Equals(EnsureBlock, other.EnsureBlock)
            && RescueBlocks.Count == other.RescueBlocks.Count;
    }

    public override int GetHashCode() => HashCode.Combine(TryBlock, RescueBlocks, EnsureBlock);
}
