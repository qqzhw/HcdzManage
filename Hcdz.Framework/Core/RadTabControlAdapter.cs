using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using Pvirtech.Framework.Core;

namespace Pvirtech.Framework
{
	public class RadTabControlAdapter : RegionAdapterBase<RadTabControl>
	{
		public RadTabControlAdapter(IRegionBehaviorFactory regionBehaviorFactory):base(regionBehaviorFactory)
		{
		   
		}
		public static Style GetItemContainerStyle(DependencyObject target)
		{
			if (target == null) throw new ArgumentNullException("target");
			return (Style)target.GetValue(RadTabControl.ItemContainerStyleProperty);
		}
		protected override void Adapt(IRegion region, RadTabControl regionTarget)
		{
			if (regionTarget == null) throw new ArgumentNullException("regionTarget");
			bool itemsSourceIsSet = regionTarget.ItemsSource != null;

			if (itemsSourceIsSet)
			{
				throw new InvalidOperationException("ItemsControlHasItemsSourceException");
			}
		}

		protected override void AttachBehaviors(IRegion region, RadTabControl regionTarget)
		{
			if (region == null) throw new ArgumentNullException("region");
			base.AttachBehaviors(region, regionTarget);
			if (!region.Behaviors.ContainsKey(RadTabControlRegionSyncBehavior.BehaviorKey))
			{
				region.Behaviors.Add(RadTabControlRegionSyncBehavior.BehaviorKey, new RadTabControlRegionSyncBehavior { HostControl = regionTarget });
			}
		}

		protected override IRegion CreateRegion()
		{
			return new SingleActiveRegion(); 
		}
	}
}
