using System.Web;
using System;
using System.Web.SessionState;

namespace ASC.Web.Controls.FileUploader.HttpModule
{
    public abstract class FileUploadHandler
    {
        public class FileUploadResult
        {
            public bool Success { get; set; }
            public string FileName { get; set; }
            public string FileURL { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }

        public virtual string GetFileName(string path)
        {
            var name = path ?? "";
            var ind = name.LastIndexOf('\\');
            if (ind != -1)
                return name.Substring(ind + 1);

            return name;
        }

        public abstract FileUploadResult ProcessUpload(HttpContext context);
    }

    public class UploadProgressHandler : System.Web.UI.Page, IHttpHandler, IRequiresSessionState 
    {
        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            if (!String.IsNullOrEmpty(context.Request["submit"]))
            {
                var result = new ASC.Web.Controls.FileUploader.HttpModule.FileUploadHandler.FileUploadResult()
                {
                    Success = false,
                    Message = "type not found"
                };

                try
                {
                    var uploadHandler = (FileUploadHandler)Activator.CreateInstance(Type.GetType(context.Request["submit"]));
                    if (uploadHandler != null)
                        result = uploadHandler.ProcessUpload(context);
                }
                catch { }

                //context.Response.ContentType = "application/json";NOTE:Don't set content type. ie cant parse it
                context.Response.StatusCode = 200;
                context.Response.Write(AjaxPro.JavaScriptSerializer.Serialize(result));
            }
            else
            {

                context.Response.ContentType = "application/json";
                string id = context.Request.QueryString[UploadProgressStatistic.UploadIdField];

                UploadProgressStatistic us = UploadProgressStatistic.GetStatistic(id);

                if (!String.IsNullOrEmpty(context.Request["limit"]))
                {
                    long limit = long.Parse(context.Request["limit"]);
                    if (us.TotalBytes > limit * 1024)
                    {
                        us.ReturnCode = 1;
                        us.IsFinished = true;
                    }
                }

                context.Response.Write(us.ToJson());
            }
        }
    }
}

