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

namespace Hcdz.ModulePcie.ViewModels
{
	public class FilesViewModel : BindableBase
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IUnityContainer _container;
		private readonly IRegionManager _regionManager;
		private readonly IServiceLocator _serviceLocator;
		public FilesViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IServiceLocator serviceLocator)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_regionManager = regionManager;
			_serviceLocator = serviceLocator;
			//_directoryItems = new ObservableObject<DirectoryInfoModel>();
			DoubleClickCmd= new DelegateCommand<MouseButtonEventArgs>(OnDoubleClickDetail);
			Initializer();
		}

		/// <summary>
		/// 双击查看详细
		/// </summary>
		/// <param name="obj"></param>
		private void OnDoubleClickDetail(MouseButtonEventArgs item)
		{
			if (item != null)
			{
                var originalSender = item.OriginalSource as FrameworkElement;

                if (originalSender!=null)
				{
					var row = originalSender.ParentOfType<GridViewRow>();
					if (row == null)
						return;
				}
			} 
		}

		private ObservableCollection<DirectoryInfoModel> _directoryItems;
		public ObservableCollection<DirectoryInfoModel> DirectoryItems
		{
			get { return _directoryItems; }
			set { SetProperty(ref _directoryItems, value); }
		}
		public ICommand DoubleClickCmd { get; private set; }
		 

		private void Initializer()
		{
			DirectoryListModel model = new DirectoryListModel();
			_directoryItems = new ObservableCollection<DirectoryInfoModel>(List(model));
		}
		public virtual List<DirectoryInfoModel> List(DirectoryListModel model)
		{
			FileSystemInfo[] dirFileitems = null;
			var list = new List<DirectoryInfoModel>();
			if (string.IsNullOrEmpty(model.SearchDriverName))
			{
				DriveInfo[] drives = DriveInfo.GetDrives();
				model.SearchDriverId = drives[0].Name;
				DirectoryInfo dirInfo = new DirectoryInfo(model.SearchDriverId);//根目录
				UtilsHelper.UploadFilePath = model.SearchDriverId;
				dirFileitems = dirInfo.GetFileSystemInfos();
			}
			else
			{
				DirectoryInfo dirInfo = new DirectoryInfo(model.SearchDriverName);//根目录
				UtilsHelper.UploadFilePath = model.SearchDriverName;
				dirFileitems = dirInfo.GetFileSystemInfos();
			}
			foreach (var item in dirFileitems)
			{
				if (item is DirectoryInfo)
				{
					var directory = item as DirectoryInfo;
					if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (directory.Attributes & FileAttributes.System) != FileAttributes.System)
					{
						list.Add(new DirectoryInfoModel()
						{
							Root = directory.Root,
							FullName = directory.FullName,
							IsDir = true,
							Icon = "folder.png",
							Name = directory.Name,
							Parent = directory.Parent,
							CreationTime = directory.CreationTime,
							Exists = directory.Exists,
							Extension = directory.Extension,
							LastAccessTime = directory.LastAccessTime,
							LastWriteTime = directory.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
						});
					}
				}
				else if (item is FileInfo)
				{
					var file = item as FileInfo;
					if ((file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden && (file.Attributes & FileAttributes.System) != FileAttributes.System)
					{
						list.Add(new DirectoryInfoModel()
						{

							FullName = file.FullName,
							// FullPath=file.FullPath
							IsDir = false,
							Name = file.Name,
							CreationTime = file.CreationTime,
							Exists = file.Exists,
							Extension = file.Extension,
							LastAccessTime = file.LastAccessTime,
							LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
							IsReadOnly = file.IsReadOnly,
							Directory = file.Directory,
							DirectoryName = file.DirectoryName,
							Length = file.Length,
						});
					}

				}
			}
			return list;
		}
	}
}
