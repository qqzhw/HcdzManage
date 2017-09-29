using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hcdz.WPFServer
{ 
	public class MyHub : Hub
	{
		public void Send(string name, string message)
		{
			Clients.All.addMessage(name, message);
		}
		public override Task OnConnected()
		{
			//Use Application.Current.Dispatcher to access UI thread from outside the MainWindow class
			Application.Current.Dispatcher.Invoke(() =>
				((MainWindow)Application.Current.MainWindow).WriteToConsole("Client connected: " + Context.ConnectionId));
			//发送消息成功  
			var hub = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
			hub.Clients.All.NoticeMessage("asdfasdf");
			return base.OnConnected();
		}
		 
		public override Task OnDisconnected(bool stopCalled)
		{
			//Use Application.Current.Dispatcher to access UI thread from outside the MainWindow class
			Application.Current.Dispatcher.Invoke(() =>
				((MainWindow)Application.Current.MainWindow).WriteToConsole("Client disconnected: " + Context.ConnectionId));

			return base.OnDisconnected(stopCalled);
		}

	}
}
