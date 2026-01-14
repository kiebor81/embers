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

    public override bool HasLocalValue(string name)
    {
        if (base.HasLocalValue(name))
            return true;

        return Parent.HasLocalValue(name);
    }

    public override object GetLocalValue(string name)
    {
        if (base.HasLocalValue(name))
            return base.GetLocalValue(name);

        return Parent.GetLocalValue(name);
    }

    public override void SetLocalValue(string name, object value)
    {
        if (Parent.HasLocalValue(name))
            Parent.SetLocalValue(name, value);
        else
            base.SetLocalValue(name, value);
    }
}

