using System;
using System.IO;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Controls;
using ASC.Web.Controls.FileUploader.HttpModule;
using ASC.Web.Core.Utility;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;

namespace ASC.Web.Files.Classes
{
    public class FilesUploader : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = "";
            var fileName = "";
            try
            {
                if (!SecurityContext.AuthenticateMe(context.Request[UrlConstant.AuthKey]))
                    throw new UnauthorizedAccessException("Access denied.");

                using (var fileDao = Global.DaoFactory.GetFileDao())
                {
                    var folderID = context.Request[UrlConstant.FolderId];
                    var fileID = context.Request[UrlConstant.FileId];

                    if (ProgressFileUploader.HasFilesToUpload(context))
                    {
                        var postedFile = new ProgressFileUploader.FileToUpload(context);
                        var contentLength = postedFile.ContentLength;
                        if (contentLength > SetupInfo.MaxUploadSize)
                            throw new Exception(FilesCommonResource.ErrorMassage_ExceededMaximumFileSize);

                        if (contentLength <= 0)
                            throw new Exception(FilesCommonResource.ErrorMassage_EmptyFile);

                        fileName = postedFile.FileName;
                        if (fileName.LastIndexOf('\\') != 0)
                            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                        fileName = Global.ReplaceInvalidCharsAndTruncate(fileName);

                        var inputStream = postedFile.InputStream;
                        var contentType = postedFile.FileContentType;

                        if (!String.IsNullOrEmpty(fileID))
                        {
                            var file = fileDao.GetFile(Convert.ToInt32(fileID));
                            if (file != null)
                                result = UpdateFile(file, fileName, contentLength, contentType, inputStream, fileDao);
                        }
                        else if (!string.IsNullOrEmpty(folderID))
                        {
                            var file = fileDao.GetFile(Convert.ToInt32(folderID), fileName);
                            result = file != null ?
                                UpdateFile(file, fileName, contentLength, contentType, inputStream, fileDao) :
                                SaveFile(folderID, fileName, contentLength, contentType, inputStream, fileDao);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new FileUploadResult()
                {
                    Success = false,
                    Message = ex.Message,
                };
            }

            return new FileUploadResult()
            {
                Success = true,
                Data = new { CurFileName = fileName },
                Message = result,
                FileName = fileName
            };
        }

        private string SaveFile(string folderID, string fileName, int contentLength, string contentType, Stream inputStream, IFileDao fileDao)
        {
            if (!FileFormats.IsSupported(fileName)) throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);

            using (var folderDao = Global.DaoFactory.GetFolderDao())
            {
                var folder = folderDao.GetFolder(Convert.ToInt32(folderID));
                if (folder == null) throw new Exception(FilesCommonResource.ErrorMassage_FolderNotFound);
                if (!Global.GetFilesSecurity().CanCreate(folder))
                    throw new System.Security.SecurityException(
                        FilesCommonResource.ErrorMassage_SecurityException_Create);
            }

            var file = new ASC.Files.Core.File()
            {
                FolderID = Convert.ToInt32(folderID),
                Title = fileName,
                ContentLength = contentLength,
                ContentType = contentType,
            };

            try
            {
                file = fileDao.SaveFile(file);
                Global.GetStore().Save(fileDao.GetUniqFilePath(file), inputStream, file.Title);
            }
            catch
            {
                fileDao.DeleteFile(file.ID);
                throw;
            }
            return file.ID.ToString();
        }

        private string UpdateFile(ASC.Files.Core.File file, string fileName, int contentLength, string contentType, Stream inputStream, IFileDao fileDao)
        {
            if (String.Compare(file.Title, fileName, true) != 0)
                throw new Exception(FilesCommonResource.ErrorMassage_DifferentName);
            if ((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                throw new Exception(FilesCommonResource.ErrorMassage_UpdateEditingFile);
            if (!Global.GetFilesSecurity().CanEdit(file))
                throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException);

            file.Title = fileName;
            file.ContentLength = contentLength;
            file.ContentType = contentType;
            file.ConvertedType = null;
            file.Version++;

            Global.GetStore().Save(fileDao.GetUniqFilePath(file), inputStream, file.Title);
            file = fileDao.SaveFile(file);

            NotifyClient.SendUpdateNoticeAsync(SecurityContext.CurrentAccount.ID, file);

            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                tagDao.SaveTags(NotifySource.Instance.GetSubscriptionProvider().GetRecipients(NotifyConstants.Event_DocumentInformer, file.UniqID).Select(r => Tag.New(new Guid(r.ID), file)).ToArray());
            }
            return file.ID.ToString();
        }
    }
}