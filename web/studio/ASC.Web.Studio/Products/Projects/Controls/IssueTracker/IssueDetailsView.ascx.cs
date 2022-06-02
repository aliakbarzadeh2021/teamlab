using System;
using System.Collections.Generic;
using System.Text;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Controls;
using ASC.Web.Controls.CommentInfoHelper;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Controls.IssueTracker
{
    [AjaxNamespace("AjaxPro.IssueDetailsView")]
    public partial class IssueDetailsView : BaseUserControl
    {
        public Issue Target
        {
            get;
            set;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(IssueDetailsView));
            InitCommentBlock();
        }

        protected String RenderStatus(IssueStatus issueStatus)
        {
            var innerHTML = new StringBuilder();
            var imgSrc = string.Empty;
            var imgTitle = string.Empty;

            switch (issueStatus)
            {
                case IssueStatus.Open:

                    imgSrc = WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID);
                    imgTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Open);

                    innerHTML.AppendFormat(@"
                       <img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />
                        <span style='margin-left: 10px;'>
                          {1}
                        </span>",
                       imgSrc, imgTitle);

                    break;

                case IssueStatus.Closed:
                    imgSrc = WebImageSupplier.GetAbsoluteWebPath("status_closed.png", ProductEntryPoint.ID);
                    imgTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Closed);

                    innerHTML.AppendFormat(@"
                       <img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />
                        <span style='margin-left: 10px;'>
                          {1}
                        </span>",
                       imgSrc, imgTitle);

                    break;


                case IssueStatus.Fixed:
                    imgTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Fixed);

                    innerHTML.AppendFormat(@"<span>{0}</span>", imgTitle);

                    break;


                case IssueStatus.New:
                    imgTitle = ResourceEnumConverter.ConvertToString(IssueStatus.New);

                    innerHTML.AppendFormat(@"<span>{0}</span>", imgTitle);

                    break;

                case IssueStatus.Rejected:
                    imgTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Rejected);

                    innerHTML.AppendFormat(@"<span>{0}</span>", imgTitle);

                    break;

                default:
                    throw new NotSupportedException("Status not supported.");
            }

            return innerHTML.ToString();
        }

        public String RenderPriorityBlock(IssuePriority priority)
        {
            var innerHTML = new StringBuilder();
            var imagePath = string.Empty;
            var priorityTitle = string.Empty;

            switch (priority)
            {
                case IssuePriority.High:
                    priorityTitle = ResourceEnumConverter.ConvertToString(IssuePriority.High);
                    imagePath = WebImageSupplier.GetAbsoluteWebPath("prior_hi.png", ProductEntryPoint.ID);

                    innerHTML.AppendFormat(@"
                       <img align='absmiddle' src='{0}' style='border: 0px;' />
                       <span style='padding-left:3px'>{1}</span>",
                       imagePath, priorityTitle);

                    break;

                case IssuePriority.Normal:
                    priorityTitle = ResourceEnumConverter.ConvertToString(IssuePriority.Normal);

                    innerHTML.AppendFormat(@"<span>{0}</span>", priorityTitle);

                    break;
                case IssuePriority.Low:
                    priorityTitle = ResourceEnumConverter.ConvertToString(TaskPriority.Low);
                    imagePath = WebImageSupplier.GetAbsoluteWebPath("prior_lo.png", ProductEntryPoint.ID);

                    innerHTML.AppendFormat(@"
                       <img align='absmiddle' src='{0}' style='border: 0px;' />
                       <span style='padding-left:3px'>{1}</span>",
                       imagePath, priorityTitle);

                    break;
                case IssuePriority.Immediate:
                    priorityTitle = ResourceEnumConverter.ConvertToString(IssuePriority.Immediate);

                    innerHTML.AppendFormat(@"<span>{0}</span>", priorityTitle);

                    break;
                case IssuePriority.Urgent:
                    priorityTitle = ResourceEnumConverter.ConvertToString(IssuePriority.Urgent);

                    innerHTML.AppendFormat(@"<span>{0}</span>", priorityTitle);

                    break;
            }

            return innerHTML.ToString();
        }

        private void InitCommentBlock()
        {
            commentList.Items = BuilderCommentInfo();
            ConfigureComments(commentList, Target);
        }

        private IList<CommentInfo> BuilderCommentInfo()
        {
            var container = new List<CommentInfo>();
            var comments = Global.EngineFactory.GetCommentEngine().GetComments(Target);
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

        private static void ConfigureComments(CommentsList commentList, Issue issueToUpdate)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.IsShowAddCommentBtn = ProjectSecurity.CanCreateComment();

            commentList.CommentsCountTitle = issueToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(issueToUpdate).ToString() : "";

            commentList.ObjectID = issueToUpdate != null ? issueToUpdate.ID.ToString() : "";
            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "AjaxPro.IssueDetailsView.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "AjaxPro.IssueDetailsView.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "AjaxPro.IssueDetailsView.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "AjaxPro.IssueDetailsView.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "AjaxPro.IssueDetailsView.UpdateComment";
            commentList.FckDomainName = "projects_comments";

            commentList.TotalCount = issueToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(issueToUpdate) : 0;

        }

        private CommentInfo GetCommentInfo(List<Comment> allComments, Comment parent)
        {
            var when = Global.EngineFactory.GetParticipantEngine().WhenReaded(Page.Participant.ID, Target.UniqID);
            var commentInfo = new CommentInfo()
            {
                TimeStampStr = parent.CreateOn.Ago(),
                Inactive = parent.Inactive,
                IsRead = when.HasValue && parent.CreateOn < when.Value,
                IsResponsePermissions = ProjectSecurity.CanCreateComment(),
                IsEditPermissions = ProjectSecurity.CanEditComment(Target.Project, parent),
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

        [AjaxMethod]
        public AjaxResponse AddComment(string parrentCommentID, string issueID, string text, string pid)
        {
            ProjectSecurity.DemandAuthentication();

            var resp = new AjaxResponse();
            var comment = new Comment
            {
                Content = text,
                TargetUniqID = ProjectEntity.BuildUniqId<Issue>(Convert.ToInt32(issueID))
            };

            resp.rs1 = parrentCommentID;

            if (!String.IsNullOrEmpty(parrentCommentID)) comment.Parent = new Guid(parrentCommentID);

            Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);

            resp.rs2 = GetHTMLComment(comment);

            Target = Global.EngineFactory.GetIssueEngine().GetIssue(Convert.ToInt32(issueID));


            foreach (var prt in Global.EngineFactory.GetProjectEngine().GetTeam(Target.ProjectID))
            {
                if (prt.ID == SecurityContext.CurrentAccount.ID)
                {
                    var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
                                    NotifyConstants.Event_NewCommentForIssue,
                                    NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                                    SecurityContext.CurrentAccount.ID.ToString())
                                    ));
                    bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, Target.NotifyId, true) == 0));
                    if (!subscribed)
                    {
                        NotifySource.Instance.GetSubscriptionProvider().Subscribe(
                                    NotifyConstants.Event_NewCommentForIssue,
                                    Target.NotifyId,
                                    NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));
                    }
                }
            }

            NotifyClient.Instance.SendNewComment(Target, text);
            TimeLinePublisher.Comment(Target, ProjectsCommonResource.ActionText_Add);

            return resp;
        }

        [AjaxMethod]
        public string GetPreview(string text, string commentID)
        {
            ProjectSecurity.DemandAuthentication();

            return GetHTMLComment(text, commentID);
        }

        [AjaxMethod]
        public string RemoveComment(string commentID, string pid)
        {
            ProjectSecurity.DemandAuthentication();

            var comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
            comment.Inactive = true;
            Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);
            return commentID;
        }

        [AjaxMethod]
        public AjaxResponse UpdateComment(string commentID, string text, string pid)
        {
            ProjectSecurity.DemandAuthentication();

            var resp = new AjaxResponse { rs1 = commentID };
            var comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
            comment.Content = text;
            Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);
            resp.rs2 = text + CodeHighlighter.GetJavaScriptLiveHighlight(true);
            return resp;

        }

        [AjaxMethod]
        public string LoadCommentBBCode(string commentID)
        {
            ProjectSecurity.DemandAuthentication();

            var finded = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
            if (finded != null) return finded.Content;
            return String.Empty;
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
                IsEditPermissions = ProjectSecurity.CanEditComment(Target.Project, comment),
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

            return CommentsHelper.GetOneCommentHtmlWithContainer(commentList, oCommentInfo, comment.Parent == Guid.Empty, false);
        }

        private string GetHTMLComment(string text, string commentID)
        {
            Comment comment = null;
            if (!String.IsNullOrEmpty(commentID))
            {
                comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
                comment.Content = text;
            }
            else
            {
                comment = new Comment
                {
                    Content = text,
                    CreateOn = TenantUtil.DateTimeNow(),
                    CreateBy = SecurityContext.CurrentAccount.ID
                };
            }

            return GetHTMLComment(comment, true);
        }

        private string GetHTMLComment(Comment comment, bool isPreview)
        {
            var info = new CommentInfo();
            info.CommentID = comment.ID.ToString();
            info.UserID = comment.CreateBy;
            info.TimeStamp = comment.CreateOn;
            info.TimeStampStr = comment.CreateOn.Ago();
            info.UserPost = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo.Title;
            info.Inactive = comment.Inactive;
            info.CommentBody = comment.Content;
            info.UserFullName = DisplayUserSettings.GetFullUserName(Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo);
            info.UserAvatar = Global.GetHTMLUserAvatar(comment.CreateBy);

            var defComment = new CommentsList();
            ConfigureComments(defComment, null);

            if (!isPreview)
            {
                info.IsEditPermissions = ProjectSecurity.CanEditComment(Target != null ? Target.Project : null, comment);
                info.IsResponsePermissions = ProjectSecurity.CanCreateComment();
                var when = Global.EngineFactory.GetParticipantEngine().WhenReaded(Page.Participant.ID, Target.UniqID);
                info.IsRead = when.HasValue && when.Value > comment.CreateOn;
            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(defComment, info, comment.Parent == Guid.Empty, false);
        }
    }
}