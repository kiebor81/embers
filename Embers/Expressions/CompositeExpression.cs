namespace Embers.Expressions
{
    /// <summary>
    /// CompositeExpression represents a sequence of expressions that are evaluated in order of precedence.
    /// A CompositeExpression can contain multiple commands, and the result of the last command is returned.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class CompositeExpression(IList<IExpression> commands) : IExpression
    {
        private readonly IList<IExpression> commands = commands;

        public IList<IExpression> Commands => commands;

        public object Evaluate(Context context)
        {
            object result = null;

            foreach (var command in commands)
                result = command.Evaluate(context);

            return result;
        }

        public IList<string> GetLocalVariables() => BaseExpression.GetLocalVariables(commands);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is CompositeExpression)
            {
                var cmd = (CompositeExpression)obj;

                if (commands.Count != cmd.commands.Count)
                    return false;

                for (int k = 0; k < commands.Count; k++)
                    if (!commands[k].Equals(cmd.commands[k]))
                        return false;

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int result = 0;

            foreach (var command in commands)
            {
                result *= 17;
                result += command.GetHashCode();
            }

            return result;
        }
    }
}
