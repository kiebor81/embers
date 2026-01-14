using System.Collections;
using Embers.Language;

namespace Embers.Expressions
{
    /// <summary>
    /// Indices an expression with another expression, allowing for dynamic access to elements in collections or strings.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class IndexedExpression(IExpression expression, IExpression indexexpression) : IExpression
    {
        private static readonly int hashcode = typeof(IndexedExpression).GetHashCode();
        private readonly IExpression expression = expression;
        private readonly IExpression indexexpression = indexexpression;

        public IExpression Expression { get { return expression; } }

        public IExpression IndexExpression { get { return indexexpression; } }

        public object? Evaluate(Context context)
        {
            object value = expression.Evaluate(context);
            object indexvalue = indexexpression.Evaluate(context);

            // Handle Proc: f[arg] is equivalent to f.call(arg)
            if (value is Proc proc)
            {
                return proc.Call([indexvalue]);
            }

            // Convert long to int for indexing (arrays need int indices)
            if (indexvalue is long l)
                indexvalue = (int)l;

            if (indexvalue is int)
            {
                int index = (int)indexvalue;

                if (value is string)
                {
                    string text = (string)value;

                    if (index >= text.Length)
                        return null;

                    if (index < 0)
                    {
                        index = text.Length + index;

                        if (index < 0)
                            return null;
                    }

                    return text[index].ToString();
                }

                var list = (IList)expression.Evaluate(context);

                if (index >= list.Count)
                    return null;

                if (index < 0)
                {
                    index = list.Count + index;

                    if (index < 0)
                        return null;
                }

                return list[index];
            }

            var dict = (IDictionary)value;

            if (dict.Contains(indexvalue))
                return dict[indexvalue];

            return null;
        }

        public IList<string> GetLocalVariables()
        {
            var list = new List<IExpression>() { indexexpression, expression };

            return BaseExpression.GetLocalVariables(list);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is IndexedExpression)
            {
                var expr = (IndexedExpression)obj;

                return expression.Equals(expr.expression) && indexexpression.Equals(expr.indexexpression);
            }

            return false;
        }

        public override int GetHashCode() => expression.GetHashCode() + indexexpression.GetHashCode() + hashcode;
    }
}
