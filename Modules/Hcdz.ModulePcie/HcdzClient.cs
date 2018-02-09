using Hcdz.ModulePcie.Models;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Transports;
using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hcdz.ModulePcie.ViewModels;

namespace Hcdz.ModulePcie
{ 
	public class HcdzClient : IHcdzClient
	{
	 
		private readonly Func<IClientTransport> _transportFactory;

		private IHubProxy _chat;
		private HubConnection _connection; 
		public HcdzClient() :
			this(transportFactory: () => new AutoTransport(new DefaultHttpClient()))
		{
			
		}

		public HcdzClient(Func<IClientTransport> transportFactory)
		{
			SourceUrl = Properties.Settings.Default.ServerUrl;
			_transportFactory = () => new AutoTransport(new DefaultHttpClient());// transportFactory;
			TraceLevel = TraceLevels.All;  
		}

		public event Action<string> MessageReceived;
		public event Action<IEnumerable<string>> LoggedOut; 
		public event Action<string, string, long,long> ProgressChanged;
        public event Action<string,int> NoticeScanByte;
        public event Action<bool, TcpClientViewModel> NoticeTcpConnect;
        public event  Action<TcpClientViewModel> NoticeTcpData;
        // Global  
       public  event Action<long> NotifyTotal;
        public event Action<bool> Connected;
        public event Action<int> NotifyFormatTime;
        public string SourceUrl { get; private set; }
		public bool AutoReconnect { get; set; } = true;
		public TextWriter TraceWriter { get; set; }
		public TraceLevels TraceLevel { get; set; }
		 
		public HubConnection Connection
		{
			get
			{
				return _connection;
			}
		}

		public ICredentials Credentials
		{
			get
			{
				return _connection.Credentials;
			}
			set
			{
				_connection.Credentials = value;
			}
		}

		public event Action Disconnected
		{
			add
			{
				_connection.Closed += value;
			}
			remove
			{
				_connection.Closed -= value;
			}
		}

		public event Action<StateChange> StateChanged
		{
			add
			{
				_connection.StateChanged += value;
			}
			remove
			{
				_connection.StateChanged -= value;
			}
		}
		private string GetAddressIp()
		{
			string Ip = string.Empty;
			foreach (IPAddress item in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (item.AddressFamily.ToString() == "InterNetwork")
				{
					Ip = item.ToString();
					return Ip;
				}
			}
			return Ip;
		}
		public Task Connect(string userName="")
		{
			var parameters = new Dictionary<string, string> { 
				{ "userName" , userName }, 
				{"ip",GetAddressIp() }
			}; 
			_connection = new HubConnection(SourceUrl, parameters); 
			_connection.Headers.Add("hcdz-client", "1");
			if (TraceWriter != null)
			{
				_connection.TraceWriter = TraceWriter;
			}

			_connection.TraceLevel = TraceLevel;

			_chat = _connection.CreateHubProxy("MyHub");

			SubscribeToEvents();

			return _connection.Start(_transportFactory());
		}

		 
		//得到在线用户
		public Task GetOnlineUsers()
		{
			if (_connection.State == ConnectionState.Connected)
			{
				return _chat.Invoke("GetOnlineUsers");
			}

			return EmptyTask;
		}
		 
		public Task LogOut()
		{
			return _chat.Invoke("LogOut");
		}

		 
		public async Task Send(ClientMessage message)
		{
			if (_connection.State == ConnectionState.Connected)
			{
				await _chat.Invoke("Send", message);
			}
		}
		 
		public async Task Send(ClientMessage message, TimeSpan timeout)
		{
			using (var cts = new CancellationTokenSource(timeout))
			{
				await Send(message);
			}
		}
		 
		public Task SetFlag(string countryCode)
		{
			return SendCommand("flag {0}", countryCode);
		}

		public Task SetNote(string noteText)
		{
			return SendCommand("note {0}", noteText);
		}

		public Task SendPrivateMessage(string userName, string message)
		{
			return SendCommand("msg {0} {1}", userName, message);
		}

		public Task Kick(string userName, string roomName)
		{
			return SendCommand("kick {0} {1}", userName, roomName);
		}

		public Task<bool> CheckStatus()
		{
			return _chat.Invoke<bool>("CheckStatus");
		}

		public Task SetTyping(string roomName)
		{
			return _chat.Invoke("Typing", roomName);
		}

		public Task<IEnumerable<ClientMessage>> GetPreviousMessages(string fromId)
		{
			return _chat.Invoke<IEnumerable<ClientMessage>>("GetPreviousMessages", fromId);
		}
		 
		public void Disconnect()
		{
			_connection.Stop();
		}

        private void SubscribeToEvents()
        {
            if (AutoReconnect)
            {
                Disconnected += OnDisconnected;
            }
            _chat.On<bool>(ClientEvents.Connected, (message) =>
            {
                Execute(Connected, msg => msg(message));
            });
            _chat.On<string>(ClientEvents.NoticeMessage, (message) =>
            {
                Execute(MessageReceived, messageReceived => messageReceived(message));
            });

            _chat.On<IEnumerable<string>>(ClientEvents.LogOut, rooms =>
            {
                Execute(LoggedOut, loggedOut => loggedOut(rooms));
            });

            _chat.On<string, string, long, long>(ClientEvents.FileProgress, (source, destination, totalFileSize, totalBytesTransferred) =>
             {
                 Execute(ProgressChanged, fileChanged => fileChanged(source, destination, totalFileSize, totalBytesTransferred));
             });
            _chat.On<long>(ClientEvents.NotifyTotalSize, (totalSize) =>
            {
                Execute(NotifyTotal, total => total(totalSize));
            });
            _chat.On<int>(ClientEvents.NotifyFormatTime, (totalTime) =>
             {
                 Execute(NotifyFormatTime, total => total(totalTime));
              });
            _chat.On<string,int>(ClientEvents.NoticeScanByte, (byteString,index) =>
            {
                Execute(NoticeScanByte, t => t(byteString,index));
            });
            _chat.On<bool, TcpClientViewModel>(ClientEvents.NoticeTcpConnect, (b, model) =>
            {
                Execute(NoticeTcpConnect, t => t(b, model));
            });
            _chat.On<TcpClientViewModel>(ClientEvents.NoticeTcpData, (model) =>
            {
                Execute(NoticeTcpData, t => t(model));
            });        
        }

    private async void OnDisconnected()
		{
			await TaskAsyncHelper.Delay(TimeSpan.FromSeconds(1));

			try
			{
				await _connection.Start(_transportFactory());

				// Join JabbR
				// await _chat.Invoke("Join", false);
			}
			catch (Exception ex)
			{
				//_connection.Trace(TraceLevels.Events, ex.Message);
				LogHelper.ErrorLog(ex);
			}
		}

		private void Execute<T>(T handlers, Action<T> action) where T : class
		{
			Task.Factory.StartNew(() =>
			{
				if (handlers != null)
				{
					try
					{
						action(handlers);
					}
					catch (Exception ex)
					{
                        //_connection.Trace(TraceLevels.Events, ex.Message);
                        LogHelper.ErrorLog(ex);
					}
				}
			});
		}

		private Task SendCommand(string command, params object[] args)
		{
			return _chat.Invoke("Send", String.Format("/" + command, args), "");
		}

		public Task GetUserInfo()
		{
			return EmptyTask;
		}
		 
		public Task EmptyTask
		{
			get
			{
				return Task.Delay(0);
			}
		}

        public bool IsConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task GetAllUsers()
		{
			if (_connection.State == ConnectionState.Connected)
			{
				await _chat.Invoke("GetAllUsers");
			}

		}
		public async Task<List<DirectoryInfoModel>> GetFileList(string path = "")
		{
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    var items = await _chat.Invoke<List<DirectoryInfoModel>>("GetFileList", path);
                    return items;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);               
            } 
			return null;
		}
        public async  Task<DriveInfo[]> GetDrives()
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    var items = await _chat.Invoke<DriveInfo[]>("GetDrives");
                    return items;
                }
            }
            catch (Exception ex)
            { 
            }
           
            return null;
        }
         
        public async Task<bool> SendMessage(object message)
		{
			try
			{
				if (_connection.State == ConnectionState.Connected)
				{
					return await _chat.Invoke<bool>("SendMessage", message);
				}
			}

			catch (Exception ex)
			{
				LogHelper.ErrorLog(ex);
			}

			return false;
		}

        public async Task<UInt32> InitializerDevice()
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<UInt32>("InitLoad");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            } 
            return 1000;
        }

		public async Task<bool> DeviceOpen(int iSelectedIndex)
		{
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<bool>("DeviceOpen", iSelectedIndex);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            } 
			return false;
		}

		public async Task<bool> DeviceClose(int iSelectedIndex)
		{
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<bool>("DeviceClose", iSelectedIndex);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }			
			return false;
		}

		public async Task<string> InitDeviceInfo(int iSelectedIndex)
		{
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<string>("InitDeviceInfo", iSelectedIndex);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }			
			return string.Empty; 
		}

        public async Task<bool> FormatDrive(string driveName)
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<bool>("FormatDrive", driveName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);                
            }           
            return false;
        }

		public async Task CopyFileEx(string sourceFullPath, string targetFullPath)
		{
			if (_connection.State == ConnectionState.Connected)
			{
				await _chat.Invoke("CopyFileEx", sourceFullPath, targetFullPath);
			}
		}

        public async Task<string> OnReadDma(string driveName, int dataSize, int deviceIndex)
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<string>("OnReadDma", driveName, dataSize, deviceIndex);
                }
            }
            catch (Exception ex)
            { 
                LogHelper.ErrorLog(ex);
            }          
            return "内存错误";
        }
         
        public async Task OpenOrCloseChannel(DeviceChannelModel model)
        {
            if (_connection.State == ConnectionState.Connected)
            {
                await _chat.Invoke("OpenOrCloseChannel", model);
            }
        }

        public async Task CloseDma()
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    await _chat.Invoke("CloseDma");
                }
            }
            catch  
            {

                
            }
            
        }

		/// <summary>
		/// 设备自检
		/// </summary>
		/// <returns></returns>
		public async Task<string> ScanDevice(int deviceIndex)
		{
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<string>("ScanDevice", deviceIndex);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }		
			return "orther";
		}

        public async Task CloseScanDevice()
        {
            if (_connection.State == ConnectionState.Connected)
            {
                  await _chat.Invoke("CloseScanDevice");
            }
          
        }

        public async Task ConnectTcpServer(string fileDir,string IP, int port, int index)
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    await _chat.Invoke("TcpConnect", fileDir, IP, port, index);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
         
        }

        public async Task CloseTcpServer(int index)
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    await _chat.Invoke("CloseTcpConnect", index);
                }
            }
            catch (Exception ex)
            { 
                LogHelper.ErrorLog(ex);
            } 
        }

        public async Task DeleteFile(string filePath)
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    await _chat.Invoke("DeleteFile", filePath);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
            
        }

        public async Task<List<TcpClientViewModel>> GetAllTcpClients()
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                  return  await _chat.Invoke<List<TcpClientViewModel>>("GetTcpModels");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
            return null;
        }

        public async Task<bool> GetNetWork()
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<bool>("GetNetWork");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
            return false;
        }

        public async Task<DriveInfoModel> GetSingleDrive(string driveName = "")
        {
            try
            {
                if (_connection.State == ConnectionState.Connected)
                {
                    return await _chat.Invoke<DriveInfoModel>("GetSingleDrive",driveName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
            return null;
        }
    }
}
