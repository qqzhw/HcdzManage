using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.Framework.Interactivity
{
	public class MessageNotification : Notification, INormalNotification
	{
		public bool IsMessage { get; set; }
	}
}
