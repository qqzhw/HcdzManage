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
using System.Windows.Controls;
using System.Threading;
using System.Net;
using System.Net.Http;

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
            //_driveInfoItems = new ObservableCollection<DriveInfo>();
            _directoryItems = new ObservableCollection<DirectoryInfoModel>();
            DoubleClickCmd = new DelegateCommand<MouseButtonEventArgs>(OnDoubleClickDetail);
			LoadDirCmd = new DelegateCommand<object>(OnBackDir);
			SelectedLoadDirCmd = new DelegateCommand<DriveInfoModel>(OnSelectLoadDir);
            ListRightMenue = new DelegateCommand<object>(OnRightMenue);
            LoadedCommand = new DelegateCommand<object>(OnLoad);
            CreateNewCmd=new DelegateCommand<object>(OnCreateNew);
			CopyNewCmd= new DelegateCommand<object>(OnCopyNew);
			FileCopyCmd = new DelegateCommand<object>(OnFileCopy);
            DeleteCmd=new DelegateCommand<object>(OnDeleteFileCopy);
            RefreshCmd = new DelegateCommand<object>(OnFileRefresh);
            FileDownloadCmd = new DelegateCommand<DirectoryInfoModel>(OnDownloadFile);
            MenuItemCommand= new DelegateCommand<object>(OnMenuCommand);
            Menu = new ObservableCollection<MenuItem>(); 
			_hcdzClient.ProgressChanged += FileCopyProgressChanged;
            _hcdzClient.Connected += ClientConnected;
            Initializer();
        }

        private void OnFileRefresh(object obj)
        {
            Initializer();
        }

        private async void OnDeleteFileCopy(object item)
        {
            if (item != null)
            {
                var model = item as DirectoryInfoModel;
                SourceFullPath = model.FullName;
              await  _hcdzClient.DeleteFile(SourceFullPath);
            }
        }

        private void OnMenuCommand(object obj)
        {
            if (SelectedDrive==null)
                return;
            var notification = new MessageNotification()
            {
                Title = "磁盘格式化",
                Content = _container.Resolve<FormatView>(new ParameterOverride("name",SelectedDrive.DriveLetter)),
            };
            PopupWindows.NormalNotificationRequest.Raise(notification, (callback) => {

            });
        }

        private void OnDownloadFile(DirectoryInfoModel model)
        {
            if (model == null)
                return;
            var notification = new MessageNotification()
            {
                Title = "文件下载",
                Content = _container.Resolve<FileDownloadView>(new ParameterOverride("fileName", model.FullName)),
            };
            PopupWindows.NormalNotificationRequest.Raise(notification, (callback) => {

            });
        }

        private void ClientConnected(bool flag)
        {
            if (flag)
            {
                Initializer();
            }
          
        }

        private void FileCopyProgressChanged(string source, string destination, long totalFileSize, long totalBytesTransferred)
		{
			double dProgress = (totalBytesTransferred / (double)totalFileSize) * 100.0;
			//progressBar1.Value = (int)dProgress;
			ProgressValue = (int)dProgress;
			ProgressText = string.Format("{0}%", ProgressValue);
			if (ProgressValue==100)
			{
				Thread.Sleep(2000);
				ProgressShow = false;
				ProgressValue = 0;
				ProgressText = string.Empty;
			}
		}

		private void OnCopyNew(object obj)
		{
            var index = SourceFullPath.LastIndexOf("\\")+1;
            var fileName = SourceFullPath.Substring(index, SourceFullPath.Length-index);

            //string fileName = "testdb.bak";
			 //String sourceFullPath = Path.Combine(SelectedPath, fileName);
			 

			 String targetFullPath = Path.Combine(SelectedPath, fileName);


			////FileUtilities.CreateDirectoryIfNotExist(Path.GetDirectoryName(targetFullPath));
			 ProgressShow = true;
			 //FileUtilities.CopyFileEx(sourceFullPath, targetFullPath, token);
			 _hcdzClient.CopyFileEx(SourceFullPath, targetFullPath);
		}

		private void InitMenu(DirectoryInfoModel model)
		{
            MenuItem mi0 = new MenuItem()
            {
                Header = "下载",
                Command = FileDownloadCmd,
                CommandParameter = model
            };
            Menu.Add(mi0);
            MenuItem mi = new MenuItem()
			{
				Header = "复制",
				Command =FileCopyCmd,
                CommandParameter=model
			};
			Menu.Add(mi);

            mi = new MenuItem()
            {
                Header = "粘贴",
                Command = FileZtCmd,
                CommandParameter = model
            };
            Menu.Add(mi);

            mi = new MenuItem()
			{
				Header = "剪切",
				Command = FileCutCmd,
                CommandParameter = model
            };
			Menu.Add(mi);

			mi = new MenuItem()
			{
				Header = "删除",
				Command = DeleteCmd,
                CommandParameter = model
            };
            mi = new MenuItem()
            {
                Header = "刷新",
                Command = RefreshCmd,
                CommandParameter = model
            };
            Menu.Add(mi); 

		}

		private void OnFileCopy(object item)
		{
			if (item != null)
			{
				var model  = item as DirectoryInfoModel;
                SourceFullPath = model.FullName;
				//if (originalSender != null)
				//{
				//	var row = originalSender.ParentOfType<GridViewRow>();
				//	if (row == null)
				//		return;
				//}
			}
		}

		private   void OnRightMenue(object obj)
		{
			Menu.Clear();
			var clickedItem = (obj as MouseButtonEventArgs).OriginalSource as FrameworkElement;
			if (clickedItem != null)
			{
				var parentRow = clickedItem.ParentOfType<GridViewRow>();
				if (parentRow != null)
				{
					parentRow.IsSelected = true;
					var model = parentRow.DataContext as DirectoryInfoModel;
                    if (!model.IsDir)
                    {
                        InitMenu(model);
                    }
                   
                }
				else
				{
					return;
				}
				
			}
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
                if (string.IsNullOrEmpty(dir))
                    return;
                string filePath = System.IO.Path.Combine(SelectedPath, dir);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
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
            if (items != null)
            { 
                DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o => o.IsDir));
            }
        }

        private async void OnSelectLoadDir(DriveInfoModel drive)
		{
            if (drive == null)
                return;
            IsBusy = true;
			var items=await List(drive.Name);
            SelectedPath = drive.Name;
            OnLoadSelectDir(SelectedPath);
            DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o=>o.IsDir));
            IsBusy = false;
        }

		private async void OnBackDir(object obj)
		{
            if (string.IsNullOrEmpty(UtilsHelper.UploadFilePath))
                return;
            var index = UtilsHelper.UploadFilePath.LastIndexOf("\\");
            if (index < 0)
                return;
            if (index==2)
            {
                index+=1;
            }
            IsBusy = true;

            var parentPath = UtilsHelper.UploadFilePath.Substring(0, index);
            SelectedPath = parentPath;
            UtilsHelper.UploadFilePath = parentPath; 
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
                if (_selectedItem != null)
                {
                    if (!_selectedItem.IsDir)
                        return;
                    IsBusy = true;
                    var items = await List(_selectedItem.FullName);
                    SelectedPath = _selectedItem.FullName;
                    UtilsHelper.UploadFilePath = _selectedItem.FullName;
                    if (items != null)
                    { 
                        DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o => o.IsDir));
                    }
                    IsBusy = false;
                }
            } 
        }
        #region 属性
        private DriveInfoModel _selectedDrive;
         public DriveInfoModel SelectedDrive
        {
            get { return _selectedDrive; }
            set { SetProperty(ref _selectedDrive, value); }
        }
        private string _sourceFullPath;
        public string SourceFullPath
        {
            get { return _sourceFullPath; }
            set { SetProperty(ref _sourceFullPath, value); }
        }
        private string _enableValue;
        public string EnableValue
        {
            get { return _enableValue; }
            set { SetProperty(ref _enableValue, value); }
        }
        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set { SetProperty(ref _selectedPath, value); }
        }
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
		private bool _canCopy = false;
		public bool CanCopy
		{
			get { return _canCopy; }
			set { SetProperty(ref _canCopy, value); }
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
		
		private DirectoryInfoModel _selectedItem;
		public DirectoryInfoModel SelectedItem
		{
			get { return _selectedItem; }
			set { SetProperty(ref _selectedItem, value); }
		} 
		public ObservableCollection<MenuItem> Menu { get; set; }
	 
		public ICommand DoubleClickCmd { get; private set; }
	    public ICommand LoadDirCmd { get; private set; }
		public ICommand SelectedLoadDirCmd { get; private set; }
        public ICommand ListRightMenue { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand CreateNewCmd { get; private set; }
		public ICommand FileCopyCmd { get;  set; }
        public ICommand FileDownloadCmd { get; set; }
        public ICommand MenuItemCommand { get; private set; }
        public ICommand FileZtCmd { get; private set; }
		public ICommand DeleteCmd { get; private set; }
        public ICommand RefreshCmd { get; private set; }
        public ICommand FileCutCmd { get; private set; }
		public ICommand CopyNewCmd { get; private set; }
        #endregion
        private ObservableCollection<DriveInfoModel> driveInfoItems;
        public ObservableCollection<DriveInfoModel> DriveInfoItems
        {
            get { return driveInfoItems; }
            set { SetProperty(ref driveInfoItems, value); }
        }
        public ICommand FormatCmd { get; private set; }

       
        private async void Initializer()
		{
            IsBusy = true;
            //获取远程磁盘
            //DriveInfo[] drives =await _hcdzClient.GetDrives();
            //if (drives!=null)
            //{
            //    DriveInfoItems = new ObservableCollection<DriveInfo>(drives);
            //}
            var models = new ObservableCollection<DriveInfoModel>();

            var drives = await _hcdzClient.GetDrives();
            if (drives == null)
                return;
            //drives = drives.Take(2).ToArray();
            foreach (var item in drives)
            {
                var drive = new DriveInfoModel()
                {
                    AvailableFreeSpace = item.AvailableFreeSpace,
                    AvailableFreeSpaceText = ByteFormatter.ToString(item.AvailableFreeSpace) + " 可用",
                    DriveFormat = item.DriveFormat,
                    DriveType = item.DriveType,
                    IsReady = item.IsReady,
                    Name = item.Name,
                    RootDirectory = item.RootDirectory,
                    TotalFreeSpace = item.TotalFreeSpace,
                    TotalFreeSpaceText = ByteFormatter.ToString(item.TotalFreeSpace),
                    TotalSize = item.TotalSize,
                    TotalSizeText = "共" + ByteFormatter.ToString(item.TotalSize),
                    VolumeLabel = string.IsNullOrEmpty(item.VolumeLabel) ? "本地磁盘 " : item.VolumeLabel,
                    Percent = 100.0 - (int)(item.AvailableFreeSpace * 100.0 / item.TotalSize),
                    DriveLetter = item.Name.Replace("\\", "")
                };
                drive.NameDesc = drive.VolumeLabel + string.Format("({0}:)", item.ToString().Replace(":", "").Replace("\\", ""));
                models.Add(drive);
            }
            DriveInfoItems = models;
            var items = await List();
            if (items != null) 
		   DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items);
            SelectedPath = DriveInfoItems.FirstOrDefault()?.Name;
            EnableValue = DriveInfoItems.FirstOrDefault()?.AvailableFreeSpaceText;
            if (!string.IsNullOrEmpty(SelectedPath))
            {
                OnLoadSelectDir(SelectedPath.Substring(0, 3));
            }
            
           IsBusy = false;         
        }
        private async void OnLoadSelectDir(object dirPath)
        {
            if (dirPath == null)
            {
                return;
            }
            try
            {
                var drive = await _hcdzClient.GetSingleDrive(dirPath.ToString());
                if (drive == null)
                    return; 
                EnableValue = ByteFormatter.ToString(drive.AvailableFreeSpace) + " 可用";
                 
            }
            catch (Exception ex)
            {

            }

            //if (drives.Count() > 1)
            //{
            //    DriveIndex = 1;
            //}
        }
        /// <summary>
        /// 远程获取文件目录及文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
		public async Task<List<DirectoryInfoModel>> List(string path="")
		{ 
			var item = await _hcdzClient.GetFileList(path);
			return item;
		}
	}
}
