using Hcdz.PcieLib;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Pvirtech.Framework.Common;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Controls;

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
        long total = 0;
       // private FileStream Stream;
		public MainViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IServiceLocator serviceLocator, IHcdzClient hcdzClient)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_regionManager = regionManager; 
			_serviceLocator = serviceLocator;
			_hcdzClient = hcdzClient;
			 
            devicesItems = new ObservableCollection<PCIE_Device>();
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            _openDeviceText =  "连接设备";
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
           
           // Stream = new FileStream("D:\\test", FileMode.Append, FileAccess.Write);
			_hcdzClient.MessageReceived += OnMessageReceived;
            _hcdzClient.Connected +=ClientConnected; 
			_hcdzClient.Connect();
            _hcdzClient.NotifyTotal += _hcdzClient_NotifyTotal;
            _hcdzClient.NoticeScanByte +=OnNoticeScanByte;
            LoadDeviceChannel();
        }

        private void OnNoticeScanByte(string strByte)
        {
          strByte=  "3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
            var result = CommonHelper.StringToByte(strByte);
            if (result!=null)
            {
                LogHelper.WriteLog("自检返回数据:  "+strByte.Take(64));
                var scanValue = result.Skip(32);
                LogInfo += strByte;
            }
        }

        private void Connection_Closed()
        {
            DeviceDesc = string.Empty;
            BtnIsEnabled = true;
            dispatcherTimer.Stop();
            TextRate = "0.00MB/s";
            total = 0;
            OpenDeviceText = "连接设备";
            IsOpen = false;
        }

       

        private async void LoadData()
		{
			DriveInfo[] drives =await _hcdzClient.GetDrives();
			DriveInfoItems = new ObservableCollection<DriveInfo>(drives);

			
		}

		private void ClientConnected(bool result)
        {
            if (result)
            {
				LoadData();//加载基本信息
				OnLoadSelectDir(_selectedDsik); 
				Initializer();
                _hcdzClient.Connection.Closed += Connection_Closed;
            }
        }

        private void OnMessageReceived(string message)
		{
            LogInfo += message;
		}

		private async void OnCloseReadDma(object obj)
        {
              BtnIsEnabled = true;
             dispatcherTimer.Stop();
            TextRate = "0.00MB/s";
              total = 0;
             await  _hcdzClient.CloseDma();
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
            model.IsOpen = false;
           await _hcdzClient.OpenOrCloseChannel(model);
        }

        private async void OnOpenChannel(DeviceChannelModel model)
        { 
            model.IsOpen = true;
            await _hcdzClient.OpenOrCloseChannel(model);
        }

        private async void OnScanDevice(object obj)
        { 
		    var result=await	_hcdzClient.ScanDevice(0);
			if (result)
			{
				//MessageBox.Show("设备运行正常!");
			}
			else
			{
				//MessageBox.Show("设备自检信息异常!");
			}
        }

        private async void OnReadDma(object obj)
        {
            BtnIsEnabled = false;
            var findItem = _deviceChannelModels.FirstOrDefault(O => O.IsOpen == true); 
			if (findItem==null)
            {
                MessageBox.Show("请打开设备通道！");
                BtnIsEnabled = true;
                return;
            }
            if (string.IsNullOrEmpty(SelectedDsik))
            {
                MessageBox.Show("请选择存储盘符！");
                BtnIsEnabled = true;
                return;
            }
            dispatcherTimer.Start();
            int dma = 16;
            var dmaSize = SelectedDMA.Content.ToString();
            int.TryParse(dmaSize.TrimEnd('K'), out dma);
           
           var  result=await _hcdzClient.OnReadDma(SelectedDsik, dma, 0); 
            Thread.Sleep(1);
           var result2=await  _hcdzClient.OnReadDma(SelectedDsik, dma, 1);
            if (result2.Contains("内存")|| result.Contains("内存"))
            {
                MessageBox.Show("分配内存失败,请重新连接设备!");
                total = 0;
                OpenDeviceText = "连接设备";
                IsOpen = false; 
            }
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

        private void _hcdzClient_NotifyTotal(long obj)
        {
            total += obj;
        }

      
        private async void OnOpenDevice(object obj)
        {
            if (!IsOpen)
            {
                var result = await _hcdzClient.DeviceOpen(0);
                if (result)
                {
                    BtnIsEnabled = true;
                    OpenDeviceText = "关闭设备";
                    IsOpen = true;
                    DeviceDesc = await _hcdzClient.InitDeviceInfo(0);
                }
                else
                {
                    RadDesktopAlertManager desktop = new RadDesktopAlertManager(AlertScreenPosition.BottomCenter);

                    desktop.ShowAlert(new RadDesktopAlert()
                    {
                        Content = "设备连接失败!"
                    });
                }
            }
            else
            {
                var flag = await _hcdzClient.DeviceClose(0);
                if (flag)
                {
                    OpenDeviceText = "连接设备";
                    IsOpen = false;
                    DeviceDesc = string.Empty;
                }
                else
                {
                    RadDesktopAlertManager desktop = new RadDesktopAlertManager(AlertScreenPosition.BottomCenter);

                    desktop.ShowAlert(new RadDesktopAlert()
                    {
                        Content = "设备断开失败!"
                    });
                }
                CloseDMAChannel();

            }

            //        else
            //        {
            //if (!IsSecOpen)
            //{
            //	var result = await _hcdzClient.DeviceOpen(1);
            //	if (result)
            //	{

            //		OpenDeviceText1 = "关闭设备";
            //		IsSecOpen = true;
            //		DeviceDesc = await _hcdzClient.InitDeviceInfo(1);              
            //	}
            //	else
            //	{
            //		RadDesktopAlertManager desktop = new RadDesktopAlertManager(AlertScreenPosition.BottomCenter);

            //		desktop.ShowAlert(new RadDesktopAlert()
            //		{
            //			Content = "设备连接失败!"
            //		});
            //	}
            //}
            //else
            //{
            //	var flag = await _hcdzClient.DeviceClose(1);
            //	if (flag)
            //	{
            //		OpenDeviceText1 = "连接设备";
            //		IsSecOpen = false;
            //	}
            //	else
            //	{
            //		RadDesktopAlertManager desktop = new RadDesktopAlertManager(AlertScreenPosition.BottomCenter);

            //		desktop.ShowAlert(new RadDesktopAlert()
            //		{
            //			Content = "设备2断开失败!"
            //		});
            //	}
            //}
            //} 
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
		private bool  _btnIsEnabled=false;
		public bool BtnIsEnabled { get { return _btnIsEnabled; } set { SetProperty(ref _btnIsEnabled, value); } }

		private string _openDeviceText;
        public string OpenDeviceText { get { return _openDeviceText; }set { SetProperty(ref _openDeviceText,value); } }
        private string _logInfo;
        public string LogInfo
        {
            get
            {
                return _logInfo;
            }
            set
            {
                SetProperty(ref _logInfo, value);
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
        private ComboBoxItem  _selectedDMA;
        public ComboBoxItem SelectedDMA
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
        public ICommand CloseDmaCmd { get; private set; }
        #endregion


        private async void Initializer()
		{
            var result =  await _hcdzClient.InitializerDevice();
            if (result>0)
            {
				LogHelper.WriteLog("错误码："+result.ToString());
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
            //Thread readThread = new Thread(new ThreadStart(ReadDMA));
            //readThread.IsBackground = true;
            //readThread.Start();
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
			_deviceChannelModels.Add(channel3);
			_deviceChannelModels.Add(channel4);
			_deviceChannelModels.Add(channel5);
        }
         
        private void ReadDMA()
		{
			//while (!IsCompleted)
			//{
   //             if (queue.IsEmpty)
   //             {
   //                 //IsCompleted = true;
   //                 //foreach (var item in _deviceChannelModels)
   //                 //{
   //                 //    if (item.Stream==null)
   //                 //    {
   //                 //        continue;
   //                 //    }
   //                 //    item.Stream.Close();
   //                 //    item.Stream.Dispose();
   //                 //}
   //             }
   //             else
   //             {
   //                 byte[] item;
   //                 //if (queue.TryDequeue(out item))
   //                 {
   //                        //if (result%2==0)
                   
   //                      // ThreadPool.QueueUserWorkItem(WriteBar);
   //                         //  WriteBar(result); 
                        
   //                     //var bytes = item.Length / 16;
   //                     //for (int i = 0; i < bytes; i++)
   //                     //{
   //                     //    var index = i * 16;
   //                     //    byte[] result = new byte[16];
   //                     //    for (int j = 0; j < 16; j++)
   //                     //    {
   //                     //        result[j] = item[index + j];
   //                     //    }
   //                     //    var barValue = Convert.ToInt32(result[15]);
   //                     //    if (barValue == 1)
   //                     //    {
   //                     //        WriteFile(result);
   //                     //    }
   //                     //}
   //                 }
   //             }
			//}
		}
         
        private void CloseDMAChannel()
        {
            foreach (var item in DeviceChannelModels)
            {
                item.IsOpen = false;
            }  
        }
    }
}
