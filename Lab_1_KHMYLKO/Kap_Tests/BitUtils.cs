using Kap_ProjectForTesting;
using Kap_TestLib;
using Kap_TestLib.Attributes;
using System.Diagnostics.Metrics;

namespace Tests
{
    [TestClass]
    public class BitUtilsTests
    {
        [TestMethod]
        public void CountOnes_ValidString_ReturnsCorrect()
        {
            int count = BitUtils.CountOnes("101101");
            Assert.IsEqual(4, count);
        }

        [TestMethod]
        public void IsEvenParity_EvenOnes_ReturnsTrue()
        {
            bool result = BitUtils.IsEvenParity("1100");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEvenParity_OddOnes_ReturnsFalse()
        {
            bool result = BitUtils.IsEvenParity("1101");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Xor_EqualLength_ReturnsCorrect()
        {
            string result = BitUtils.Xor("1010", "1100");
            Assert.IsEqual("0111", result);
        }

        [TestMethod]
        public void Xor_DifferentLength_ThrowsArgumentException()
        {
            Assert.ThrowsException<System.ArgumentException>(() => BitUtils.Xor("101", "1100"));
        }

        [TestMethod]
        public void Xor_EqualLength_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => BitUtils.Xor("1010", "1100"));
        }

        [TestMethod]
        public void Increment_ChangesValue()
        {
            string binValue_1 = "1010";
            string binValue_2 = BitUtils.Xor(binValue_1, "0001");
            int oldValue = BitUtils.CountOnes(binValue_1);
            int newValue = BitUtils.CountOnes(binValue_2);
            Assert.IsNotEqual(oldValue, newValue);
        }
    }
}