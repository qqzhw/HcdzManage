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
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Collections.Concurrent;
using Hcdz.Framework.Common;
using Pvirtech.Framework.Common;

namespace Hcdz.WPFServer
{
    public class MyHub : Hub
    {
        private readonly static Dictionary<PCIE_Device, List<DeviceChannelModel>> DeviceChannelList = new Dictionary<PCIE_Device, List<DeviceChannelModel>>();
        private readonly static List<DeviceChannelModel> DeviceChannelModels = new List<DeviceChannelModel>();
        private static DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private static bool DeviceStatus;
        private bool IsCompleted = false;
        private static bool IsStop = false;
        private static ConcurrentQueue<byte[]> concurrentQueue = new ConcurrentQueue<byte[]>();
        static long ReadTotalSize = 0;
        //private static FileStream   Stream = new FileStream("D:\\test", FileMode.Append, FileAccess.Write);
        public MyHub()
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
            DeviceClose(0);
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
        public void CopyFileEx(string sourceFullPath, string targetFullPath)
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
                                LengthText = Models.ByteFormatter.ToString(file.Length),
                                Length = file.Length
                            });
                        }

                    }
                }
                return list;
            });
            return list;
        }

        public DWORD InitLoad()
        {
            //PCIE_DeviceList.TheDeviceList().Add(new PCIE_Device(new WD_PCI_SLOT() { dwSlot=3}));
            //PCIE_DeviceList.TheDeviceList().Add(new PCIE_Device(new WD_PCI_SLOT() { dwSlot = 5 }));
            var pciDevList = PCIE_DeviceList.TheDeviceList();
			//GlobalHost.DependencyResolver.Register(typeof(PCIE_DeviceList), () => pciDevList);
			
			try
            {
                if (DeviceStatus)
                    return 0;
                DWORD dwStatus = pciDevList.Init(Settings.Default.License);
                if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
                {
                    return dwStatus;
                }
				LogHelper.WriteLog(string.Format("总发现{0}张设备卡", pciDevList.Count));
				DeviceStatus = true;
                return dwStatus;

            }
            catch (Exception ex)
            {
				LogHelper.ErrorLog(ex);
                return 1000;
            }
		
        }

        /* Open a handle to a device */
        public bool DeviceOpen(int iSelectedIndex)
        {
			LogHelper.WriteLog(string.Format("连接第{0}张板卡",iSelectedIndex+1));
            DWORD dwStatus=0;
            var devices = PCIE_DeviceList.TheDeviceList();
            PCIE_Device device = PCIE_DeviceList.TheDeviceList().Get(iSelectedIndex);
            if (device == null)
                return false;
			/* Open a handle to the device */
			try
			{
                for (int i = 0; i < devices.Count; i++)
                {
                    var dev = devices.Get(i);
                    if (dev!=null)
                    {
                        dwStatus = dev.Open();
                        if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
                        {
                            string str = "打开设备: 连接设备失败 (" + dev.ToString(false) + ")\n";
                            Clients.Client(Context.ConnectionId).NoticeMessage(str);
                            LogHelper.WriteLog(str); 
                        }
                    } 
                }
                Clients.Client(Context.ConnectionId).NoticeMessage("已成功连接设备!\n");
				if (dwStatus>0)
				{
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				LogHelper.ErrorLog(ex, "DeviceOpen");
                Clients.Client(Context.ConnectionId).NoticeMessage(ex.Message + " open\n");
                return false;
			}
          
        }

        /* Close handle to a NEWAMD86 device */
        public bool DeviceClose(int iSelectedIndex)
        {
            //pBoard->pci_e.WriteBAR0(0, 0x10, regval);
            LogHelper.WriteLog(string.Format("断开第{0}张板卡", iSelectedIndex+1));
            var devices = PCIE_DeviceList.TheDeviceList();
            PCIE_Device device = PCIE_DeviceList.TheDeviceList().Get(iSelectedIndex);
            bool bStatus = false;
            try
            {
                Clients.Client(Context.ConnectionId).NoticeMessage("正在停止数据读取...\n");
                CloseDma();
                Clients.Client(Context.ConnectionId).NoticeMessage("已停止数据读取!\n");
                for (int i = 0; i < devices.Count; i++)
                {
                    var dev = devices.Get(i);
                    if (dev != null)
                    {
                        if (dev.Handle != IntPtr.Zero && !(bStatus = dev.Close()))
                        { 
                            string str = "断开设备: 关闭设备失败 (" + dev.ToString(false) + ")";
                            Clients.Client(Context.ConnectionId).NoticeMessage(str);
                            LogHelper.WriteLog(str);
                        }
                        else
                        {
                            dev.Handle = IntPtr.Zero;
                            dev.ppwDma = IntPtr.Zero;
                            dev.pReportWrBuffer = IntPtr.Zero;
                            dev.pReportWrDMA = IntPtr.Zero;
                            dev.pWbuffer = IntPtr.Zero; 
                            bStatus = true;
                        }
                    }
                    if (bStatus)
                    {
                        Clients.Client(Context.ConnectionId).NoticeMessage(string.Format("已成功断开设备{0}!\n",i+1));
                    } 
                }
				return bStatus;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex, "DeviceClose");
                Clients.Client(Context.ConnectionId).NoticeMessage(ex.Message+"close \n");
            }
			return false;
        }

        public string InitDeviceInfo(int index)
        {
            Clients.Client(Context.ConnectionId).NoticeMessage("获取链路信息..." + "\n");
            DeviceChannelList.Clear();
            string desc = string.Empty;
            var device = PCIE_DeviceList.TheDeviceList().Get(index);
            for (int i = 0; i < PCIE_DeviceList.TheDeviceList().Count; i++)
            {
                var item = PCIE_DeviceList.TheDeviceList().Get(i);
                DeviceChannelList.Add(item, AddChannel(i));
            }         
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

        public void OpenOrCloseChannel(DeviceChannelModel model)
        {
            foreach (var item in DeviceChannelList)
            {
                foreach (var childitem in item.Value)
                {
                    if (childitem.Id == model.Id)
                    {
                        childitem.IsOpen = model.IsOpen;
                    }
                }
            }
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

        public bool ScanDevice(int deviceIndex)
        {
            PCIE_Device dev = PCIE_DeviceList.TheDeviceList().Get(deviceIndex);

            if (dev.WDCScan_DMAContigBufLock() != 0)
            {
                //MessageBox.Show(("分配报告内存失败"));
                return false;
            }
            ////DWORD wrDMASize = dataSize; //16kb
            if (!dev.ScanDMAWriteMenAlloc(1024))
            {
                //MessageBox.Show("内存分配失败!");
                return false;
            }
            dev.StartWrDMA(0);
            //dev.WriteBAR0(0, 0x60, 1);		//中断屏蔽
            //dev.WriteBAR0(0, 0x50, 1);		//dma 写报告使能

            var dma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.pScanReportWrDMA);
            var ppwDma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.pScanpwDma);

            dev.WriteBAR0(0, 0x58, (uint)dma.Page[0].pPhysicalAddr);		//dma 写报告地址
            //设置初始DMA写地址,长度等
            dev.WriteBAR0(0, 0x4, (uint)ppwDma.Page[0].pPhysicalAddr);		//wr_addr low
            dev.WriteBAR0(0, 0x8, (uint)(ppwDma.Page[0].pPhysicalAddr >> 32));	//wr_addr high
            dev.WriteBAR0(0, 0xC, (UInt32)1024);           //dma wr size
            dev.WriteBAR0(0, 0x28, 1);
            dev.WriteBAR0(0, 0x10, 1);


            //启动DMA
            //       //dma wr 使能
            byte[] tmpResult = new Byte[1024];
            Marshal.Copy(dev.pScanWbuffer, tmpResult, 0, tmpResult.Length);
            if (dev == null)
            {
                return false;
            }
            Thread.Sleep(1);
            dev.WriteBAR0(0, 0x28, 0);
            return true;
        }

        public string OnReadDma(string dvireName, int dataSize, int deviceIndex)
        {
            PCIE_Device dev = PCIE_DeviceList.TheDeviceList().Get(deviceIndex);
            if (dev == null)
            {
                Clients.Client(Context.ConnectionId).NoticeMessage("设备读取发生异常 null" + "\n");
                return "设备读取异常,请重试";
            }
            if (dev.Status == 1)
                return "正在读取数据...";
            Clients.Client(Context.ConnectionId).NoticeMessage(string.Format("正在读取设备{0}数据...\n",deviceIndex+1 ));
            dev.FPGAReset(0);
            if (dev.WDC_DMAContigBufLock() != 0)
            {
                Clients.Client(Context.ConnectionId).NoticeMessage("锁定内存空间失败..." + "\n");
                //MessageBox.Show(("分配报告内存失败"));
                DeviceClose(0);
                return "锁定内存空间失败";
            }
            //DWORD wrDMASize = dataSize; //16kb
            if (!dev.DMAWriteMenAlloc((uint)0, (uint)1, (UInt32)dataSize * 1024))
            {
                //MessageBox.Show("内存分配失败!");
                Clients.Client(Context.ConnectionId).NoticeMessage("内存分配失败..." + "\n");
                DeviceClose(0);
                return "内存分配失败";
            }
            var dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            var list = DeviceChannelList[dev];
            foreach (var item in list)
            {
                var dir = Path.Combine(dvireName, item.DiskPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var filePath = Path.Combine(dir, dt);
                //File.Create(filePath);
                item.FilePath = filePath;
                if (item.IsOpen)
                {
                    item.Stream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                }

            }
            dev.StartWrDMA(0);
            
            //Thread readThread = new Thread(new ParameterizedThreadStart(p => OnWriteDMA(dev)));
            //readThread.IsBackground = true;
            //readThread.Start();

            //Thread nonParameterThread = new Thread(new ParameterizedThreadStart(p => NonParameterRun(dev, dvireName, dataSize, deviceIndex)));
            //nonParameterThread.Start();
            Task.Factory.StartNew(new Action(()=>NonParameterRun(dev, dvireName, dataSize, deviceIndex))).ContinueWith(t=> {
                foreach (var item in list)
                {
                    if (item.Stream == null)
                        continue;
                    item.Stream.Flush();
                    item.Stream.Close();
                    item.Stream.Dispose();
                }
            });
            return string.Empty;
        }

        private void OnWriteDMA(PCIE_Device dev)
        {
            while (true)
            {
                if (concurrentQueue.Count > 0)
                {
                    byte[] item;
                    if (concurrentQueue.TryDequeue(out item))
                    {
                        WriteFile(item, DeviceChannelList[dev]);
                    }
                }
            }
        }

       
        private void WriteBar(object state)
        {

        } 

        private void NonParameterRun(PCIE_Device dev,string dvireName,int dataSize,int deviceIndex)
        {
            int wrDMASize = dataSize * 1024; //16kb
            dev.WriteBAR0(0, 0x60, 1);		//中断屏蔽
            dev.WriteBAR0(0, 0x50, 1);		//dma 写报告使能

            var dma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.pReportWrDMA);
            var ppwDma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.ppwDma);

            dev.WriteBAR0(0, 0x58, (uint)dma.Page[0].pPhysicalAddr);		//dma 写报告地址
            //设置初始DMA写地址,长度等
            dev.WriteBAR0(0, 0x4, (uint)ppwDma.Page[0].pPhysicalAddr);		//wr_addr low
            dev.WriteBAR0(0, 0x8, (uint)(ppwDma.Page[0].pPhysicalAddr >> 32));	//wr_addr high
            dev.WriteBAR0(0, 0xC, (UInt32)wrDMASize);           //dma wr size

            //dev.WriteBAR0(0, 56, 1);
           // dev.WriteBAR0(0, 48, 1);
           
            var list = DeviceChannelList[dev];
            foreach (var item in list)
            {
               dev.WriteBAR0(0, item.RegAddress, item.IsOpen == true ? (UInt32)1 : 0);
            }
           
            //启动DMA
            dev.WriteBAR0(0, 0x10, 1);          //dma wr 使能
            
            dev.Status = 1;
            IsStop = false;
            while (!IsStop)
            {

                 byte[] tmpResult = new Byte[wrDMASize];
                   Marshal.Copy(dev.pWbuffer, tmpResult, 0, wrDMASize);
                // concurrentQueue.Enqueue(tmpResult);
               // dev.WriteFile(fs.Handle,ref dev.pWbuffer, (uint)dataSize * 1024, out dd, 0);
                // Stream.Write(tmpResult, 0, tmpResult.Length);
                var bytes = tmpResult.Length /16;
                for (int i = 0; i < bytes; i++)
                {
                    var index = i * 16;
                    byte[] result = new byte[16];
                    for (int j = 0; j < 16; j++)
                    {
                        result[j] = tmpResult[index + j];
                    }
                    if (result[15] == 1)
                    {
                        WriteFile(result, list);
                    }
                }
              
               // ReadTotalSize = wrDMASize;
                Clients.Client(Context.ConnectionId).NotifyTotal(wrDMASize);
                dev.WriteBAR0(0, 0x10, 1);//执行下次读取 
            }
            dev.WriteBAR0(0, 0x10, 0);
        }
        private void WriteFile(byte[] result,List<DeviceChannelModel> channelModels)
        {
            var channelNo = result[8];
            var item = channelModels.Find(o => o.Id == channelNo);
            if (item == null)
             return;
                byte[] trueValue = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                trueValue[i] = result[i];
            }

           // result[8] = 0;
           // result[15] = 0;
			//var bt = result.Take(8).ToArray();
         //   item.FileByte.AddRange(bt);
           item.Stream?.Write(trueValue, 0, 8);
          //  item.Stream.Flush();            
        }
        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public  void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[80 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
        public void CloseDma()
        {
            IsStop = true;
           var devices = PCIE_DeviceList.TheDeviceList();
            for (int i = 0; i < devices.Count; i++)
            {
                var dev = (PCIE_Device)devices[i];
                dev.WriteBAR0(0, 0x10, 0);
                dev.Status = 0;
            } 
          
        }
        public void CloseScanDevice()
        {
            var devices = PCIE_DeviceList.TheDeviceList();
            for (int i = 0; i < devices.Count; i++)
            {
                var dev = (PCIE_Device)devices[i];
                dev.WriteBAR0(0, 0x28, 0);
                dev.WriteBAR0(0, 0x10, 0); 
            }
        }
    }
}
