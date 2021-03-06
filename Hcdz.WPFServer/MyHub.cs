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
using Pvirtech.Framework.Common;
using Pvirtech.TcpSocket.Scs.Client;
using Pvirtech.TcpSocket.Scs.Communication.EndPoints.Tcp;
using Pvirtech.TcpSocket.Scs.Communication.Messages;
using Pvirtech.TcpSocket.Scs.Communication;
using System.Management;

namespace Hcdz.WPFServer
{
    public class MyHub : Hub
    {
        private readonly static Dictionary<PCIE_Device, List<DeviceChannelModel>> DeviceChannelList = new Dictionary<PCIE_Device, List<DeviceChannelModel>>();
        private readonly static List<DeviceChannelModel> DeviceChannelModels = new List<DeviceChannelModel>();        
        private static bool DeviceStatus;
        private static bool IsStop = false;
        private static long readSize=0;
        private static long tcpReadSize = 0;
        private static string scanStr = string.Empty;
        private static string tmpInfo = string.Empty;
        //private static DispatcherTimer dispatcherTimer;
        private readonly static List<TcpClientModel> TcpModels = new List<TcpClientModel>()
        {  new TcpClientModel() { Id = 1 },       new TcpClientModel() { Id = 2 }
        };
        private static Action _onPushM;
        private static readonly object lockObj = new object();
         
        //private static FileStream DeviceFile;
        public MyHub()
        {

            ActiveEvent();
        }
        private   void ActiveEvent()
        { 
            lock (lockObj)
            {
                if (_onPushM == null)
                {
                    if (_onPushM != null) return;
                    _onPushM = PushMessage;
                    MainWindow.CurrentWindow.OnPush += PushMessage;
                }
            } 
            // _mqService.OnPushM += PushMessage;
            // _mqService.OnPush -= PushMessage;
            // _mqService.OnPush += PushMessage;
            //OnPushM= _mqService.OnPush;
        }

        private async void PushMessage()
        {

            await Task.Run(() =>
            {
                if (readSize > 0)
                {
                    Clients.All.NotifyTotal(readSize);
                    readSize = 0;
                }
            });
            
            await Task.Run(() =>
            {
                foreach (var item in TcpModels)
                {
                    if (item.DataSize > 0)
                    {
                        Clients.All.NoticeTcpData(item);
                    }
                    item.DataSize = 0;
                }
            });


        }

        private   void DispatcherTimer_Tick(object sender, EventArgs e)
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
            return base.OnDisconnected(true);
        }

        public IEnumerable<DriveInfo> GetDrives()
        {
            var drives = DriveInfo.GetDrives().Skip(2);
            return drives;
        }
        public bool FormatDrive(string driveName)
        {
            //var dt = DateTime.Now;
            bool flag = CommonHelper.FormatDrive(driveName);
            //var dt2 = DateTime.Now;
            // TimeSpan ts = dt2 - dt;
            // Clients.Client(Context.ConnectionId).NotifyFormatTime(ts.Milliseconds);
            return flag;
        }
        public  DriveInfoModel GetSingleDrive(string driveName="")
        {
            DriveInfoModel model = new DriveInfoModel();
             var item = DriveInfo.GetDrives().FirstOrDefault(o => o.Name == driveName);
            if (item!=null)
            {
                model.AvailableFreeSpace = item.AvailableFreeSpace;
                model.DriveFormat = item.DriveFormat;
                model.Name = item.Name;
                model.TotalFreeSpace = item.TotalFreeSpace;
                model.TotalSize = item.TotalSize;
            }
            return model;
        }

        public bool GetNetWork()
        {
            //var flag = CommonHelper.IsWanAlive();
            //var connect = CommonHelper.IsConnected();
            ManagementObjectSearcher connectedCount = new ManagementObjectSearcher(
              @"SELECT DeviceID FROM Win32_NetworkAdapter WHERE NetConnectionStatus=2 AND PNPDeviceID LIKE 'PCI%'");
            ManagementObjectSearcher totalWan = new ManagementObjectSearcher(
             @"SELECT DeviceID FROM Win32_NetworkAdapter WHERE PNPDeviceID LIKE 'PCI%'");
            if (connectedCount==totalWan)
            {
                return true;
            }
            return false;
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
            var drives = DriveInfo.GetDrives().Skip(2);
            FileSystemInfo[] dirFileitems = null;
            var list = new List<DirectoryInfoModel>();
            await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = drives.First().Name;
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
            var pciDevList = PCIE_DeviceList.TheDeviceList();
            //GlobalHost.DependencyResolver.Register(typeof(PCIE_DeviceList), () => pciDevList);

            try
            {
                if (DeviceStatus)
                    return 0;
                DWORD dwStatus = pciDevList.Init(Settings.Default.License);
                if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
                {
                    LogHelper.WriteLog(string.Format("设备初始化异常，错误码：{0}", dwStatus));
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
            LogHelper.WriteLog(string.Format("连接第{0}张板卡", iSelectedIndex + 1));
            DWORD dwStatus = 0;
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
                    if (dev != null)
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
                if (dwStatus > 0)
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
            readSize = 0;
            //pBoard->pci_e.WriteBAR0(0, 0x10, regval);
            LogHelper.WriteLog(string.Format("断开第{0}张板卡", iSelectedIndex + 1));
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
                    
                }
                if (bStatus)
                {
                    Clients.Client(Context.ConnectionId).NoticeMessage("已成功断开设备!\n");
                }
                return bStatus;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex, "DeviceClose");
                Clients.Client(Context.ConnectionId).NoticeMessage(ex.Message + "close \n");
            }
            return false;
        }

        public string InitDeviceInfo(int index)
        {
          
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
            Clients.Client(Context.ConnectionId).NoticeMessage("已成功获取链路信息!" + "\n");
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

        public string ScanDevice(int deviceIndex)
        {
            int count = PCIE_DeviceList.TheDeviceList().Count;
            PCIE_Device dev = PCIE_DeviceList.TheDeviceList().Get(deviceIndex);
            if (dev == null)
            {
                return "device";
            }
            if (dev.WDCScan_DMAContigBufLock() != 0)
            {
                //MessageBox.Show(("分配报告内存失败"));
                return "device";
            }
            ////DWORD wrDMASize = dataSize; //16kb
            if (!dev.ScanDMAWriteMenAlloc(16*1024))
            {
                //MessageBox.Show("内存分配失败!");
                return "device";
            }
            dev.StartWrDMA(0);
            dev.WriteBAR0(0, 0x60, 1);		//中断屏蔽
            dev.WriteBAR0(0, 0x50, 1);		//dma 写报告使能

            var dma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.pScanReportWrDMA);
            var ppwDma = (WD_DMA)dev.m_dmaMarshaler.MarshalNativeToManaged(dev.pScanpwDma);

            dev.WriteBAR0(0, 0x58, (uint)dma.Page[0].pPhysicalAddr);		//dma 写报告地址
            //设置初始DMA写地址,长度等
            dev.WriteBAR0(0, 0x4, (uint)ppwDma.Page[0].pPhysicalAddr);		//wr_addr low
            dev.WriteBAR0(0, 0x8, (uint)(ppwDma.Page[0].pPhysicalAddr >> 32));	//wr_addr high
            dev.WriteBAR0(0, 0xC, (UInt32)16 * 1024);           //dma wr size
             
            dev.WriteBAR0(0, 0x28, 1);            
            //int readIndex = 0;
            var dt = DateTime.Now;
            while(true)
            {               
                var current = (DateTime.Now - dt).Seconds;
                if (current>1)
                {
                    break;
                }
               dev.WriteBAR0(0, 0x10, 1);
            }
            dev.WriteBAR0(0, 0x10, 1);             
            
            dev.WriteBAR0(0, 0x28, 0);
            dev.WriteBAR0(0, 0x10, 0);
            //启动DMA
            //       //dma wr 使能
            byte[] tmpResult = new Byte[16*1024];
            Marshal.Copy(dev.pScanWbuffer, tmpResult, 0, 16*1024);
            LogHelper.WriteLog(string.Format("设备{0}自检返回数据: ", deviceIndex + 1) + CommonHelper.ByteToString(tmpResult));
            var findItem = tmpResult.Where(o => o== 63).Count();
            LogHelper.WriteLog(findItem.ToString());
            //var findItem = barList.FirstOrDefault(o => o != 63);
            if (findItem>50)
            {
                // scanStr = "设备所有通道自检正常!";
                return string.Empty;
            }
            else
            {                
                string errorStr = deviceIndex == 0 ? "1,2,3" : " 4,5,6"; 
                var query = tmpResult.Where(o =>o==3).Count();
                if (query> 50)
                {
                    errorStr = deviceIndex == 0 ? "2,3" : " 5,6";
                }
                var query1 = tmpResult.Where(o =>o==12).Count();
                if (query1 > 50)
                {
                    errorStr = deviceIndex == 0 ? "1,3" : " 4,6";
                }
                var query2 = tmpResult.Where(o => o==15).Count();
                if (query2 > 50)
                {
                    errorStr = deviceIndex == 0 ? "3" : " 6";
                    LogHelper.WriteLog(query2.ToString());
                }
                var query3 = tmpResult.Where(o =>o==48).Count();
                if (query3 > 50)
                {
                    errorStr = deviceIndex == 0 ? "1,2" : " 4,5";
                }
                var query4 = tmpResult.Where(o =>o==51).Count();
                if (query4 > 50)
                {
                    errorStr = deviceIndex == 0 ? "2" : " 5";
                }
                var query5 = tmpResult.Where(o =>o== 60);
                
                if (query5.Count() > 50)
                {
                    errorStr = deviceIndex == 0 ? "1" : " 4";
                    LogHelper.WriteLog(query5.ToString());
                }
                //var query6 = tmpResult.Where(o => o == 0);
                //if (query6.Count()==tmpResult.Count())
                //{
                //    errorStr = deviceIndex == 0 ? "1,2,3" : " 4,5,6";
                //}
                //switch (findItem)
                //{
                //    case 3:
                //        errorStr = deviceIndex == 0 ? "1,2" : " 4,5";
                //        break;
                //    case 12:
                //        errorStr = deviceIndex == 0 ? "1,3" : " 4,6";
                //        break;
                //    case 15:
                //        errorStr = deviceIndex == 0 ? "1" : " 4";
                //        break;
                //    case 48:
                //        errorStr = deviceIndex == 0 ? "2,3" : " 5,6";
                //        break;
                //    case 51:
                //        errorStr = deviceIndex == 0 ? "2" : " 5";
                //        break;
                //    case 60:
                //        errorStr = deviceIndex == 0 ? "3" : " 6";
                //        break;
                //    default:
                //        errorStr ="Error";
                //        break;
                //}
                // scanStr += errorStr;
               //  tmpInfo +=errorStr+"";
                return errorStr;
            } 
            
        }

        public string OnReadDma(string dvireName, int dataSize, int deviceIndex)
        {
            //var findDrive = DriveInfo.GetDrives().FirstOrDefault(o => o.Name == dvireName);
            //if (findDrive != null)
            //{
            //    if (findDrive.AvailableFreeSpace < 1024 * 1024 * 1024.0)
            //    {

            //    }
            //}
            int cout = PCIE_DeviceList.TheDeviceList().Count;
            if (cout==1&&deviceIndex==1)
            {
                return "";
            }
            PCIE_Device dev = PCIE_DeviceList.TheDeviceList().Get(deviceIndex);
            if (dev == null)
            {
                Clients.Client(Context.ConnectionId).NoticeMessage("设备DMA读取发生异常" + "\n");
                return "设备读取异常,请重试";
            }
            if (dev.Status == 1)
                return "正在读取数据...";
            Clients.Client(Context.ConnectionId).NoticeMessage("正在读取设备数据...\n");
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
            string dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            // var list = DeviceChannelList[dev];
            //foreach (var item in list)
            //{
            //    var dir = Path.Combine(dvireName, item.DiskPath);
            //    if (!Directory.Exists(dir))
            //    {
            //        Directory.CreateDirectory(dir);
            //    }
            //    var filePath = Path.Combine(dir, dt);
            //    //File.Create(filePath);
            //    item.FilePath = filePath;
            //    if (item.IsOpen)
            //    {
            //        item.Stream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            //    }

            //}
            var dir = Path.Combine(dvireName, "device" + deviceIndex.ToString());
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var filePath = Path.Combine(dir, dt);
            dev.DeviceFile = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            dev.StartWrDMA(0);

            //Thread readThread = new Thread(new ParameterizedThreadStart(p =>);
            //readThread.IsBackground = true;
            //readThread.Start();
            Task.Factory.StartNew(new Action(() => OnScanDrive(dev, dvireName, deviceIndex)));
            //Thread nonParameterThread = new Thread(new ParameterizedThreadStart(p => NonParameterRun(dev, dvireName, dataSize, deviceIndex)));
            //nonParameterThread.Start();
            Task.Factory.StartNew(new Action(() => NonParameterRun(dev, dvireName, dataSize, deviceIndex))).ContinueWith(t =>
            {              
                dev.DeviceFile.Flush();
                dev.DeviceFile.Close();
                dev.DeviceFile.Dispose();
            });
            return string.Empty;
        }

        private void OnScanDrive(PCIE_Device dev, string dvireName, int deviceIndex)
        {
            while (true)
            {
                Thread.Sleep(10);
                if (readSize==0)
                {
                    continue;
                }
                var findDrive = DriveInfo.GetDrives().FirstOrDefault(o => o.Name == dvireName);
                if (findDrive != null)
                {
                    if (findDrive.AvailableFreeSpace < 1024 * 1024 * 1024.0 * 20)
                    {

                        var fileName = dev.DeviceFile.Name;
                        var index = fileName.LastIndexOf("\\");
                        var dirPath = fileName.Substring(0, index + 1);
                        DirectoryInfo dir = new DirectoryInfo(dirPath);

                        var fileList = dir.GetFiles("*", SearchOption.TopDirectoryOnly).OrderBy(o => o.CreationTime);
                        
                        try
                        {
                            var childFile = fileList.FirstOrDefault();
                            if (childFile == null)
                                return;
                            File.Delete(childFile.FullName);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog(ex);                             
                        }
                       
                    }
                }
            }
        }

        private void NonParameterRun(PCIE_Device dev, string dvireName, int dataSize, int deviceIndex)
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
            //foreach (var item in list)
            //{
            //   dev.WriteBAR0(0, item.RegAddress, item.IsOpen == true ? (UInt32)1 : 0);             
            //} 
            Parallel.ForEach(list, item =>
            {
                dev.WriteBAR0(0, item.RegAddress, item.IsOpen == true ? (UInt32)1 : 0);
            });
            Thread.Sleep(1000);
            //启动DMA
            dev.WriteBAR0(0, 0x10, 1);          //dma wr 使能

            dev.Status = 1;
            IsStop = false;
            while (!IsStop)
            {
                //var findDrive = DriveInfo.GetDrives().FirstOrDefault(o => o.Name == dvireName);
                //if (findDrive != null)
                //{
                //    if (findDrive.AvailableFreeSpace < 1024 * 1024 * 256.0)
                //    {
                //        dev.DeviceFile.SetLength(0);
                //    }
                //}
                byte[] tmpResult = new Byte[wrDMASize];
                Marshal.Copy(dev.pWbuffer, tmpResult, 0, wrDMASize);
                dev.DeviceFile.Write(tmpResult, 0, wrDMASize);
                dev.DeviceFile.Flush();
                // concurrentQueue.Enqueue(tmpResult);
                // DWORD lpNumberOfBytesWritten = 0;
                //  PCIE_Device.WriteFile(dev.DeviceFile.SafeFileHandle.DangerousGetHandle(),ref dev.pWbuffer, (uint)dataSize * 1024, out lpNumberOfBytesWritten, null);
                // Stream.Write(tmpResult, 0, tmpResult.Length);

                //var bytes = tmpResult.Length /16;
                //for (int i = 0; i < bytes; i++)
                //{
                //    var index = i * 16;
                //    byte[] result = new byte[16];
                //    for (int j = 0; j < 16; j++)
                //    {
                //        result[j] = tmpResult[index + j];
                //    }
                //    if (result[15] == 1)
                //    {
                //        WriteFile(result, list);
                //    }
                //}

                // ReadTotalSize = wrDMASize;
                readSize += wrDMASize;
               // Clients.Client(Context.ConnectionId).NotifyTotal(wrDMASize);

                dev.WriteBAR0(0, 0x10, 1);//执行下次读取 
            }
            dev.WriteBAR0(0, 0x10, 0);
        }
        private void WriteFile(byte[] result, List<DeviceChannelModel> channelModels)
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

        public void CloseDma()
        {
            IsStop = true;
            var devices = PCIE_DeviceList.TheDeviceList();
            for (int i = 0; i < devices.Count; i++)
            {
                var dev = (PCIE_Device)devices[i];
                dev.WriteBAR0(0, 0x28, 0);
                dev.WriteBAR0(0, 0x10, 0);
                dev.Status = 0;
            }
            readSize = 0;

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

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog(ex, "文件删除");
                }

            }
        }

        #region TCP/IP
        public List<TcpClientModel> GetTcpModels()
        {
            return TcpModels;
        }
        public bool TcpConnect(string fileDir, string ip, int port, int index = 1)
        {
            var findItem = TcpModels.Find(o => o.Id == index);
            if (findItem == null)
                return false;
            if (!findItem.IsConnected)
            {
                findItem.IP = ip;
                findItem.Port = port;
                findItem.FileDir = fileDir;
                var dt = DateTime.Now.ToString("yyyyMMddHHmmss");
                var dir = fileDir + "Wan" + index.ToString();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
              //  findItem.TcpStream = new FileStream(dir + "\\" + dt, FileMode.Append, FileAccess.Write);
            }
            try
            {

                //Create a client object to connect a server on 127.0.0.1 (local) IP and listens 10085 TCP port
                findItem.Client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(ip, port));
                // client.WireProtocol = new CustomWireProtocol(); //Set custom wire protocol
                //Register to MessageReceived event to receive messages from server.
                findItem.Client.ConnectTimeout = 5;                
                findItem.Client.MessageReceived += (s, e) => Client_MessageReceived(s, e, findItem);
                findItem.Client.Connected += (s, e) => Client_Connected(s, e, findItem);
                findItem.Client.Disconnected += (s, e) => Client_Disconnected(s, e, findItem);
                findItem.Client.Connect(); //Connect to the server 


                //Send message to the server
                //findItem.Client.SendMessage(new ScsTextMessage("3F", "q1"));

                //client.Disconnect(); //Close connection to server
            }
            catch (Exception ex)
            {
                findItem.Client.Dispose();
                LogHelper.ErrorLog(ex, "连接异常!");
                return false;
            }
            return true;
        }

        public void CloseTcpConnect(int index)
        {
          
            var findItem = TcpModels.Find(o => o.Id == index);
            if (findItem == null)
                return; 
            findItem.Client.Disconnect();
            //  findItem.TcpStream.Flush();
            findItem.TcpStream.Close();
            findItem.TcpStream.Dispose();
            findItem.DataSize = 0;
            findItem.IsConnected = false;
        }
        private void Client_Disconnected(object sender, EventArgs e, TcpClientModel model)
        {
            model.DataSize = 0;
            model.IsConnected = false;
            Clients.Client(Context.ConnectionId).NoticeTcpConnect(false, model);
        }

        private void Client_Connected(object sender, EventArgs e, TcpClientModel model)
        {
            var client = sender as IScsClient;
            if (client.CommunicationState == CommunicationStates.Connected)
            {
                model.IsConnected = true;
                Clients.Client(Context.ConnectionId).NoticeTcpConnect(true, model);
            }

        }

        private void Client_MessageReceived(object sender, MessageEventArgs e, TcpClientModel model)
        {
            var dt = (DateTime.Now - model.ConnectedTime).TotalSeconds;
            if (dt>10)
            { 
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                var dir = model.FileDir + "Wan" + model.Id.ToString();
                model.TcpStream = new FileStream(dir + "\\" + fileName, FileMode.Append, FileAccess.Write);                
            }
            else
            {
                if (model.TcpStream == null)
                {
                    model.TcpStream = new FileStream(model.FileDir + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss"), FileMode.Append, FileAccess.Write);
                    Task.Run(()=>OnScanDriveByWan(model));
                } 
            }
            model.ConnectedTime = DateTime.Now;
            var message = e.Message as ScsTextMessage;
            // var byteArray = System.Text.Encoding.Default.GetBytes(message.Text);
            // var b1 = System.Text.Encoding.ASCII.GetBytes(message.Text); 
            //var ss = CommonHelper.ByteToString(byteArray);
            //var bb = CommonHelper.StrToHexByte(ss);
            //var bb2 = CommonHelper.StringToByte(ss); 
            if (message == null)
            {
                return;
            }
            var byteArray = Encoding.Default.GetBytes(message.Text);
              
            model.TcpStream.Write(byteArray, 0, byteArray.Length);
            //stream.CopyTo(model.TcpStream);
            model.TcpStream.Flush();
             
            model.DataSize += byteArray.Length;
            
        }

        private void OnScanDriveByWan(TcpClientModel model)
        {
            while (true)
            {
                Thread.Sleep(10000);
                string drive = string.Empty;
                if (model.TcpStream == null)
                    continue;
                 
                  drive = model.TcpStream.Name.Substring(0, 3);
                
                 
                var findDrive = DriveInfo.GetDrives().FirstOrDefault(o => o.Name == drive);
                if (findDrive != null)
                {
                    if (findDrive.AvailableFreeSpace < 1024 * 1024 * 1024.0 * 5)
                    {

                        var fileName = model.TcpStream.Name;
                        var index = fileName.LastIndexOf("\\");
                        var dirPath = fileName.Substring(0, index + 1);
                        DirectoryInfo dir = new DirectoryInfo(dirPath);

                        var fileList = dir.GetFiles("*", SearchOption.TopDirectoryOnly).OrderBy(o => o.CreationTime);

                        try
                        {
                            var childFile = fileList.FirstOrDefault();
                            if (childFile == null)
                                return;
                            File.Delete(childFile.FullName);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog(ex);
                        }

                    }
                }
            }
        }

       
        #endregion
    }
}
