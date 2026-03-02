using System;
using Kap_TestLib;
using Kap_TestLib.Attributes;
using Kap_ProjectForTesting;

namespace Kap_Tests
{
    [TestClass]
    public class NumberBaseConverterTests
    {
        [TestMethod]
        public void ConvertNumber_BinaryToOctal_ReturnsCorrect()
        {
            string result = NumberBaseConverter.ConvertNumber("1010", 2, 8);
            Assert.IsEqual("12", result);
        }

        [TestMethod]
        public void ConvertNumber_HexToDecimal_ReturnsCorrect()
        {
            string result = NumberBaseConverter.ConvertNumber("FF", 16, 10);
            Assert.IsEqual("255", result);
        }

        [TestMethod]
        public void ConvertNumber_InvalidDigit_ThrowsException()
        {
            Assert.ThrowsException<Exception>(() => NumberBaseConverter.ConvertNumber("102", 2, 10));
        }

        [TestDataAttribute("1010", 2, 8, "12")]
        [TestDataAttribute("1111", 2, 16, "F")]
        [TestMethod]
        public void ConvertNumber_Parameterized(string number, int fromBase, int toBase, string expected)
        {
            string result = NumberBaseConverter.ConvertNumber(number, fromBase, toBase);
            Assert.IsEqual(expected, result);
        }

        [TestMethod]
        public void ToBinary_FromHex_ReturnsBinary()
        {
            string result = NumberBaseConverter.ToBinary("A", 16);
            Assert.IsEqual("1010", result);
        }
    }
}