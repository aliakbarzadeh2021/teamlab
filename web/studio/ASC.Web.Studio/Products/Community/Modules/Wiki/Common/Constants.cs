using System;
using ASC.Common.Security.Authorizing;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Community.Wiki.Resources;

namespace ASC.Web.Community.Wiki.Common
{
    public class Constants
    {
        private readonly static string Action_AddPage_ID = "{D49F4E30-DA10-4b39-BC6D-B41EF6E039D3}";
        private readonly static string Action_EditPage_ID = "{D852B66F-6719-45e1-8657-18F0BB791690}";
        private readonly static string Action_RemovePage_ID = "{557D6503-633B-4490-A14C-6473147CE2B3}";
        private readonly static string Action_UploadFile_ID = "{088D5940-A80F-4403-9741-D610718CE95C}";
        private readonly static string Action_RemoveFile_ID = "{7CB5C0D1-D254-433f-ABE3-FF23373EC631}";
        private readonly static string Action_AddComment_ID = "{C426C349-9AD4-47cd-9B8F-99FC30675951}";
        private readonly static string Action_EditRemoveComment_ID = "{B630D29B-1844-4bda-BBBE-CF5542DF3559}";


        public static readonly ASC.Common.Security.Authorizing.Action Action_AddPage = new ASC.Common.Security.Authorizing.Action(
                                                                                        new Guid(Action_AddPage_ID),
                                                                                        "New Page"
                                                                                    );

        public static readonly ASC.Common.Security.Authorizing.Action Action_EditPage = new ASC.Common.Security.Authorizing.Action(
                                                                                                new Guid(Action_EditPage_ID),
                                                                                                "Edit page"
                                                                                            );

        public static readonly ASC.Common.Security.Authorizing.Action Action_RemovePage = new ASC.Common.Security.Authorizing.Action(
                                                                                                new Guid(Action_RemovePage_ID),
                                                                                                "Delete page"
                                                                                            );

        public static readonly ASC.Common.Security.Authorizing.Action Action_UploadFile = new ASC.Common.Security.Authorizing.Action(
                                                                                                new Guid(Action_UploadFile_ID),
                                                                                                "Upload file"
                                                                                            );

        public static readonly ASC.Common.Security.Authorizing.Action Action_RemoveFile = new ASC.Common.Security.Authorizing.Action(
                                                                                                new Guid(Action_RemoveFile_ID),
                                                                                                "Delete file"
                                                                                            );



        public static readonly ASC.Common.Security.Authorizing.Action Action_AddComment = new ASC.Common.Security.Authorizing.Action(
                                                                                                new Guid(Action_AddComment_ID),
                                                                                                "Add Comment"
                                                                                            );

        public static readonly ASC.Common.Security.Authorizing.Action Action_EditRemoveComment = new ASC.Common.Security.Authorizing.Action(
                                                                                                    new Guid(Action_EditRemoveComment_ID),
                                                                                                    "Edit/Delete comment"
                                                                                                );



        public static readonly AuthCategory BaseAuthCategory = new AuthCategory(
                                                        "Basic actions",
                                                        new[]{
                                                            Action_AddPage,
                                                            Action_EditPage,
                                                            Action_RemovePage,
                                                            Action_UploadFile,
                                                            Action_RemoveFile,
                                                            Action_AddComment,
                                                            Action_EditRemoveComment
                                                        }
                                                    );

        public static readonly AuthCategory[] AuthorizingCategories = new[]{
                                        BaseAuthCategory
                                        };


        public static INotifyAction NewPage = new NotifyAction("new wiki page", WikiResource.NotifyAction_NewPage);
        public static INotifyAction EditPage = new NotifyAction("edit wiki page", WikiResource.NotifyAction_ChangePage);
        public static INotifyAction AddPageToCat = new NotifyAction("add page to cat", WikiResource.NotifyAction_AddPageToCat);

        public const string _NewBlogSubscribeCategory = "{5B671A93-588D-40a7-BDC5-3E2EEC781CA4}";

        public static ITag TagPageName = new Tag("PageName");
        public static ITag TagURL = new Tag("URL");

        public static ITag TagUserName = new Tag("UserName");
        public static ITag TagUserURL = new Tag("UserURL");
        public static ITag TagDate = new Tag("Date");

        public static ITag TagPostPreview = new Tag("PagePreview");
        public static ITag TagCommentBody = new Tag("CommentBody");

        public static ITag TagChangePageType = new Tag("ChangeType");
        public static ITag TagCatName = new Tag("CategoryName");

    }
}