using Pvirtech.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pvirtech.Framework.Interactivity
{
	/// <summary>
	/// Basic implementation of <see cref="INotification"/>.
	/// </summary>
	public class Notification : INotification
	{
		public string Id { get; set; }
		/// <summary>
		/// Gets or sets the title to use for the notification.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the content of the notification.
		/// </summary>
		public object Content { get; set; }

		public bool Topmost { get; set; }
		
		public bool SecondOwner { get; set; }
		public bool HasOwner { get; set; }
		public MetroWindow SecOwner { get; set; }
		public bool IsModal { get; set; }
		 
	}
}
