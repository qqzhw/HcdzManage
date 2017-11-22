using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.WPFServer.Models
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
        public static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        public static byte[] StringToByte(string InString)
		{
			string[] ByteStrings;
			ByteStrings = InString.Split(" ".ToCharArray());
			byte[] ByteOut;
			ByteOut = new byte[ByteStrings.Length - 1];
			for (int i = 0; i <ByteStrings.Length - 1; i++)
			{
				ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]),16);
			}
			return ByteOut;
		}
        public static bool FormatDrive(string driveLetter, string fileSystem = "NTFS", bool quickFormat = true, int clusterSize = 8192, string label = "", bool enableCompression = false)
        {
            if (driveLetter.Length != 2 || driveLetter[1] != ':' || !char.IsLetter(driveLetter[0]))
                return false;

            //format given drive    
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"select * from Win32_Volume WHERE DriveLetter = '" + driveLetter + "'");

                foreach (ManagementObject vi in searcher.Get())
                {
                    vi.InvokeMethod("Format", new object[] { fileSystem, quickFormat, clusterSize, label, enableCompression });
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex, "格式化磁盘");
                return false;
            } 
         
        }

    }
}
