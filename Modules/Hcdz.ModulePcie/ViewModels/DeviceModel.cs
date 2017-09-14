using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.ViewModels
{
    public class DeviceModel: BindableBase
    {
        public string VendorId { get; set; }
        public string DeviceId { get; set; }
          
    }
}
