using ASC.Notify.Model;
using ASC.Notify.Patterns;

namespace ASC.Web.Files.Services.NotifyService
{
    public static class NotifyConstants
    {
        #region Events

        public static INotifyAction Event_DocumentInformer = new NotifyAction("DocumentInformer", "document informer");

        //public static INotifyAction Event_UploadDocument = new NotifyAction("UploadDocument", "upload document");
        //public static INotifyAction Event_DeleteDocument = new NotifyAction("DeleteDocument", "delete document");

        public static INotifyAction Event_UpdateDocument = new NotifyAction("UpdateDocument", "update document");
        public static INotifyAction Event_ShareDocument = new NotifyAction("ShareDocument", "share document");
        public static INotifyAction Event_ShareFolder = new NotifyAction("ShareFolder", "share folder");

        #endregion

        #region  Tags

        public static ITag Tag_DocumentTitle = new Tag("DocumentTitle");
        public static ITag Tag_DocumentUrl = new Tag("DocumentURL");
        public static ITag Tag_VersionNumber = new Tag("VersionNumber");
        public static ITag Tag_DownloadURL = new Tag("DownloadURL");
        public static ITag Tag_DocumentSize = new Tag("DocumentSize");
        public static ITag Tag_FolderID = new Tag("FolderID");
        public static ITag Tag_FolderTitle = new Tag("FolderTitle");
        public static ITag Tag_EventType = new Tag("EventType");
        public static ITag Tag_Comment = new Tag("Comment");
        public static ITag Tag_AccessRights = new Tag("AccessRights");

        public static ITag Tag_AuthorName = new Tag("AuthorName");
        public static ITag Tag_AuthorUrl = new Tag("AuthorUrl");

        #endregion
    }
}