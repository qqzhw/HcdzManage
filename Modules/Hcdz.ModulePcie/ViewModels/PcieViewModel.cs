using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.ViewModels
{
    public class PcieViewModel: BindableBase
    {
        public string DeviceName { get; set; }
        private bool _isStart;
        public bool IsStart {
            get { return _isStart; }
            set { SetProperty(ref _isStart, value); }
        }
        private int _blockSize;
        public int BlockSize
        {
            get { return _blockSize; }
            set { SetProperty(ref _blockSize, value); }

        }
        private bool _isEnable;
        public bool IsEnable {
            get { return _isEnable; }
            set { SetProperty(ref _isEnable, value); }

        }
        private bool _dmaEnable;
        public bool DmaEnable
        {
            get { return _dmaEnable; }
            set { SetProperty(ref _dmaEnable, value); }

        }
        private long _speed;
        public long Speed
        {
            get { return _speed; }
            set { SetProperty(ref _speed, value); }
        }
        private string _speedText;
        public string SpeedText
        {
            get { return _speedText; }
            set { SetProperty(ref _speedText, value); }
        }
    }
}
