using System;
using System.Web;
using System.Web.SessionState;
using ASC.Web.Core.Utility;

namespace ASC.Web.Studio.HttpHandlers
{
    public class AjaxFileUploadHandler : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            FileUploadResult result = new FileUploadResult()
            {
                Success = false,
                Message = "type not found"
            };

            if (!String.IsNullOrEmpty(context.Request["type"]))
            {
                try
                {
                    var uploadHandler = (IFileUploadHandler)Activator.CreateInstance(Type.GetType(context.Request["type"]));
                    if (uploadHandler != null)
                        result = uploadHandler.ProcessUpload(context);
                }
                catch { }
            }

            context.Response.StatusCode = 200;
            context.Response.Write(AjaxPro.JavaScriptSerializer.Serialize(result));
        }

        #endregion
    }
}
