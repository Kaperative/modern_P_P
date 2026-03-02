using System;
using System.Text;

namespace Kap_ProjectForTesting
{
    public static class GrayCodeConverter
    {
       
        public static string BinaryToGray(string binary)
        {
            if (string.IsNullOrEmpty(binary) || !IsBinaryString(binary))
                throw new Exception("Input must be a non-empty binary string");

            StringBuilder gray = new StringBuilder();
            gray.Append(binary[0]);
            for (int i = 1; i < binary.Length; i++)
            {

                char bit = (binary[i] == binary[i - 1]) ? '0' : '1';
                gray.Append(bit);
            }
            return gray.ToString();
        }

        public static string GrayToBinary(string gray)
        {
            if (string.IsNullOrEmpty(gray) || !IsBinaryString(gray))
                throw new Exception("Input must be a non-empty binary string");

            StringBuilder binary = new StringBuilder();
            binary.Append(gray[0]); 
            for (int i = 1; i < gray.Length; i++)
            {
                
                char bit = (gray[i] == binary[i - 1]) ? '0' : '1';
                binary.Append(bit);
            }
            return binary.ToString();
        }

        private static bool IsBinaryString(string s)
        {
            foreach (char c in s)
                if (c != '0' && c != '1') return false;
            return true;
        }
    }
}