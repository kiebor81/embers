namespace Embers.Language
{
    /// <summary>
    /// ProcClass represents the Proc class in the Embers language.
    /// Procs are first-class callable objects that wrap blocks.
    /// </summary>
    public class ProcClass : NativeClass
    {
        public ProcClass(Machine machine)
            : base("Proc", machine)
        {
            SetInstanceMethod("call", Call);
            SetInstanceMethod("[]", Call); // Ruby alias for call
        }

        /// <summary>
        /// Calls the proc with the given arguments.
        /// The proc itself is a Proc object wrapping a Block.
        /// </summary>
        private static object Call(object obj, IList<object> values)
        {
            var proc = (Proc)obj;
            return proc.Call(values);
        }
    }
}
