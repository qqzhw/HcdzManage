using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Hcdz.PcieLib;
using wdc_err = Jungo.wdapi_dotnet.WD_ERROR_CODES;
using DWORD = System.UInt32;
using WORD = System.UInt16;
using BYTE = System.Byte;
using BOOL = System.Boolean;
using UINT32 = System.UInt32;
using UINT64 = System.UInt64;
using WDC_DEVICE_HANDLE = System.IntPtr;
using WDC_ADDR_SIZE = System.UInt32;
using HANDLE = System.IntPtr;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;

namespace Hcdz.ModulePcie.ViewModels
{
    public class MainViewModel: BindableBase
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IUnityContainer _container;
		private readonly IRegionManager _regionManager;
		private readonly IServiceLocator _serviceLocator;
		private ConcurrentQueue<int> queue;
		private bool IsCompleted=false;
        private int index = 0;
        private int index1 = 0;
		public MainViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IServiceLocator serviceLocator)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_regionManager = regionManager; 
			_serviceLocator = serviceLocator;
			 queue = new ConcurrentQueue<int>();
            devicesItems = new ObservableCollection<PCIE_Device>();

             Initializer();
		}
        ConcurrentQueue<byte[]> queue1;
        private PCIE_DeviceList pciDevList;
        private Log log;
        private ObservableCollection<PCIE_Device> devicesItems;
        public ObservableCollection<PCIE_Device> DevicesItems
        {
            get { return devicesItems; }
            set { SetProperty(ref devicesItems, value); }
        }
        private void Initializer()
		{
           
            pciDevList = PCIE_DeviceList.TheDeviceList();
            queue1 = new ConcurrentQueue<byte[]>();
			Thread readThread = new Thread(new ThreadStart(ReadDMA));
			readThread.IsBackground = true;
			readThread.Start();
			Thread writeThread = new Thread(new ThreadStart(WriteDMA));
			writeThread.IsBackground = true;
			writeThread.Start();
			//DWORD dwStatus = pciDevList.Init();
			//if (dwStatus == (DWORD)wdc_err.WD_STATUS_SUCCESS)
			//{
			//   RadDesktopAlert alert = new RadDesktopAlert();
			//    alert.Content = "加载设备失败!";
			//    RadWindow.Alert(new DialogParameters
			//    {
			//        Content = "加载设备失败！", 
			//        DefaultPromptResultValue = "default name",
			//        Theme = new Windows8TouchTheme(),
			//          Header="提示",
			//          TopOffset=30,

			//    });
			//    return;
			//}

			//foreach (PCIE_Device dev in pciDevList)
			//    devicesItems.Add(dev); 

		}
		private List<int> s1 = new List<int>();
		private List<int> s2 = new List<int>();
		private void WriteDMA()
		{
			for (int i = 0; i < 10000; i++)
			{
				queue.Enqueue(i);
			}
			
		}
        Task task;
        private void ReadDMA()
		{
			while (!IsCompleted)
			{
				if (queue.IsEmpty)
				{
					//IsCompleted = true;
                    //sw.Close(); 
                    //sw.Dispose();
                }
				else
				{
					int result;
					if (queue.TryDequeue(out result))
					{
						if (result%2==0)
						{
                            // ThreadPool.QueueUserWorkItem(WriteBar);
                            //  WriteBar(result); 
                            index1++;
                           WriteBar(result);
                        }
						else
						{
							//Task.Run(() => {
							//	using (var sw = new StreamWriter("D:\\ss1.txt",true))
							//	{
							//		sw.WriteLine(result);
							//	}
							//});
						}
					}
				}
			}
		}
        FileStream sw = new FileStream("E:\\ss.txt", FileMode.Append, FileAccess.Write);
        private void WriteBar(object state)
        {  
            byte[] s = new byte[1];
            s[0] = Convert.ToByte(1);
            sw.Position = sw.Length;
            sw.Write(s, 0, 1);
            index++;
            if (queue.IsEmpty)
            { 
                var s1 = index;
            }
        }
	}
}
