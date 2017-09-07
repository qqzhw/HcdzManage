using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pvirtech.Framework
{
	public partial class MergeDictionaryRegistry : IMergeDictionaryRegistry
	{
		public void AddDictionaryResource(Uri packUri)
		{
			Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
			{
				Source = packUri
			});
		}
	}
}
