#region Import

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Controls;
using ASC.Web.Controls.CommentInfoHelper;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

#endregion

namespace ASC.Web.Projects.Controls.Messages
{
    [AjaxNamespace("AjaxPro.MessageDetailsView")]
    public partial class MessageDetailsView : BaseUserControl
    {
        #region Members

        public Message Message { get; set; }
        public ProjectFat ProjectFat { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageActionView));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageDetailsView));
            InitCommentBlock();

            Global.EngineFactory.GetParticipantEngine().Read(Page.Participant.ID, Message.UniqID, TenantUtil.DateTimeNow());
        }

        #endregion

        #region Methods

        public string GetAvatarURL()
        {
            return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.GetBigPhotoURL();
        }
        public string GetFilesImgURL()
        {
            return WebImageSupplier.GetAbsoluteWebPath("skrepka.gif", ProductEntryPoint.ID);
        }
        public string GetUserProfileLink()
        {
            return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.RenderProfileLink(ProductEntryPoint.ID);
        }
        public string GetMessageDate()
        {
            return Message.CreateOn.ToShortString();
        }
        public string GetMessageText()
        {
            return HtmlUtility.GetFull(Message.Content, ProductEntryPoint.ID);
        }
        public string GetMessageTitle()
        {
            return Message.Title;
        }
        public int GetMessageID()
        {
            return Message.ID;
        }
        public int GetPrjID()
        {
            return Message.Project.ID;
        }
        public string GetMessageCommentsCount()
        {
            return Global.EngineFactory.GetCommentEngine().Count(Message).ToString();
        }
        public string GetActions()
        {
            if (!ProjectSecurity.CanEdit(Message)) return String.Empty;

            return new StringBuilder("<div style='text-align:right;float:right'>")
                .AppendFormat("<a class='linkAction' href='messages.aspx?prjID={0}&id={1}&action=edit' >{2}</a>", Message.Project.ID, Message.ID, MilestoneResource.Edit)
                .AppendLine("<span class='splitter'>|</span>")
                .AppendFormat("<a class='linkAction' href='javascript:void(0)' onclick='javascript:ASC.Projects.Messages.deleteMessage({0},0)'>{1}</a>", Message.ID, MessageResource.DeleteMessage)
                .AppendFormat("</div>")
                .ToString();
        }

        public int GetCountSubscribedPeople()
        {
            return NotifySource.Instance.GetSubscriptionProvider().GetRecipients(NotifyConstants.Event_NewCommentForMessage, String.Format("{0}_{1}", Message.UniqID, Message.Project.ID)).Length;
        }
        public string GetSubscribedPeople()
        {
            var reciipients = NotifySource.Instance.GetSubscriptionProvider().GetRecipients(NotifyConstants.Event_NewCommentForMessage, String.Format("{0}_{1}", Message.UniqID, Message.Project.ID));
            StringBuilder sb = new StringBuilder();
            foreach (var rec in reciipients)
            {
                sb.Append("<div class='pm-messageRecipient'>");
                Participant prt = Global.EngineFactory.GetParticipantEngine().GetByID(new Guid(rec.ID));
                sb.Append(prt.UserInfo.RenderProfileLink(ProductEntryPoint.ID));
                sb.Append("</div>");
            }
            return sb.ToString();
        }
        public string GetAttachedImages()
        {
            if (!Global.ModuleManager.IsVisible(ModuleType.TMDocs) || !ProjectSecurity.CanReadFiles(ProjectFat.Project))
                return string.Empty;
            
            var html = new StringBuilder();
            var images = new List<ASC.Files.Core.File>();
            var anotherFiles = new List<ASC.Files.Core.File>();
            foreach (var file in FileEngine2.GetMessageFiles(Message))
            {
                if (string.IsNullOrEmpty(file.ThumbnailURL))
                {
                    anotherFiles.Add(file);
                }
                else
                {
                    images.Add(file);
                }
            }

            if (images.Count > 0)
            {
                html.Append("<div id='gallery' style='padding-top:15px;'>");
                html.AppendLine("<table width='100%' cellpadding='5'><tr>");
                int i = 1;
                int j = 1;
                foreach (var file in images)
                {
                    var thumbUrl = file.ThumbnailURL;
                    var fileUrl = file.FileUri;
                    html.AppendLine("<td width='25%' align='center'>");
                    html.AppendFormat("<img style='border:none;cursor:pointer;' src='{0}' title='{1}' _zoom='{2}' />", thumbUrl, file.Title, fileUrl);
                    html.AppendFormat("<br/><a class='linkDescribe' href='{1}' title='{2}' >{0}</a>", HtmlUtility.GetText(file.Title, 20), fileUrl, file.Title);
                    html.AppendLine("</td>");
                    j++;
                    if (i % 4 == 0) { html.AppendLine("</tr><tr>"); j = 1; }
                    i++;
                }

                if (j == 2) html.AppendLine("<td width='25%' align='center'></td><td width='25%' align='center'></td><td width='25%' align='center'></td>");
                if (j == 3) html.AppendLine("<td width='25%' align='center'><td width='25%' align='center'></td>");
                if (j == 4) html.AppendLine("<td width='25%' align='center'>");

                html.AppendLine("</tr></table></div>");
            }

            if (anotherFiles.Count > 0)
            {
                html.Append("<br /><div><div id='attached-files'><table><tr><td valign=top>");
                html.AppendFormat("<img align='absmiddle' style='float:left' src='{0}' alt='{1}' title='{2}'/></td>", GetFilesImgURL(), ProjectsFileResource.Files, ProjectsFileResource.Files);
                html.Append("<td><div>");
                int i = anotherFiles.Count;
                int j = 1;
                foreach (var file in anotherFiles)
                {
                    html.AppendFormat("<a class='linkDescribe' href='{1}' >{0}</a>", HtmlUtility.GetText(file.Title, 25), file.FileUri);
                    if (j < i) html.AppendLine(", ");
                    j++;
                }

                html.Append("</div></td></tr></table></div></div>");
            }
            return HtmlUtility.GetFull(html.ToString(), ProductEntryPoint.ID);
        }
        #endregion

        #region AjaxPro

        [AjaxMethod]
        public String ChangeSubscribe(int messageID)
        {
            ProjectSecurity.DemandAuthentication();

            Message message = Global.EngineFactory.GetMessageEngine().GetByID(messageID);

            String objectID = String.Format("{0}_{1}", message.UniqID, message.Project.ID);

            var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
              NotifyConstants.Event_NewCommentForMessage,
              NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                  SecurityContext.CurrentAccount.ID.ToString())
              ));

            bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, objectID, true) == 0));

            if (subscribed)
            {
                NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForMessage, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString()));

                return ProjectsCommonResource.SubscribeOnNewComment;


            }
            NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForMessage, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString()));

            return ProjectsCommonResource.UnSubscribeOnNewComment;
        }


        #region Comment Block Managment

        [AjaxMethod]
        public AjaxResponse AddComment(string parrentCommentID, string messageID, string text, string pid)
        {
            if(!ProjectSecurity.CanCreateComment())
                throw ProjectSecurity.CreateSecurityException();

            AjaxResponse resp = new AjaxResponse();

            Comment comment = new Comment
            {
                Content = text,
                TargetUniqID = ProjectEntity.BuildUniqId<Message>(Convert.ToInt32(messageID))
            };

            resp.rs1 = parrentCommentID;

            if (!String.IsNullOrEmpty(parrentCommentID))
                comment.Parent = new Guid(parrentCommentID);

            Message = Global.EngineFactory.GetMessageEngine().GetByID(Convert.ToInt32(messageID));
            Global.EngineFactory.GetMessageEngine().SaveMessageComment(Message, comment);
            resp.rs2 = GetHTMLComment(comment);
            return resp;
        }

        private string GetHTMLComment(Comment comment)
        {

            var oCommentInfo = new CommentInfo
            {
                TimeStamp = comment.CreateOn,
                TimeStampStr = comment.CreateOn.Ago(),
                CommentBody = comment.Content,
                CommentID = comment.ID.ToString(),
                UserID = comment.CreateBy,
                UserFullName = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo.DisplayUserName(),
                Inactive = comment.Inactive,
                IsEditPermissions = ProjectSecurity.CanEditComment(Message != null ? Message.Project : null, comment),
                IsResponsePermissions = ProjectSecurity.CanCreateComment(),
                IsRead = true,
                UserAvatar = Global.GetHTMLUserAvatar(comment.CreateBy),
                UserPost = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo.Title

            };

            if (commentList == null)
            {

                commentList = new CommentsList();

                ConfigureComments(commentList, null);

            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                    commentList,
                    oCommentInfo,
                    comment.Parent == Guid.Empty,
                    false);

        }

        [AjaxMethod]
        public AjaxResponse UpdateComment(string commentID, string text, string pid)
        {

            var resp = new AjaxResponse { rs1 = commentID };

            Comment comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));

            comment.Content = text;

            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var targetProject = Global.EngineFactory.GetMessageEngine().GetByID(targetID).Project;
            if(!ProjectSecurity.CanEditComment(targetProject, comment))
                throw ProjectSecurity.CreateSecurityException();

            Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);

            resp.rs2 = text + ASC.Web.Controls.CodeHighlighter.GetJavaScriptLiveHighlight(true);

            return resp;

        }

        [AjaxMethod]
        public string RemoveComment(string commentID, string pid)
        {

            Comment comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));

            comment.Inactive = true;

            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var targetProject = Global.EngineFactory.GetMessageEngine().GetByID(targetID).Project;
            if (!ProjectSecurity.CanEditComment(targetProject, comment))
                throw ProjectSecurity.CreateSecurityException();

            Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);

            return commentID;
        }

        [AjaxMethod]
        public string GetPreview(string text, string commentID)
        {
            ProjectSecurity.DemandAuthentication();

            return GetHTMLComment(text, commentID);
        }

        [AjaxMethod]
        public string LoadCommentBBCode(string commentID)
        {
            ProjectSecurity.DemandAuthentication();

            var finded = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));

            if (finded != null) return finded.Content;

            return String.Empty;
        }


        private string GetHTMLComment(Comment comment, bool isPreview)
        {

            CommentInfo info = new CommentInfo();

            info.CommentID = comment.ID.ToString();
            info.UserID = comment.CreateBy;
            info.TimeStamp = comment.CreateOn;
            info.TimeStampStr = comment.CreateOn.Ago();
            info.UserPost = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo.Title;

            info.IsRead = true;
            info.Inactive = comment.Inactive;
            info.CommentBody = comment.Content;
            info.UserFullName = DisplayUserSettings.GetFullUserName(Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo);

            info.UserAvatar = Global.GetHTMLUserAvatar(comment.CreateBy);


            var defComment = new CommentsList();
            ConfigureComments(defComment, null);

            if (!isPreview)
            {
                info.IsEditPermissions = ProjectSecurity.CanEditComment(Message.Project, comment);
                info.IsResponsePermissions = ProjectSecurity.CanCreateComment();

                var when = Global.EngineFactory.GetParticipantEngine().WhenReaded(Page.Participant.ID, Message.UniqID);
                info.IsRead = when.HasValue && when.Value > comment.CreateOn;
            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                    defComment,
                    info,
                    comment.Parent == Guid.Empty,
                    false);

        }

        private static void ConfigureComments(CommentsList commentList, Message messageToUpdate)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.IsShowAddCommentBtn = ProjectSecurity.CanCreateComment();

            commentList.CommentsCountTitle = messageToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(messageToUpdate).ToString() : "";

            commentList.ObjectID = messageToUpdate != null ? messageToUpdate.ID.ToString() : "";
            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "AjaxPro.MessageDetailsView.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "AjaxPro.MessageDetailsView.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "AjaxPro.MessageDetailsView.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "AjaxPro.MessageDetailsView.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "AjaxPro.MessageDetailsView.UpdateComment";
            commentList.FckDomainName = "projects_comments";
            commentList.TotalCount = messageToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(messageToUpdate) : 0;

        }


        private string GetHTMLComment(string text, string commentID)
        {

            Comment comment;

            if (!String.IsNullOrEmpty(commentID))
            {

                comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
                comment.Content = text;
            }
            else
                comment = new Comment
                {
                    Content = text,
                    CreateOn = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                    CreateBy = SecurityContext.CurrentAccount.ID
                };


            return GetHTMLComment(comment, true);

        }

        #endregion

        #endregion

        #region Comment List Control Block

        private void InitCommentBlock()
        {

            commentList.Items = BuilderCommentInfo();

            ConfigureComments(commentList, Message);



        }

        private IList<CommentInfo> BuilderCommentInfo()
        {
            var container = new List<CommentInfo>();
            var comments = Global.EngineFactory.GetCommentEngine().GetComments(Message);
            comments.Sort((x, y) => DateTime.Compare(x.CreateOn, y.CreateOn));

            foreach (var comment in comments)
            {
                if (comment.Parent == Guid.Empty)
                {
                    container.Add(GetCommentInfo(comments, comment));
                }
            }

            return container;
        }

        private CommentInfo GetCommentInfo(List<Comment> allComments, Comment parent)
        {
            var commentInfo = new CommentInfo()
            {
                TimeStampStr = parent.CreateOn.Ago(),
                IsRead = true,
                Inactive = parent.Inactive,
                IsResponsePermissions = ProjectSecurity.CanCreateComment(),
                IsEditPermissions = ProjectSecurity.CanEditComment(Message.Project, parent),
                CommentID = parent.ID.ToString(),
                CommentBody = parent.Content,
                UserID = parent.CreateBy,
                UserFullName = Global.EngineFactory.GetParticipantEngine().GetByID(parent.CreateBy).UserInfo.DisplayUserName(),
                UserPost = Global.EngineFactory.GetParticipantEngine().GetByID(parent.CreateBy).UserInfo.Title,
                UserAvatar = Global.GetHTMLUserAvatar(parent.CreateBy),
                CommentList = new List<CommentInfo>(),
            };

            foreach (var comment in allComments)
            {
                if (comment.Parent == parent.ID) commentInfo.CommentList.Add(GetCommentInfo(allComments, comment));
            }
            return commentInfo;
        }

        #endregion
    }
}