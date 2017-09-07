using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Logging;
using Prism.Modularity;
using Pvirtech.Framework.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pvirtech.Framework
{
	public partial class RoleBasedModuleInitializer: IModuleInitializer
	{
		private readonly IServiceLocator serviceLocator;
		private readonly ILoggerFacade loggerFacade;
		private readonly IEventAggregator eventAggregator;

		public RoleBasedModuleInitializer(IServiceLocator serviceLocator, ILoggerFacade loggerFacade, IEventAggregator eventAggregator)
		{
			this.serviceLocator = serviceLocator ?? throw new ArgumentNullException("serviceLocator");
			this.loggerFacade = loggerFacade ?? throw new ArgumentNullException("loggerFacade");
			this.eventAggregator = eventAggregator ?? throw new ArgumentNullException("eventAggregator");
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catches Exception to handle any exception thrown during the initialization process with the HandleModuleInitializationError method.")]
		public void Initialize(ModuleInfo moduleInfo)
		{
			if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");

			IModule moduleInstance = null;
			try
			{
				//if (ModuleIsInUserRole(moduleInfo))
				{
					//moduleInstance = this.CreateModule(moduleInfo);
					//if (moduleInstance != null)
					//	moduleInstance.Initialize();
				}
			}
			catch (Exception ex)
			{
				this.HandleModuleInitializationError(
					moduleInfo,
					moduleInstance != null ? moduleInstance.GetType().Assembly.FullName : null,
					ex);
			}
		}
		private bool ModuleIsInUserRole(ModuleInfo moduleInfo)
		{
			bool isInRole = false;

			var roles = GetModuleRoles(moduleInfo);
			if (roles == null) return true;
			foreach (var role in roles)
			{ 
				if (WindowsPrincipal.Current.IsInRole(role))
				{
					isInRole = true;
					GetModuleInfoDetail(moduleInfo);
					break;
				}
			}

			return isInRole;
		}
		private void GetModuleInfoDetail(ModuleInfo moduleInfo)
		{
			moduleInfo.InitializationMode = InitializationMode.OnDemand; 
			var type = Type.GetType(moduleInfo.ModuleType); 
			foreach (var attr in GetCustomAttribute<ModuleInfoAttribute>(type))
			{
				var info = new SystemInfo()
				{
					Id = attr.Id,
					Title = attr.Title,
					InitMode = attr.InitMode,
					IsDefaultShow = attr.IsDefaultShow,
					ModuleInfo = moduleInfo
				};				
				eventAggregator.GetEvent<MessageSentEvent<SystemInfo>>().Publish(info);
			}
			//foreach (var att in attr.GetInfoAttribute(type))
			//{
			//	return att.AsEnumerable();
			//}
			
		}
		private IEnumerable<string> GetModuleRoles(ModuleInfo moduleInfo)
		{
			var type = Type.GetType(moduleInfo.ModuleType);

			foreach (var attr in GetCustomAttribute<RolesAttribute>(type))
			{
				return attr.Roles.AsEnumerable();
			}

			return null;
		}

		private IEnumerable<T> GetCustomAttribute<T>(Type type)
		{
			return type.GetCustomAttributes(typeof(T),true).OfType<T>();
		}
	 
		public virtual void HandleModuleInitializationError(ModuleInfo moduleInfo, string assemblyName, Exception exception)
		{
			if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");
			if (exception == null) throw new ArgumentNullException("exception");

			Exception moduleException;

			if (exception is ModuleInitializeException)
			{
				moduleException = exception;
			}
			else
			{
				if (!string.IsNullOrEmpty(assemblyName))
				{
					moduleException = new ModuleInitializeException(moduleInfo.ModuleName, assemblyName, exception.Message, exception);
				}
				else
				{
					moduleException = new ModuleInitializeException(moduleInfo.ModuleName, exception.Message, exception);
				}
			}

			this.loggerFacade.Log(moduleException.ToString(), Category.Exception, Priority.High);

			throw moduleException;
		}
		protected virtual IModule CreateModule(ModuleInfo moduleInfo)
		{
			if (moduleInfo == null)
				throw new ArgumentNullException(nameof(moduleInfo));

			return CreateModule(moduleInfo.ModuleType);
		}
		protected virtual IModule CreateModule(string typeName)
		{
			Type moduleType = Type.GetType(typeName);
			if (moduleType == null)
			{
				throw new ModuleInitializeException(string.Format(CultureInfo.CurrentCulture, "Unable to retrieve the module type {0} from the loaded assemblies.  You may need to specify a more fully-qualified type name.", typeName));
			}

			return (IModule)this.serviceLocator.GetInstance(moduleType);
		}
	}
}
