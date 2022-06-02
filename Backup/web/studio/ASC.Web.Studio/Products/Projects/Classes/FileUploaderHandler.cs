using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using ASC.Common.Web;
using ASC.Files.Core;
using ASC.Web.Controls;
using ASC.Web.Controls.FileUploader.HttpModule;
using ASC.Web.Studio.Core;

namespace ASC.Web.Projects.Classes
{
    public class FileUploaderHandler : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var fileUploadResult = new FileUploadResult();
            if (!ProgressFileUploader.HasFilesToUpload(context) || string.IsNullOrEmpty(context.Request["ProjectID"])) return fileUploadResult;

            var file = new ProgressFileUploader.FileToUpload(context);
            if (String.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
            {
                throw new InvalidOperationException("Invalid file.");
            }
            if (0 < SetupInfo.MaxUploadSize && SetupInfo.MaxUploadSize < file.ContentLength)
            {
                throw new InvalidOperationException(string.Format("Exceeds the maximum file size ({0}MB).", SetupInfo.MaxUploadSizeToMBFormat));
            };

            if (CallContext.GetData("CURRENT_ACCOUNT") == null)
            {
                CallContext.SetData("CURRENT_ACCOUNT", new Guid(context.Request["UserID"]));
            }

            File document = null;
            if (!string.IsNullOrEmpty(context.Request["FileID"]) && !string.IsNullOrEmpty(context.Request["FileVersion"]))
            {
                document = FileEngine2.GetFile(Convert.ToInt32(context.Request["FileID"]), Convert.ToInt32(context.Request["FileVersion"]));
                document.Version++;
            }
            if (document == null)
            {
                document = new File
                {
                    Title = file.FileName.LastIndexOf('\\') != -1 ? file.FileName.Substring(file.FileName.LastIndexOf('\\') + 1) : file.FileName,
                    FolderID = FileEngine2.GetRoot(Convert.ToInt32(context.Request["ProjectID"])),
                };
            }
            document.ContentLength = file.ContentLength;
            document.ContentType = MimeMapping.GetMimeMapping(document.Title);

            FileEngine2.SaveFile(document, file.InputStream);

            fileUploadResult.Success = true;
            fileUploadResult.Data = String.Format("{0}_{1}", document.ID, document.Version);
            return fileUploadResult;
        }
    }
}