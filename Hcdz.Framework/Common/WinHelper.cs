using Pvirtech.Metro.Controls;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pvirtech.Framework.Common
{
    public class WinHelper
    {


        private static System.Media.SoundPlayer Player;
        public static void PlayWav(string fileName)
        {
			Player = null ?? new System.Media.SoundPlayer();
			string wavPath = string.Format(AppDomain.CurrentDomain.BaseDirectory + "Wave\\{0}", fileName);
			try
			{
				Player.SoundLocation = wavPath;
				Player.Load();
				Player.PlayLooping();
			}
			catch (Exception ex)
			{
				LogHelper.ErrorLog(ex);
			}
		}
        public static void StopPlayWav()
        {
			if (Player == null)
				return;
			Player.Stop();
			Player.Dispose();
		}

        public static string GetHostIP()
        {
            System.Net.IPAddress[] localIP = System.Net.Dns.GetHostAddresses(GetHostName());
            foreach (System.Net.IPAddress one in localIP)
            {
                if (one.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return one.ToString();
            }
            return "0.0.0.0";
        }


        public static void ProcessStart(string filepath)
        {
            try
            {

                System.Diagnostics.Process.Start(filepath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public static string GetHostName()
        {
            return System.Net.Dns.GetHostName();
        }

        /// <summary>
        /// 消息提示框
        /// </summary>
        /// <param name="showTime">提示框展示时间 0就不自动关闭 若需自动关闭大于0  单位 秒</param>
        /// <returns></returns>
        private static Timer timer;
        public static void MessageWindow(string title, string text, int showTime = 0, bool hasOwner = false,bool isDialog=false)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MetroWindow mwin = new MetroWindow()
                {
                    //Topmost = true,
                    //Owner = MainWindow.CurrentWindow, 
                    ShowMinButton = false,
                    ShowMaxRestoreButton = false,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Title = title,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    MinWidth = 240,
                    Width = 240,
                    MinHeight = 150,
                    Height = GridLength.Auto.Value,
                    ResizeMode = ResizeMode.NoResize,
                    TitlebarHeight = 26,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(65, 177, 225)),
                    Icon = UtilsHelper.GetBitmapImage("Logo.ico"),
                };
                if (hasOwner)
                {
                    mwin.Owner = Application.Current.MainWindow;
                }
                mwin.Content = new TextBlock() { Width = 240, TextWrapping = TextWrapping.Wrap, Text = text, FontSize = 12, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left };
                mwin.Closed += (o, args) => mwin = null;
                mwin.Show();
                if ((showTime / 1000) > 0)
                {
                    timer = new Timer(showTime);
                    timer.AutoReset = false;
                    timer.Elapsed += new System.Timers.ElapsedEventHandler((s, e) => WinCloseTimer_Elapsed(s, e, mwin));
                    timer.Start();
                }
            }));
        }

        /// <summary>
        /// 消息框
        /// </summary>
        /// <returns></returns>
        public static MetroWindow ShowWindow(string title, string message, UIElement element = null)
        {

            var messageWindow = new MetroWindow()
               {
                   //Topmost = true,
                   //Owner = MainWindow.CurrentWindow,
                   ShowMinButton = false,
                   ShowMaxRestoreButton = false,
                   WindowStartupLocation = WindowStartupLocation.CenterScreen,
                   Title = title,
                   Width = 240,
                   ResizeMode = ResizeMode.NoResize,
                   Height = 80,
                   TitlebarHeight = 26,
                   BorderThickness = new Thickness(1),
                   BorderBrush = new SolidColorBrush(Color.FromRgb(65, 177, 225))
               };
            if (element == null)
            {
                messageWindow.Content = new TextBlock() { Text = message, FontSize = 12, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            }
            else
            {
                messageWindow.Content = element;
            }
            messageWindow.Closed += (o, args) => messageWindow = null;
            return messageWindow;
        }


        /// <summary>
        /// 关闭提示框
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="from"></param>
        private static void WinCloseTimer_Elapsed(object s, ElapsedEventArgs e, MetroWindow from)
        {
            try
            {
                if (from != null)
                {
                    from.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        from.Close();
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("弹出窗体出现异常：" + ex.Message);
            }
        }

    }
}
