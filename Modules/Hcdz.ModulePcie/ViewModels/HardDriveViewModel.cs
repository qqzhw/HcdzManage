using Prism.Mvvm;
using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Hcdz.ModulePcie.Models;
using Microsoft.Practices.Unity;
using System.Windows.Input;
using Prism.Commands;

namespace Hcdz.ModulePcie.ViewModels
{
    public class HardDriveViewModel: BindableBase
    {
        private readonly IHcdzClient _hcdzClient;
        public HardDriveViewModel(IHcdzClient  hcdzClient)
        {
            driveInfoItems = new ObservableCollection<DriveInfoModel>();
			FormatCmd= new DelegateCommand<object>(OnFormatDisk);
            _hcdzClient = hcdzClient;
			Init();
        }

		private void OnFormatDisk(object obj)
		{
			 
		}

		private ObservableCollection<DriveInfoModel> driveInfoItems;
        public ObservableCollection<DriveInfoModel> DriveInfoItems
        {
            get { return driveInfoItems; }
            set { SetProperty(ref driveInfoItems,value); }
        }
         public ICommand FormatCmd { get; private set; }

		private async  void Init()
        {
            var models = new ObservableCollection<DriveInfoModel>();
            
            DriveInfo[] drives = await _hcdzClient.GetDrives();
            if (drives == null)
                return;
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
        }
    }
}
