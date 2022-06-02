using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Studio.Core;
using Microsoft.ServiceModel.Web;

namespace ASC.Web.Files
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class FileHandler : IHttpHandler
    {
        public static string FileHandlerPath
        {
            get { return VirtualPathUtility.ToAbsolute(FileConst.UrlFileHandler); }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var publicActions = new[] { "download", "view", "bulk" };
            var action = context.Request[UrlConstant.Action];

            if (!SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey))
                && !(CoreContext.TenantManager.GetCurrentTenant().Public && publicActions.Contains(action)))
            {
                context.Response.Redirect("~/auth.aspx");
            }

            switch (action)
            {
                case "upload":
                    UploadFile(context);
                    break;
                case "view":
                    DownloadFile(context, true);
                    break;
                case "download":
                    DownloadFile(context, false);
                    break;
                case "bulk":
                    BulkDownloadFile(context);
                    break;
                case "useractivity":
                    break;
                case "save":
                    SaveFile(context);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void BulkDownloadFile(HttpContext context)
        {
            var store = Global.GetStoreTmp();
            var path = string.Format(@"{0}\{1}.zip", SecurityContext.CurrentAccount.ID, UrlConstant.DownloadTitle);
            if (!store.IsFile(path))
            {
                context.Response.Redirect((context.Request.UrlReferrer != null
                                               ? context.Request.UrlReferrer.ToString()
                                               : PathProvider.StartURL())
                                          + "#" + UrlConstant.Error + "/" +
                                          HttpUtility.UrlEncode(FilesCommonResource.ErrorMassage_FileNotFound));
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/zip";
            context.Response.AddHeader("Content-Disposition",
                                       "attachment; filename=\"" + UrlConstant.DownloadTitle + ".zip\"");

            using (var readStream = store.IronReadStream(string.Empty, path, 40))
            {
                context.Response.AddHeader("Content-Length", readStream.Length.ToString());
                readStream.StreamCopyTo(context.Response.OutputStream);
            }
            try
            {
                context.Response.Flush();
                context.Response.End();
            }
            catch (HttpException) { }
        }

        private void DownloadFile(HttpContext context, bool inline)
        {
            var id = context.Request[UrlConstant.FileId];
            var ver = context.Request[UrlConstant.Version];

            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(id);

            var store = Global.GetStore();
            File file;
            string path;

            using (var dao = Global.DaoFactory.GetFileDao())
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                file = String.IsNullOrEmpty(ver)
                           ? dao.GetFile(Convert.ToInt32(id))
                           : dao.GetFile(Convert.ToInt32(id), Convert.ToInt32(ver));
                if (!Global.GetFilesSecurity().CanRead(file))
                {
                    context.Response.Redirect((context.Request.UrlReferrer != null
                                                   ? context.Request.UrlReferrer.ToString()
                                                   : PathProvider.StartURL())
                                              + "#" + UrlConstant.Error + "/" +
                                              HttpUtility.UrlEncode(
                                                  FilesCommonResource.ErrorMassage_SecurityException_ReadFile));
                    return;
                }

                path = dao.GetUniqFilePath(file);
                if (!store.IsFile(path))
                {
                    context.Response.Redirect((context.Request.UrlReferrer != null
                                                   ? context.Request.UrlReferrer.ToString()
                                                   : PathProvider.StartURL())
                                              + "#" + UrlConstant.Error + "/" +
                                              HttpUtility.UrlEncode(FilesCommonResource.ErrorMassage_FileNotFound));
                    return;
                }

                tagDao.RemoveTags(Tag.New(SecurityContext.CurrentAccount.ID, file));
            }

            context.Response.Clear();
            context.Response.ContentType = file.ContentType;
            context.Response.Charset = "utf-8";

            var browser = context.Request.Browser.Browser;
            if (browser == "AppleMAC-Safari" && 0 <= context.Request.UserAgent.IndexOf("chrome", StringComparison.InvariantCultureIgnoreCase)) browser = "Chrome";
            var format = browser == "IE" || browser == "AppleMAC-Safari" ? "{0}; filename=\"{1}\"" : "{0}; filename*=utf-8'en'{1}";
            var filename = browser == "AppleMAC-Safari" ? file.Title : HttpUtility.UrlPathEncode(file.Title);
            context.Response.AddHeader("Content-Disposition", string.Format(format, inline ? "inline" : "attachment", filename));

            if (inline && string.Equals(context.Request.Headers["If-None-Match"], GetEtag(file)))
            {
                //Its cached. Reply 304
                context.Response.StatusCode = 304;
                context.Response.Cache.SetETag(GetEtag(file));
            }
            else
            {
                context.Response.CacheControl = "public";
                context.Response.Cache.SetETag(GetEtag(file));
                context.Response.Cache.SetCacheability(HttpCacheability.Public);

                if (file.ConvertedType == null)
                {
                    using (var readStream = store.IronReadStream(string.Empty, path, 40))
                    {
                        context.Response.AddHeader("Content-Length", readStream.Length.ToString());
                        readStream.StreamCopyTo(context.Response.OutputStream);
                    }
                }
                else
                {
                    //Take from converter
                    var readStream = Global.GetConvertedFile(file);
                    if (readStream != null)
                    {
                        readStream.StreamCopyTo(context.Response.OutputStream);
                    }
                }
                try
                {
                    context.Response.Flush();
                    context.Response.End();
                }
                catch (HttpException) { }
            }
        }

        private string GetEtag(File file)
        {
            return file.ID + ":" + file.Version;
        }

        private void UploadFile(HttpContext context)
        {
            string result;
            var store = Global.GetStore();

            var folderID = context.Request[UrlConstant.FolderId];
            var title = Global.ReplaceInvalidCharsAndTruncate(context.Request[UrlConstant.FileTitle]);

            if (!FileFormats.IsSupported(title))
            {
                throw new Exception(FilesCommonResource.ErrorMassage_NotSupportedFormat);
            }
            if (context.Request.ContentLength > SetupInfo.MaxUploadSize)
            {
                throw new Exception(FilesCommonResource.ErrorMassage_ExceededMaximumFileSize);
            }
            if (context.Request.ContentLength <= 0)
            {
                throw new Exception(FilesCommonResource.ErrorMassage_EmptyFile);
            }

            using (var dao = Global.DaoFactory.GetFileDao())
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var file = dao.GetFile(Convert.ToInt32(folderID), title);
                if (file != null)
                {
                    if (!Global.GetFilesSecurity().CanEdit(file))
                    {
                        throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException);
                    }
                    if ((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                        throw new Exception(FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFile);

                    file.Title = title;
                    file.ContentLength = context.Request.ContentLength;
                    file.ContentType = context.Request.ContentType;
                    file.ConvertedType = null;
                    file.Version++;

                    store.Save(dao.GetUniqFilePath(file), context.Request.InputStream, file.Title);
                    file = dao.SaveFile(file);

                    NotifyClient.SendUpdateNoticeAsync(SecurityContext.CurrentAccount.ID, file);
                    tagDao.SaveTags(
                        NotifySource.Instance.GetSubscriptionProvider().GetRecipients(
                            NotifyConstants.Event_DocumentInformer, file.UniqID).Select(
                                r => Tag.New(new Guid(r.ID), file)).ToArray());
                }
                else
                {
                    using (var folderDao = Global.DaoFactory.GetFolderDao())
                    {
                        var folder = folderDao.GetFolder(Convert.ToInt32(folderID));
                        if (folder == null) throw new Exception(FilesCommonResource.ErrorMassage_FolderNotFound);
                        if (!Global.GetFilesSecurity().CanCreate(folder))
                            throw new System.Security.SecurityException(
                                FilesCommonResource.ErrorMassage_SecurityException_Create);
                    }
                    file = new File
                               {
                                   FolderID = Convert.ToInt32(folderID),
                                   Title = title,
                                   ContentLength = context.Request.ContentLength,
                                   ContentType = context.Request.ContentType
                               };
                    try
                    {
                        file = dao.SaveFile(file);
                        store.Save(dao.GetUniqFilePath(file), context.Request.InputStream, file.Title);
                    }
                    catch
                    {
                        dao.DeleteFile(file.ID);
                        throw;
                    }
                }

                result = file.ID.ToString() + "|" + file.FolderID.ToString();
            }
            context.Response.Write(result);
        }

        private void SaveFile(HttpContext context)
        {
            var fileID = context.Request[UrlConstant.FileId];
            if (string.IsNullOrEmpty(fileID)) throw new ArgumentNullException(fileID);

            var downloadUri = context.Request[UrlConstant.FileUri];
            if (string.IsNullOrEmpty(downloadUri)) throw new ArgumentNullException(downloadUri);

            using (var dao = Global.DaoFactory.GetFileDao())
            using (var tagDao = Global.DaoFactory.GetTagDao())
            {
                var file = dao.GetFile(Convert.ToInt32(fileID));
                if (file == null)
                    throw GenerateException(FilesCommonResource.ErrorMassage_FileNotFound);
                if (!Global.GetFilesSecurity().CanEdit(file))
                    throw GenerateException(FilesCommonResource.ErrorMassage_SecurityException);
                if (file.RootFolderType == FolderType.TRASH)
                    throw GenerateException(FilesCommonResource.ErrorMassage_ViewTrashItem);

                lock (File.NowEditing)
                {
                    File.NowEditing[file.ID] = DateTime.UtcNow;
                }
                var store = Global.GetStore();
                var filePath = dao.GetUniqFilePath(file);
                var versionEdit = context.Request[UrlConstant.Version];
                var currentType = file.ConvertedType ?? FileFormats.GetExtension(file.Title);
                var newType = FileFormats.GetExtension(downloadUri);
                var updateVersion = file.Version > 1 || file.ConvertedType == null || string.IsNullOrEmpty(context.Request[UrlConstant.New]);

                if ((string.IsNullOrEmpty(versionEdit) || file.Version <= Convert.ToInt32(versionEdit) || currentType != newType)
                    && updateVersion)
                {
                    file.Version++;
                }
                else
                {
                    updateVersion = false;
                    store.Delete(filePath);
                }

                var client = new WebClient();
                var bytes = client.DownloadData(downloadUri);

                file.ContentLength = bytes.Length;
                file.ConvertedType = newType;

                store.Save(dao.GetUniqFilePath(file), new System.IO.MemoryStream(bytes));
                dao.SaveFile(file);

                if (updateVersion)
                {
                    NotifyClient.SendUpdateNoticeAsync(SecurityContext.CurrentAccount.ID, file);
                    tagDao.SaveTags(
                        NotifySource.Instance.GetSubscriptionProvider().GetRecipients(
                            NotifyConstants.Event_DocumentInformer, file.UniqID).Select(r => Tag.New(new Guid(r.ID), file)).
                            ToArray());
                }
            }
        }

        private WebProtocolException GenerateException(string message)
        {
            return new WebProtocolException(HttpStatusCode.BadRequest, FilesCommonResource.ErrorMassage_BadRequest, new Exception(message));
        }
    }
}