 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes; 
using Hcdz.ModulePcie.Models;
using Pvirtech.TcpSocket.Scs.Communication.Messages;
using Pvirtech.TcpSocket.Scs.Client;
using Pvirtech.TcpSocket.Scs.Communication.EndPoints.Tcp;

namespace Hcdz.ModulePcie.Views
{
    /// <summary>
    /// SettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsView : UserControl
    {
		private IScsClient client;
        public SettingsView()
        {
            InitializeComponent();
        }

		 
		private void ConnectClick(object sender, RoutedEventArgs e)
		{
			string ip = txtIp.Text.Trim(); 
			if (string.IsNullOrWhiteSpace(ip)||string.IsNullOrWhiteSpace(txtPort.Text.Trim()))
			{
				MessageBox.Show("请填写IP及端口号!");
				return;
			}
			int port = 0;
			if (!int.TryParse(txtPort.Text.Trim(),out port))
			{
				MessageBox.Show("端口号必须为数字!");
				return;
			}
			try
			{
				//Create a client object to connect a server on 127.0.0.1 (local) IP and listens 10085 TCP port
				client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(ip, port));
				 // client.WireProtocol = new CustomWireProtocol(); //Set custom wire protocol
				//Register to MessageReceived event to receive messages from server.
				client.MessageReceived += Client_MessageReceived;
				client.Connected += Client_Connected;
				client.Disconnected += Client_Disconnected;
				client.Connect(); //Connect to the server 
			  
				var messageText = "连接服务端成功!"; //Get a message from user 
				//Send message to the server
				client.SendMessage(new ScsTextMessage(messageText,"q1"));  
				 
				//client.Disconnect(); //Close connection to server
			}
			catch (Exception)
			{
				client.Dispose();
				MessageBox.Show("连接异常!");
			}
			
		}

		private void Client_Connected(object sender, EventArgs e)
		{
			MessageBox.Show("连接成功!");
		}

		private void Client_Disconnected(object sender, EventArgs e)
		{
			MessageBox.Show("连接已断开!");
		}

		private void Client_MessageReceived(object sender, MessageEventArgs e)
		{
			var message = e.Message as ScsTextMessage;
			if (message == null)
			{
				return;
			}
			this.Dispatcher.Invoke(() =>
			{
				//Console.WriteLine("Server sent a message: " + message.Text);
				txtMsg.Text += message.Text + "\n";
			});
		}

		private void CloseClick(object sender, RoutedEventArgs e)
		{
			if (client!=null)
			{
				client.Disconnect();
				client.Dispose();
			}
		}
	}
}
