using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace ASC.Web.Controls
{
    [Themeable(true)]
    [ToolboxData("<{0}:ProgressFileUploader runat=server></{0}:ProgressFileUploader>")]
    public class ProgressFileUploader : WebControl
    {
		public bool EnableHtml5 { get; set; }

        private string imageTrashLocation;
        private string imageUploadLocation;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            string  ajaxuploadScriptLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.FileUploader.js.ajaxupload.js");
            Page.ClientScript.RegisterClientScriptInclude("ajaxuploader_script", ajaxuploadScriptLocation);

            string fileUploaderScriptLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.FileUploader.js.fileuploader.js");
            Page.ClientScript.RegisterClientScriptInclude("fileuploader_script", fileUploaderScriptLocation);

            string swfUploadJSLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.FileUploader.js.swfupload.js");
            Page.ClientScript.RegisterClientScriptInclude("fileuploader_swf_script", swfUploadJSLocation);

			string fileHtml5UploaderJSLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.FileUploader.js.fileHtml5Uploader.js");
			Page.ClientScript.RegisterClientScriptInclude("fileHtml5Uploader_script", fileHtml5UploaderJSLocation);

            Page.ClientScript.RegisterClientScriptBlock(typeof(string), "fileHtml5Uploader_Enable", string.Format("FileHtml5Uploader.EnableHtml5 = typeof window.FileReader != 'undefined' && typeof (new XMLHttpRequest()).upload != 'undefined' && {0};", EnableHtml5.ToString().ToLower()), true);

            imageTrashLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.FileUploader.img.trash.png");
            imageUploadLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.FileUploader.img.upload.png");
			
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Page.ClientScript.RegisterClientScriptBlock(typeof(string), "fileuploader_swf_init", string.Format(" ASC.Controls.FileUploaderSWFLocation = '{0}'; ", ASC.Data.Storage.WebPath.GetPath("js/swfupload.swf")), true);
        }
    
        protected override void RenderContents(HtmlTextWriter writer)
        {
            var sb = new StringBuilder();
            sb.Append("<div id=\"asc_fileuploaderSWFContainer\" style='position:absolute;'><span id=\"asc_fileuploaderSWFObj\"></span></div>");            
            writer.Write(sb.ToString());
        }

		private static bool IsHtml5Upload(HttpContext context)
		{
			return "html5".Equals(context.Request["type"]);
		}

		private static string GetFileName(HttpContext context)
		{
			return context.Request["fileName"];
		}

		private static string GetFileContentType(HttpContext context)
		{
			return context.Request["fileContentType"];
		}

		public static bool HasFilesToUpload(HttpContext context)
		{
			if (context.Request.Files.Count > 0)
			{
				return true;
			}
			if (context.Request.InputStream != null && IsHtml5Upload(context))
			{
				return true;
			}
			return false;
		}

		public class FileToUpload
		{
			public string FileName {get; private set;}
			public Stream InputStream {get; private set;}
			public string FileContentType {get; private set;}
			public int ContentLength { get; private set; }

			public FileToUpload(HttpContext context)
			{
				if (ProgressFileUploader.IsHtml5Upload(context))
				{
					FileName = ProgressFileUploader.GetFileName(context);
					InputStream = context.Request.InputStream;
					FileContentType = ProgressFileUploader.GetFileContentType(context);
					ContentLength = (int)context.Request.InputStream.Length;
				}
				else
				{
					var file = context.Request.Files[0];
					FileName = file.FileName;
					InputStream = file.InputStream;
					FileContentType = file.ContentType;
					ContentLength = file.ContentLength;
				}
				if (string.IsNullOrEmpty(FileContentType))
				{
					FileContentType = ASC.Common.Web.MimeMapping.GetMimeMapping(FileName) ?? string.Empty;
				}
				FileName = FileName.Replace("'", "_").Replace("\"", "_");
			}
		}
        
    }
}
