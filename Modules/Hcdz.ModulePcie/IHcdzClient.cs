using Hcdz.ModulePcie.Models;
using Hcdz.ModulePcie.ViewModels;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWORD = System.UInt32;
namespace Hcdz.ModulePcie
{
	public interface IHcdzClient
	{
		event Action<string> MessageReceived;
		event Action<IEnumerable<string>> LoggedOut; 
	 
		event Action Disconnected;
        event Action<bool> Connected;
		event Action<string, string, long, long> ProgressChanged;
        event Action<long> NotifyTotal;
        event Action<string,int> NoticeScanByte;
        event Action<bool, TcpClientViewModel> NoticeTcpConnect;
        event Action<TcpClientViewModel> NoticeTcpData;
        
        event Action<int> NotifyFormatTime;
        bool IsConnected { get; set; }
		string SourceUrl { get; }
		bool AutoReconnect { get; set; } 
		HubConnection Connection { get; } 
		Task Connect(string userName="");
		Task GetOnlineUsers(); 
		Task LogOut();
		Task Send(ClientMessage message);  
		Task SetFlag(string countryCode);
		Task SetNote(string noteText); 
	 
		Task<bool> CheckStatus();
		Task SetTyping(string roomName);

		Task<List<DirectoryInfoModel>> GetFileList(string path);
        Task<DriveInfo[]> GetDrives();
        Task<List<TcpClientViewModel>> GetAllTcpClients();
        Task<DWORD> InitializerDevice();

		Task<bool> DeviceOpen(int iSelectedIndex); 
		Task<bool> DeviceClose(int iSelectedIndex);
		Task<string> InitDeviceInfo(int iSelectedIndex);
        Task ConnectTcpServer(string fileDir,string IP,int port,int index);
        Task CloseTcpServer(int index);
        Task<bool> FormatDrive(string driveName);
		Task CopyFileEx(string sourceFullPath, string targetFullPath);

        Task<string> OnReadDma(string driveName,int dataSize,int deviceIndex);
        Task CloseDma();
        Task OpenOrCloseChannel(DeviceChannelModel model);
		Task<bool> ScanDevice(int index);
        Task CloseScanDevice();
        Task DeleteFile(string filePath);
        Task<bool>  GetNetWork();
    }
}
