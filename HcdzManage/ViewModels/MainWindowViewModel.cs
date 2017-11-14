using Hcdz.Framework.Common;
using HcdzManage.Properties;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Interactivity.InteractionRequest;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Pvirtech.Framework;
using Pvirtech.Framework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcdzManage.ViewModels
{ 
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "上位机客户端";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IModuleManager _moduleManager;
        private readonly IServiceLocator _serviceLocator;
        public MainWindowViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IModuleManager moduleManager, IServiceLocator serviceLocator)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            _moduleManager = moduleManager;
            _serviceLocator = serviceLocator;
            CustomPopupRequest = new InteractionRequest<INotification>();
            CustomPopupCommand = new DelegateCommand(RaiseCustomPopup);
            InitLoadSetting();
        }
        public DelegateCommand<object[]> SelectedCommand { get; private set; }
        private ObservableCollection<SystemInfoViewModel> _systemInfos;
        public ObservableCollection<SystemInfoViewModel> SystemInfos
        {
            get { return _systemInfos; }
            set { SetProperty(ref _systemInfos, value); }
        }
        public InteractionRequest<INotification> CustomPopupRequest { get; set; }
        public DelegateCommand CustomPopupCommand { get; set; }
        /// <summary>
        /// 加载设置选项
        /// </summary>
        public void InitLoadSetting()
        {
            FileStream objReader = new FileStream("f:\\dd2",FileMode.Open,FileAccess.Read);
            BinaryReader br = new BinaryReader(objReader);
            string sLine = "";
            ArrayList LineList = new ArrayList();
            var array = new byte[16];
            int index = 0;
            while (objReader.Read(array, 0, 16)>0)
            {
                index++;
                //objReader.Read(array, 0, 16);
                //objReader.Seek(16, SeekOrigin.Begin);
                //objReader.Read(array, 0, 16);
                // sLine = objReader.a
                if (sLine != null && !sLine.Equals(""))
                    LineList.Add(sLine);
            }
            objReader.Close();
            String str = @"f:\\dd2";
            using (FileStream fsWriter = new FileStream(str + @"\opencv-3.0.exe", FileMode.Create, FileAccess.Write))
            {

                using (FileStream fsReader = new FileStream(str + @"\opencv-2.4.9.exe", FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[1024 * 4];//4kB是合适的；
                    int readNum;
                    while ((readNum = fsReader.Read(bytes, 0, bytes.Length)) != 0)//小于说明读完了
                    {
                        fsWriter.Write(bytes, 0, readNum);
                    }


                }//suing reader
            }//using writer
        }

        [InjectionMethod]
        public void Init()
        {
            _systemInfos = new ObservableCollection<SystemInfoViewModel>();
            _eventAggregator.GetEvent<MessageSentEvent<SystemInfo>>().Subscribe(MessageReceived);
            SelectedCommand = new DelegateCommand<object[]>(OnItemSelected);
            InitHeader();


			//string fileName = "testdb.bak";
			//String sourceFullPath = Path.Combine("D:\\", fileName);
			//if (!File.Exists(sourceFullPath))
			//{
			//	throw new Exception("A file given by the sourcePath doesn't exist."); 
			//}

			//String targetFullPath = Path.Combine("F:\\5555\\", fileName); 


			//FileUtilities.CreateDirectoryIfNotExist(Path.GetDirectoryName(targetFullPath));

			//FileUtilities.CopyFileEx(sourceFullPath, targetFullPath, token);

			SetMaxProgress();
		   
	}

		private void token(string source, string destination, long totalFileSize, long totalBytesTransferred)
		{
			double dProgress = (totalBytesTransferred / (double)totalFileSize) * 100.0;
			//progressBar1.Value = (int)dProgress;
		}

		private void SetMaxProgress()
		{
			 
		}

		private void InitHeader()
        {
            _systemInfos.Add(new SystemInfoViewModel()
            {
                Id = "MainView",
                Title = "数据采集",
                InitMode = InitializationMode.OnDemand,
                IsDefaultShow = true,
                IsSelected = true,
            });
          
            _systemInfos.Add(new SystemInfoViewModel()
            {
                Id = "FilesView",
                Title = "文件管理",
                InitMode = InitializationMode.OnDemand,
                IsDefaultShow = false,
            });
            _systemInfos.Add(new SystemInfoViewModel()
            {
                Id = "SettingsView",
                Title = "基本设置",
                InitMode = InitializationMode.OnDemand,
                IsDefaultShow = false,
            });
            _systemInfos.Add(new SystemInfoViewModel()
            {
                Id = "HardDriveView",
                Title = "磁盘管理",
                InitMode = InitializationMode.OnDemand,
                IsDefaultShow = false,
            });
        }

        private void RaiseCustomPopup()
        {

        }

        private void OnItemSelected(object[] selectedItems)
        {
            if (selectedItems != null && selectedItems.Count() > 0)
            {
                foreach (var item in _systemInfos)
                {
                    item.IsSelected = false;
                }
                var model = selectedItems[0] as SystemInfoViewModel;
                model.IsSelected = true;               
               var region = _regionManager.Regions["MainRegion"];
               region.ActiveViews.CollectionChanged += Views_CollectionChanged;
               _regionManager.RequestNavigate("MainRegion", model.Id, navigationCallback);
              //  CustomPopupRequest.Raise(new Notification { Title = "Custom Popup", Content = "Custom Popup Message " });
            }
        }

        private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int s = 9;
        }

        private void navigationCallback(NavigationResult result)
        {
            var s = result;
        }

        private void InitModule(string moduleName)
        {
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
                return;
            var moduleInstance = (IModule)_serviceLocator.GetInstance(moduleType);
            if (moduleInstance != null)
            {
                moduleInstance.Initialize();//调用模块Initialize方法
            }
        }

        private void MessageReceived(SystemInfo model)
        {
            _systemInfos.Add(new SystemInfoViewModel()
            {
                Id = model.Id,
                Title = model.Title,
                InitMode = model.InitMode,
                IsDefaultShow = model.IsDefaultShow,
                State = model.State,
                ModuleInfo = model.ModuleInfo
            });
        }
    }
}
 
