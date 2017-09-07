using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.Framework
{
	public class SystemInfo
	{
		/// <summary>
		/// 模块ID
		/// </summary>
		public string Id { get; set; }
        public int DisplayOrder { get; set; }
		public string Title { get; set; }
		public InitializationMode InitMode { get; set; }
		public bool IsDefaultShow { get; set; }
		public bool IsReadOnly { get; set; }
		public ModuleState State { get; set; }
		public int MsgCount { get; set; } = 0;
		public ModuleInfo ModuleInfo { get; set; }
	}
}
