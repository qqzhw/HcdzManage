using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Hcdz.PcieLib;
using wdc_err = Jungo.wdapi_dotnet.WD_ERROR_CODES;
using DWORD = System.UInt32;
using WORD = System.UInt16;
using BYTE = System.Byte;
using BOOL = System.Boolean;
using UINT32 = System.UInt32;
using UINT64 = System.UInt64;
using WDC_DEVICE_HANDLE = System.IntPtr;
using WDC_ADDR_SIZE = System.UInt32;
using HANDLE = System.IntPtr;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using Jungo.wdapi_dotnet;
using System.Runtime.InteropServices;
using Pvirtech.Framework.Common;

namespace Hcdz.ModulePcie.ViewModels
{
    public class MainViewModel: BindableBase
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IUnityContainer _container;
		private readonly IRegionManager _regionManager;
		private readonly IServiceLocator _serviceLocator;
		private readonly IHcdzClient  _hcdzClient;
		private DispatcherTimer dispatcherTimer;
        private ConcurrentQueue<byte[]> queue;
		private bool IsCompleted=false;
        private bool IsStop = false;
        Int64 times;
        long total = 0;
        private FileStream Stream;
		public MainViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IServiceLocator serviceLocator, IHcdzClient hcdzClient)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_regionManager = regionManager; 
			_serviceLocator = serviceLocator;
			_hcdzClient = hcdzClient;
			 queue = new ConcurrentQueue<byte[]>();
            devicesItems = new ObservableCollection<PCIE_Device>();
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            _openDeviceText =_openDeviceText1= "连接设备";
            OpenDevice = new DelegateCommand<object>(OnOpenDevice);
            ScanDeviceCmd=new DelegateCommand<object>(OnScanDevice);
            ReadDmaCmd =new DelegateCommand<object>(OnReadDma);
            CloseDmaCmd= new DelegateCommand<object>(OnCloseReadDma);
            OpenChannel =new DelegateCommand<DeviceChannelModel>(OnOpenChannel);
            CloseChannel = new DelegateCommand<DeviceChannelModel>(OnCloseChannel);
            SelectedDirCmd = new DelegateCommand<object>(OnLoadSelectDir);
            _deviceChannelModels = new ObservableCollection<DeviceChannelModel>();//主板1 四通道
            _deviceChannel2 = new ObservableCollection<DeviceChannelModel>();//主板2 通道
            _viewModel = new PcieViewModel();
           
            Stream = new FileStream("D:\\test", FileMode.Append, FileAccess.Write);
			_hcdzClient.MessageReceived += _hcdzClient_MessageReceived;
            _hcdzClient.Connected +=ClientConnected;
			_hcdzClient.Connect("dddd");
			
			LoadData();//加载基本信息
        }

		private void LoadData()
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			_driveInfoItems = new ObservableCollection<DriveInfo>(drives);

			LoadDeviceChannel();
		}

		private void ClientConnected(bool result)
        {
            if (result)
            {
				 
			   OnLoadSelectDir(_selectedDsik);
			 
				Initializer();
            }
        }

        private void _hcdzClient_MessageReceived(string obj)
		{
			 
		}

		private void OnCloseReadDma(object obj)
        {
            IsStop = true;
            dispatcherTimer.Stop();
        }

        private  async void OnLoadSelectDir(object dirPath)
        {
            if (dirPath==null)
            {
                return;
            }
            DriveInfo[] drives = await _hcdzClient.GetDrives();
			if (drives == null)
				return;
            foreach (var drive in drives)
            {
                if (drive.Name.Contains(dirPath.ToString()))
                {
                    DiskVal = ByteFormatter.ToString(drive.AvailableFreeSpace) + " 可用";
                    DiskPercent = 100.0 - (int)(drive.AvailableFreeSpace * 100.0 / drive.TotalSize); 
                }
            }
        }

        private async void OnCloseChannel(DeviceChannelModel model)
        {
            //var device = model.Device;
            // var device = pciDevList.Get(0);
            //device.WriteBAR0(0, 0x28, 1);
            //   device.WriteBAR0(0, model.RegAddress, 0);
            model.IsOpen = false;
           await _hcdzClient.OpenOrCloseChannel(model);
        }

        private async void OnOpenChannel(DeviceChannelModel model)
        {
            //var device = model.Device;
           // var device = pciDevList.Get(0);
            //device.WriteBAR0(0, 0x28, 1);
          //  device.WriteBAR0(0, 0x34, 1);
            model.IsOpen = true;
            await _hcdzClient.OpenOrCloseChannel(model);
        }

        private void OnScanDevice(object obj)
        {
            if (pciDevList.Count==0)
            {
                MessageBox.Show("未发现设备!");
            }
            var device = pciDevList.Get(0);
            //device.WriteBAR0(0, 0x28, 1);
            DWORD outData = 0;
            device.WriteBAR0(0, 0x28, 1);
           //if ()
           // {
           //     MessageBox.Show("自检失败!");
           // }
        }

        private void OnReadDma(object obj)
        {
            var findItem = _deviceChannelModels.FirstOrDefault(O => O.IsOpen == true);
            if(findItem==null)
            {
                MessageBox.Show("请打开相关通道！");
                return;
            }
            _hcdzClient.OnReadDma("D:\\", 16, 0);
            //PCIE_Device dev =pciDevList.Get(0);
            //dev.FPGAReset(0);
            //if (dev.WDC_DMAContigBufLock() != 0)
            //{
            //    MessageBox.Show(("分配报告内存失败"));
            //    return;
            //}
            //DWORD wrDMASize = 16; //16kb
            //if (!dev.DMAWriteMenAlloc((uint)0, (uint)1, wrDMASize * 1024))
            //{
            //    MessageBox.Show("内存分配失败!");
            //    return;
            //}
            //var dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            //foreach (var item in _deviceChannelModels)
            //{
            //    var dir = Path.Combine(SelectedDsik, item.DiskPath);
            //    if (!Directory.Exists(dir))
            //    {
            //        Directory.CreateDirectory(dir);
            //    }
            //    var filePath = Path.Combine(dir, dt);
            //    //File.Create(filePath);
            //    item.FilePath = filePath;
            //    if (item.IsOpen)
            //    {
            //      item.Stream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            //    }

            //}
            //dev.StartWrDMA(0);
            //dispatcherTimer.Start();
            ////if (p->bWriteDisc[0])
            ////CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)savefile0, p, 0, NULL);
            //Thread nonParameterThread = new Thread(new ParameterizedThreadStart(p => NonParameterRun(dev)));

            //nonParameterThread.Start();



            //p->WriteBAR0(0, 0x28, 1);			//dma wr 使能
            //using (FileStream fs = new FileStream("G:\\dd4", FileMode.OpenOrCreate, FileAccess.Write))
            //{
            //    var s = Marshal.ReadByte(dev.pWbuffer);
            //    var sr = dev.pWbuffer.ToString();
            //    byte[] ss = new Byte[16*1024];
            //    var temp = new byte[Marshal.SizeOf(dev.pWbuffer)];
            //    Marshal.Copy(dev.pWbuffer, ss, 0, ss.Length);
            //    int d = 0;
            //    IntPtr dd = IntPtr.Zero;
            //    StringBuilder stringBuilder = new StringBuilder();

            //    var tmp = new byte[16];
            //    var bytes = ss.Length / 16;
            //    for (int i = 0; i <bytes; i++)
            //    {
            //        var index = i * 16;
            //        byte[] result = new byte[16];
            //        for (int j = 0; j < 16; j++)
            //        {
            //            result[j] = ss[index+j]; 
            //        }
            //        var barValue = Convert.ToBoolean(result[15]);
            //        if (barValue)
            //        {
            //            int s2 = 9;
            //            var barfile = result[8]; 
            //            WriteFile(result);
            //        }
            //    }
            //    fs.Write(ss, 0, ss.Length);
            //    UInt32* h = (UInt32*)d;
            //    // dev.pReportWrBuffer = IntPtr.Zero;
            //    var ssss = *(UInt32*)dev.pReportWrBuffer;
            //    ByteToString(ss);
            // 	中断模式*/
            //  while (*(UInt32*)dev.pReportWrBuffer != 0x1)
            //{
            //if (p->stop[0])//还得通知程序已经完成
            //{		
            //    goto dma_write_end;
            //}
            //  }
            // *(UInt32*)dev.pReportWrBuffer = 0;

            //dev.WriteBAR0(0, 0x10, 1);

            //  var ssss22 = dev.pReportWrBuffer;
            //  byte[] ss1 = new Byte[8192];
            //  Marshal.Copy(dev.pWbuffer, ss1, 0, ss.Length);
            // fs.Write(ss1,0, 8192);
            //NEWAMD86_Device.WriteFile(fs.Handle,ref dev.pWbuffer, (uint)8192, out dd, 0);
            // }

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
            dev.WriteBAR0(0, 0xC, 16 * 1024);			//dma wr size
           
            //dev.WriteBAR0(0, 56, 1);
            //dev.WriteBAR0(0, 48, 1);
            //dev.WriteBAR0(0, 0x34, 1);
            foreach (var item in _deviceChannelModels)
            { 
             dev.WriteBAR0(0, item.RegAddress, item.IsOpen==true?(UINT32)1:0);
            }
            //启动DMA
            dev.WriteBAR0(0, 0x10, 1);			//dma wr 使能
            var startTime = DateTime.Now.Ticks;
            
            while (!IsStop)
            {
              
                byte[]  tmpResult = new Byte[16 * 1024];
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
                foreach (var item in _deviceChannelModels)
                {
                    dev.WriteBAR0(0, item.RegAddress, item.IsOpen == true ? (UINT32)1 : 0);
                }
                dev.WriteBAR0(0, 0x10, 1);//执行下次读取
                var end = DateTime.Now.Ticks - startTime;
                times = TimeSpan.FromTicks(end).Ticks;              
            }
            dev.WriteBAR0(0, 0x10, 0);
        }

        private async void OnOpenDevice(object obj)
        {
            int index = 0;
            int.TryParse(obj.ToString(), out index);
            if (!IsOpen)
            {
				var result =await _hcdzClient.DeviceOpen(index);
                if (result)
                {
                    if (index == 1)
                    {
                        OpenDeviceText1 = "关闭设备";
                        IsOpen = true;
                    }
                    else
                    {
                        OpenDeviceText = "关闭设备";
                        IsOpen = true;
                        DeviceDesc= await _hcdzClient.InitDeviceInfo(index);
                    }
                    //var device = pciDevList.Get(index);
                    //DWORD outData=0;
                    //device.ReadBAR0(0, 0x00, ref outData);
                    //if ((outData & 0x10) == 0x10)
                    //    DeviceDesc += "链路速率：2.5Gb/s\r\n";
                    //else if ((outData & 0x20) == 0x20)
                    //    DeviceDesc += "链路速率：5.0Gb/s\r\n";
                    //else
                    //    DeviceDesc += "speed judge error/s\r\n";

                    //outData = (outData & 0xF);
                    //if (outData == 1)
                    //    DeviceDesc += "链路宽度：x1";
                    //else if (outData == 2)
                    //    DeviceDesc += "链路宽度：x2";
                    //else if (outData == 4)
                    //    DeviceDesc += "链路宽度：x4";
                    //else if (outData == 8)
                    //    DeviceDesc += "链路宽度：x8";
                    //else
                    //    DeviceDesc += "width judge error/s\r\n";                    
                }
            }
            else
            {
              //  PCIE_Device dev = pciDevList.Get(0);
               //    DeviceClose(0);
                OpenDeviceText = "连接设备";
                IsOpen = false;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                var text = (total /1048576.0).ToString("f2")+"MB/s";
                TextRate = text;
                total = 0;
            });
        }

        #region 属性
        
        ConcurrentQueue<byte[]> queue1;
        private PCIE_DeviceList pciDevList;
        private Log log;

        private PcieViewModel _viewModel;
        public PcieViewModel ViewModel
        {
            get { return _viewModel; }
            set { SetProperty(ref _viewModel,value); }
        }
        private ObservableCollection<PCIE_Device> devicesItems;
        public ObservableCollection<PCIE_Device> DevicesItems
        {
            get { return devicesItems; }
            set { SetProperty(ref devicesItems, value); }
        }
        private bool _isOpen;
        public bool IsOpen { get { return _isOpen; } set { SetProperty(ref _isOpen, value); } }

        private string _openDeviceText;
        public string OpenDeviceText { get { return _openDeviceText; }set { SetProperty(ref _openDeviceText,value); } }
        private string _openDeviceText1;
        public string OpenDeviceText1
        {
            get
            {
                return _openDeviceText1;
            }
            set
            {
                SetProperty(ref _openDeviceText1, value);
            }
        }
        /// <summary>
        /// 选中磁盘目录
        /// </summary>
        private string _selectedDsik;
        public string SelectedDsik
        {
            get
            { 
                return _selectedDsik;
            }
            set
            {
                SetProperty(ref _selectedDsik, value);
            }
        }
        /// <summary>
        /// DMA读取数据大小
        /// </summary>
        private string  _selectedDMA;
        public string SelectedDMA
        {
            get
            {
                return _selectedDMA;
            }
            set
            {
                SetProperty(ref _selectedDMA, value);
            }
        }
        /// <summary>
        /// 硬盘使用百分比
        /// </summary>
        private double _diskPercent;
        public double  DiskPercent
        {
            get
            {
                return _diskPercent;
            }
            set
            {
                SetProperty(ref _diskPercent, value);
            }
        }

        /// <summary>
        /// 硬盘剩余空间
        /// </summary>
        private string _diskVal;
        public string  DiskVal
        {
            get
            {
                return _diskVal;
            }
            set
            {
                SetProperty(ref _diskVal, value);
            }
        }
        private string _textRate;
        public string TextRate {
            get
            {
                return _textRate;
            }
            set
            {
                SetProperty(ref _textRate, value);
            }
        }
        private string  _deviceDesc;
        public string DeviceDesc { get { return _deviceDesc; } set { SetProperty(ref _deviceDesc, value); } }
                
        private ObservableCollection<DriveInfo> _driveInfoItems;
        public ObservableCollection<DriveInfo> DriveInfoItems
        {
            get { return _driveInfoItems; }
            set { SetProperty(ref _driveInfoItems, value); }
        }
        private ObservableCollection<DeviceChannelModel> _deviceChannelModels;
        public ObservableCollection<DeviceChannelModel> DeviceChannelModels
        {
            get { return _deviceChannelModels; }
            set { SetProperty(ref _deviceChannelModels, value); }
        }
        private ObservableCollection<DeviceChannelModel> _deviceChannel2;
        public ObservableCollection<DeviceChannelModel> DeviceChannel2
        {
            get { return _deviceChannel2; }
            set { SetProperty(ref _deviceChannel2, value); }
        }

        public ICommand OpenDevice { get; private set; }
        public ICommand ReadDmaCmd { get; private set; }
        public ICommand ScanDeviceCmd { get; private set; }
        public ICommand OpenChannel { get; private set; }
        public ICommand CloseChannel { get; private set; }
        public ICommand SelectedDirCmd { get; private set; }
        public DelegateCommand<object> CloseDmaCmd { get; private set; }
        #endregion


        private async void Initializer()
		{
            var result =  await _hcdzClient.InitializerDevice();
            if (result>0)
            {
               
              string content = "加载设备失败!";
                if (result == 536870921)
                {
                    content = "License无效!";
                }
                Application.Current.Dispatcher.Invoke(delegate {
                    RadWindow.Alert(new DialogParameters
                    {
                        Content = content, 
                        Theme = new Windows8TouchTheme(),
                        Header = "提示",
                         DialogStartupLocation=WindowStartupLocation.CenterOwner,
						 OkButtonContent="关闭",
						 Owner = Application.Current.MainWindow,
					});
                }); 
                return;
            }

            //  queue1 = new ConcurrentQueue<byte[]>();
            Thread readThread = new Thread(new ThreadStart(ReadDMA));
            readThread.IsBackground = true;
            readThread.Start();
            //Thread writeThread = new Thread(new ThreadStart(WriteDMA));
            //writeThread.IsBackground = true;
            //writeThread.Start();
            //try
            //{
            //    DWORD dwStatus = pciDevList.Init();
            //    if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
            //    {
            //        RadDesktopAlert alert = new RadDesktopAlert();
            //        alert.Content = "加载设备失败!";
            //        RadWindow.Alert(new DialogParameters
            //        {
            //            Content = "加载设备失败！",
            //            DefaultPromptResultValue = "default name",
            //            Theme = new Windows8TouchTheme(),
            //            Header = "提示",
            //            TopOffset = 30,
            //        });
            //        return;
            //    }

                //foreach (PCIE_Device dev in pciDevList)
                //    devicesItems.Add(dev);
                //if (devicesItems.Count > 0)
                //{
                //    ViewModel.ShortDesc = devicesItems[0].Name;
                //}
           // }
            //catch (Exception)
            //{
            //    MessageBox.Show("初始化设备失败!");                
            //}
            
           
        }

        private void LoadDeviceChannel()
        {
            DeviceChannelModel channel0 = new DeviceChannelModel
            {
                Id = 1,
                Name = "通道1",
                RegAddress = 0x30,
                DiskPath="Bar1",
                DeviceNo = 0,
            };
            DeviceChannelModel channel1 = new DeviceChannelModel
            {
                Id = 2,
                Name = "通道2",
                RegAddress = 0x34,
                DiskPath = "Bar2",
                DeviceNo = 0,
            };
            DeviceChannelModel channel2 = new DeviceChannelModel
            {
                Id = 3,
                Name = "通道3",
                RegAddress = 0x38,
                DiskPath = "Bar3",
                DeviceNo = 0,
            };
            DeviceChannelModel channel3 = new DeviceChannelModel
            {
                Id = 4,
                Name = "通道4",
                RegAddress = 0x40,
                DiskPath = "Bar4",
                DeviceNo = 1,
            };
            DeviceChannelModel channel4 = new DeviceChannelModel
            {
                Id = 5,
                Name = "通道5",
                RegAddress = 0x44,
                DiskPath = "Bar5",
                DeviceNo = 1,
            };
            DeviceChannelModel channel5 = new DeviceChannelModel
            {
                Id = 6,
                Name = "通道6",
                RegAddress = 0x48,
                DiskPath = "Bar6",
                DeviceNo = 1,
            };
            _deviceChannelModels.Add(channel0);
            _deviceChannelModels.Add(channel1);
            _deviceChannelModels.Add(channel2);
            _deviceChannel2.Add(channel3);
            _deviceChannel2.Add(channel4);
            _deviceChannel2.Add(channel5);
        }
         
        private void ReadDMA()
		{
			while (!IsCompleted)
			{
                if (queue.IsEmpty)
                {
                    //IsCompleted = true;
                    //foreach (var item in _deviceChannelModels)
                    //{
                    //    if (item.Stream==null)
                    //    {
                    //        continue;
                    //    }
                    //    item.Stream.Close();
                    //    item.Stream.Dispose();
                    //}
                }
                else
                {
                    byte[] item;
                    if (queue.TryDequeue(out item))
                    {
                           //if (result%2==0)
                   
                         // ThreadPool.QueueUserWorkItem(WriteBar);
                            //  WriteBar(result); 
                        
                        //var bytes = item.Length / 16;
                        //for (int i = 0; i < bytes; i++)
                        //{
                        //    var index = i * 16;
                        //    byte[] result = new byte[16];
                        //    for (int j = 0; j < 16; j++)
                        //    {
                        //        result[j] = item[index + j];
                        //    }
                        //    var barValue = Convert.ToInt32(result[15]);
                        //    if (barValue == 1)
                        //    {
                        //        WriteFile(result);
                        //    }
                        //}
                    }
                }
			}
		}

        private void WriteFile(BYTE[] result)
        {
            var channelNo = Convert.ToInt32(result[8]);
            var item = _deviceChannelModels.FirstOrDefault(o => o.Id == channelNo);
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
