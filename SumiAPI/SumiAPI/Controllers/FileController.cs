using SumiAPI.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SumiAPI.Controllers
{
    public class FileController : ApiController
    {
        [HttpPost]
        [BasicAuthentication]
        [Route("file-upload")]
        public async Task<HttpResponseMessage> FileUpload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var provider = await Request.Content.ReadAsMultipartAsync<InMemoryMultipartFormDataStreamProvider>(new InMemoryMultipartFormDataStreamProvider());
            NameValueCollection formData = provider.FormData;
            var response = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                string username = User.Identity.Name;
                IList<HttpContent> files = provider.Files;
                List<string> lstImagePath = new List<string>();
                foreach (HttpContent file1 in files)
                {
                    string thisFileName = DateTime.Now.ToString("dd-MM-yyy_HH-mm-ss") + "_" + file1.Headers.ContentDisposition.FileName.Replace("\"", "").Replace("\"", "");
                    Stream input = await file1.ReadAsStreamAsync();
                    string serverSubPath = HttpContext.Current.Server.MapPath(string.Format("~/Content/Upload_Image/" + username));
                    bool exists = System.IO.Directory.Exists(serverSubPath);
                    if (!exists)
                        System.IO.Directory.CreateDirectory(serverSubPath);

                    string serverPath = serverSubPath + "\\" + thisFileName;
                    if (File.Exists(serverPath))
                    {
                        File.Delete(serverPath);
                    }
                    using (Stream file = File.OpenWrite(serverPath))
                    {
                        input.CopyTo(file);
                        file.Close();
                    }
                }
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent("success", Encoding.UTF8, "application/json");
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.Forbidden);
                response.Content = new StringContent(ex.Message, Encoding.UTF8, "application/json");
            }

            return response;

        }
        [HttpGet]
        [BasicAuthentication]
        [Route("file-name-get")]
        public HttpResponseMessage FileListGet()
        {
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            string json = "{\"Result\":#Result#}";
            try
            {
                string username = User.Identity.Name;
                string serverSubPath = HttpContext.Current.Server.MapPath(string.Format("~/Content/Upload_Image/" + username));
                bool exists = System.IO.Directory.Exists(serverSubPath);
                string fileNames = "";
                if (exists)
                {
                    DirectoryInfo d = new DirectoryInfo(serverSubPath);
                    FileInfo[] Files = d.GetFiles("*.*"); //Getting Text files

                    foreach (FileInfo file in Files)
                    {
                        fileNames = fileNames + "\"" + file.Name + "\",";
                    }
                    if (!fileNames.Equals(""))
                        fileNames = fileNames.Remove(fileNames.Length - 1);
                }
                json = json.Replace("#Result#", "[" + fileNames + "]");
            }
            catch (Exception ex)
            {
                json = "{\"Result\":[]}";
            }
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }
        [HttpGet]
        [BasicAuthentication]
        [Route("file-get")]
        public HttpResponseMessage GetFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            string username = User.Identity.Name;
            string localFilePath;

            localFilePath = HttpContext.Current.Server.MapPath(string.Format("~/Content/Upload_Image/" + username + "/" + fileName));
            if (!File.Exists(localFilePath))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return response;
        }
        [HttpGet]
        [BasicAuthentication]
        [Route("validate")]
        public HttpResponseMessage Validate()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }
    }
}
