using System;
using System.Collections.Generic;
using System.Text;

namespace Kap_ProjectForTesting
{
    public class Hamming74Decoder
    {
       
        public (string correctedMessage, bool errorDetected, int? errorPosition) Decode(string codeWord)
        {
            if (string.IsNullOrEmpty(codeWord) || codeWord.Length != 7 || !IsBinaryString(codeWord))
                throw new Exception("Code word must be a 7-bit binary string");

            int[] bits = new int[7];
            for (int i = 0; i < 7; i++) bits[i] = int.Parse(codeWord[i].ToString());

            int p1 = bits[4];
            int p2 = bits[5];
            int p3 = bits[6];

            int d1 = bits[0];
            int d2 = bits[1];
            int d3 = bits[2];
            int d4 = bits[3];

            int s1 = p1 ^ d1 ^ d2 ^ d4;
            int s2 = p2 ^ d1 ^ d3 ^ d4;
            int s3 = p3 ^ d2 ^ d3 ^ d4;

            int syndrome = (s1 << 2) | (s2 << 1) | s3; 

            bool errorDetected = syndrome != 0;
            int? errorPosition = null;

            if (errorDetected)
            {
                errorPosition = syndrome;
                
                int[] syndromeToIndex = { -1, 0, 1, 2, 3, 4, 5, 6 };
                if (syndrome >= 1 && syndrome <= 7)
                {
                    errorPosition = syndromeToIndex[syndrome];
                    bits[errorPosition.Value] ^= 1;
                }
               
            }

            int correctedD1 = bits[0];
            int correctedD2 = bits[1];
            int correctedD3 = bits[2];
            int correctedD4 = bits[3];

            string correctedMessage = $"{correctedD1}{correctedD2}{correctedD3}{correctedD4}";
            return (correctedMessage, errorDetected, errorPosition);
        }

        private bool IsBinaryString(string s)
        {
            foreach (char c in s)
                if (c != '0' && c != '1') return false;
            return true;
        }
    }

}
