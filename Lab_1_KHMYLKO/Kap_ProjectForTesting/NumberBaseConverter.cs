using System;
using System.Text;

namespace Kap_ProjectForTesting
{
    public static class NumberBaseConverter
    {
        
        public static string ConvertNumber(string number, int fromBase, int toBase)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Number cannot be empty");

            if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
                throw new NotSupportedException("Only bases 2, 8, 10, 16 are supported for input");
            if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
                throw new NotSupportedException("Only bases 2, 8, 10, 16 are supported for output");

            long decimalValue;
            try
            {
                decimalValue = Convert.ToInt64(number, fromBase);
            }
            catch (FormatException)
            {
                throw new Exception($"Number '{number}' contains invalid digits for base {fromBase}");
            }
            catch (OverflowException)
            {
                throw new OverflowException("Number is too large");
            }

        
            string result = Convert.ToString(decimalValue, toBase).ToUpper();
            return result;
        }

        public static string ToBinary(string number, int fromBase) => ConvertNumber(number, fromBase, 2);
        public static string ToOctal(string number, int fromBase) => ConvertNumber(number, fromBase, 8);
        public static string ToDecimal(string number, int fromBase) => ConvertNumber(number, fromBase, 10);
        public static string ToHex(string number, int fromBase) => ConvertNumber(number, fromBase, 16);
    }
}