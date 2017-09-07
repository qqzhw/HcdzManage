using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.Framework
{
	public static class CollectionExtensions
	{
		public static Collection<T> AddRange<T>(this Collection<T> collection, IEnumerable<T> items)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			if (items == null) throw new ArgumentNullException("items");

			foreach (var each in items)
			{
				collection.Add(each);
			}

			return collection;
		}
	}
}
