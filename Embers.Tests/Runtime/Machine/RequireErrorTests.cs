using Embers.Exceptions;

namespace Embers.Tests
{
    [TestClass]
    public class RequireErrorTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void RequireUnsupportedExtension_ThrowsUnsupportedFileError()
        {
            Assert.ThrowsException<UnsupportedFileError>(() =>
                machine.RequireFile("MachineFiles\\foo.exe"));
        }

        [TestMethod]
        public void RequireMissingRubyFile_ThrowsFileNotFoundException()
        {
            Assert.ThrowsException<FileNotFoundException>(() =>
                machine.RequireFile("MachineFiles\\DoesNotExist.rb"));
        }
    }
}
