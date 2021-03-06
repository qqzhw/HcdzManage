﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.Models
{
   	public static class FileInfoExtensions
	{
		public static void Hide(this FileInfo file) => FileHelper.HideFile(file);
		public static void Show(this FileInfo file) => FileHelper.ShowFile(file);
		public static bool IsHidden(this FileInfo file) => FileHelper.IsHidden(file);
		public static void MakeReadOnly(this FileInfo file) => FileHelper.MakeReadOnly(file);
		public static void MakeWritable(this FileInfo file) => FileHelper.MakeWritable(file);
		public static bool IsReadOnly(this FileInfo file) => FileHelper.IsReadOnly(file);
	}
}
