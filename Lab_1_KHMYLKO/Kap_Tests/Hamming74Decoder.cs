using Kap_TestLib;
using Kap_TestLib.Attributes;
using Kap_ProjectForTesting;

namespace Kap_Tests
{
    [TestClass]
    public class Hamming74DecoderTests
    {
        private Hamming74Decoder _decoder;

        [TestSetUpAttribute]
        public void Setup() => _decoder = new Hamming74Decoder();

        [TestTearDownAttribute]
        public void Cleanup() => _decoder = null;

        [TestMethod]
        public void Decode_NoError_ReturnsCorrectMessage()
        {
            var (message, errorDetected, pos) = _decoder.Decode("1011010");
            Assert.IsEqual("1011", message);
            Assert.IsFalse(errorDetected);
            Assert.IsNull(pos);
        }

        [TestMethod]
        public void Decode_SingleError_CorrectsAndDetects()
        {
            // D = 1101: P = 1101_100
            //  1101_100 Add error bit (d2): 1111_100
            var (message, errorDetected, pos) = _decoder.Decode("1111100");
            Assert.IsEqual("1101", message);
            Assert.IsTrue(errorDetected);
            Assert.IsNotNull(pos);
        }

        [TestMethod]
        public void Decode_InvalidCodeWord_ThrowsException()
        {
            Assert.ThrowsException<Exception>(() => _decoder.Decode("101"));
        }
    }
}