using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;

namespace BitcoinCryptography
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string hexString = "0C28FCA386C7A227600B2FE50B7CAE11EC86D3BF1FBE471BE89827E19D72AA1D";
            BigInteger theCounter = 0;
            string thePrivateKeyWIF;

            thePrivateKeyWIF = GetWIFString(theCounter);
            Console.WriteLine(thePrivateKeyWIF);

            //Cryptography.ECDSA.Secp256K1Manager.GetPublicKey()
        }

        static BigInteger hexStringToBigInt(string hexString)
        {
            return BigInteger.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        static byte[] BigIntToPrivateKey(BigInteger theNumber)
        {
            List<byte> bytes = new List<byte>(theNumber.ToByteArray());
            bytes.Reverse();
            while (bytes.Count < 32)
            {
                bytes.Insert(0, 0);
            }
            return bytes.ToArray();
        }


        static string GetWIFString(BigInteger theNumber)
        {
            string wifString;
            List<byte> bytes = new List<byte>(theNumber.ToByteArray());
            bytes.Reverse();
            while (bytes.Count < 32)
            {
                bytes.Insert(0, 0);
            }

            //Console.WriteLine(BitConverter.ToString(bytes.ToArray()));

            bytes.Insert(0, 0x80);
            //Console.WriteLine(BitConverter.ToString(bytes.ToArray()));

            using (SHA256 mySHA256 = SHA256.Create())
            {
                var result1 = mySHA256.ComputeHash(bytes.ToArray());
                //Console.WriteLine(BitConverter.ToString(result1));
                var result2 = mySHA256.ComputeHash(result1);
                //Console.WriteLine(BitConverter.ToString(result2));
                for (int i = 0; i < 4; i++)
                {
                    bytes.Add(result2[i]);
                }

                //Console.WriteLine(BitConverter.ToString(bytes.ToArray()));
                wifString = Base58Check.Base58CheckEncoding.EncodePlain(bytes.ToArray());
                //Console.WriteLine(wifString);
            }

            return wifString;
        }
    }
}
