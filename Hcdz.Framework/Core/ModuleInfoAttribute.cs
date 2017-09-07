using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.Framework
{
	public class ModuleInfoAttribute:Attribute
	{
		/// <summary>
		/// 模块ID
		/// </summary>
		public string Id { get; set; }
		/// <summary>
		/// 模块显示名称
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// 模块初始选项
		/// </summary>
		public InitializationMode InitMode { get; set; }
		public bool IsDefaultShow { get; set; }
		//public bool IsReadOnly { get; set; }
		/// <summary>
		/// 模块状态
		/// </summary>
		public ModuleState State { get; set; }
        public int DisplayOrder { get; set; } 
	}
}
