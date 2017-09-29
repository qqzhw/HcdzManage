using Hcdz.ModulePcie.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie
{
	public interface IHcdzClient
	{
		event Action<ClientMessage, string> MessageReceived;
		event Action<IEnumerable<string>> LoggedOut; 
		event Action<string, string, string> AddMessageContent;  
		event Action Disconnected; 
		event Action<ClientMessage> OnGetMessage;
	   
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
		  
		Task SendSms(string msgId, string content, string telPhones);
	}
}
