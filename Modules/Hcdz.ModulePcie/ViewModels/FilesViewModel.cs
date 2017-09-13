﻿using Hcdz.ModulePcie.Models;
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
			LoadDirCmd = new DelegateCommand<object>(OnBackDir);
			SelectedLoadDirCmd = new DelegateCommand<DriveInfo>(OnSelectLoadDir);
			Initializer();
		}

		private void OnSelectLoadDir(DriveInfo drive)
		{
			var items=List(drive.Name);
			DirectoryItems = new ObservableCollection<DirectoryInfoModel>(items.OrderByDescending(o=>o.IsDir));
		}

		private void OnBackDir(object obj)
		{
			 
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
				if (SelectedItem!=null)
				{

				}
			} 
		}
		#region 属性

		
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
		#endregion

		private void Initializer()
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			_driveInfoItems = new ObservableCollection<DriveInfo>(drives);
			DirectoryListModel model = new DirectoryListModel();
			_directoryItems = new ObservableCollection<DirectoryInfoModel>(List());
		}
		public virtual List<DirectoryInfoModel> List(string path="")
		{
			 FileSystemInfo[] dirFileitems = null;
			var list = new List<DirectoryInfoModel>();
			if (string.IsNullOrEmpty(path))
			{				
				path= _driveInfoItems[0].Name;
				DirectoryInfo dirInfo = new DirectoryInfo(path);//根目录				
				dirFileitems = dirInfo.GetFileSystemInfos();
			}
			else
			{
				DirectoryInfo dirInfo = new DirectoryInfo(path);//根目录				 
				dirFileitems = dirInfo.GetFileSystemInfos();
			}
			UtilsHelper.UploadFilePath = path;
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
							Icon = "pack://application:,,,/Hcdz.ModulePcie;component/Images/folder.png",
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
