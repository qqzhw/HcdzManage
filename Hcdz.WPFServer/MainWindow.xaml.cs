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
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System.Reflection;
using System.ComponentModel;
using Hcdz.WPFServer.Properties;

namespace Hcdz.WPFServer
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public IDisposable SignalR { get; set; }
		protected string ServerURI ;

		public MainWindow()
		{
			InitializeComponent();
            ServerURI = Settings.Default.Server;
        }

		private void ButtonStart_Click(object sender, RoutedEventArgs e)
		{
			WriteToConsole("Starting server...");
			ButtonStart.IsEnabled = false;
			Task.Run(() => StartServer());
		}

		private void ButtonStop_Click(object sender, RoutedEventArgs e)
		{
			SignalR.Dispose();
			Close();
		}
        protected override void OnClosing(CancelEventArgs e)
        {
            SignalR.Dispose();
            base.OnClosing(e);
        }
        /// <summary>
        /// Starts the server and checks for error thrown when another server is already 
        /// running. This method is called asynchronously from Button_Start.
        /// </summary>
        private void StartServer()
		{
			try
			{
				SignalR = WebApp.Start(ServerURI);
			}
			catch (TargetInvocationException ex)
			{
				WriteToConsole("A server is already running at " + ServerURI);
				this.Dispatcher.Invoke(() => ButtonStart.IsEnabled = true);
				return;
			}
			this.Dispatcher.Invoke(() => ButtonStop.IsEnabled = true);
			WriteToConsole("Server started at " + ServerURI);
		}
		///This method adds a line to the RichTextBoxConsole control, using Dispatcher.Invoke if used
		/// from a SignalR hub thread rather than the UI thread.
		public void WriteToConsole(String message)
		{
			if (!(RichTextBoxConsole.CheckAccess()))
			{
				this.Dispatcher.Invoke(() =>
					WriteToConsole(message)
				);
				return;
			}
			RichTextBoxConsole.AppendText(message + "\r");
		}
	}
}
