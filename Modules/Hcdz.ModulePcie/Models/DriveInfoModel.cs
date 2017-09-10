using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.Models
{
    public class DriveInfoModel
    {
        public virtual int Id { get; set; }
        //
        // 摘要:
        //     获取驱动器的名称，如 C:\。
        //
        // 返回结果:
        //     驱动器的名称。
        public string Name { get; set; }
        //
        // 摘要:
        //     获取驱动器类型，如 CD-ROM、可移动、网络或固定。
        //
        // 返回结果:
        //     指定驱动器类型的枚举值之一。
        public DriveType DriveType { get; set; }
        //
        // 摘要:
        //     获取文件系统的名称，例如 NTFS 或 FAT32。
        //
        // 返回结果:
        //     指定驱动器上文件系统的名称。
        // 
        public string DriveFormat { get; set; }
        //
        // 摘要:
        //     获取一个指示驱动器是否已准备好的值。
        //
        // 返回结果:
        //     如果驱动器已准备好，则为 true；如果驱动器未准备好，则为 false。
        public bool IsReady { get; set; }
        //
        // 摘要:
        //     指示驱动器上的可用空闲空间总量（以字节为单位）。
        //
        // 返回结果:
        //     驱动器上的可用空闲空间量（以字节为单位）。
        //

        public long AvailableFreeSpace { get; set; }
        //
        // 摘要:
        //     获取驱动器上的可用空闲空间总量（以字节为单位）。
        //
        // 返回结果:
        //     驱动器上的可用空闲空间总量（以字节为单位）。 
        public string AvailableFreeSpaceText { get; set; }

        public long TotalFreeSpace { get; set; }
        public string TotalFreeSpaceText { get; set; }
        //
        // 摘要:
        //     获取驱动器上存储空间的总大小（以字节为单位）。
        //
        // 返回结果:
        //     驱动器的总大小（以字节为单位）。 

        public long TotalSize { get; set; }
        public string TotalSizeText { get; set; }
        //
        // 摘要:
        //     获取驱动器的根目录。
        //
        // 返回结果:
        //     包含驱动器根目录的对象。
        public DirectoryInfo RootDirectory { get; set; }
        //
        // 摘要:
        //     获取或设置驱动器的卷标。
        //
        // 返回结果:
        //     卷标。 
        public string VolumeLabel { get; set; }
        public double Percent { get; set; }
        public string NameDesc { get; set; }
        public string DriveLetter { get; set; }
    }
   
}
