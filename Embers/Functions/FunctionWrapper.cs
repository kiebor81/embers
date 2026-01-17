namespace Embers.Functions;
/// <summary>
/// FunctionWrapper is a base class for wrapping functions with a specific context.
/// Allows for creating delegates that can be used to invoke the function with the provided context.
/// </summary>
public class FunctionWrapper(IFunction function, Context context)
{
    private readonly IFunction function = function;
    private readonly Context context = context;

    protected IFunction Function { get { return function; } }

    protected Context Context { get { return context; } }

    protected DynamicObject? Self { get { return context?.Self; } }

    public virtual ThreadStart CreateThreadStart() => new(DoAction);

    public virtual Delegate CreateActionDelegate() => Delegate.CreateDelegate(typeof(Action), this, "DoAction");

    public virtual Delegate CreateFunctionDelegate() => Delegate.CreateDelegate(typeof(Func<object>), this, "DoFunction");

    private object DoFunction() => function.Apply(null, context, null);

    private void DoAction() => function.Apply(null, context, null);
}

public class FunctionWrapper<TR, TD>(IFunction function, Context context) : FunctionWrapper(function, context)
{
    public override Delegate CreateFunctionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoFunction");

    public override Delegate CreateActionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoAction");

    public TR DoFunction() => (TR)Function.Apply(null, Context, null);

    public void DoAction() => Function.Apply(null, Context, null);
}

public class FunctionWrapper<T1, TR, TD>(IFunction function, Context context) : FunctionWrapper(function, context)
{
    public override Delegate CreateFunctionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoFunction");

    public override Delegate CreateActionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoAction");

    public TR DoFunction(T1 t1) => (TR)Function.Apply(Self, Context, [t1]);

    public void DoAction(T1 t1) => Function.Apply(Self, Context, [t1]);
}

public class FunctionWrapper<T1, T2, TR, TD>(IFunction function, Context context) : FunctionWrapper(function, context)
{
    public override Delegate CreateFunctionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoFunction");

    public override Delegate CreateActionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoAction");

    public TR DoFunction(T1 t1, T2 t2) => (TR)Function.Apply(Self, Context, [t1, t2]);

    public void DoAction(T1 t1, T2 t2) => Function.Apply(Self, Context, [t1, t2]);
}

public class FunctionWrapper<T1, T2, T3, TR, TD>(IFunction function, Context context) : FunctionWrapper(function, context)
{
    public override Delegate CreateFunctionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoFunction");

    public override Delegate CreateActionDelegate() => Delegate.CreateDelegate(typeof(TD), this, "DoAction");

    public TR DoFunction(T1 t1, T2 t2, T3 t3) => (TR)Function.Apply(Self, Context, [t1, t2, t3]);

    public void DoAction(T1 t1, T2 t2, T3 t3) => Function.Apply(Self, Context, [t1, t2, t3]);
}

