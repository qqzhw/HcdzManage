using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.ViewModels
{
    public class CommonHelper
    {
        public string ByteToString(byte[] InBytes, int len)
        {
            string StringOut = "";
            for (int i = 0; i < len; i++)
            {
                StringOut = StringOut + String.Format("{0:X2} ", InBytes[i]);
            }
            return StringOut;
        }
        public static string ByteToString(byte[] InBytes)
        {
            string StringOut = "";
            foreach (byte InByte in InBytes)
            {
                StringOut = StringOut + String.Format("{0:X2} ", InByte);
            }
            return StringOut;
        }
        public static byte[] StringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut = new byte[ByteStrings.Length - 1];
            for (int i = 0; i == ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
                // ByteOut[i] =Convert.ToByte(ByteStrings[i]);
            }
            return ByteOut;
        }
        public static byte[] hexStr2Bytes(string src)
        {
            int m = 0, n = 0;
            int l = src.Length / 2;
            byte[] ret = new byte[l];
            for (int i = 0; i < l; i++)
            {
                m = i * 2 + 1;
                n = m + 1;
                //  ret[i] = Byte.decode("0x" + src.substring(i * 2, m) + src.substring(m, n));
            }
            return ret;
        }
    }
}
