using Hcdz.PcieLib;
using Hcdz.WPFServer.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wdc_err = Jungo.wdapi_dotnet.WD_ERROR_CODES;
using DWORD = System.UInt32;
using Jungo.wdapi_dotnet;
using Hcdz.WPFServer.Properties;

namespace Hcdz.WPFServer
{ 
	public class MyHub : Hub
	{
		private readonly static Dictionary<PCIE_Device, List<DeviceChannelModel>> DeviceChannelList = new Dictionary<PCIE_Device, List<DeviceChannelModel>>();
		public  MyHub()
        { 
        }
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
			hub.Clients.Client(Context.ConnectionId).Connected(true);
			return base.OnConnected();
		}
		 
		public override Task OnDisconnected(bool stopCalled)
		{
			//Use Application.Current.Dispatcher to access UI thread from outside the MainWindow class
			Application.Current.Dispatcher.Invoke(() =>
				((MainWindow)Application.Current.MainWindow).WriteToConsole("Client disconnected: " + Context.ConnectionId));

			return base.OnDisconnected(stopCalled);
		}

        public DriveInfo[] GetDrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            return drives;
        }
        public bool FormatDrive(string driveName)
        {
            return CommonHelper.FormatDrive(driveName);
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

        public   DWORD InitLoad()
        {
			//PCIE_DeviceList.TheDeviceList().Add(new PCIE_Device(new WD_PCI_SLOT() { dwSlot=3}));
			//PCIE_DeviceList.TheDeviceList().Add(new PCIE_Device(new WD_PCI_SLOT() { dwSlot = 5 }));
		 var pciDevList = PCIE_DeviceList.TheDeviceList();
            //GlobalHost.DependencyResolver.Register(typeof(PCIE_DeviceList), () => pciDevList);
            try
            {

                DWORD dwStatus = pciDevList.Init(Settings.Default.License);
                if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
                {
                    return dwStatus;
                }
                return dwStatus;
              
            }
            catch (Exception)
            {
                return 1000;
            } 
        }

		/* Open a handle to a device */
		public bool DeviceOpen(int iSelectedIndex)
		{
			DWORD dwStatus;
			PCIE_Device device = PCIE_DeviceList.TheDeviceList().Get(iSelectedIndex);

			/* Open a handle to the device */
			//dwStatus = device.Open();
			//if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
			//{
			//	Log.ErrLog("NEWAMD86_diag.DeviceOpen: Failed opening a " +
			//		"handle to the device (" + device.ToString(false) + ")");
			//	return false;
			//} 
			return true;
		}

		/* Close handle to a NEWAMD86 device */
		public bool DeviceClose(int iSelectedIndex)
		{
			PCIE_Device device = PCIE_DeviceList.TheDeviceList().Get(iSelectedIndex);
			bool bStatus = false;

			//if (device.Handle != IntPtr.Zero && !(bStatus = device.Close()))
			//{
			//	Log.ErrLog("NEWAMD86_diag.DeviceClose: Failed closing NEWAMD86 "
			//		+ "device (" + device.ToString(false) + ")");
			//}
			//else
			//	device.Handle = IntPtr.Zero;
			return bStatus;
		}

		public string InitDeviceInfo(int index)
		{ 
			string desc = string.Empty;
			var device = PCIE_DeviceList.TheDeviceList().Get(index);
			 DeviceChannelList.Add(device, AddChannel(index));
			DWORD outData = 0;
			device.ReadBAR0(0, 0x00, ref outData);
			if ((outData & 0x10) == 0x10)
				desc += "链路速率：2.5Gb/s\r\n";
			else if ((outData & 0x20) == 0x20)
				desc += "链路速率：5.0Gb/s\r\n";
			else
				desc += "speed judge error/s\r\n";

			outData = (outData & 0xF);
			if (outData == 1)
				desc += "链路宽度：x1";
			else if (outData == 2)
				desc += "链路宽度：x2";
			else if (outData == 4)
				desc += "链路宽度：x4";
			else if (outData == 8)
				desc += "链路宽度：x8";
			else
				desc += "width judge error/s\r\n";
			return desc;
		}

		private List<DeviceChannelModel> AddChannel(int index)
		{
			List<DeviceChannelModel> list = new List<DeviceChannelModel>();
			switch (index)
			{
				case 0:
					DeviceChannelModel channel0 = new DeviceChannelModel
					{
						Id = 1,
						Name = "通道1",
						RegAddress = 0x30,
						DiskPath = "Bar1"
					};
					DeviceChannelModel channel1 = new DeviceChannelModel
					{
						Id = 2,
						Name = "通道2",
						RegAddress = 0x34,
						DiskPath = "Bar2"
					};
					DeviceChannelModel channel2 = new DeviceChannelModel
					{
						Id = 3,
						Name = "通道3",
						RegAddress = 0x38,
						DiskPath = "Bar3"
					};
					list.Add(channel0);
					list.Add(channel1);
					list.Add(channel2);
					break;
				case 1:
					DeviceChannelModel channel3 = new DeviceChannelModel
					{
						Id = 4,
						Name = "通道4",
						RegAddress = 0x40,
						DiskPath = "Bar4"
					};
					DeviceChannelModel channel4 = new DeviceChannelModel
					{
						Id = 5,
						Name = "通道5",
						RegAddress = 0x44,
						DiskPath = "Bar5"
					};
					DeviceChannelModel channel5 = new DeviceChannelModel
					{
						Id = 6,
						Name = "通道6",
						RegAddress = 0x48,
						DiskPath = "Bar6"
					};
					list.Add(channel3);
					list.Add(channel4);
					list.Add(channel5);
					break;
				default:
					break;
			}
			return list;
		}
	}
}
