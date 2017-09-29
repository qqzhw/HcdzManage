using Hcdz.ModulePcie.Models;
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
		event Action<string, string, string> AddMessageContent;  
		event Action Disconnected;
        event Action<bool> Connected;
        event Action<ClientMessage> OnGetMessage;
	   
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

        Task<DWORD> InitializerDevice();
    }
}
