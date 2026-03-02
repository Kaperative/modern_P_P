using Kap_TestLib;
using Kap_TestLib.Attributes;
using Kap_ProjectForTesting;

namespace Kap_Tests
{
    [TestClass]
    public class Hamming74EncoderTests
    {
        private Hamming74Encoder _encoder;

        [TestSetUpAttribute]
        public void Setup()
        {
            _encoder = new Hamming74Encoder();
        }

        [TestTearDownAttribute]
        public void Cleanup()
        {
            _encoder = null;
        }

        [TestMethod("Check correct P vector")]
        public void Encode_ValidMessage_ReturnsCorrectCodeWord()
        {
            string code = _encoder.Encode("1011");
            Assert.IsEqual("1011010", code);
        }

        [TestDataAttribute("0000", "0000000")]
        [TestDataAttribute("1111", "1111111")]
        [TestDataAttribute("1010", "1010101")] 
        [TestMethod("Encode_Hamming")]
        public void Encode_Parameterized(string message, string expectedCode)
        {
            string code = _encoder.Encode(message);
            Assert.IsEqual(expectedCode, code);
        }

        [TestMethod("Encode_Hamming")]
        public void Encode_InvalidLength_ThrowsException()
        {
            Assert.ThrowsException<Exception>(() => _encoder.Encode("101"));
        }
    }
}