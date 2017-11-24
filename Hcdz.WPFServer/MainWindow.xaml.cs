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
using Pvirtech.Framework.Common;
using Microsoft.Win32;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR;

namespace Hcdz.WPFServer
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{

		public IDisposable SignalR { get; set; }
		protected string ServerURI ;
        private   DispatcherTimer dispatcherTimer;
        public event Action OnPush;
        public static MainWindow CurrentWindow { get; set; }
        public MainWindow()
		{
			InitializeComponent();
			Init();
            this.Loaded += MainWindow_Loaded;
            CurrentWindow = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                dispatcherTimer.Tick += DispatcherTimer_Tick;
                dispatcherTimer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (OnPush!=null)
            {
                OnPush();
            }
           
        }

        private void Init()
        {
            ServerURI = Settings.Default.Server;
            txtLicence.Text = Settings.Default.License;
            txtServer.Text = Settings.Default.Server;
            chkReg.IsChecked = Settings.Default.IsAutoStart;
            chkService.IsChecked = Settings.Default.IsAutoConnect;

            InitStart();

        }

		private void ButtonStart_Click(object sender, RoutedEventArgs e)
		{ 
            InitStart();
		}
        /// <summary>  
        /// 在注册表中添加、删除开机自启动键值  
        /// </summary>  
        private   void SetAutoBootStart(bool isAutoBoot)
        {
            try
            {
                string execPath = Assembly.GetExecutingAssembly().Location; 
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                RegistryKey rk0 = rk.CreateSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (isAutoBoot)
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        rk0.SetValue("hcdzServer", execPath);
                        Settings.Default.IsAutoStart = true;
                    }
                    else
                    {
                        rk2.SetValue("hcdzServer", execPath);
                        Settings.Default.IsAutoStart = true;
                    }
                    //  Console.WriteLine(string.Format("[注册表操作]添加注册表键值：path = {0}, key = {1}, value = {2} 成功", rk2.Name, "TuniuAutoboot", execPath));
                }
                else
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        rk0.DeleteValue("hcdzServer", false);
                        Settings.Default.IsAutoStart = false;
                    }
                    else
                    {
                        rk2.DeleteValue("hcdzServer", false);
                        Settings.Default.IsAutoStart = false;
                    }
                    //  Console.WriteLine(string.Format("[注册表操作]删除注册表键值：path = {0}, key = {1} 成功", rk2.Name, "TuniuAutoboot"));
                }
                Settings.Default.Save();
                rk2.Close();
                rk0.Close();
                rk.Close(); 
            }
            catch (Exception ex)
            {
               LogHelper.ErrorLog(ex,string.Format("[注册表操作]向注册表写开机启动信息失败\n  Exception: {0}", ex.Message));               
            }
        }
    
 
       private void InitStart()
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
			if (SignalR != null)
			{
				SignalR.Dispose();
			}
            base.OnClosing(e);
            Application.Current.Shutdown();
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

		private void ButtonSave_Click(object sender, RoutedEventArgs e)
		{
			Settings.Default.License = txtLicence.Text.Trim();
			Settings.Default.Server = txtServer.Text.Trim();
			Settings.Default.Save();
		}

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (chkReg.IsChecked==true)
            {
                SetAutoBootStart(true);
            }
            else
            {
                SetAutoBootStart(false);
            }
        }

        private void ServiceChecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsAutoConnect = chkService.IsChecked.Value; 
            Settings.Default.Save();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        
    }
}
