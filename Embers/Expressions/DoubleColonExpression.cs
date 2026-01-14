using Embers.Exceptions;
using Embers.Language;
using Embers.Utilities;

namespace Embers.Expressions
{
    /// <summary>
    /// DoubleColonExpression represents a double colon (::) expression.
    /// This expression is used to access constants, static members, or .NET types.
    /// Supports both Ruby-style constant access and .NET type access (e.g., System::DateTime::Now).
    /// </summary>
    /// <seealso cref="Embers.Expressions.BaseExpression" />
    /// <seealso cref="Embers.Expressions.INamedExpression" />
    public class DoubleColonExpression : BaseExpression, INamedExpression
    {
        private static readonly int hashcode = typeof(DoubleColonExpression).GetHashCode();

        private readonly IExpression expression;
        private readonly string name;
        private readonly string qname;

        public DoubleColonExpression(IExpression expression, string name)
        {
            this.expression = expression;
            this.name = name;
            qname = AsQualifiedName();
        }

        public IExpression TargetExpression { get { return expression; } }

        public string Name { get { return name; } }

        public override object Evaluate(Context context)
        {
            // Try to resolve as .NET type first if it looks like a qualified type name
            if (qname != null)
            {
                try
                {
                    Type type = TypeUtilities.AsType(qname);

                    if (type != null)
                        return type;
                }
                catch (TypeAccessError)
                {
                    // Not a .NET type or access denied - fall through to Ruby constant resolution
                }
            }

            //_ = [];
            var result = expression.Evaluate(context);

            if (result is Type)
            {
                // Check if it's an enum value
                Type typeResult = (Type)result;
                if (typeResult.IsEnum)
                    return TypeUtilities.ParseEnumValue(typeResult, name);
                
                // Otherwise, try to access static member
                return TypeUtilities.InvokeTypeMember(typeResult, name, []);
            }

            var obj = (DynamicClass)result;

            if (!obj.Constants.HasLocalValue(name))
                throw new NameError(string.Format("unitialized constant {0}::{1}", obj.Name, name));

            return obj.Constants.GetLocalValue(name);
        }

        public string? AsQualifiedName()
        {
            if (!char.IsUpper(name[0]))
                return null;

            if (expression is NameExpression nameExpr)
            {
                string prefix = nameExpr.AsQualifiedName();

                if (prefix == null)
                    return null;

                return prefix + "::" + name;
            }

            if (expression is DoubleColonExpression dcExpr)
            {
                string prefix = dcExpr.AsQualifiedName();

                if (prefix == null)
                    return null;

                return prefix + "::" + name;
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is DoubleColonExpression)
            {
                var expr = (DoubleColonExpression)obj;

                return name.Equals(expr.name) && expression.Equals(expr.expression);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int result = name.GetHashCode() + expression.GetHashCode() + hashcode;

            return result;
        }
    }
}
