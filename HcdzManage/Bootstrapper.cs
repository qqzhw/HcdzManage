using Prism.Modularity;
using Prism.Unity;
using Pvirtech.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using System.IO;
using HcdzManage.Views;
using HcdzManage.ViewModels;

namespace HcdzManage
{ 
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            // var ident = WindowsIdentity.GetCurrent();
            // var principal = new GenericPrincipal(ident, new string[] { "User" });
            //Thread.CurrentPrincipal = principal; 
            //  AppDomain.CurrentDomain.SetThreadPrincipal(principal);
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }
        protected override void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();
          	Container.RegisterType<MainWindowViewModel>(new ContainerControlledLifetimeManager());
        }
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
           // Container.RegisterType<IModuleInitializer, RoleBasedModuleInitializer>(new ContainerControlledLifetimeManager());
           
        }
        protected override IModuleCatalog CreateModuleCatalog()
        {
            DynamicDirectoryModuleCatalog catalog = new DynamicDirectoryModuleCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules"));
            return catalog;
        }
    }
}
