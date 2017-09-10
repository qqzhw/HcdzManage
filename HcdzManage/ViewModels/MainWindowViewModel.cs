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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
           
        }

        [InjectionMethod]
        public void Init()
        {
            _systemInfos = new ObservableCollection<SystemInfoViewModel>();
            _eventAggregator.GetEvent<MessageSentEvent<SystemInfo>>().Subscribe(MessageReceived);
            SelectedCommand = new DelegateCommand<object[]>(OnItemSelected);
            InitHeader();
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
                Id = "HardDriveView",
                Title = "磁盘管理",
                InitMode = InitializationMode.OnDemand,
                IsDefaultShow = false,
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
 
