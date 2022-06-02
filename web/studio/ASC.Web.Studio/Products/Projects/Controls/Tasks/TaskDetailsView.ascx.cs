#region Import

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
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


#endregion

namespace ASC.Web.Projects.Controls.Tasks
{

    [AjaxNamespace("AjaxPro.TaskDetailsView")]
    public partial class TaskDetailsView : BaseUserControl
    {
        #region Members

        #endregion

        #region Property

        public Task Task { get; set; }

        #endregion

        #region Methods

        protected String RenderActionBlock(Task task)
        {
            var milestone = task.Milestone != 0 ? Global.EngineFactory.GetMilestoneEngine().GetByID(task.Milestone) : null;
            if (task.Milestone != 0 && milestone.Status == MilestoneStatus.Closed) return String.Empty;
            if (!ProjectSecurity.CanEdit(task)) return string.Empty;

            var innerHTML = new StringBuilder();

            innerHTML.AppendLine("<dl class='pm-flexible'>");
            innerHTML.AppendLine("<dt class='textBigDescribe'>&nbsp;</dt>");
            innerHTML.AppendLine("<dd>");

            switch (task.Status)
            {

                case TaskStatus.NotAccept:
                    innerHTML.AppendFormat("<a class='baseLinkButton' onclick='ASC.Projects.TaskDetailsView.execTaskChangeStatus({1}, {2})' href='javascript:void(0)'>{0}</a>", TaskResource.TaskOpen, Task.ID, (int)TaskStatus.Open);
                    innerHTML.Append("<span class='button-splitter'>&nbsp;</span>");
                    innerHTML.AppendFormat("<a class='baseLinkButton' onclick='ASC.Projects.TaskDetailsView.execTaskChangeStatus({1}, {2})' href='javascript:void(0)'>{0}</a>", ProjectsCommonResource.Complete, Task.ID, (int)TaskStatus.Closed);
                    break;
                case TaskStatus.Open:
                    //innerHTML.AppendFormat("<a class='baseLinkButton' onclick='ASC.Projects.TaskDetailsView.execTaskChangeStatus({1}, {2})' href='javascript:void(0)'>{0}</a>", TaskResource.TaskStopExecute, Task.ID, (int)TaskStatus.NotAccept);
                    //innerHTML.Append("<span class='button-splitter'>&nbsp;</span>");
                    innerHTML.AppendFormat("<a class='baseLinkButton' onclick='ASC.Projects.TaskDetailsView.execTaskChangeStatus({1}, {2})' href='javascript:void(0)'>{0}</a>", ProjectsCommonResource.Complete, Task.ID, (int)TaskStatus.Closed);
                    break;
                case TaskStatus.Closed:
                    innerHTML.AppendFormat("<a class='baseLinkButton' onclick='ASC.Projects.TaskDetailsView.execTaskChangeStatus({1}, {2})' href='javascript:void(0)'>{0}</a>", TaskResource.TaskReopen, Task.ID, (int)TaskStatus.Open);
                    break;
                case TaskStatus.Disable:
                    innerHTML.AppendFormat("<a class='baseLinkButton' onclick='ASC.Projects.TaskDetailsView.execTaskChangeStatus({1}, {2})' href='javascript:void(0)'>{0}</a>", TaskResource.TaskReopen, Task.ID, (int)TaskStatus.NotAccept);
                    break;
                case TaskStatus.Unclassified:
                    innerHTML.AppendFormat("<a class='baseLinkButton' onclick='ASC.Projects.TaskDetailsView.execTaskBeResponsible({0});' href='javascript:void(0)'>{1}</a>", Task.ID, TaskResource.Accept);
                    break;
                default:
                    return String.Empty;
            }

            innerHTML.AppendLine("</dd>");
            innerHTML.AppendLine("</dl>");

            return innerHTML.ToString();

        }

        public string GetTimeTrackingImagePath()
        {
            if (Global.EngineFactory.GetTimeTrackingEngine().HasTime(Task.ID))
                return WebImageSupplier.GetAbsoluteWebPath("clock_active.png", ProductEntryPoint.ID);
            else
                return WebImageSupplier.GetAbsoluteWebPath("clock_noactive.png", ProductEntryPoint.ID);
        }

        public string GetTimeTrackingTimerImagePath()
        {
            return WebImageSupplier.GetAbsoluteWebPath("start-track.png", ProductEntryPoint.ID);
        }

        public string GetTimeTrackingAction()
        {
            return ProjectSecurity.CanEdit(Task) && Task.Status != TaskStatus.Unclassified ?
                string.Format("ASC.Projects.TimeSpendActionPage.ViewTimeLogPanel({0},{1})", Task.Project.ID, Task.ID) :
                string.Empty;
        }

        public string GetTimeTrackingTimerAction()
        {
            return ProjectSecurity.CanEdit(Task) && Task.Status != TaskStatus.Unclassified && Task.Responsible == SecurityContext.CurrentAccount.ID ?
                string.Format("ASC.Projects.TimeSpendActionPage.showTimer('timetracking.aspx?prjID={0}&ID={1}&action=timer');", Task.Project.ID, Task.ID) :
                string.Empty;   
        }

        #region AjaxPro

        [AjaxMethod]
        public int SaveOrUpdateTask(int projectID, int taskID, int milestoneID, String title, String description, Guid responsible)
        {

            Task = taskID != -1 ? Global.EngineFactory.GetTaskEngine().GetByID(taskID) : new Task();

            Task.Milestone = milestoneID;
            Task.Title = title;
            Task.Description = description;
            Task.Responsible = responsible;
            Task.Project = Global.EngineFactory.GetProjectEngine().GetByID(projectID);
            if (responsible.Equals(Guid.Empty)) Task.Status = TaskStatus.Unclassified;

            Global.EngineFactory.GetTaskEngine().SaveOrUpdate(Task,null,false);


            if (taskID == -1) return Task.ID;

            return -1;


        }

        public String RenderPriorityBlock(TaskPriority priority)
        {


            var innerHTML = new StringBuilder();


            String imagePath;
            String priorityTitle;

            switch (priority)
            {

                case TaskPriority.High:
                    priorityTitle = ResourceEnumConverter.ConvertToString(TaskPriority.High);
                    imagePath = WebImageSupplier.GetAbsoluteWebPath("prior_hi.png",
                                                                                               ProductEntryPoint.ID);

                    innerHTML.AppendFormat(@"
                       <img align='absmiddle' src='{0}' style='border: 0px;' />
                       <span style='padding-left:3px'>{1}</span>",
                       imagePath, priorityTitle);

                    break;

                case TaskPriority.Normal:
                    priorityTitle = ResourceEnumConverter.ConvertToString(TaskPriority.Normal);

                    innerHTML.AppendFormat(@"<span>{0}</span>", priorityTitle);

                    break;
                case TaskPriority.Low:
                    priorityTitle = ResourceEnumConverter.ConvertToString(TaskPriority.Low);
                    imagePath = WebImageSupplier.GetAbsoluteWebPath("prior_lo.png",
                                                                           ProductEntryPoint.ID);

                    innerHTML.AppendFormat(@"
                       <img align='absmiddle' src='{0}' style='border: 0px;' />
                       <span style='padding-left:3px'>{1}</span>",
                       imagePath, priorityTitle);

                    break;
            }

            return innerHTML.ToString();

        }

        [AjaxMethod]
        public String ChangeSubscribe(int taskID)
        {
            ProjectSecurity.DemandAuthentication();

            Task task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);

            String objectID = String.Format("{0}_{1}", task.UniqID, task.Project.ID);

            var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
              NotifyConstants.Event_NewCommentForTask,
              NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                  SecurityContext.CurrentAccount.ID.ToString())
              ));

            bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, objectID, true) == 0));

            if (subscribed)
            {
                NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForTask, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString()));

                return ProjectsCommonResource.SubscribeOnNewComment;


            }
            else
            {
                NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForTask, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString()));

                return ProjectsCommonResource.UnSubscribeOnNewComment;
            }
        }

        [AjaxMethod]
        public void ExecChangeStatus(int taskID, int newStatus)
        {

            Global.EngineFactory.GetTaskEngine().ChangeStatus(Global.EngineFactory.GetTaskEngine().GetByID(taskID), (TaskStatus)Enum.ToObject(typeof(TaskStatus), newStatus));

        }

        [AjaxMethod]
        public void ExecBeResponsible(int taskID)
        {

            Task target = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
            target.Responsible = SecurityContext.CurrentAccount.ID;
            target.Status = TaskStatus.Open;
            Global.EngineFactory.GetTaskEngine().SaveOrUpdate(target,null,false);

        }

        [AjaxMethod]
        public void ExecDeleteTask(int taskID)
        {
            Global.EngineFactory.GetTaskEngine().Delete(Global.EngineFactory.GetTaskEngine().GetByID(taskID));
        }

        [AjaxMethod]
        public void DeleteFile(int prjID, int id, int version)
        {
            var file = FileEngine2.GetFile(id, version);
            if (!FileEngine2.CanDelete(file, prjID))
                throw ProjectSecurity.CreateSecurityException();

            FileEngine2.RemoveFile(id);
        }


        #region Comment Block Managment

        [AjaxMethod]
        public AjaxResponse AddComment(string parrentCommentID, string taskID, string text, string pid)
        {
            if (!ProjectSecurity.CanCreateComment())
                throw ProjectSecurity.CreateSecurityException();

            AjaxResponse resp = new AjaxResponse();

            Comment comment = new Comment
            {
                Content = text,
                TargetUniqID = ProjectEntity.BuildUniqId<Task>(Convert.ToInt32(taskID))
            };

            resp.rs1 = parrentCommentID;

            if (!String.IsNullOrEmpty(parrentCommentID))
                comment.Parent = new Guid(parrentCommentID);

            Task = Global.EngineFactory.GetTaskEngine().GetByID(Convert.ToInt32(taskID));
            Global.EngineFactory.GetTaskEngine().SaveOrUpdateTaskComment(Task, comment);
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
                IsEditPermissions = ProjectSecurity.CanEditComment(Task != null ? Task.Project : null, comment),
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

            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var targetProject = Global.EngineFactory.GetTaskEngine().GetByID(targetID).Project;
            if (!ProjectSecurity.CanEditComment(targetProject, comment))
                throw ProjectSecurity.CreateSecurityException();

            comment.Content = text;

            Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);

            resp.rs2 = text + ASC.Web.Controls.CodeHighlighter.GetJavaScriptLiveHighlight(true);

            return resp;

        }

        [AjaxMethod]
        public string RemoveComment(string commentID, string pid)
        {

            Comment comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));

            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var targetProject = Global.EngineFactory.GetTaskEngine().GetByID(targetID).Project;
            if (!ProjectSecurity.CanEditComment(targetProject, comment))
                throw ProjectSecurity.CreateSecurityException();
            
            comment.Inactive = true;

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
            info.Inactive = comment.Inactive;
            info.CommentBody = comment.Content;
            info.UserFullName = DisplayUserSettings.GetFullUserName(Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo);

            info.UserAvatar = Global.GetHTMLUserAvatar(comment.CreateBy);

            var defComment = new CommentsList();
            ConfigureComments(defComment, null);

            if (!isPreview)
            {
                info.IsEditPermissions = ProjectSecurity.CanEditComment(Task.Project, comment);
                info.IsResponsePermissions = ProjectSecurity.CanCreateComment();

                var when = Global.EngineFactory.GetParticipantEngine().WhenReaded(Page.Participant.ID, Task.UniqID);
                info.IsRead = when.HasValue && when.Value > comment.CreateOn;
            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                    defComment,
                    info,
                    comment.Parent == Guid.Empty,
                    false);

        }

        private static void ConfigureComments(CommentsList commentList, Task taskToUpdate)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.IsShowAddCommentBtn = ProjectSecurity.CanCreateComment();

            commentList.CommentsCountTitle = taskToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(taskToUpdate).ToString() : "";

            commentList.ObjectID = taskToUpdate != null ? taskToUpdate.ID.ToString() : "";
            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "AjaxPro.TaskDetailsView.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "AjaxPro.TaskDetailsView.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "AjaxPro.TaskDetailsView.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "AjaxPro.TaskDetailsView.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "AjaxPro.TaskDetailsView.UpdateComment";
            commentList.FckDomainName = "projects_comments";

            commentList.TotalCount = taskToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(taskToUpdate) : 0;

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

            ConfigureComments(commentList, Task);

        }

        private IList<CommentInfo> BuilderCommentInfo()
        {
            var container = new List<CommentInfo>();
            var comments = Global.EngineFactory.GetCommentEngine().GetComments(Task);
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
            var when = Global.EngineFactory.GetParticipantEngine().WhenReaded(Page.Participant.ID, Task.UniqID);
            var commentInfo = new CommentInfo()
            {
                TimeStampStr = parent.CreateOn.Ago(),
                Inactive = parent.Inactive,
                IsRead = when.HasValue && parent.CreateOn < when.Value,
                IsResponsePermissions = ProjectSecurity.CanCreateComment(),
                IsEditPermissions = ProjectSecurity.CanEditComment(Task.Project, parent),
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

        protected String RenderStatus(TaskStatus taskStatus)
        {

            String imgSrc;

            String imgTitle;

            switch (taskStatus)
            {

                case TaskStatus.Open:

                    imgSrc = WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID);
                    imgTitle = ResourceEnumConverter.ConvertToString(TaskStatus.Open);

                    break;

                case TaskStatus.Closed:
                    imgSrc = WebImageSupplier.GetAbsoluteWebPath("status_closed.png", ProductEntryPoint.ID);
                    imgTitle = ResourceEnumConverter.ConvertToString(TaskStatus.Closed);
                    break;


                case TaskStatus.NotAccept:
                    imgSrc = WebImageSupplier.GetAbsoluteWebPath("status_wait.png", ProductEntryPoint.ID);
                    imgTitle = ResourceEnumConverter.ConvertToString(TaskStatus.NotAccept);
                    break;


                case TaskStatus.Disable:
                    imgSrc = WebImageSupplier.GetAbsoluteWebPath("status_trash.png", ProductEntryPoint.ID);
                    imgTitle = ResourceEnumConverter.ConvertToString(TaskStatus.Disable);
                    break;

                case TaskStatus.Unclassified:
                    imgSrc = WebImageSupplier.GetAbsoluteWebPath("status_unclass.png", ProductEntryPoint.ID);
                    imgTitle = ResourceEnumConverter.ConvertToString(TaskStatus.Unclassified);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("TaskStatus");
            }

            return String.Format(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />
                                    <span style='margin-left: 10px;'>
                                      {1}
                                    </span>
                                 ", imgSrc, imgTitle);

        }

        protected string GetTaskResponsible()
        {
            if (!Task.Responsible.Equals(Guid.Empty))
                return StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(Task.Responsible), ProductEntryPoint.ID);
            return TaskResource.WithoutResponsible;
        }

        protected void InitControls()
        {
            TaskActionView cntrlTaskActionView = (TaskActionView)LoadControl(PathProvider.GetControlVirtualPath("TaskActionView.ascx"));

            cntrlTaskActionView.ProjectFat = RequestContext.GetCurrentProjectFat();
            cntrlTaskActionView.Target = Task;
            phAddTaskPanel.Controls.Add(cntrlTaskActionView);
            
            InitCommentBlock();
        }

        protected void WriteClientScripts()
        {

            if (Page.ClientScript.IsClientScriptBlockRegistered(typeof(TaskDetailsView), "7883251B-1149-4617-8490-45AF33F2631A"))
                return;

            string deadline = Task.Deadline != DateTime.MinValue ? Task.Deadline.ToString(DateTimeExtension.DateFormatPattern) : string.Empty;

            if (Task.Milestone != 0)
                Page.ClientScript.RegisterClientScriptBlock(typeof(TaskDetailsView), "E58A5C45-ACB2-4bf3-8A2C-83BA25C7018B", "CurrTask = " +
                                                            JavaScriptSerializer.Serialize(
                                                                new
                                                                {
                                                                    Title = HttpUtility.HtmlDecode(Task.Title),
                                                                    Task.ID,
                                                                    ResponsibleID = Task.Responsible,
                                                                    Task.Priority,
                                                                    Description = Task.Description,
                                                                    MilestoneID = Task.Milestone,
                                                                    Deadline = deadline
                                                                }) + "; ", true);

            else
                Page.ClientScript.RegisterClientScriptBlock(typeof(TaskDetailsView), "E58A5C45-ACB2-4bf3-8A2C-83BA25C7018B", "CurrTask = " +
                                                                          JavaScriptSerializer.Serialize(
                                                                              new
                                                                              {
                                                                                  Title = HttpUtility.HtmlDecode(Task.Title),
                                                                                  Task.ID,
                                                                                  ResponsibleID = Task.Responsible,
                                                                                  Task.Priority,
                                                                                  Description = Task.Description,
                                                                                  MilestoneID = 0,
                                                                                  Deadline = deadline
                                                                              }) + "; ", true);


        }

        protected double TaskHoursCount()
        {
            double count = 0;

            List<TimeSpend> list = Global.EngineFactory.GetTimeTrackingEngine().GetByTask(Task.ID);

            foreach (TimeSpend ts in list)
            {
                count += ts.Hours;
            }

            return Math.Round(count, 2);
        }

        public string GetAttachedImages()
        {
            if (!Global.ModuleManager.IsVisible(ModuleType.TMDocs) || !ProjectSecurity.CanReadFiles(Task.Project))
                return string.Empty;
            
            var html = new StringBuilder();

            var images = new List<ASC.Files.Core.File>();
            var anotherFiles = new List<ASC.Files.Core.File>();
            foreach (var file in FileEngine2.GetTaskFiles(Task))
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
            if (images.Count == 0 && anotherFiles.Count == 0) return string.Empty;

            html.Append("<dt class=\"textBigDescribe\"></dt><dd>");
            if (images.Count > 0)
            {
                html.Append("<div id='gallery' style='padding-top:15px;'>");
                html.AppendLine("<table width='100%' cellpadding='5'><tr>");
                int i = 1;
                int j = 1;

                foreach (var file in images)
                {
                    string thumbUrl = file.ThumbnailURL;
                    string fileUrl = file.FileUri;

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

            html.Append("<div>");
            if (anotherFiles.Count > 0)
            {
                html.Append("<div id='attached-files'>");
                html.Append("<table><tr><td valign=top>");
                html.AppendFormat("<img align='absmiddle' style='float:left' src='{0}' alt='{1}' title='{2}'/></td>", GetFilesImgURL(), ProjectsFileResource.Files, ProjectsFileResource.Files);
                html.Append("<td><div>");

                int i = anotherFiles.Count;
                int j = 1;

                foreach (var file in anotherFiles)
                {
                    var filePath = file.FileUri;
                    html.AppendFormat("<a class='linkDescribe' href='{1}' >{0}</a>", HtmlUtility.GetText(file.Title, 25), filePath);
                    if (j < i) html.AppendLine(", ");
                    j++;
                }

                html.Append("</div></td></tr></table></div>");
            }
            else
            {
                html.Append("&nbsp;");
            }

            html.Append("</div></dd>");

            return HtmlUtility.GetFull(html.ToString(), ProductEntryPoint.ID);
        }

        public string GetFilesImgURL()
        {
            return WebImageSupplier.GetAbsoluteWebPath("skrepka.gif", ProductEntryPoint.ID);
        }

        public string GetReportUri()
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.ViewType = 1;
            filter.ProjectIds.Add(Task.Project.ID);
            return string.Format("reports.aspx?action=generate&reportType=8&{0}", filter.ToUri());
        }

        public string GetTaskDeadline()
        {
            if (Task.Deadline == null || Task.Deadline == DateTime.MinValue)
                return string.Empty;

            if (Task.Status != TaskStatus.Closed)
            {
                int count = Task.Deadline.Date.Subtract(TenantUtil.DateTimeNow().Date).Days;
                //string days = GrammaticalHelper.ChooseNumeralCase(Math.Abs(count), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural);

                if (count > 0)
                    return string.Format("{0} {1}", Math.Abs(count), TaskResource.DaysLeft);

                if (count < 0)
                    return string.Format("<b class='pm-taskDeadlineLateInfoContainer'>{0} {1}</b>", TaskResource.TaskDeadlineLateInfo, Task.Deadline.ToString(DateTimeExtension.DateFormatPattern));

                return string.Format("<b class='pm-redText'>{0}</b>", ProjectsCommonResource.Today);
            }
            else
                return Task.Deadline.ToString(DateTimeExtension.DateFormatPattern);
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskDetailsView));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockView));

            InitControls();

            //WriteClientScripts();

            Global.EngineFactory.GetParticipantEngine().Read(Page.Participant.ID, Task.UniqID, TenantUtil.DateTimeNow());
        }

        #endregion

    }
}