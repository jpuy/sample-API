using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SocialDistancing.API.Common.Middlewares
{
    public class ApiKeyValidation : IApiKeyValidation
    {
        static readonly byte[] axesKey = new byte[] {0x8B, 0xCA, 0xEB, 0x8C, 0x17, 0xE3, 0x55, 0x16, 0x67, 0x72, 0x1, 0xA1, 0x16, 0x49, 0xA2, 0x93,
                                                     0xD5, 0x72, 0x69, 0xD2, 0xB9, 0x74, 0x9A, 0x9E, 0xF1, 0x3B, 0x5F, 0x43, 0x16, 0xEA, 0x72, 0x84};

        static readonly byte[] axesVI = { 0xDC, 0x53, 0xF6, 0x9F, 0x11, 0x74, 0x52, 0xC3, 0xEB, 0x74, 0x92, 0xBC, 0x39, 0x65, 0x94, 0x19 };

        private byte[] AxesAESGetKey(UInt32 NoTerminal)
        {
            byte[] keyForTerminal = axesKey.ToArray();
            keyForTerminal[0] = (byte)(keyForTerminal[0] ^ (NoTerminal >> 24));
            keyForTerminal[1] = (byte)(keyForTerminal[1] ^ (NoTerminal >> 8));
            keyForTerminal[2] = (byte)(keyForTerminal[2] ^ (NoTerminal >> 16));
            keyForTerminal[3] = (byte)(keyForTerminal[3] ^ (NoTerminal >> 0));
            return keyForTerminal;
        }
        private byte[] AxesAESEncrypt(byte[] InData, byte[] Key)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            MemoryStream memStream = new MemoryStream();

            rijndaelManaged.Mode = CipherMode.CBC;
            rijndaelManaged.BlockSize = 128;
            rijndaelManaged.KeySize = 256;
            rijndaelManaged.IV = axesVI;
            rijndaelManaged.Key = Key;
            rijndaelManaged.Padding = PaddingMode.Zeros;

            CryptoStream cryptoStream = new CryptoStream(memStream, rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV), CryptoStreamMode.Write);
            cryptoStream.Write(InData, 0, InData.Length);
            cryptoStream.Close();

            return memStream.ToArray();
        }

        private byte[] AxesAESDecrypt(byte[] InData, byte[] Key)
        {
            if (InData.Length == 16)
            {
                RijndaelManaged rijndaelManaged = new RijndaelManaged();
                MemoryStream memStream = new MemoryStream();
                rijndaelManaged.Mode = CipherMode.CBC;
                rijndaelManaged.BlockSize = 128;
                rijndaelManaged.KeySize = 256;
                rijndaelManaged.IV = axesVI;
                rijndaelManaged.Key = Key;
                rijndaelManaged.Padding = PaddingMode.Zeros;
                CryptoStream cryptoStream = new CryptoStream(memStream, rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV), CryptoStreamMode.Write);
                cryptoStream.Write(InData, 0, InData.Length);
                cryptoStream.Close();
                return memStream.ToArray();
            }
            else
            {
                return null;
            }
        }
        public string GenerateApiKey()
        {
            byte[] uaBinaryData = new byte[16];
            Random rand = new Random();
            UInt32 uDateTimeUnix;

            rand.NextBytes(uaBinaryData);
            uaBinaryData[4] = (byte)'A';
            uaBinaryData[5] = (byte)'X';
            uaBinaryData[6] = (byte)'E';
            uaBinaryData[7] = (byte)'S';
            uaBinaryData[8] = 0;
            uaBinaryData[9] = 0;
            uaBinaryData[10] = 0;
            uaBinaryData[11] = 0;
            uDateTimeUnix = (UInt32)(TimeZoneInfo.ConvertTimeToUtc(DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            Buffer.BlockCopy(BitConverter.GetBytes(uDateTimeUnix), 0, uaBinaryData, 12, 4);
            byte[] decryptedData = AxesAESEncrypt(uaBinaryData, axesKey);
            return Convert.ToBase64String(decryptedData);
        }

        public bool ValidateApiKey(string key)
        {
            string sBase64Data = key.Substring(5);
            string sSignature = "INVALID";
            string sSAMNumberDecrypted = "";

            DateTime dReceivedDateTime = DateTime.MinValue;
            try
            {
                byte[] binaryData = Convert.FromBase64String(sBase64Data);
                byte[] decryptedData = AxesAESDecrypt(binaryData, axesKey);
                if (decryptedData != null)
                {
                    if (decryptedData.Length == 16)
                    {
                        sSignature = "" + (char)decryptedData[4] + (char)decryptedData[5] + (char)decryptedData[6] + (char)decryptedData[7];
                        sSAMNumberDecrypted = ((UInt32)decryptedData[8] + (UInt32)(decryptedData[9] << 8) + (UInt32)(decryptedData[10] << 16) + (UInt32)(decryptedData[11] << 24)).ToString();
                        UInt32 uUnixTimeStamp = (UInt32)decryptedData[12] + (UInt32)(decryptedData[13] << 8) + (UInt32)(decryptedData[14] << 16) + (UInt32)(decryptedData[15] << 24);
                        dReceivedDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(uUnixTimeStamp);
                    }
                }
            }
            catch (Exception)
            {
            }

            if (sSignature.ToUpper() == "AXES" && Math.Abs(DateTime.Now.Subtract(dReceivedDateTime).TotalSeconds) <= 600)
            {
                return true;
            }
            return false;
        }


    }
}
