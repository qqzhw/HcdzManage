using Hcdz.ModulePcie.Properties;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hcdz.ModulePcie.ViewModels
{
    public class SettingsViewModel: BindableBase
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IUnityContainer _container; 
		private readonly IServiceLocator _serviceLocator; 
		public SettingsViewModel(IUnityContainer container, IEventAggregator eventAggregator, IServiceLocator serviceLocator)
		{
			_container = container;
			_eventAggregator = eventAggregator; 
			_serviceLocator = serviceLocator;
			SaveCommand = new DelegateCommand(OnSaveData);
			Initializer();
		}

		private void OnSaveData()
		{
			Settings.Default.Bar0 = Bar0;
			Settings.Default.Bar1 = Bar1;
			Settings.Default.Bar2 = Bar2;
			Settings.Default.Bar3 = Bar3;
			Settings.Default.Bar4 = Bar4;
			Settings.Default.Bar5 = Bar5;
			Settings.Default.ServerUrl = SignalrServer;
			Settings.Default.DwonloadUrl = DownloadUrl;
            Settings.Default.LocalPath = LocalPath;
            Settings.Default.Save();
		}

		private void Initializer()
		{
			_bar0 =Settings.Default.Bar0;
			_bar1 = Settings.Default.Bar1;
			_bar2 = Settings.Default.Bar2;
			_bar3 = Settings.Default.Bar3;
			_bar4 = Settings.Default.Bar4;
			_bar5 = Settings.Default.Bar5;
			_signalrServer = Settings.Default.ServerUrl;
			_downloadUrl = Settings.Default.DwonloadUrl;
            _localPath= Settings.Default.LocalPath;
        }
        #region 属性
        
        private string  _localPath;
        public string LocalPath
        {
            get { return _localPath; }
            set { SetProperty(ref _localPath, value); }
        }
        private string _bar0;
		public string Bar0
		{
			get { return _bar0; }
			set { SetProperty(ref _bar0,value); }
		}
		private string _bar1;
		public string Bar1
		{
			get { return _bar1; }
			set { SetProperty(ref _bar1, value); }
		}
		private string _bar2;
		public string Bar2
		{
			get { return _bar2; }
			set { SetProperty(ref _bar2, value); }
		}
		private string _bar3;
		public string Bar3
		{
			get { return _bar3; }
			set { SetProperty(ref _bar3, value); }
		}
		private string _bar4;
		public string Bar4
		{
			get { return _bar4; }
			set { SetProperty(ref _bar4, value); }
		}
		private string _bar5;
		public string Bar5
		{
			get { return _bar5; }
			set { SetProperty(ref _bar5, value); }
		}
		private string _signalrServer;
		public string SignalrServer
		{
			get { return _signalrServer; }
			set { SetProperty(ref _signalrServer, value); }
		}
		private string _downloadUrl;
		public string DownloadUrl
		{
			get { return _downloadUrl; }
			set { SetProperty(ref _downloadUrl, value); }
		}

		public ICommand SaveCommand { get; private set; }

		#endregion
	}
}
