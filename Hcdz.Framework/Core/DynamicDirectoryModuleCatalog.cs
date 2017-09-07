using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Modularity;
using Pvirtech.Framework.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Pvirtech.Framework
{
	public class DynamicDirectoryModuleCatalog:ModuleCatalog
	{
		private SynchronizationContext _context;
        private IEventAggregator _eventAggregator;

        public string ModulePath { get; set; }
        private List<SystemInfo> _menuList;
        public List<SystemInfo> MenuList { get=>_menuList; set=>_menuList=value; }
		public DynamicDirectoryModuleCatalog(string modulePath)
		{
			_context = SynchronizationContext.Current;            
			ModulePath = modulePath;
			FileSystemWatcher fileWatcher = new FileSystemWatcher(ModulePath, "*.dll");
			fileWatcher.Created += FileWatcher_Created;
			fileWatcher.EnableRaisingEvents = true;
           _menuList = new List<SystemInfo>();
        }

		 /// <summary>
        /// Rasied when a new file is added to the ModulePath directory
        /// </summary>
        void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                LoadModuleCatalog(e.FullPath, true);
            }
        } 
		protected override void InnerLoad()
		{ 
			LoadModuleCatalog(ModulePath);
		}
		private void LoadModuleCatalog(string path, bool isFile=false)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new InvalidOperationException("Path cannot be null.");
			}
			if (isFile)
			{
				if (!File.Exists(path))
					throw new InvalidOperationException(string.Format("File {0} could not be found.", path));
			}
			else
			{
				if (!Directory.Exists(path))
					throw new InvalidOperationException(string.Format("Directory {0} could not be found.", path));
			}
			AppDomain childDomain= this.BuildChildDomain(AppDomain.CurrentDomain);

			try
			{
				List<string> loadedAssemblies = new List<string>();

				var assemblies = (
									 from Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
									 where !(assembly is System.Reflection.Emit.AssemblyBuilder)
										&& assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
										&& !String.IsNullOrEmpty(assembly.Location)
									 select assembly.Location
								 );

				loadedAssemblies.AddRange(assemblies);

				Type loaderType = typeof(InnerModuleInfoLoader);

				if (loaderType.Assembly != null)
				{
					var loader =
						(InnerModuleInfoLoader)
						childDomain.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName).Unwrap();
					loader.LoadAssemblies(loadedAssemblies);
					//得到所有模块信息
					ModuleInfo[] modules = loader.GetModuleInfos(path, isFile);
					this.Items.AddRange(modules);
					if (isFile)
					{
						LoadModules(modules);
					}
                     
                    foreach (var item in modules)
                    {
                       ModuleIsInUserRole(item);
                    }                    
                    _menuList = _menuList.OrderBy(o => o.DisplayOrder).ToList();
                    _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
                    _eventAggregator.GetEvent<MessageSentEvent<List<SystemInfo>>>().Publish(_menuList);
                }
			}
			finally
			{
				AppDomain.Unload(childDomain);
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
                    DisplayOrder=attr.DisplayOrder,
                    Title = attr.Title,
                    InitMode = attr.InitMode,
                    IsDefaultShow = attr.IsDefaultShow,
                    ModuleInfo = moduleInfo
                };
                //eventAggregator.GetEvent<MessageSentEvent<SystemInfo>>().Publish(info);
                 _menuList.Add(info);
            }
            //foreach (var att in attr.GetInfoAttribute(type))
            //{
            //	return att.AsEnumerable();
            //}

        }
        private IEnumerable<string> GetModuleRoles(ModuleInfo moduleInfo)
        {
            var type = Type.GetType(moduleInfo.ModuleType);
            if (type==null)
            {
                return null;
            }
            foreach (var attr in GetCustomAttribute<RolesAttribute>(type))
            {
                return attr.Roles.AsEnumerable();
            }

            return null;
        }

        private IEnumerable<T> GetCustomAttribute<T>(Type type)
        {
            return type.GetCustomAttributes(typeof(T), true).OfType<T>();
        }


        /// <summary>
        /// Uses the IModuleManager to load the modules into memory
        /// </summary>
        /// <param name="modules"></param>
        private void LoadModules(ModuleInfo[] modules)
		{
			if (_context == null)
				return;

			IModuleManager manager = ServiceLocator.Current.GetInstance<IModuleManager>();

			_context.Send(new SendOrPostCallback(delegate (object state)
			{
				foreach (var module in modules)
				{
					manager.LoadModule(module.ModuleName);
				}
			}), null);
		}
		protected virtual AppDomain BuildChildDomain(AppDomain parentDomain)
		{
			if (parentDomain == null)
				throw new ArgumentNullException(nameof(parentDomain));

			Evidence evidence = new Evidence(parentDomain.Evidence);
			AppDomainSetup setup = parentDomain.SetupInformation;
			return AppDomain.CreateDomain("DiscoveryRegion", evidence, setup);
		}

		private class InnerModuleInfoLoader : MarshalByRefObject
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
			internal ModuleInfo[] GetModuleInfos(string path,bool isFile = false)
			{
				Assembly moduleReflectionOnlyAssembly =
					AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().First(
						asm => asm.FullName == typeof(IModule).Assembly.FullName);
				Type IModuleType = moduleReflectionOnlyAssembly.GetType(typeof(IModule).FullName);

				FileSystemInfo info = null;
				if (isFile)
					info = new FileInfo(path);
				else
					info = new DirectoryInfo(path);
				 

				ResolveEventHandler resolveEventHandler =
					delegate (object sender, ResolveEventArgs args) { return OnReflectionOnlyResolve(args, info); };

				AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;
				 
				IEnumerable<ModuleInfo> modules = GetNotAllreadyLoadedModuleInfos(info, IModuleType);
				 AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
				var array = modules.ToArray();
              
				return array;
			} 
            private static IEnumerable<ModuleInfo> GetNotAllreadyLoadedModuleInfos(FileSystemInfo info, Type IModuleType)
			{
				List<FileInfo> validAssemblies = new List<FileInfo>();
				Assembly[] alreadyLoadedAssemblies = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();
				FileInfo fileInfo = info as FileInfo;
				if (fileInfo!=null)
				{
					if(alreadyLoadedAssemblies
									  .FirstOrDefault(
									  assembly =>
									  String.Compare(Path.GetFileName(assembly.Location), fileInfo.Name,
													 StringComparison.OrdinalIgnoreCase) == 0) == null)
					{
						var moduleInfos= Assembly.ReflectionOnlyLoadFrom(fileInfo.FullName)
											 .GetExportedTypes()
											 .Where(IModuleType.IsAssignableFrom)
											 .Where(t => t != IModuleType)
											 .Where(t => !t.IsAbstract)
											 .Select(type => CreateModuleInfo(type));
						return moduleInfos;
					}
				}
				DirectoryInfo directory = info as DirectoryInfo;
				var fileInfos = directory.GetFiles("*.dll")
				  .Where(file => alreadyLoadedAssemblies
									 .FirstOrDefault(
									 assembly =>
									 String.Compare(Path.GetFileName(assembly.Location), file.Name,
													StringComparison.OrdinalIgnoreCase) == 0) == null);

				foreach (FileInfo file in fileInfos)
				{
					try
					{
						Assembly.ReflectionOnlyLoadFrom(file.FullName);
						validAssemblies.Add(file);
					}
					catch (BadImageFormatException)
					{
						// skip non-.NET Dlls
					}
				}

				return validAssemblies.SelectMany(file => Assembly.ReflectionOnlyLoadFrom(file.FullName)
											.GetExportedTypes()
											.Where(IModuleType.IsAssignableFrom)
											.Where(t => t != IModuleType)
											.Where(t => !t.IsAbstract)
											.Select(type => CreateModuleInfo(type)))
                                            .ToList();



			}

			private static Assembly OnReflectionOnlyResolve(ResolveEventArgs args, FileSystemInfo info)
			{
				Assembly loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault(
					asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));
				if (loadedAssembly != null)
				{
					return loadedAssembly;
				}
				DirectoryInfo directory = info as DirectoryInfo;
				if (directory!=null)
				{
					AssemblyName assemblyName = new AssemblyName(args.Name);
					string dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");
					if (File.Exists(dependentAssemblyFilename))
					{
						return Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename);
					}
				}
				
				return Assembly.ReflectionOnlyLoad(args.Name);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
			internal void LoadAssemblies(IEnumerable<string> assemblies)
			{
				foreach (string assemblyPath in assemblies)
				{
					try
					{
						Assembly.ReflectionOnlyLoadFrom(assemblyPath);
					}
					catch (FileNotFoundException)
					{
						// Continue loading assemblies even if an assembly can not be loaded in the new AppDomain
					}
				}
			}

			private static ModuleInfo CreateModuleInfo(Type type)
			{
				string moduleName = type.Name;
				List<string> dependsOn = new List<string>();
				bool onDemand = false;
				var moduleAttribute =
					CustomAttributeData.GetCustomAttributes(type).FirstOrDefault(
						cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleAttribute).FullName);

				if (moduleAttribute != null)
				{
					foreach (CustomAttributeNamedArgument argument in moduleAttribute.NamedArguments)
					{
						string argumentName = argument.MemberInfo.Name;
						switch (argumentName)
						{
							case "ModuleName":
								moduleName = (string)argument.TypedValue.Value;
								break;

							case "OnDemand":
								onDemand = (bool)argument.TypedValue.Value;
								break;

							case "StartupLoaded":
								onDemand = !((bool)argument.TypedValue.Value);
								break;
						}
					}
				}

				var moduleDependencyAttributes =
					CustomAttributeData.GetCustomAttributes(type).Where(
						cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleDependencyAttribute).FullName);

				foreach (CustomAttributeData cad in moduleDependencyAttributes)
				{
					dependsOn.Add((string)cad.ConstructorArguments[0].Value);
				}

				ModuleInfo moduleInfo = new ModuleInfo(moduleName, type.AssemblyQualifiedName)
				{
					InitializationMode =
													onDemand
														? InitializationMode.OnDemand
														: InitializationMode.WhenAvailable,
					Ref = type.Assembly.CodeBase,
				};
				moduleInfo.DependsOn.AddRange(dependsOn);
				return moduleInfo;
			}
		}

	}
}
	
 
