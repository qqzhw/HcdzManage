
using Pvirtech.Framework.Interactivity.DefaultPopupWindows;
using Pvirtech.Metro.Controls;

namespace Pvirtech.Framework.Interactivity
{
	/// <summary>
	/// Represents an interaction request used for notifications.
	/// </summary>
	public interface INotification
	{
		string Id { get; set; }
		/// <summary>
		/// Gets or sets the title to use for the notification.
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Gets or sets the content of the notification.
		/// </summary>
		object Content { get; set; }
		bool Topmost { get; set; }
		bool HasOwner { get; set; }
		bool SecondOwner { get; set; }
		bool IsModal{get;set;}
		MetroWindow SecOwner { get; set; }
	}
}
