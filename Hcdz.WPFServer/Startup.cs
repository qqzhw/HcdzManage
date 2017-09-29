using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Transports;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Hcdz.WPFServer
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			//var resolver = new AutofacDependencyResolver(service);
			 //GlobalHost.DependencyResolver = resolver;
			app.UseCors(CorsOptions.AllowAll);
			app.UseErrorPage();
			// We need to extend this since the inital connect might take a while
			//configuration.TransportConnectTimeout = TimeSpan.FromSeconds(6);
			//GlobalHost.DependencyResolver = resolver;
			var config = new HubConfiguration
			{
				Resolver = GlobalHost.DependencyResolver,
				EnableJSONP=true, 
			};
			 
			app.MapSignalR(config);
			//app.MapSignalR();
			SetupWebApi(app); 
			SetupFileUpload(app);
		
		}
		 

		private void SetupWebApi(IAppBuilder app)
		{
			var httpConfiguration = new HttpConfiguration();

			httpConfiguration.Formatters.Clear();
			httpConfiguration.Formatters.Add(new JsonMediaTypeFormatter());

			httpConfiguration.Formatters.JsonFormatter.SerializerSettings =
				new JsonSerializerSettings
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver()
				};

			httpConfiguration.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional });

			app.UseWebApi(httpConfiguration);
			 
		}
		 
		private void SetupFileUpload(IAppBuilder app)
		{
			app.Map("/upload-file", map =>
			{ 
				map.Run(async context =>
				{
					if (!context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
					{
						context.Response.StatusCode = 404;
					}
					 
					else
					{
						var form = await context.Request.ReadFormAsync();

						string roomName = form["room"];
						string connectionId = form["connectionId"];
						string file = form["file"];
						string fileName = form["filename"];
						string contentType = form["type"];

						//    BinaryBlob binaryBlob = BinaryBlob.Parse(file);

						//    if (String.IsNullOrEmpty(contentType))
						//    {
						//        contentType = "application/octet-stream";
						//    }

						//    var stream = new MemoryStream(binaryBlob.Data);

						//    await uploadHandler.Upload(context.Request.User.GetUserId(),
						//                               connectionId,
						//                               roomName,
						//                               fileName,
						//                               contentType,
						//                               stream);
					}
				});
			});
		}
		private static void SetupSignalR( IAppBuilder app)
		{
 
			//set dependency resolver
			// var resolver = new AutofacDependencyResolver(EngineContext.Current.ContainerManager.Container);
			//   DependencyResolver.SetResolver(resolver);
			//var connectionManager = resolver.Resolve<IConnectionManager>();
			//var heartbeat = resolver.Resolve<ITransportHeartbeat>();
			//var hubPipeline = resolver.Resolve<IHubPipeline>();
			//var configuration = resolver.Resolve<IConfigurationManager>();

			// Enable service bus scale out
			//if (!String.IsNullOrEmpty(jabbrConfig.ServiceBusConnectionString) &&
			//    !String.IsNullOrEmpty(jabbrConfig.ServiceBusTopicPrefix))
			//{
			//    var sbConfig = new ServiceBusScaleoutConfiguration(jabbrConfig.ServiceBusConnectionString,
			//                                                       jabbrConfig.ServiceBusTopicPrefix)
			//    {
			//        TopicCount = 5
			//    };

			//    resolver.UseServiceBus(sbConfig);
			//}

			//if (jabbrConfig.ScaleOutSqlServer)
			//{
			//    resolver.UseSqlServer(jabbrConfig.SqlConnectionString.ConnectionString);
			//}
			  
		}

		//private static void SetupWebApi(IKernel kernel, IAppBuilder app)
		//{
			//var config = new HttpConfiguration();
			//var jsonFormatter = new JsonMediaTypeFormatter();

			//config.Formatters.Clear();
			//jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			//config.Formatters.Add(jsonFormatter);
			//config.DependencyResolver = new NinjectWebApiDependencyResolver(kernel);

			//config.Routes.MapHttpRoute(
			//	name: "MessagesV1",
			//	routeTemplate: "api/v1/{controller}/{room}"
			//);

			//config.Routes.MapHttpRoute(
			//	name: "DefaultApi",
			//	routeTemplate: "api",
			//	defaults: new { controller = "ApiFrontPage" }
			//);

			//app.UseWebApi(config);
		//}
	}
  
}
