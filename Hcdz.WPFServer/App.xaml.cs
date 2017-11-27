using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Hcdz.WPFServer
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		private static Mutex SingleInstanceMutex = new Mutex(true, "{86A802DF-C96B-8769-BAA6-1BC527857BEB}");

        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            //判断当前登录用户是否为管理员  
            if (IsAdministrator())
            {
                //LogHelper.WriteLog("当前登录用户是管理员 ");
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
            else
            { 
                //创建启动对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                //设置运行文件  
                startInfo.FileName = Assembly.GetExecutingAssembly().Location; 
               
                //设置启动动作,确保以管理员身份运行  
                startInfo.Verb = "runas";
                //如果不是管理员，则启动UAC  
                 Process.Start(startInfo);                 
            }
        }
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private static bool SingleInstanceCheck()
		{

			if (!SingleInstanceMutex.WaitOne(TimeSpan.Zero, true))
			{
				Process thisProc = Process.GetCurrentProcess();
				Process process = Process.GetProcessesByName(thisProc.ProcessName).FirstOrDefault(delegate (Process p)
				{
					if (p.Id != thisProc.Id)
					{
						return true;
					}
					return false;
				});
				if (process != null)
				{
					IntPtr mainWindowHandle = process.MainWindowHandle;
					if (NativeMethods.IsIconic(mainWindowHandle))
					{
						NativeMethods.ShowWindow(mainWindowHandle, 9);
					}
					NativeMethods.SetForegroundWindow(mainWindowHandle);
				}
				Application.Current.Shutdown(1);
				return false;
			}
			return true;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			if (!SingleInstanceCheck())
			{
				return;
			}
			base.OnStartup(e);
			
			Initialize();
		}

		public void Initialize()
		{
			this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			 
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogHelper.ErrorLog((Exception)e.ExceptionObject);
		}

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			LogHelper.ErrorLog(e.Exception);
		}

	}
}
