using System;
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Utility;
using ASC.Files.Core.Security;

namespace ASC.Web.Files.Services.NotifyService
{
    public class NotifyClient
    {
        public static INotifyClient Instance
        {
            get;
            private set;
        }

        static NotifyClient()
        {
            Instance = WorkContext.NotifyContext.NotifyService.RegisterClient(NotifySource.Instance);
        }

        public static void SendShareNoticeAsync(File document, FileShare fileShare, IRecipient[] recipients)
        {
            if (document == null || recipients.Length == 0) return;

            using (var dao = Global.DaoFactory.GetFolderDao())
            {
                var folder = dao.GetFolder(document.FolderID);
                if (folder == null) return;

                string aceString;
                switch (fileShare)
                {
                    case FileShare.Read:
                        aceString = Files.Resources.FilesUCResource.AceStatusEnum_Read;
                        break;
                    case FileShare.ReadWrite:
                        aceString = Files.Resources.FilesUCResource.AceStatusEnum_ReadWrite;
                        break;
                    default:
                        return;
                }

                var url = document.ViewUrl;
                if (FileFormats.PreviewedDocFormats.Contains(FileFormats.GetExtension(document.Title), StringComparer.CurrentCultureIgnoreCase))
                    url = PathProvider.BaseAbsolutePath + "docviewer.aspx?" + UrlConstant.FileId + "=" + document.ID;

                Instance.SendNoticeToAsync(
                    NotifyConstants.Event_ShareDocument,
                    document.UniqID,
                    recipients,
                    null,
                    new TagValue(NotifyConstants.Tag_EventType, "ShareDocument"),
                    new TagValue(NotifyConstants.Tag_DocumentTitle, document.Title),
                    new TagValue(NotifyConstants.Tag_DocumentUrl, CommonLinkUtility.GetFullAbsolutePath(url)),
                    new TagValue(NotifyConstants.Tag_AccessRights, aceString)
                    );
            }
        }

        public static void SendShareNoticeAsync(Folder folder, FileShare fileShare, IRecipient[] recipients)
        {
            if (folder == null || recipients.Length == 0) return;

            using (var dao = Global.DaoFactory.GetFolderDao())
            {
                string aceString;
                switch (fileShare)
                {
                    case FileShare.Read:
                        aceString = Files.Resources.FilesUCResource.AceStatusEnum_Read;
                        break;
                    case FileShare.ReadWrite:
                        aceString = Files.Resources.FilesUCResource.AceStatusEnum_ReadWrite;
                        break;
                    default:
                        return;
                }


                Instance.SendNoticeToAsync(
                    NotifyConstants.Event_ShareFolder,
                    folder.UniqID,
                    recipients,
                    null,
                    new TagValue(NotifyConstants.Tag_EventType, "ShareFolder"),
                    new TagValue(NotifyConstants.Tag_FolderTitle, folder.Title),
                    new TagValue(NotifyConstants.Tag_FolderID, folder.ID),
                    new TagValue(NotifyConstants.Tag_AccessRights, aceString)
                    );
            }
        }

        public static void SendUpdateNoticeAsync(Guid userId, params File[] documents)
        {
            if (documents == null || documents.Length == 0 ||
                userId.Equals(ASC.Core.Configuration.Constants.Guest.ID)) return;

            foreach (var document in documents)
            {
                var url = document.ViewUrl;
                if (FileFormats.PreviewedDocFormats.Contains(FileFormats.GetExtension(document.Title), StringComparer.CurrentCultureIgnoreCase))
                    url = PathProvider.BaseAbsolutePath + "docviewer.aspx?" + UrlConstant.FileId + "=" + document.ID;

                Instance.SendNoticeAsync(
                    NotifyConstants.Event_UpdateDocument,
                    document.UniqID,
                    null,
                    new TagValue(NotifyConstants.Tag_EventType, "UpdateDocument"),
                    new TagValue(NotifyConstants.Tag_DocumentTitle, document.Title),
                    new TagValue(NotifyConstants.Tag_DocumentUrl, CommonLinkUtility.GetFullAbsolutePath(url)),
                    new TagValue(NotifyConstants.Tag_VersionNumber, document.Version),
                    new TagValue(NotifyConstants.Tag_AuthorName, CoreContext.UserManager.GetUsers(userId).DisplayUserName()),
                    new TagValue(NotifyConstants.Tag_AuthorUrl, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(userId, Files.Configuration.ProductEntryPoint.ID))));
            }
        }

        //public static void SendDeleteNoticeAsync(params File[] documents)
        //{
        //    if (documents == null || documents.Length == 0) return;

        //    foreach (var document in documents)
        //        Instance.SendNoticeAsync(
        //            NotifyConstants.Event_DocumentInformer,
        //            document.UniqID,
        //            null,
        //            new TagValue(NotifyConstants.Tag_EventType, "DeleteDocument"),
        //            new TagValue(NotifyConstants.Tag_DocumentTitle, document.Title));
        //}

        //public static void SendUploadNoticeAsync(File document, string comment, IRecipient[] recipients)
        //{
        //    if (document == null) return;

        //    using (var dao = Global.DaoFactory.GetFolderDao())
        //    {
        //        var folder = dao.GetFolder(document.FolderID);
        //        if (folder == null) return;
        //        Instance.SendNoticeToAsync(
        //            NotifyConstants.Event_DocumentInformer,
        //            document.UniqID,
        //            recipients,
        //            null,
        //            new TagValue(NotifyConstants.Tag_EventType, "UploadDocument"),
        //            new TagValue(NotifyConstants.Tag_DocumentTitle, document.Title),
        //            new TagValue(NotifyConstants.Tag_DocumentUrl, CommonLinkUtility.GetFullAbsolutePath(document.ViewUrl)),
        //            new TagValue(NotifyConstants.Tag_FolderTitle, folder.Title),
        //            new TagValue(NotifyConstants.Tag_FolderID, folder.ID),
        //            new TagValue(NotifyConstants.Tag_Comment, comment),
        //            new TagValue(NotifyConstants.Tag_AuthorName, CoreContext.UserManager.GetUsers(document.CreateBy).DisplayUserName()),
        //            new TagValue(NotifyConstants.Tag_AuthorUrl, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(document.CreateBy, Files.Configuration.ProductEntryPoint.ID))));
        //    }
        //}
    }
}