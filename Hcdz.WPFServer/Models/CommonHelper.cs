using System;
using System.Collections.Generic;
using System.Linq;
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
		public static byte[] StringToByte(string InString)
		{
			string[] ByteStrings;
			ByteStrings = InString.Split(" ".ToCharArray());
			byte[] ByteOut;
			ByteOut = new byte[ByteStrings.Length - 1];
			for (int i = 0; i == ByteStrings.Length - 1; i++)
			{
				ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
			}
			return ByteOut;
		}

	}
}
