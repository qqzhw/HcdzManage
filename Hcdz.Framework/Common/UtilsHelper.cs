
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pvirtech.Framework.Common
{
    /// <summary>
    /// 常用工具函数
    /// </summary>
    public class UtilsHelper
    {
      //  public static IScsClient ScsClient = ScsClientFactory.CreateClient(new ScsTcpEndPoint("127.0.0.1", 32000));
       
        private static char[] constant =
       {
        '0','1','2','3','4','5','6','7','8','9'
       };
        /// <summary>
        /// 获取一个随机数
        /// </summary>
        /// <param name="Length">随机数位数</param>
        /// <returns></returns>
        public static string GenerateRandomNumber(int Length)
        {
            StringBuilder newRandom = new StringBuilder(10);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(10)]);
            }
            return newRandom.ToString();
        }

        public static void LogTexts(string Type)
        {
            LogHelper.WriteLog(string.Format("【" + Type + "】" + "{0}" + "\r\n", DateTime.Now.ToString("HH:mm:ss.fff")));
            //System.IO.File.AppendAllText(@"D:\LogTexts", string.Format("【" + Type + "】" + "{0}" + "\r\n", DateTime.Now.ToString("HH:mm:ss.fff")));
        }

        public static void LogTexts(string Type,string content)
        {
            LogHelper.WriteLog(string.Format("【" + Type + "】" + "{0}" + "【{1}】" + "\r\n", content, DateTime.Now.ToString("HH:mm:ss.fff")));
            //System.IO.File.AppendAllText(@"D:\LogTexts", string.Format("【" + Type + "】" + "{0}" + "\r\n", DateTime.Now.ToString("HH:mm:ss.fff")));
        }

        public static void CopyEntity(object e1, object e2)
        {
            if (e1 == null || e2 == null)
                return;
             Type te1 = e1.GetType();
            Type te2 = e2.GetType();

            foreach (PropertyInfo source in te2.GetProperties())
            {
                string pName = source.Name;
                PropertyInfo dest = te1.GetProperty(pName);
                bool canSet = dest != null && dest.CanWrite
                    && dest.PropertyType.IsAssignableFrom(source.PropertyType);
                if (canSet)
                {
                    dest.SetValue(e1, source.GetValue(e2, null), null);
                }
            }
        }


        public static void CopyEntityAlarmInfo(object e1, object e2)
        {
            if (e1 == null || e2 == null)
                return;
            Type te1 = e1.GetType();
            Type te2 = e2.GetType();

            bool isSameAlarm = te1.GetProperty("Jqlsh").GetValue(e1, null).ToString().
                Equals(te2.GetProperty("Jqlsh").GetValue(e2, null).ToString());

            foreach (PropertyInfo source in te2.GetProperties())
            {
                string pName = source.Name;
                PropertyInfo dest = te1.GetProperty(pName);
                bool canSet = dest != null && dest.CanWrite 
                    && dest.PropertyType.IsAssignableFrom(source.PropertyType);
                if (canSet)
                {
                    if (isSameAlarm && (source.GetValue(e2, null) == null || source.GetValue(e2, null).GetType().Name.Contains("DelegateCommand")))
                        continue;
                    dest.SetValue(e1, source.GetValue(e2, null), null);
                }
            }
        }

        public static List<T> Clone<T>(List<T> old)
        {
            List<T> dest = new List<T>();
            foreach (T one in old)
            {
                T destone = (T)Activator.CreateInstance(typeof(T));
                CopyEntity(destone, one);
                dest.Add(destone);
            }
            return dest;
        }

        public static Dictionary<string, T> CopyDict<T>(Dictionary<string, T> para)
        {
            Dictionary<string, T> ones = new Dictionary<string, T>();
            foreach (string key in para.Keys)
            {
                ones.Add(key, para[key]);
            }
            return ones;
        }
        public static BitmapImage GetBitmapImage(string imgPath)
        {
            string path = string.Format("pack://application:,,,/Pvirtech.Framework.Resources;component/{0}", imgPath);
            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.CacheOption = BitmapCacheOption.OnLoad;
            bImg.UriSource = new Uri(path);
            bImg.EndInit();
            bImg.Freeze();           
            return bImg;
        }
		 
		public static Uri GetImageUrl(string imgPath)
		{
			string path = string.Format("pack://application:,,,/Pvirtech.Framework.Resources;component/Images/{0}", imgPath);
			return new Uri(path, UriKind.RelativeOrAbsolute);
		}
		public static string GetImageRelativePath(string imgPath)
		{
			string path = string.Format("pack://application:,,,/Pvirtech.Framework.Resources;component/{0}", imgPath);
			return path;
		}
		/// <summary>
		/// 分割字符串
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string[] ByStringSplit(string str)
        {
            return str.Split(SplitChars);
        }

        public static char[] SplitChars
        {
            get{

                return new char[5] { ',', '+', '-', '=', ' ' };
            }
            
        }
        /// <summary>
        /// 根据毫秒转换时间格式
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static string formatLongToTimeStr(long l)
        {
            int hour = 0;
            int minute = 0;
            int second = 0;
            //second = Int32.Parse(l.ToString()) / 1000;
            second = Int32.Parse(l.ToString());
            if (second > 60)
            {
                minute = second / 60;
                second = second % 60;
            }
            if (minute > 60)
            {
                hour = minute / 60;
                minute = minute % 60;
            }

            #region  组装显示数据

            string backValue = string.Empty;
            if (hour < 10)
                backValue = "0" + hour.ToString();
            else
                backValue = hour.ToString();
            if (minute < 10)
                backValue += ":0" + minute.ToString();
            else
                backValue += ":" + minute.ToString();
            if (second < 10)
                backValue += ":0" + second.ToString();
            else
                backValue += ":" + second.ToString();

            #endregion

            return backValue;
        } 
       
        public static string GetMapFeatureUrl(string mapServer,int layerId)
        {
            string url = string.Format("{0}/arcgis/rest/services/Feature/Feature/MapServer/{1}", mapServer, layerId);
            return url;
        }

        public static Action<T> GetCmd<T>(Type Source,string MethodName)
        {
            Type at = typeof(Action<T>);
            MethodInfo mi = Source.GetMethod(MethodName);
            Action<T> one = null;
            if (mi != null)
                one = (Action<T>)Delegate.CreateDelegate(at, Source.GetMethod(MethodName));
            return one;
        }

        public static T FindFirstVisualChild<T>(DependencyObject obj, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && child.GetValue(FrameworkElement.NameProperty).ToString() == childName)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindFirstVisualChild<T>(child, childName);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        public static Process IsExistProcess(string processName)
        {
            using (Process process = new Process())
            {
                var name = processName.Substring(0, processName.Length - 4);
                var findprocess = Process.GetProcessesByName(name).FirstOrDefault();
                if (findprocess != null)
                {
                    return findprocess;
                }
                return null;
            }
        }

        /// <summary>  
        /// 图片转二进制  
        /// </summary>  
        /// <param name="imagepath">图片地址</param>  
        /// <returns>二进制</returns>  
        public static byte[] GetPictureData(string imagepath)
        {
            //根据图片文件的路径使用文件流打开，并保存为byte[]   
            FileStream fs = new FileStream(imagepath, FileMode.Open);//可以是其他重载方法   
            byte[] byData = new byte[fs.Length];
            fs.Read(byData, 0, byData.Length);
            fs.Close();
            return byData;
        }

        /// <summary>
        /// 用二进制方式读取文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>读取到的数据</returns>
        public static Byte[] ReadFileByByte(string fileName)
        {
            //以独占方式打开一个文件
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
            //BinaryWriter bw = new BinaryWriter(fs);//以二进制方式写文件
            //创建一个Byte用来存放读取到的文件内容
            Byte[] data = new Byte[fs.Length];
            //定义变量存储初始读取位置
            int offset = 0;
            //定义变量存储当前数据剩余未读的长度
            int remaining = data.Length / 2;
            try
            {
                while (remaining > 0)
                {
                    int read = fs.Read(data, offset, remaining);
                    if (read <= 0)
                        throw new EndOfStreamException("文件读取到" + read.ToString() + "失败！");
                    // 减少剩余的字节数
                    remaining -= read;
                    // 增加偏移量
                    offset += read;
                }
            }
            catch (Exception e)
            {
                data = null;
                MessageBox.Show(e.Message);
            }
            fs.Flush();
            fs.Dispose();
            return data;
        }

        /// <summary>
        /// 数组组合
        /// </summary>
        /// <param name="Array1"></param>
        /// <param name="Array2"></param>
        /// <returns></returns>
        public static byte[] ComposeArrays(byte[] Array1, byte[] Array2)
        {
            byte[] Temp = new byte[Array1.Length + Array2.Length];
            Array1.CopyTo(Temp, 0);
            Array2.CopyTo(Temp, Array1.Length);
            return Temp;
        }

        public static void SetForegroundWindow(Process process)
        {
            IntPtr mainWindowHandle = process.MainWindowHandle;
            if (NativeMethods.IsIconic(mainWindowHandle))
            {
                NativeMethods.ShowWindow(mainWindowHandle, 9);
            }
            NativeMethods.SetForegroundWindow(mainWindowHandle);
        }

        /// <summary>
        /// 启动宏信程序
        /// </summary>
        public static bool StartXHProgram()
        {
            using (Process process = new Process())
            {
                process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                process.StartInfo.FileName = "clientLdt.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    var name = process.StartInfo.FileName.Substring(0, process.StartInfo.FileName.Length - 4);
                    var findprocess = Process.GetProcessesByName(name).FirstOrDefault();
                    if (findprocess != null)
                    { 
                        IntPtr mainWindowHandle = findprocess.MainWindowHandle;
                        if (NativeMethods.IsIconic(mainWindowHandle))
                        {
                            NativeMethods.ShowWindow(mainWindowHandle, 9);
                        }
                        NativeMethods.SetForegroundWindow(mainWindowHandle);
                        return true;
                    }
                    else
                    {
                        //正常执行
                        process.Start(); 
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }           
        }
      
        public static void CallPhone(object message)
        {
            try
            {
              //  var phoneEvent=Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<CallPhoneEvent>();
            //    if (ocxHelper==null)
            //    {
            //        ocxHelper =new ClientOCXHelper();
            //        //ScsClient.Connect(); //Connect to the server
            //        //string msg = string.Format("callmobile:{0};", message);
            //        //ScsClient.SendMessage(new ScsTextMessage(msg)); 
            //        ocxHelper.OnInit += (s, e) => helper_OnInit(s, e, message);
            //        ocxHelper.OnOcxCallOver += ocxHelper_OnCallOver;
            //        ocxHelper.OnOcxCallReceived += ocxHelper_OnCallReceived;
            //        string user = Application.Current.Resources["XhUser"].ToString();
            //        string password = Application.Current.Resources["Xhpassword"].ToString();
            //        ocxHelper.Init(user, password);
            //    }
            //    else
            //    {
            //        string phone = message.ToString();//"313808099512";
            //        if (message.ToString().Contains("-"))
            //        {
            //            phone = message.ToString().Replace("-", "");
            //        }
            //        else if (message.ToString().Contains("AA"))
            //        {
            //            phone = message.ToString().Replace("AA","");
            //        }
            //        else
            //        {
            //            phone = "3" + phone;
            //        }
            //        ocxHelper.CallMobile(phone, "1");
            //    }                
            
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
        
        }

    
         
        //static void helper_OnInit(object sender, InitOcxEventArgs e, object message)
        //{
        //    if (e.ResultCode == 1)
        //    {
        //        //ocxHelper.CallMobile("13808099512", "1");
        //        string phone = message.ToString();// "313808099512";
        //        if (message.ToString().Contains("-"))
        //        {
        //            phone = message.ToString().Replace("-", "");
        //        }
        //        else if (message.ToString().Contains("AA"))
        //        {
        //            phone = message.ToString().Replace("AA", "");
        //        }
        //        else
        //        {
        //            phone = "3" + phone;
        //        }
        //        ocxHelper.CallMobile(phone, "1");
        //    }
        //    else
        //        MessageBox.Show("初始化连接登录失败", "提示");
        //}

        public static string DateMinues(DateTime source, DateTime dest)
        {
            string s = "";
            TimeSpan t = dest - source;

            if (t.Days != 0)
                s = t.Days.ToString() + "天";

            if (t.Hours != 0)
                s += t.Hours.ToString() + "小时" ;

            if (t.Minutes != 0)
                s += t.Minutes.ToString() + "分";

            if (t.Seconds != 0)
                s += t.Seconds.ToString() + "秒";

            return s;
        }

        /// <summary>
        /// 计算时间
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static string DateDistance(DateTime source, DateTime dest)
        {
            string backString = "00:00:00";
            if (source != null &&　dest !=null)
            {
                TimeSpan span = source - dest;
                backString = ((int)span.TotalHours).ToString() + ":" + span.Minutes.ToString() + ":" + span.Seconds.ToString();
            }
            return backString;
        }

        public static void CopyDirectory(string srcDir, string tgtDir)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i].FullName, target.FullName + @"\" + files[i].Name, true);
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            for (int j = 0; j < dirs.Length; j++)
            {
                CopyDirectory(dirs[j].FullName, target.FullName + @"\" + dirs[j].Name);
            }
        }

    }
}
