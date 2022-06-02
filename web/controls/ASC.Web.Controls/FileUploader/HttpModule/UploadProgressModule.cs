using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace ASC.Web.Controls.FileUploader.HttpModule
{
    public class UploadProgressModule : IHttpModule, System.Web.SessionState.IRequiresSessionState
    {
        private readonly static FieldInfo _requestWorkerField = typeof(HttpRequest).GetField("_wr", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly Regex _isUrlWithExtension = new Regex(@"[^\.]+\.a[^x]+x", RegexOptions.Compiled);
        

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(ContextBeginRequest);
            context.EndRequest += new EventHandler(ContextEndRequest);
        }

        public void Dispose()
        {
        }

        private void ContextEndRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            
            HttpUploadWorkerRequest origWr = _requestWorkerField.GetValue(app.Context.Request) as HttpUploadWorkerRequest;
            if (origWr != null)
                origWr.EndOfUploadRequest();
        }

        

        private void ContextBeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            var request = app.Context.Request;
            var origWr = _requestWorkerField.GetValue(request) as HttpWorkerRequest;
            
            string id;
            if (UploadProgressUtils.IsUploadStatusRequest(origWr, out id))
            {

            }

            if (UploadProgressUtils.IsUpload(origWr))
            {
                var s = request.RawUrl;

                if (string.IsNullOrEmpty(s))
                    return;

                if (!_isUrlWithExtension.IsMatch(s))
                    return;

                var newWr = new HttpUploadWorkerRequest(origWr);                
                _requestWorkerField.SetValue(request, newWr);
            }
        }
    }
        
}
