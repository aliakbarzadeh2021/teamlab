﻿using System;
using System.Web;
using System.Web.SessionState;
using ASC.Data.Storage;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio.HttpHandlers
{
    public class FCKEditorFileUploadHandler : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {   
                var storeDomain = context.Request["esid"];
                var itemID= context.Request["iid"]??"";
                
                HttpPostedFile file = context.Request.Files["NewFile"];

                if (file.ContentLength > SetupInfo.MaxImageUploadSize)
                {
                    SendFileUploadResponse(context, 1, true, string.Empty, string.Empty, Resources.Resource.ErrorFileSizeLimitText + " (" +SetupInfo.MaxImageUploadSizeToMBFormat+" MB)");
                    return;
                }

                var filename = file.FileName;
                var ind = file.FileName.LastIndexOf("\\");
                if(ind>=0)
                    filename = file.FileName.Substring(ind+1);

                var folderID = CommonControlsConfigurer.FCKAddTempUploads(storeDomain, filename, itemID);

                var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "fckuploaders");
                string saveUri = store.Save(storeDomain, folderID + "/" + filename, file.InputStream).ToString();

                

                SendFileUploadResponse(context, 0, true, saveUri, filename, string.Empty);
            }
            catch (Exception e)
            {
                SendFileUploadResponse(context, 1, true, string.Empty, string.Empty, e.Message.HtmlEncode());
            }
        }

        #endregion


        protected void SendFileUploadResponse(HttpContext context, int errorNumber, bool isQuickUpload, string fileUrl, string fileName, string customMsg)
        {
            context.Response.Clear();

            context.Response.Write("<script type=\"text/javascript\">");

            // Minified version of the document.domain automatic fix script.
            // The original script can be found at _dev/domain_fix_template.js
            context.Response.Write(@"(function(){var d=document.domain; while (true){try{var A=window.top.opener.document.domain;break;}catch(e) {};d=d.replace(/.*?(?:\.|$)/g,'');if (d.length==0) break;try{document.domain=d;}catch (e){break;}}})();");

            if (isQuickUpload)
                context.Response.Write("window.parent.OnUploadCompleted(" + errorNumber + ",'" + fileUrl.Replace("'", "\\'") + "','" + fileName.Replace("'", "\\'") + "','" + customMsg.Replace("'", "\\'") + "') ;");
            else
                context.Response.Write("window.parent.frames['frmUpload'].OnUploadCompleted(" + errorNumber + ",'" + fileName.Replace("'", "\\'") + "') ;");

            context.Response.Write("</script>");
        }
    }
}
