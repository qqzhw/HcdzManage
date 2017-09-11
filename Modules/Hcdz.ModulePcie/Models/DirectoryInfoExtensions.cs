using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.Models
{ 
	public static class DirectoryInfoExtensions
	{
		public static DirectoryInfo EnsureExists(this DirectoryInfo directoryInfo)
		{
			directoryInfo.Refresh();
			if (!directoryInfo.Exists) directoryInfo.Create();
			return directoryInfo;
		}
		public static void Clear(this DirectoryInfo directoryInfo) => FileHelper.ClearFolder(directoryInfo);
		public static void ClearRecursive(this DirectoryInfo directoryInfo) => FileHelper.ClearFolderRecursive(directoryInfo);
		public static long GetSize(this DirectoryInfo directoryInfo) => FileHelper.GetFolderSize(directoryInfo);
		public static long GetSizeRecursive(this DirectoryInfo directoryInfo) => FileHelper.GetFolderSizeRecursive(directoryInfo);
		public static int CountFilesRecursive(this DirectoryInfo directoryInfo) => FileHelper.CountFilesRecursive(directoryInfo);
		public static bool IsEmpty(this DirectoryInfo directoryInfo) => FileHelper.FolderIsEmpty(directoryInfo);
	}
}
