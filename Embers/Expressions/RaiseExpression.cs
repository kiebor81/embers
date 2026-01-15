namespace Embers.Expressions;
/// <summary>
/// RaiseExpression is used to raise an exception in the Embers language.
/// It can take either a single argument (which can be a string or an exception instance)
/// It represents the 'raise' statement in the language, allowing for exception handling and explicit throws.
/// </summary>
/// <seealso cref="BaseExpression" />
public class RaiseExpression(IExpression exceptionTypeOrMessage, IExpression? messageExpr) : BaseExpression
{
    private readonly IExpression exceptionTypeOrMessage = exceptionTypeOrMessage;
    private readonly IExpression? messageExpr = messageExpr;

    public override object? Evaluate(Context context)
    {
        if (messageExpr == null)
        {
            // Only one argument: could be a string or an exception instance
            var value = exceptionTypeOrMessage.Evaluate(context);
            if (value is Exception ex)
                throw ex;
            throw new Exception(value?.ToString() ?? "raised");
        }
        else
        {
            // Two arguments: first is exception type, second is message
            var typeValue = exceptionTypeOrMessage.Evaluate(context);
            var messageValue = messageExpr.Evaluate(context)?.ToString() ?? "";

            // If typeValue is a Type and is a subclass of Exception, instantiate it
            if (typeValue is Type type && typeof(Exception).IsAssignableFrom(type))
            {
                // Try to find a constructor with a string parameter
                var ctor = type.GetConstructor([typeof(string)]);
                if (ctor != null)
                    throw (Exception)ctor.Invoke([messageValue]);
                // Otherwise, try parameterless constructor
                ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                    throw (Exception)ctor.Invoke(null);
            }
            // If typeValue is an Exception instance, throw it
            if (typeValue is Exception ex)
                throw ex;
            // Otherwise, fallback to generic Exception
            throw new Exception(messageValue);
        }
    }
}