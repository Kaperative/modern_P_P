using System;
using System.Collections.Generic;
using System.Text;

namespace Kap_ProjectForTesting
{
    public class Hamming74Encoder
    {
        
        public string Encode(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length != 4 || !IsBinaryString(message))
                throw new Exception("Message must be a 4-bit binary string");

            int[] data = new int[4];
            for (int i = 0; i < 4; i++) data[i] = int.Parse(message[i].ToString());

            int d1 = data[0];
            int d2 = data[1];
            int d3 = data[2];
            int d4 = data[3];

            int p1 = d1 ^ d2 ^ d4;
            int p2 = d1 ^ d3 ^ d4;
            int p3 = d2 ^ d3 ^ d4;

            int[] code = {  d1, d2, d3, d4, p1, p2, p3 };
            return string.Join("", code);
        }

        private bool IsBinaryString(string s)
        {
            foreach (char c in s)
                if (c != '0' && c != '1') return false;
            return true;
        }
    }
}
