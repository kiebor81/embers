namespace Embers.Tests
{
    [TestClass]
    public class ContextTests
    {
        [TestMethod]
        public void GetUndefinedValueAsNull()
        {
            Context context = new();

            Assert.IsNull(context.GetValue("foo"));
            Assert.IsFalse(context.HasValue("foo"));
        }

        [TestMethod]
        public void SetAndGetValue()
        {
            Context context = new();

            context.SetLocalValue("one", 1);
            Assert.AreEqual(1, context.GetValue("one"));
            Assert.IsTrue(context.HasValue("one"));
        }

        [TestMethod]
        public void GetRootContext()
        {
            Context context = new();

            Assert.AreSame(context, context.RootContext);
        }

        [TestMethod]
        public void GetRootContextWithParent()
        {
            Context parent = new();
            Context context = new(parent);

            Assert.AreSame(parent, context.RootContext);
        }

        [TestMethod]
        public void GetRootContextWithGrandParent()
        {
            Context grandparent = new();
            Context parent = new(grandparent);
            Context context = new(parent);

            Assert.AreSame(grandparent, context.RootContext);
        }

        [TestMethod]
        public void SetValueAtParentGetValue()
        {
            Context parent = new();
            Context context = new(parent);

            parent.SetLocalValue("one", 1);
            Assert.AreEqual(1, context.GetValue("one"));
            Assert.IsTrue(parent.HasLocalValue("one"));
            Assert.IsTrue(context.HasValue("one"));
        }

        [TestMethod]
        public void GetLocalNames()
        {
            Context parent = new();
            Context context = new(parent);

            parent.SetLocalValue("one", 1);
            context.SetLocalValue("two", 2);
            context.SetLocalValue("three", 3);

            var result = context.GetLocalNames();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("two"));
            Assert.IsTrue(result.Contains("three"));
        }
    }
}
