﻿using Hcdz.ModulePcie.ViewModels;
using Hcdz.ModulePcie.Views;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Pvirtech.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie
{
    //[Roles("User")]
    //[ModuleInfo(Id = "PCIEModule", Title = "PCIE采集", IsDefaultShow = true, InitMode = InitializationMode.OnDemand)]
    public class PCIEModule : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly IUnityContainer _unityContainer;
        public PCIEModule(IRegionManager regionManager, IUnityContainer unityContainer)
        {
            _regionManager = regionManager;
            _unityContainer = unityContainer;
			RegisterService();
		}

		private void RegisterService()
		{
			_unityContainer.RegisterType<IHcdzClient,HcdzClient>(new ContainerControlledLifetimeManager());
			_unityContainer.RegisterType<SettingsViewModel>(new ContainerControlledLifetimeManager());
		}

		public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("MainRegion", typeof(MainView));
           //  _unityContainer.RegisterTypeForNavigation<MainView>("MainView");
            _unityContainer.RegisterTypeForNavigation<HardDriveView>("HardDriveView");
            _unityContainer.RegisterTypeForNavigation<FilesView>("FilesView");
            _unityContainer.RegisterTypeForNavigation<SettingsView>("SettingsView");

        }
    }
}
