using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Nop.Web.Controllers
{
    public partial class DownloadController : ApiController
    {
      
        public DownloadController ()
        {
           
        }
	
		public async Task<HttpResponseMessage> GetBackup(string filePath)
		{
			HttpResponseMessage response;
			try
			{
				response = await Task.Run<HttpResponseMessage>(() =>
				{
                    if (string.IsNullOrEmpty(filePath))
                    {
                      return  new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    //filePath = "d:\\testdb.bak";
                    //var directory = new DirectoryInfo(filePath);
                    //   var files = File.OpenRead(filePath);
                    //	var lastCreatedFile = files.OrderByDescending(f => f.CreationTime).FirstOrDefault();
                    var filestream = File.OpenRead(filePath);
					var fileResponse = new HttpResponseMessage(HttpStatusCode.OK);
					fileResponse.Content = new StreamContent(filestream, 1024*1024); //1M（1024*1024）
					fileResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					return fileResponse;
				});
			}
			catch (Exception e)
			{ 
				response = Request.CreateResponse(HttpStatusCode.InternalServerError);
			}
			return response;
		}
	 
       
    }
}