﻿using Prism.Mvvm;
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
        private bool _btnIsEnabled = true;
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

        private int _port;
        public int Port
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
        private string _fileDir;
        public string FileDir
        {
            get
            {
                return _fileDir;
            }
            set
            {
                SetProperty(ref _fileDir, value);
            }
        }
        private long _dataSize;
        public long DataSize
        {
            get
            {
                return _dataSize;
            }
            set
            {
                SetProperty(ref _dataSize, value);
            }
        }
        public long _rateSize;
        public long RateSize
        {
            get
            {
                return _rateSize;
            }
            set
            {
                SetProperty(ref _rateSize, value);
            }
        }
        public long _currentSize;
        public long CurrentSize
        {
            get
            {
                return _currentSize;
            }
            set
            {
                SetProperty(ref _currentSize, value);
            }
        }
        public int TimeIndex { get; set; }
        public bool IsBegin { get; set; }
        public string _rateText;
        public string RateText
        {
            get
            {
                return _rateText;
            }
            set
            {
                SetProperty(ref _rateText, value);
            }
        }
    }
}
