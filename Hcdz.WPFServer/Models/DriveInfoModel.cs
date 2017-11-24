using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.WPFServer.Models
{
    
    public class DriveInfoModel
    {
        public virtual int Id { get; set; }
        
        public string Name { get; set; }
       
        
        public string DriveFormat { get; set; }
        
        public bool IsReady { get; set; }
        

        public long AvailableFreeSpace { get; set; }
        
        public string AvailableFreeSpaceText { get; set; }

        public long TotalFreeSpace { get; set; }
        public string TotalFreeSpaceText { get; set; }
         
        public long TotalSize { get; set; }
        public string TotalSizeText { get; set; }
       
        
        public string VolumeLabel { get; set; }
        public double Percent { get; set; }
        public string NameDesc { get; set; }
        public string DriveLetter { get; set; }
    }
}
