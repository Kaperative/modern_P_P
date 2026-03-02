using System;
using System.Collections.Generic;
using System.Text;

namespace Kap_ProjectForTesting
{
    public static class BitUtils
    {
        public static int CountOnes(string binary)
        {
            if (!IsBinaryString(binary))
                throw new Exception("Input must be a binary string");
            int count = 0;
            foreach (char c in binary)
                if (c == '1') count++;
            return count;
        }

       
        public static bool IsEvenParity(string binary) => CountOnes(binary) % 2 == 0;

        public static string Xor(string a, string b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Strings must have equal length");
            char[] result = new char[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = (a[i] == b[i]) ? '0' : '1';
            }
            return new string(result);
        }

        private static bool IsBinaryString(string s)
        {
            foreach (char c in s)
                if (c != '0' && c != '1') return false;
            return true;
        }
    }
}
