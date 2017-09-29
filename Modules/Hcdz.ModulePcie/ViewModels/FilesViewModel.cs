using Hcdz.ModulePcie.Models;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Hcdz.ModulePcie.Views;
using Pvirtech.Framework.Interactivity;
using Microsoft.AspNet.SignalR.Client;

namespace Hcdz.ModulePcie.ViewModels
{
	public class FilesViewModel : BindableBase
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IUnityContainer _container;
		private readonly IRegionManager _regionManager;
		private readonly IServiceLocator _serviceLocator;
		private readonly IHcdzClient  _hcdzClient;
		public FilesViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IServiceLocator serviceLocator, IHcdzClient  hcdzClient)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_regionManager = regionManager;
			_serviceLocator = serviceLocator;
			_hcdzClient = hcdzClient;
			//_directoryItems = new ObservableObject<DirectoryInfoModel>();
			DoubleClickCmd= new DelegateCommand<MouseButtonEventArgs>(OnDoubleClickDetail);
			LoadDirCmd = new DelegateCommand<object>(OnBackDir);
			SelectedLoadDirCmd = new DelegateCommand<DriveInfo>(OnSelectLoadDir);
            ListRightMenue = new DelegateCommand<object>(OnRightMenue);
            LoadedCommand = new DelegateCommand<object>(OnLoad);
            CreateNewCmd=new DelegateCommand<object>(OnCreateNew);
            Initializer(); 
        }

        private void OnRightMenue(object obj)
        {
            
        }

        private void OnCreateNew(object obj)
        {
            var confirmation = new Confirmation()
            {
                Content = _container.Resolve<ChildView>(),
                IsModal=true,
                Title="新建",
            };
            PopupWindows.NotificationRequest.Raise(confirmation, (o)=> {
                var context = (ChildView)o.Content;
                var dir=context.txtDir.Text.Trim();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    OnLoad();
                }
                else
                {
                    MessageBox.Show("创建失败,已存在该文件夹！");
                }
            });
           //ChildView window = new ChildView();
          //  window.ShowDialog();
        }

        private async void OnLoad(object obj=null)
        {
            var items = await List(SelectedPath);
            DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o => o.IsDir));

        }

        private async void OnSelectLoadDir(DriveInfo drive)
		{
            IsBusy = true;
			var items=await List(drive.Name);
			DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o=>o.IsDir));
            IsBusy = false;
        }

		private async void OnBackDir(object obj)
		{
            if (string.IsNullOrEmpty(UtilsHelper.UploadFilePath))
            {
                return;
            }
            var index = UtilsHelper.UploadFilePath.LastIndexOf("\\");
            if (index < 0)
                return;
            if (index==2)
            {
                index+=1;
            }
            var parentPath = UtilsHelper.UploadFilePath.Substring(0, index);
            IsBusy = true;
            var items =await List(parentPath);
            DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o => o.IsDir));
            IsBusy = false;
    }

        /// <summary>
        /// 双击查看详细
        /// </summary>
        /// <param name="obj"></param>
        private async void OnDoubleClickDetail(MouseButtonEventArgs item)
        {
            if (item != null)
            {
                var originalSender = item.OriginalSource as FrameworkElement;

                if (originalSender != null)
                {
                    var row = originalSender.ParentOfType<GridViewRow>();
                    if (row == null)
                        return;
                }
                if (SelectedItem != null)
                {
                    if (!SelectedItem.IsDir)
                        return;
                    IsBusy = true;
                    var items = await List(SelectedItem.FullName);
                    DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o => o.IsDir));
                    // UtilsHelper.UploadFilePath = SelectedItem.FullName;
                    IsBusy = false;
                }
            } 
        }
        #region 属性
        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set { SetProperty(ref _selectedPath, value); }
        }
        
        private bool _isBusy=false;
        public bool IsBusy {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
        private ObservableCollection<DirectoryInfoModel> _directoryItems;
		public ObservableCollection<DirectoryInfoModel> DirectoryItems
		{
			get { return _directoryItems; }
			set { SetProperty(ref _directoryItems, value); }
		}
		private ObservableCollection<DriveInfo>  _driveInfoItems;
		public ObservableCollection<DriveInfo> DriveInfoItems
		{
			get { return _driveInfoItems; }
			set { SetProperty(ref _driveInfoItems, value); }
		}
		private DirectoryInfoModel _selectedItem;
		public DirectoryInfoModel SelectedItem
		{
			get { return _selectedItem; }
			set { SetProperty(ref _selectedItem, value); }
		}
		public ICommand DoubleClickCmd { get; private set; }
	    public ICommand LoadDirCmd { get; private set; }
		public ICommand SelectedLoadDirCmd { get; private set; }
        public ICommand ListRightMenue { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand CreateNewCmd { get; private set; }
        #endregion

        private async void Initializer()
		{
            IsBusy = true;
			DriveInfo[] drives = DriveInfo.GetDrives();
			_driveInfoItems = new ObservableCollection<DriveInfo>(drives);
			  
           var items = await List();
		  DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items);
          IsBusy = false;
          
        }
		public async Task<List<DirectoryInfoModel>> List(string path="")
		{ 
			var item = await _hcdzClient.GetFileList(path);
			return item;

			//         _directoryItems?.Clear();
			//         FileSystemInfo[] dirFileitems = null;
			//var list = new List<DirectoryInfoModel>();
			//           await Task.Run(() =>
			//         {
			//             if (string.IsNullOrEmpty(path))
			//             {
			//                 path = _driveInfoItems[0].Name;
			//                 DirectoryInfo dirInfo = new DirectoryInfo(path);//根目录				
			//                 dirFileitems = dirInfo.GetFileSystemInfos("*",SearchOption.TopDirectoryOnly);
			//             }
			//             else
			//             {                    
			//                 DirectoryInfo dirInfo = new DirectoryInfo(path);//根目录				 
			//                 dirFileitems = dirInfo.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
			//             }
			//             SelectedPath = path;
			//             UtilsHelper.UploadFilePath = path;
			//             foreach (var item in dirFileitems)
			//             {
			//                 if (item is DirectoryInfo)
			//                 {
			//                     var directory = item as DirectoryInfo;
			//                     if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (directory.Attributes & FileAttributes.System) != FileAttributes.System)
			//                     {
			//                         list.Add(new DirectoryInfoModel()
			//                         {
			//                             Root = directory.Root,
			//                             FullName = directory.FullName,
			//                             IsDir = true,
			//                             Icon = "pack://application:,,,/Hcdz.ModulePcie;component/Images/folder.png",
			//                             Name = directory.Name,
			//                             Parent = directory.Parent,
			//                             CreationTime = directory.CreationTime,
			//                             Exists = directory.Exists,
			//                             Extension = directory.Extension,
			//                             LastAccessTime = directory.LastAccessTime,
			//                             LastWriteTime = directory.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
			//                         });
			//                     }
			//                 }
			//                 else if (item is FileInfo)
			//                 {
			//                     var file = item as FileInfo;
			//                     if ((file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (file.Attributes & FileAttributes.System) != FileAttributes.System)
			//                     {
			//                         list.Add(new DirectoryInfoModel()
			//                         {
			//                             FullName = file.FullName,
			//                             // FullPath=file.FullPath
			//                             IsDir = false,
			//                             Name = file.Name,
			//                             CreationTime = file.CreationTime,
			//                             Exists = file.Exists,
			//                             Extension = file.Extension,
			//                             LastAccessTime = file.LastAccessTime,
			//                             LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
			//                             IsReadOnly = file.IsReadOnly,
			//                             //Directory = file.Directory,
			//                             DirectoryName = file.DirectoryName,
			//                             LengthText= ByteFormatter.ToString(file.Length),
			//                             Length =file. Length
			//                         });
			//                     }

			//                 }
			//             }
			//             return list;
			//         });            
			//         return list;
		}
	}
}
