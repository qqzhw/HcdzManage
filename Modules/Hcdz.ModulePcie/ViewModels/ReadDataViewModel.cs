using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Hcdz.ModulePcie.ViewModels
{
    
    public class ReadDataViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IServiceLocator _serviceLocator;
        private DispatcherTimer dispatcherTimer;
        private long readtotal = 0;
        private long totalSize = 0;
        public ReadDataViewModel(IUnityContainer container, IServiceLocator serviceLocator, string fileName)
        {
            _container = container;
            _serviceLocator = serviceLocator;
            CloseWindow = new DelegateCommand<object>(OnCloseWindow);
            FileName = fileName;
            _progresstext = "正在解析并存储通道数据...";
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background);
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            Init();

        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            ProgressValue =(int) (readtotal / totalSize)*100;
        }

        private void Init()
        {
            var index = FileName.LastIndexOf("\\");
            var name = FileName.Substring(index + 1, FileName.Length - index - 1);
            if (!Properties.Settings.Default.LocalPath.EndsWith("\\"))
            {
                Properties.Settings.Default.LocalPath += "\\";
            }
            var saveFilePath = Properties.Settings.Default.LocalPath + name;
            var dir1 = CreateDir("Bar1") + name;
             var dir2 =CreateDir("Bar2")+name;
            var dir3 = CreateDir("Bar3") + name;
            var dir4 = CreateDir("Bar4") + name;
            var dir5 = CreateDir("Bar5") + name;
            var dir6 = CreateDir("Bar6") + name; 
          
            Dictionary<int, FileStream> dicFiles = new Dictionary<int, FileStream>();
            dicFiles.Add(1,new FileStream(dir1,FileMode.Append,FileAccess.Write));
            dicFiles.Add(2, new FileStream(dir2, FileMode.Append, FileAccess.Write));
            dicFiles.Add(3, new FileStream(dir3, FileMode.Append, FileAccess.Write));
            dicFiles.Add(4, new FileStream(dir4 + name, FileMode.Append, FileAccess.Write));
            dicFiles.Add(5, new FileStream(dir5, FileMode.Append, FileAccess.Write));
            dicFiles.Add(6, new FileStream(dir6, FileMode.Append, FileAccess.Write));
            using (FileStream fsReader = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                dispatcherTimer.Start();
                totalSize = fsReader.Length;
                byte[] bytes = new byte[16];//4kB是合适的；
                int readNum;
                while ((readNum = fsReader.Read(bytes, 0, bytes.Length)) != 0)//小于说明读完了
                {
                    //   fsWriter.Write(bytes, 0, readNum);
                    WriteFile(bytes, dicFiles);
                    readtotal += 16;
                }
                foreach (var item in dicFiles)
                {
                    item.Value.Flush();
                    item.Value.Close();
                    item.Value.Dispose();
                }              
            }
        }
        private string CreateDir(string directory)
        {
            var dir = Properties.Settings.Default.LocalPath + string.Format("\\{0}\\", directory);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
        private void WriteFile(byte[] result, Dictionary<int, FileStream> dicts)
        {
            var channelNo = result[8];
            var item = dicts.Keys.FirstOrDefault(o=>o==channelNo);
            if (item==0)
                return;
            byte[] trueValue = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                trueValue[i] = result[i];
            }

            // result[8] = 0;
            // result[15] = 0;
            //var bt = result.Take(8).ToArray();
            //   item.FileByte.AddRange(bt);
            dicts[item].Write(trueValue, 0, 8);
            dicts[item].Flush();            
        }
        //private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    ProgressText = "下载完毕！";

        //}

        //private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        //{
        //    ProgressText = string.Format("正在下载...{0}%", e.ProgressPercentage.ToString());
        //    ProgressValue = e.ProgressPercentage;
        //}

        private void OnCloseWindow(object obj)
        {
            dispatcherTimer.Stop();
            var window = obj as Window;
            window.Close();
        }
        public string FileName { get; set; }
        public string FullName { get; set; }
        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }
        private string _progresstext;
        public string ProgressText
        {
            get { return _progresstext; }
            set { SetProperty(ref _progresstext, value); }
        }
        private bool _progressShow = false;
        public bool ProgressShow
        {
            get { return _progressShow; }
            set { SetProperty(ref _progressShow, value); }
        }
        public ICommand CloseWindow { get; private set; }
    }
}
