using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.ViewModels
{
   public  class TcpClientViewModel: BindableBase
    {
        public int Id { get; set; }
         
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }
        private bool _btnIsEnabled = false;
        public bool BtnIsEnabled { get { return _btnIsEnabled; } set { SetProperty(ref _btnIsEnabled, value); } }

        private  string  _messageText;
        public    string  MessageText {
            get { return _messageText; }
            set { SetProperty(ref _messageText, value); }
        }

        private string _ip;
        public string Ip
        {
            get
            {
                return _ip;
            }
            set
            {
                SetProperty(ref _ip, value);
            }
        }

        private string _port;
        public string Port
        {
            get
            {
                return _port;
            }
            set
            {
                SetProperty(ref _port, value);
            }
        }
    }
}
