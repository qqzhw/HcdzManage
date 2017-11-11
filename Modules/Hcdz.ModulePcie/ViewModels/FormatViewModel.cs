using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Hcdz.ModulePcie.ViewModels
{
    public class FormatViewModel: BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IServiceLocator _serviceLocator;
        private readonly IHcdzClient _hcdzClient;
        private int index = 0;
        public FormatViewModel(IUnityContainer container, IServiceLocator serviceLocator, string name, IHcdzClient hcdzClient)
        {
            _container = container;
            _serviceLocator = serviceLocator;
            _hcdzClient = hcdzClient;
            CloseWindow = new DelegateCommand<object>(OnCloseWindow);
            FileName =name;
            _progresstext = "正在格式化...";
            Init();
        }

        private async void Init()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return;
            }
            // var index = FileName.LastIndexOf("\\"); 

            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);   //间隔1秒
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            var result = await _hcdzClient.FormatDrive(FileName);
            timer.Stop();
            ProgressShow = false;
            if (result)
            { 
                ProgressText = string.Format("格式化成功，用时{0}秒！",index);
            }
            else
            {
                ProgressText = string.Format("格式化失败，用时{0}秒！", index);
            }

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            index++;
            ProgressText = string.Format("正在格式化...{0}秒", index);
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
        private bool _progressShow = true;
        public bool ProgressShow
        {
            get { return _progressShow; }
            set { SetProperty(ref _progressShow, value); }
        }
        public ICommand CloseWindow { get; private set; }
    }
}
