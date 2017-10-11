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
		[HttpGet] 
		public async Task<HttpResponseMessage> GetBackup(string filePath)
		{
			HttpResponseMessage response;
			try
			{
				response = await Task.Run<HttpResponseMessage>(() =>
				{
					var directory = new DirectoryInfo(filePath);
					var files = directory.GetFiles();
					var lastCreatedFile = files.OrderByDescending(f => f.CreationTime).FirstOrDefault();
					var filestream = lastCreatedFile.OpenRead();
					var fileResponse = new HttpResponseMessage(HttpStatusCode.OK);
					fileResponse.Content = new StreamContent(filestream);
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