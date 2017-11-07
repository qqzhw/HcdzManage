using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hcdz.ModulePcie.ViewModels
{
    public class FileDownloadViewModel
    {
        private readonly IUnityContainer _container; 
        private readonly IServiceLocator _serviceLocator;
        public FileDownloadViewModel(IUnityContainer  container, IServiceLocator  serviceLocator,string fileName)
        {
            _container = container;
            _serviceLocator = serviceLocator;
            CloseWindow= new    DelegateCommand<object>(OnCloseWindow);
        }

        private void OnCloseWindow(object obj)
        {
            
        }

        public ICommand CloseWindow { get; private set; }
    }
}
