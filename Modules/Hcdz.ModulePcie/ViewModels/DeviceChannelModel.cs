using Hcdz.PcieLib;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.ViewModels
{
    public class DeviceChannelModel: BindableBase
    {
        public int Id { get; set; }
        public string VendorId { get; set; }
        public string DeviceId { get; set; }        
        public string Name { get; set; }
        private bool _isAurora;
        public bool IsAurora
        {
            get { return _isAurora; }
            set { SetProperty(ref _isAurora, value); }
        }
        private string _error;
        public string Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value); }
        }
        /// <summary>
        /// 状态
        /// </summary>
        private bool _status;
        public bool  Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }
        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set { SetProperty(ref _isOpen, value); }
        }
        private bool _btnOpenTrue;
        public bool BtnOpenTrue
        {
            get { return _btnOpenTrue; }
            set { SetProperty(ref _btnOpenTrue, value); }
        }
      

        private string _diskPath;
        public string DiskPath
        {
            get { return _diskPath; }
            set { SetProperty(ref _diskPath,value); }
        }
        private string _dmaData;
        public string DmaData
        {
            get { return _dmaData; }
            set { SetProperty(ref _dmaData, value); }
        }
        /// <summary>
        /// 读取方式 存盘或者测速 
        /// </summary>
        private string _dmaMethod;
        public string DmaMethod
        {
            get { return _dmaMethod; }
            set { SetProperty(ref _dmaMethod, value); }
        }
        private string _info;
        public string Info
        {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }
        private string _speed;
        public string Speed
        {
            get { return _speed; }
            set { SetProperty(ref _speed, value); }
        }
        private long _length;
        public long Length
        {
            get { return _length; }
            set { SetProperty(ref _length, value); }
        }

        private UInt32 _regAddress;
        public UInt32 RegAddress
        {
            get { return _regAddress; }
            set { SetProperty(ref _regAddress, value); }
        }
        public PCIE_Device Device { get; set; }
    }
}
