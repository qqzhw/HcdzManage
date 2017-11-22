using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Hcdz.ModulePcie.ViewModels
{
    public class FileDownloadViewModel: BindableBase
    {
        private readonly IUnityContainer _container; 
        private readonly IServiceLocator _serviceLocator;
        private DispatcherTimer dispatcherTimer;
        private long dataSize=0;
        private long currentSize=0;
        private WebClient client;
        public FileDownloadViewModel(IUnityContainer  container, IServiceLocator  serviceLocator,string fileName)
        {
            _container = container;
            _serviceLocator = serviceLocator;
            CloseWindow= new    DelegateCommand<object>(OnCloseWindow);
            LoadCmd = new DelegateCommand(OnLoad);
            FileName = fileName;
            _progresstext = "正在下载...";
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background);
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            RateText = string.Format("{0}MB/s", ((currentSize-dataSize) / 1048576.0).ToString("f2"));
            dataSize = currentSize;
        }

        private void OnLoad()
        {
            Init();
        }

        private void Init()
        {
            var index = FileName.LastIndexOf("\\");
            var name = FileName.Substring(index + 1, FileName.Length - index - 1);
            if (!Properties.Settings.Default.LocalPath.EndsWith("\\"))
            {
                Properties.Settings.Default.LocalPath += "\\";
            }
            var saveFilePath = Properties.Settings.Default.LocalPath + name;
            client = new WebClient();

            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(Properties.Settings.Default.DwonloadUrl + FileName), saveFilePath);


            dispatcherTimer.Start();
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            RateText = string.Empty;
            ProgressText = "下载完毕！";
            dispatcherTimer.Stop();
            client.Dispose();
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            currentSize = e.BytesReceived;
            ProgressText =string.Format("正在下载...{0}%" , e.ProgressPercentage.ToString());
            ProgressValue = e.ProgressPercentage;
        }

        private void OnCloseWindow(object obj)
        {
            var window = obj as Window;
            dispatcherTimer.Stop();
            client.CancelAsync();
            client.Dispose();
            window.Close();
        }
        public string FileName { get; set; }
        public string FullName { get; set; }
        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }
        private string _progresstext;
        public string ProgressText
        {
            get { return _progresstext; }
            set { SetProperty(ref _progresstext, value); }
        }
        private string _ratetext;
        public string  RateText
        {
            get { return _ratetext; }
            set { SetProperty(ref _ratetext, value); }
        }
        private bool _progressShow = false;
        public bool ProgressShow
        {
            get { return _progressShow; }
            set { SetProperty(ref _progressShow, value); }
        }
        public ICommand CloseWindow { get; private set; }
        public ICommand LoadCmd { get; private set; }
    }
}
