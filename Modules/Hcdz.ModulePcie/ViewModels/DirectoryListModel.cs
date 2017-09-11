 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Hcdz.ModulePcie.ViewModels
{
    public class DirectoryListModel
    {
        public DirectoryListModel()
        {
            AvailableDrivers = new List<ComboBoxItem>();
        }

        [DisplayName("文件路径")]
        public string SearchDriverName { get; set; }

        [DisplayName("本地磁盘")]
        public string SearchDriverId { get; set; }
        public IList<ComboBoxItem> AvailableDrivers { get; set; }
    }
}
