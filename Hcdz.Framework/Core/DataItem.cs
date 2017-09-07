using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.Framework
{
	public partial class DataItem
	{
		public string Header { get; set; }
		public DataItem(string _header)
		{
			this.Header = _header;
		} 
	}
}
