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
		public MainViewModel(IUnityContainer container, IEventAggregator eventAggregator, IRegionManager regionManager, IServiceLocator serviceLocator)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_regionManager = regionManager; 
			_serviceLocator = serviceLocator;
			 queue = new ConcurrentQueue<int>();
			 Initializer();
		}

		private void Initializer()
		{
			Thread readThread = new Thread(new ThreadStart(ReadDMA));
			readThread.IsBackground = true;
			readThread.Start(); 
			Thread writeThread = new Thread(new ThreadStart(WriteDMA));
			writeThread.IsBackground = true;
			writeThread.Start();
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

		private void ReadDMA()
		{
			while (!IsCompleted)
			{
				if (queue.IsEmpty)
				{
					IsCompleted = true;
				}
				else
				{
					int result;
					if (queue.TryDequeue(out result))
					{
						if (result%2==0)
						{
							ThreadPool.QueueUserWorkItem(WriteBar); 
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

		private void WriteBar(object state)
		{
			using (StreamWriter sw = new StreamWriter("D:\\ss.txt", true))
			{
				sw.WriteLine(1);
			}
		}
	}
}
