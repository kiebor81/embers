namespace Embers.Tests
{
    [TestClass]
    public class BlockContextTests
    {
        [TestMethod]
        public void UndefinedLocalVariable()
        {
            Context parent = new();
            BlockContext context = new(parent);

            Assert.IsFalse(context.HasLocalValue("foo"));
        }

        [TestMethod]
        public void ParentLocalVariable()
        {
            Context parent = new();
            parent.SetLocalValue("foo", "bar");
            BlockContext context = new(parent);

            Assert.IsTrue(context.HasLocalValue("foo"));
            Assert.AreEqual("bar", context.GetLocalValue("foo"));
        }

        [TestMethod]
        public void ChangeParentLocalVariable()
        {
            Context parent = new();
            parent.SetLocalValue("foo", "bar");
            BlockContext context = new(parent);
            context.SetLocalValue("foo", "newbar");

            Assert.IsTrue(context.HasLocalValue("foo"));
            Assert.AreEqual("newbar", context.GetLocalValue("foo"));
            Assert.AreEqual("newbar", parent.GetLocalValue("foo"));
        }

        [TestMethod]
        public void BlockContextLocalVariable()
        {
            Context parent = new();
            BlockContext context = new(parent);
            context.SetLocalValue("foo", "bar");

            Assert.IsTrue(context.HasLocalValue("foo"));
            Assert.IsFalse(parent.HasLocalValue("foo"));
            Assert.AreEqual("bar", context.GetLocalValue("foo"));
        }
    }
}
