using Hcdz.WPFServer.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
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

		public async Task<List<DirectoryInfoModel>> GetFileList(string path = "")
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			FileSystemInfo[] dirFileitems = null;
			var list = new List<DirectoryInfoModel>();
			await Task.Run(() =>
			{
				if (string.IsNullOrEmpty(path))
				{
					path = drives[0].Name;
					DirectoryInfo dirInfo = new DirectoryInfo(path);//根目录				
					dirFileitems = dirInfo.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
				}
				else
				{
					DirectoryInfo dirInfo = new DirectoryInfo(path);//根目录				 
					dirFileitems = dirInfo.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
				} 
				foreach (var item in dirFileitems)
				{
					if (item is DirectoryInfo)
					{
						var directory = item as DirectoryInfo;
						if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (directory.Attributes & FileAttributes.System) != FileAttributes.System)
						{
							list.Add(new DirectoryInfoModel()
							{
								Root = directory.Root,
								FullName = directory.FullName,
								IsDir = true,
								Icon = "pack://application:,,,/Hcdz.ModulePcie;component/Images/folder.png",
								Name = directory.Name,
								Parent = directory.Parent,
								CreationTime = directory.CreationTime,
								Exists = directory.Exists,
								Extension = directory.Extension,
								LastAccessTime = directory.LastAccessTime,
								LastWriteTime = directory.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
							});
						}
					}
					else if (item is FileInfo)
					{
						var file = item as FileInfo;
						if ((file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (file.Attributes & FileAttributes.System) != FileAttributes.System)
						{
							list.Add(new DirectoryInfoModel()
							{
								FullName = file.FullName,
								// FullPath=file.FullPath
								IsDir = false,
								Name = file.Name,
								CreationTime = file.CreationTime,
								Exists = file.Exists,
								Extension = file.Extension,
								LastAccessTime = file.LastAccessTime,
								LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
								IsReadOnly = file.IsReadOnly,
								//Directory = file.Directory,
								DirectoryName = file.DirectoryName,
								LengthText = ByteFormatter.ToString(file.Length),
								Length = file.Length
							});
						}

					}
				}
				return list;
			});
			return list;
		}
	}
}
