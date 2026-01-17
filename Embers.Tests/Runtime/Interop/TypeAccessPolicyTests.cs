using Embers.Exceptions;
using Embers.Security;

namespace Embers.Tests
{
    [TestClass]
    public class TypeAccessPolicyTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            var machine = new Machine();
            machine.SetTypeAccessPolicy([], SecurityMode.Unrestricted);
        }

        [TestMethod]
        public void WhitelistOnly_DeniesNonWhitelistedType()
        {
            var machine = new Machine();
            machine.SetTypeAccessPolicy(["System.DateTime"], SecurityMode.WhitelistOnly);
            Assert.ThrowsException<NameError>(() => machine.ExecuteText("System::Guid.NewGuid()"));
        }

        [TestMethod]
        public void WhitelistOnly_AllowsExplicitType()
        {
            var machine = new Machine();
            machine.SetTypeAccessPolicy(["System.DateTime"], SecurityMode.WhitelistOnly);
            machine.AllowType("System.DateTime");
            var result = machine.ExecuteText("System::DateTime.Now");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        public void WhitelistOnly_AllowsNamespacePrefix()
        {
            var machine = new Machine();
            machine.SetTypeAccessPolicy(["System.*"], SecurityMode.WhitelistOnly);
            var result = machine.ExecuteText("System::DateTime.Now");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }
    }
}
