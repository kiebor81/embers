namespace Embers;

/// <summary>
/// Block context for the current execution state, including local variables and self object.
/// This context is used to manage the scope of variables and methods during execution within a block.
/// </summary>
/// <seealso cref="Context" />
public class BlockContext : Context
{
    public BlockContext(Context parent) : base(parent)
    {
        Self = parent.Self;
        //this.Module = parent.Module;
    }

    /// <summary>
    /// Determines whether the specified local variable exists in the current context or any parent context.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override bool HasLocalValue(string name)
    {
        if (base.HasLocalValue(name))
            return true;

        return Parent.HasLocalValue(name);
    }

    /// <summary>
    /// Gets the value of the specified local variable from the current context or any parent context.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override object GetLocalValue(string name)
    {
        if (base.HasLocalValue(name))
            return base.GetLocalValue(name);

        return Parent.GetLocalValue(name);
    }

    /// <summary>
    /// Sets the value of the specified local variable in the current context or parent context if it exists there.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public override void SetLocalValue(string name, object value)
    {
        if (Parent.HasLocalValue(name))
            Parent.SetLocalValue(name, value);
        else
            base.SetLocalValue(name, value);
    }
}

