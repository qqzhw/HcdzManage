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

namespace Hcdz.ModulePcie.ViewModels
{
    public class FileDownloadViewModel: BindableBase
    {
        private readonly IUnityContainer _container; 
        private readonly IServiceLocator _serviceLocator;
        public FileDownloadViewModel(IUnityContainer  container, IServiceLocator  serviceLocator,string fileName)
        {
            _container = container;
            _serviceLocator = serviceLocator;
            CloseWindow= new    DelegateCommand<object>(OnCloseWindow);
            FileName = fileName;
            _progresstext = "正在下载...";
            Init();
        }

        private  void Init()
        {
            var index = FileName.LastIndexOf("\\");
            var name = FileName.Substring(index+1, FileName.Length - index-1);
            if (!Properties.Settings.Default.LocalPath.EndsWith("\\"))
            {
                Properties.Settings.Default.LocalPath += "\\";
            }
            var saveFilePath = Properties.Settings.Default.LocalPath+name;
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                client.DownloadFileAsync(new Uri(Properties.Settings.Default.DwonloadUrl + FileName), saveFilePath);
            }
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ProgressText = "下载完毕！";
            
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressText =string.Format("正在下载...{0}%" , e.ProgressPercentage.ToString());
            ProgressValue = e.ProgressPercentage;
        }

        private void OnCloseWindow(object obj)
        {
            var window = obj as Window;
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
        private bool _progressShow = false;
        public bool ProgressShow
        {
            get { return _progressShow; }
            set { SetProperty(ref _progressShow, value); }
        }
        public ICommand CloseWindow { get; private set; }
    }
}
