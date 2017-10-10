﻿using Hcdz.PcieLib;
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
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Collections.Concurrent;
using Hcdz.Framework.Common;

namespace Hcdz.WPFServer
{ 
	public class MyHub : Hub
	{
		private readonly static Dictionary<PCIE_Device, List<DeviceChannelModel>> DeviceChannelList = new Dictionary<PCIE_Device, List<DeviceChannelModel>>();
        private readonly static List<DeviceChannelModel> DeviceChannelModels = new  List<DeviceChannelModel>();
        private DispatcherTimer dispatcherTimer;
        private ConcurrentQueue<byte[]> queue;
        private bool IsCompleted = false;
        private bool IsStop = false;
        Int64 times;
        long total = 0;
        private FileStream Stream;
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
		public void CopyFileEx(string sourceFullPath,string targetFullPath)
		{
			FileUtilities.CreateDirectoryIfNotExist(Path.GetDirectoryName(targetFullPath));
			FileUtilities.CopyFileEx(sourceFullPath, targetFullPath, token);
		}

		private void token(string source, string destination, long totalFileSize, long totalBytesTransferred)
		{
			Clients.Client(Context.ConnectionId).FileProgress(source, destination, totalFileSize, totalBytesTransferred);
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
						DiskPath = "Bar1",
                        DeviceNo = index
                    };
					DeviceChannelModel channel1 = new DeviceChannelModel
					{
						Id = 2,
						Name = "通道2",
						RegAddress = 0x34,
						DiskPath = "Bar2",
                        DeviceNo = index,
                    };
					DeviceChannelModel channel2 = new DeviceChannelModel
					{
						Id = 3,
						Name = "通道3",
						RegAddress = 0x38,
						DiskPath = "Bar3",
                        DeviceNo = index
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
						DiskPath = "Bar4",
                        DeviceNo = index
                    };
					DeviceChannelModel channel4 = new DeviceChannelModel
					{
						Id = 5,
						Name = "通道5",
						RegAddress = 0x44,
						DiskPath = "Bar5",
                        DeviceNo = index
                    };
					DeviceChannelModel channel5 = new DeviceChannelModel
					{
						Id = 6,
						Name = "通道6",
						RegAddress = 0x48,
						DiskPath = "Bar6",
                        DeviceNo = index
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

        public void OnReadDma()
        {
            PCIE_Device dev = PCIE_DeviceList.TheDeviceList().Get(0);
            dev.FPGAReset(0);
            if (dev.WDC_DMAContigBufLock() != 0)
            {
                MessageBox.Show(("分配报告内存失败"));
                return;
            }
            DWORD wrDMASize = 16; //16kb
            if (!dev.DMAWriteMenAlloc((uint)0, (uint)1, wrDMASize * 1024))
            {
                MessageBox.Show("内存分配失败!");
                return;
            }
            var dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            foreach (var item in DeviceChannelList)
            {
                //var dir = Path.Combine(SelectedDsik, item.DiskPath);
                //if (!Directory.Exists(dir))
                //{
                //    Directory.CreateDirectory(dir);
                //}
                //var filePath = Path.Combine(dir, dt);
                ////File.Create(filePath);
                //item.FilePath = filePath;
                //if (item.IsOpen)
                //{
                //    item.Stream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                //}

            }
            dev.StartWrDMA(0);
            dispatcherTimer.Start();
            //if (p->bWriteDisc[0])
            //CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)savefile0, p, 0, NULL);
            Thread nonParameterThread = new Thread(new ParameterizedThreadStart(p => NonParameterRun(dev)));

            nonParameterThread.Start();
        }

        private void NonParameterRun(PCIE_Device dev)
        {
            dev.WriteBAR0(0, 0x60, 1);		//中断屏蔽
            dev.WriteBAR0(0, 0x50, 1);		//dma 写报告使能

            var dma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.pReportWrDMA);
            var ppwDma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.ppwDma);

            dev.WriteBAR0(0, 0x58, (uint)dma.Page[0].pPhysicalAddr);		//dma 写报告地址
            //设置初始DMA写地址,长度等
            dev.WriteBAR0(0, 0x4, (uint)ppwDma.Page[0].pPhysicalAddr);		//wr_addr low
            dev.WriteBAR0(0, 0x8, (uint)(ppwDma.Page[0].pPhysicalAddr >> 32));	//wr_addr high
            dev.WriteBAR0(0, 0xC, 16 * 1024);           //dma wr size

            //dev.WriteBAR0(0, 56, 1);
            //dev.WriteBAR0(0, 48, 1);
            //dev.WriteBAR0(0, 0x34, 1);
            foreach (var item in DeviceChannelModels)
            {
                dev.WriteBAR0(0, item.RegAddress, item.IsOpen == true ? (UInt32)1 : 0);
            }
            //启动DMA
            dev.WriteBAR0(0, 0x10, 1);			//dma wr 使能
            var startTime = DateTime.Now.Ticks;

            while (!IsStop)
            {

                byte[] tmpResult = new Byte[16 * 1024];
                Marshal.Copy(dev.pWbuffer, tmpResult, 0, tmpResult.Length);
                Stream.Write(tmpResult, 0, tmpResult.Length);
                var bytes = tmpResult.Length / 16;
                for (int i = 0; i < bytes; i++)
                {
                    var index = i * 16;
                    byte[] result = new byte[16];
                    for (int j = 0; j < 16; j++)
                    {
                        result[j] = tmpResult[index + j];
                    }
                    var barValue = Convert.ToInt32(result[15]);
                    if (barValue == 1)
                    {
                        WriteFile(result);
                    }
                }
                //  queue.Enqueue(tmpResult);
                //tranIndex++;
                total += 16 * 1024;
                foreach (var item in DeviceChannelModels)
                {
                    dev.WriteBAR0(0, item.RegAddress, item.IsOpen == true ? (UInt32)1 : 0);
                }
                dev.WriteBAR0(0, 0x10, 1);//执行下次读取
                var end = DateTime.Now.Ticks - startTime;
                times = TimeSpan.FromTicks(end).Ticks;
            }
            dev.WriteBAR0(0, 0x10, 0);
        }
        private void WriteFile(byte[] result)
        {
            var channelNo = Convert.ToInt32(result[8]);
            var item = DeviceChannelModels.FirstOrDefault(o => o.Id == channelNo);
            if (item == null)
                return;
            byte[] trueValue = new byte[16];
            for (int i = 0; i < 8; i++)
            {
                trueValue[i] = result[i];
            }
            //using (var stream= new FileStream(item.FilePath, FileMode.Append, FileAccess.Write))
            //{
            //    stream.Write(trueValue, 0, 16);
            //}
            item.Stream.Write(trueValue, 0, 16);
        }

    }
}
