using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.Models
{

	public class DirectoryInfoEventArgs : EventArgs
	{
		public DirectoryInfo DirectoryInfo;
		public DirectoryInfoEventArgs(DirectoryInfo directoryInfo)
		{
			this.DirectoryInfo = directoryInfo;
		}
	}
}
