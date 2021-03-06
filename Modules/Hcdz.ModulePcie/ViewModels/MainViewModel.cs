﻿using Hcdz.ModulePcie.Views;
 using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Pvirtech.Framework.Common;
using Pvirtech.Framework.Interactivity;
using Pvirtech.TcpSocket.Scs.Client;
using System;
using System.Collections.Generic;
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
        private DispatcherTimer timer;
        private int scanIndex = 0;
        private string scanStr = string.Empty;
       // private FileStream Stream;
		public MainViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IServiceLocator serviceLocator, IHcdzClient hcdzClient)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_regionManager = regionManager; 
			_serviceLocator = serviceLocator;
			_hcdzClient = hcdzClient;
			 
            //devicesItems = new ObservableCollection<PCIE_Device>();
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
            ConnectClick= new DelegateCommand<TcpClientViewModel>(OnConnectTcp);
            CloseClick=new DelegateCommand<TcpClientViewModel>(OnCloseTcp);
            LocalDataJxCmd = new DelegateCommand<object>(OnLocalDataRead);
            SelectedDirCmd = new DelegateCommand<object>(OnLoadSelectDir);
            _deviceChannelModels = new ObservableCollection<DeviceChannelModel>();//主板1 四通道
                                                                                  // _deviceChannel2 = new ObservableCollection<DeviceChannelModel>();//主板2 通道
            _tcpViewModel = new ObservableCollection<TcpClientViewModel>();
            _viewModel = new PcieViewModel();
           
           // Stream = new FileStream("D:\\test", FileMode.Append, FileAccess.Write);
			_hcdzClient.MessageReceived += OnMessageReceived;
            _hcdzClient.Connected +=ClientConnected; 
			_hcdzClient.Connect();
            _hcdzClient.NotifyTotal += _hcdzClient_NotifyTotal;
            _hcdzClient.NoticeScanByte +=OnNoticeScanByte;
            _hcdzClient.NoticeTcpConnect +=NoticeTcpConnect;
            _hcdzClient.NoticeTcpData +=NoticeTcpData;
            LoadDeviceChannel();
            InitRefresh();
            Application.Current.Exit += Current_Exit;
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            Application.Current.MainWindow.Closed += MainWindow_Closed;
        }

        private async void MainWindow_Closed(object sender, EventArgs e)
        { 
            var flag = await _hcdzClient.DeviceClose(0);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (IsStart)
            {
                MessageBox.Show("DMA正在读取数据,请先停止读取并断开设备连接！");
                e.Cancel = true;
            }
            var flag = false;
            foreach (var item in TcpViewModel)
            {
                if (item.IsBegin)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                MessageBox.Show("TCP网络正在读取数据,请关闭TCP连接！");
                e.Cancel = true;
            }
        }

        private   void Current_Exit(object sender, ExitEventArgs e)
        {
            OnCloseReadDma(null);
             
        }

        private void NoticeTcpData(TcpClientViewModel model)
        {
            var findItem = TcpViewModel.FirstOrDefault(o => o.Id == model.Id);
            findItem.CurrentSize = model.DataSize;
            findItem.IsBegin = true;

            findItem.RateText = string.Format("速率{0}MB/s", (model.DataSize / 1048576.0).ToString("f2"));
           // findItem.DataSize += model.DataSize; 

        }

        /// <summary>
        /// 本地数据读取，按通道存储
        /// </summary>
        /// <param name="obj"></param>
        private void OnLocalDataRead(object obj)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                var fileName = openFileDialog.FileName;
                var notification = new MessageNotification()
                {
                    Title = "数据解析",
                    Content = _container.Resolve<ReadDataView>(new ParameterOverride("fileName", fileName)),
                };
                PopupWindows.NormalNotificationRequest.Raise(notification, (callback) => {

                });
            }
        }

        private void InitRefresh()
        {
            timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private  void Timer_Tick(object sender, EventArgs e)
        {
            OnLoadSelectDir(SelectedDsik);
        }

        private async void OnCloseTcp(TcpClientViewModel model)
        {
            model.IsBegin = false;
            await   _hcdzClient.CloseTcpServer(model.Id);
        }

        private void NoticeTcpConnect(bool arg, TcpClientViewModel model)
        {
            var findItem = TcpViewModel.FirstOrDefault(o => o.Id == model.Id);
            findItem.IsConnected = model.IsConnected;
            findItem.MessageText += model.IsConnected == true ? "TCP连接成功！\n" : "TCP连接断开！\n";
            findItem.BtnIsEnabled = model.IsConnected == true ? false : true;
        }

        private async  void OnConnectTcp(TcpClientViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Ip)&& (model.Port>0&&model.Port<65535))
            {
                model.FileDir = SelectedDsik;
                await _hcdzClient.ConnectTcpServer(SelectedDsik,model.Ip, model.Port, model.Id);
            }
           
        }

        private void OnNoticeScanByte(string strByte,int deviceIndex)
        {
            //strByte=  "3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
            //scanStr = string.Format("设备通道{0}有异常! \n", tmpInfo);
            scanStr += strByte;
            scanIndex++;
            if (scanIndex == 2)
            {
                scanStr = string.Empty;
                scanIndex = 0;
                if (string.IsNullOrEmpty(scanStr))
                {
                    MessageBox.Show("设备所有通道自检正常!");
                    LogInfo += "设备所有通道自检正常!\n";
                }
                else
                {
                    if (scanStr.Contains("Error"))
                    {
                        MessageBox.Show("设备通道未知异常!");
                        LogInfo += "设备通道未知异常!\n";
                    }
                    else
                    {
                        string result = string.Format("设备通道{0}有异常! \n", scanStr);
                        MessageBox.Show(result);
                        LogInfo += result;
                    }

                }
                
            }


            //List<byte> bytes = new List<byte>();
            //var index = result.Length / 16;
            //for (int i = 0; i <index; i++)
            //{
            //    bytes.Add(result[16 * i]);
            //}
            //var barList = bytes.Skip(10);
            //var findItem = barList.FirstOrDefault(o => o != 63);
            //if (findItem==0)
            //{
            //    MessageBox.Show(string.Format("所有通道运行正常!",deviceIndex+1));
            //}
            // else
            {
                    //var V2 = Convert.ToString(findItem, 2);
                    //string[] deviceInfo= new string[V2.Length/2];
                    //for (int i = 0; i < V2.Length / 2; i++)
                    //{
                    //    deviceInfo[i] = V2.Substring(i * 2, 2); 
                    //}
                    //string errorInfo ="设备自检返回数据:  " + string.Join("  ", deviceInfo);
                    //LogHelper.WriteLog("设备自检返回数据: "+strByte);
                    //LogInfo += errorInfo + "\n";
                    //var list=deviceInfo.Reverse().ToList();
                    //string tmpInfo = string.Empty;
                    //for (int i = 0; i < list.Count(); i++)
                    //{
                    //    if (i>2)
                    //    {
                    //        break;
                    //    }
                    //    if (list[i]!="11")
                    //    {
                    //        string info = string.Format("设备通道有异常\n");
                    //        tmpInfo += info;
                    //    }
                   
                      
            }
        }

        private void Connection_Closed()
        {
            DeviceDesc = string.Empty;
            BtnIsEnabled = true;
           // dispatcherTimer.Stop();
            TextRate = ""; 
            OpenDeviceText = "连接设备";
            IsOpen = false;
        }
         
        private async void LoadData()
		{
			DriveInfo[] drives =await _hcdzClient.GetDrives();
            if (drives == null)
                return;
			DriveInfoItems = new ObservableCollection<DriveInfo>(drives);
            _selectedDsik = DriveInfoItems.FirstOrDefault()?.Name;
            if (!string.IsNullOrEmpty(_selectedDsik))
            {
                OnLoadSelectDir(_selectedDsik);
            }
            
        }

		private void ClientConnected(bool result)
        {
            if (result)
            {
				LoadData();//加载基本信息
				 
				Initializer();
                InitTcpClient();
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
             //dispatcherTimer.Stop(); 
             await  _hcdzClient.CloseDma();
            TextRate = "";
            IsStart = false;
        }

        private  async void OnLoadSelectDir(object dirPath)
        {
            if (dirPath==null)
            {
                return;
            }
            try
            {
                var drive = await _hcdzClient.GetSingleDrive(dirPath.ToString());
                if (drive == null)
                    return;

                DiskVal = ByteFormatter.ToString(drive.AvailableFreeSpace) + " 可用";
                DiskPercent = 100.0 - (int)(drive.AvailableFreeSpace * 100.0 / drive.TotalSize);
            }
            catch (Exception ex)
            {

            }
           
            //if (drives.Count() > 1)
            //{
            //    DriveIndex = 1;
            //}
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
            ScanBtnEnable = false;
            var result=await	_hcdzClient.ScanDevice(0);
            Thread.Sleep(200);
            var result2=await _hcdzClient.ScanDevice(1);
            if (string.IsNullOrEmpty(result)&& string.IsNullOrEmpty(result2))
            {
                MessageBox.Show("设备所有通道自检正常！");
                ScanBtnEnable = true;
            }
            else if(result.Contains("device")||result.Contains("orther")||result2.Contains("device") || result.Contains("orther"))
            {
                MessageBox.Show("设备自检发生异常！");
                ScanBtnEnable = true;
            }
            else
            {
                string showMsg = string.Format("设备通道{0},{1}有异常! \n", result,result2);
                MessageBox.Show(showMsg);
                LogInfo += showMsg;
                
            }
            ScanBtnEnable = true;
            var network = await _hcdzClient.GetNetWork();
            if (network)
            {
                LogInfo += "网络连接正常!\n";
            }
            else
            {
                LogInfo += "网络连接异常,请检查服务端网口!\n";
            }
        }

        private async void OnReadDma(object obj)
        {
            IsStart = true;
            BtnIsEnabled = false;
            var findItem = _deviceChannelModels.FirstOrDefault(O => O.IsOpen == true); 
			if (findItem==null)
            {
                MessageBox.Show("请打开设备通道！");
                BtnIsEnabled = true;
                IsStart = false;
                return;
            }
            if (string.IsNullOrEmpty(SelectedDsik))
            {
                MessageBox.Show("请选择存储盘符！");
                BtnIsEnabled = true;
                IsStart = false;
                return;
            }
           
            int dma = 16;
            var dmaSize = SelectedDMA.Content.ToString();
            int.TryParse(dmaSize.TrimEnd('K'), out dma);
           
           var  result=await _hcdzClient.OnReadDma(SelectedDsik, dma, 0); 
            Thread.Sleep(1);
           var result2=await  _hcdzClient.OnReadDma(SelectedDsik, dma, 1);
            if (result2.Contains("内存")|| result.Contains("内存"))
            {
                MessageBox.Show("分配内存失败,请重新连接设备!");
                
                OpenDeviceText = "连接设备";
                IsOpen = false;
                IsStart = false;
            }
          

        }

        private void _hcdzClient_NotifyTotal(long size)
        {             
            Application.Current.Dispatcher.Invoke(() => {
                var text = (size / 1048576.0).ToString("f2") + "MB/s";
                TextRate = text;
                if (!IsStart)
                {
                    TextRate = "0.00MB/s";
                }
            });
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
             
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
           
            //foreach (var item in TcpViewModel)
            //{
            //    if (item.IsBegin)
            //    {
            //        item.RateText = string.Format("速率{0}MB/s", ((item.CurrentSize - item.DataSize) / 1048576.0).ToString("f2"));
            //        item.DataSize = item.CurrentSize;                   
            //    }
            //    else
            //    {
            //        item.RateText =string.Empty;
            //        item.DataSize = 0;
            //        item.CurrentSize = 0;
            //    }
            //}
        }

        #region 属性
        private int _driveIndex=-1;
        public int DriveIndex {
            get { return _driveIndex; }
            set { SetProperty(ref _driveIndex, value); }
        }
        public ObservableCollection<TcpClientViewModel> _tcpViewModel;
        public ObservableCollection<TcpClientViewModel> TcpViewModel
        {
            get { return _tcpViewModel; }
            set { SetProperty(ref _tcpViewModel, value); }
        }
        private PcieViewModel _viewModel;
        public PcieViewModel ViewModel
        {
            get { return _viewModel; }
            set { SetProperty(ref _viewModel,value); }
        }
        private bool _isStart;
        public bool IsStart
        {
            get { return _isStart; }
            set { SetProperty(ref _isStart, value); }
        }
        private bool _scanBtnEnable=true;
        public bool ScanBtnEnable
        {
            get { return _scanBtnEnable; }
            set { SetProperty(ref _scanBtnEnable, value); }
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
        public ICommand ConnectClick { get; private set; }
        public ICommand CloseClick { get; private set; }
        public ICommand LocalDataJxCmd { get; private set; }
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
             
           
        }

        private void LoadDeviceChannel()
        {
            var list = new List<DeviceChannelModel>();
            DeviceChannelModel channel0 = new DeviceChannelModel
            {
                Id = 1,
                Name = "通道 1",
                RegAddress = 0x30,
                DiskPath="Bar1",
                DeviceNo = 0,
            };
            DeviceChannelModel channel1 = new DeviceChannelModel
            {
                Id = 2,
                Name = "通道 2",
                RegAddress = 0x34,
                DiskPath = "Bar2",
                DeviceNo = 0,
            };
            DeviceChannelModel channel2 = new DeviceChannelModel
            {
                Id = 3,
                Name = "通道 3",
                RegAddress = 0x38,
                DiskPath = "Bar3",
                DeviceNo = 0,
            };
            DeviceChannelModel channel3 = new DeviceChannelModel
            {
                Id = 4,
                Name = "通道 4",
                RegAddress = 0x40,
                DiskPath = "Bar4",
                DeviceNo = 1,
            };
            DeviceChannelModel channel4 = new DeviceChannelModel
            {
                Id = 5,
                Name = "通道 5",
                RegAddress = 0x44,
                DiskPath = "Bar5",
                DeviceNo = 1,
            };
            DeviceChannelModel channel5 = new DeviceChannelModel
            {
                Id = 6,
                Name = "通道 6",
                RegAddress = 0x48,
                DiskPath = "Bar6",
                DeviceNo = 1,
            };
            list.Add(channel0);
            list.Add(channel3);
            list.Add(channel1);
            list.Add(channel4);
            list.Add(channel2); 
            list.Add(channel5); 
            _deviceChannelModels = new ObservableCollection<DeviceChannelModel>(list);
            //_tcpViewModel = new ObservableCollection<TcpClientViewModel>();
            //_tcpViewModel.Add(new TcpClientViewModel() { Id=1});
            //_tcpViewModel.Add(new TcpClientViewModel() { Id = 2 });
            dispatcherTimer.Start(); 
        }

        private async void InitTcpClient()
        {
            var clients = await _hcdzClient.GetAllTcpClients();
            if (clients == null)
                return;
            foreach (var item in clients)
            {
                if (item.IsConnected)
                {
                    item.BtnIsEnabled = false;
                }
            }
            TcpViewModel = new ObservableCollection<TcpClientViewModel>(clients);
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
