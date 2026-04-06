using Kap_TestLib;
using Kap_TestLib.Attributes;
using Kap_ProjectForTesting;

namespace Kap_Tests
{
    [TestClass]
    public class GrayCodeConverterTests
    {
       
        [TestMethod]

        public void ConvertNumber_ReturnsString()
        {         
            var result = NumberBaseConverter.ConvertNumber("1010", 2, 16);
            Assert.IsInstanceOfType(result, typeof(string));
            
        }



        [TestMethod]
        public void BinaryToGray_Simple_ReturnsCorrect()
        {
  
            string gray = GrayCodeConverter.BinaryToGray("1011");
            Assert.IsEqual("1110", gray);
        }

        [TestMethod]
        public void GrayToBinary_Simple_ReturnsCorrect()
        {
            string binary = GrayCodeConverter.GrayToBinary("1110");
            Assert.IsEqual("1011", binary);
        }

        [TestDataAttribute("0000", "0000")]
        [TestDataAttribute("0001", "0001")]
        [TestDataAttribute("0010", "0011")]
        [TestDataAttribute("0011", "0010")]
        [TestDataAttribute("0100", "0110")]
        [TestMethod]
        public void BinaryToGray_Parameterized(string binary, string expectedGray)
        {
            string gray = GrayCodeConverter.BinaryToGray(binary);
            Assert.IsEqual(expectedGray, gray);
        }

        [TestMethod]
        public void BinaryToGray_EmptyString_ThrowsException()
        {
            Assert.ThrowsException<Exception>(() => GrayCodeConverter.BinaryToGray(""));
        }

        [TestMethod]
        public void GrayToBinary_InvalidChar_ThrowsException()
        {
            Assert.ThrowsException<Exception>(() => GrayCodeConverter.GrayToBinary("102"));
        }
    }
}