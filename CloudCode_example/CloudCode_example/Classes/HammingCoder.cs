using System.Collections;
using System.Collections.Generic;

namespace CloudCode_example.Classes
{
    // 7/4 Hamming alhoritm - allows us to fix 1 error every 7 bits if information.
    // It's adding 3 control bits to every 4 bits of information
    internal class HammingCoder
    {
        public class HammingEncoder
        {
            public byte[] Encode(byte[] bytes)  //Encoder
            {
                var bitArray = new BitArray(bytes);
                var encodedBits = new List<bool>();

                for (int i = 0; i < bitArray.Length; i += 4)
                {
                    bool[] dataBits = new bool[4];
                    for (int j = 0; j < 4; j++)
                    {
                        if (i + j < bitArray.Length)
                        {
                            dataBits[j] = bitArray[i + j];
                        }
                        else
                        {
                            dataBits[j] = false;  // padding bit
                        }
                    }

                    bool[] encodedBlock = EncodeBlock(dataBits);
                    encodedBits.AddRange(encodedBlock);
                }

                return encodedBits.ToByteArray();
            }

            private bool[] EncodeBlock(bool[] dataBits) //Algo
            {
                bool[] encodedBits = new bool[7];

                encodedBits[2] = dataBits[0];
                encodedBits[4] = dataBits[1];
                encodedBits[5] = dataBits[2];
                encodedBits[6] = dataBits[3];

                encodedBits[0] = encodedBits[2] ^ encodedBits[4] ^ encodedBits[6];
                encodedBits[1] = encodedBits[2] ^ encodedBits[5] ^ encodedBits[6];
                encodedBits[3] = encodedBits[4] ^ encodedBits[5] ^ encodedBits[6];

                return encodedBits;
            }
        }

        public class HammingDecoder
        {
            public byte[] Decode(byte[] bytes) //Decoder
            {
                var encodedBits = new BitArray(bytes);
                var decodedBits = new List<bool>();

                for (int i = 0; i < encodedBits.Length; i += 7)
                {
                    bool[] encodedBlock = new bool[7];
                    for (int j = 0; j < 7; j++)
                    {
                        if (i + j < encodedBits.Length)
                        {
                            encodedBlock[j] = encodedBits[i + j];
                        }
                        else
                        {
                            encodedBlock[j] = false;  // padding bit
                        }
                    }

                    bool[] decodedBlock = DecodeBlock(encodedBlock);
                    decodedBits.AddRange(decodedBlock);
                }

                return decodedBits.ToByteArray();
            }

            private bool[] DecodeBlock(bool[] encodedBits) //Algo
            {
                bool[] decodedBits = new bool[4];

                bool p0 = encodedBits[0] ^ encodedBits[2] ^ encodedBits[4] ^ encodedBits[6];
                bool p1 = encodedBits[1] ^ encodedBits[2] ^ encodedBits[5] ^ encodedBits[6];
                bool p3 = encodedBits[3] ^ encodedBits[4] ^ encodedBits[5] ^ encodedBits[6];

                int errorBit = (p3 ? 4 : 0) + (p1 ? 2 : 0) + (p0 ? 1 : 0);

                if (errorBit != 0)
                {
                    encodedBits[errorBit - 1] = !encodedBits[errorBit - 1];
                }

                decodedBits[0] = encodedBits[2];
                decodedBits[1] = encodedBits[4];
                decodedBits[2] = encodedBits[5];
                decodedBits[3] = encodedBits[6];

                return decodedBits;
            }
        }
    }

    public static class ListBoolExtensions //Transformation List<bool> to byte[]
    {
        public static byte[] ToByteArray(this List<bool> bools)
        {
            int numBytes = bools.Count / 8;
            if (bools.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteValue = 0;
            int count = 0;

            foreach (bool b in bools)
            {
                if (b) byteValue |= 1 << (7 - count);
                count++;

                if (count == 8)
                {
                    bytes[bytes.Length - numBytes] = (byte)byteValue;
                    numBytes--;
                    byteValue = 0;
                    count = 0;
                }
            }

            if (count > 0) bytes[bytes.Length - numBytes] = (byte)byteValue;

            return bytes;
        }
    }
}
