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
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

#endregion

namespace ASC.Web.Projects.Controls.Milestones
{
    [AjaxNamespace("AjaxPro.MilestoneDetailsView")]
    public partial class MilestoneDetailsView : BaseUserControl
    {
        #region Members

        public string status { get; set; }
        public int countTasks { get; set; }

        public ProjectFat ProjectFat;
        public Milestone Milestone;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MilestoneDetailsView));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockView));


            status = Milestone.Status.ToString();

            TaskBlockView cntrlTaskBlockView = (TaskBlockView)LoadControl(PathProvider.GetControlVirtualPath("TaskBlockView.ascx"));

            cntrlTaskBlockView.ProjectFat = ProjectFat;
            cntrlTaskBlockView.BlockMilestone = Milestone;

            var list = Global.EngineFactory.GetTaskEngine().GetMilestoneTasks(Milestone.ID).FindAll(item => item.Status != TaskStatus.Disable);

            if (!ProjectSecurity.CanReadTasks(ProjectFat.Project))
                list = list.FindAll(item => ProjectSecurity.CanRead(item));

            countTasks = list.Count;

            cntrlTaskBlockView.Items = list;
            cntrlTaskBlockView.TaskHasTimeSpend = Global.EngineFactory.GetTimeTrackingEngine().HasTime(list.ConvertAll(t => t.ID).ToArray());
            cntrlTaskBlockView.ShowAllTasks = false;

            milestoneTasksContent.Controls.Add(cntrlTaskBlockView);

            if (Milestone.Status != MilestoneStatus.Closed)
            {
                EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
                emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", TaskResource.AddTask, "javascript:ASC.Projects.TaskActionPage.init(-1," + Milestone.ID + ", null); ASC.Projects.TaskActionPage.show();");
                emptyScreenControl.HeaderDescribe = TaskResource.EmptyScreenHeaderDescribe;
                taskEmptyContent.Controls.Add(emptyScreenControl);
            }

            TaskActionView cntrlTaskActionView = (TaskActionView)LoadControl(PathProvider.GetControlVirtualPath("TaskActionView.ascx"));
            cntrlTaskActionView.ProjectFat = ProjectFat;
            phAddTaskPanel.Controls.Add(cntrlTaskActionView);

            MoveTaskView cntrlMoveTaskView = (MoveTaskView)LoadControl(PathProvider.GetControlVirtualPath("MoveTaskView.ascx"));
            cntrlMoveTaskView.ProjectFat = ProjectFat;
            phMoveTaskPanel.Controls.Add(cntrlMoveTaskView);

            InitCommentBlock();
            Global.EngineFactory.GetParticipantEngine().Read(Page.Participant.ID, Milestone.UniqID, TenantUtil.DateTimeNow());
        }

        #endregion

        #region Methods


        public String GetMilestoneName()
        {
            return Milestone.Title;
        }
        public String GetTotalTasksCount()
        {

            var count = Global.EngineFactory.GetTaskEngine().GetTaskCount(Milestone.ID, TaskStatus.NotAccept, TaskStatus.Open, TaskStatus.Unclassified, TaskStatus.Closed);

            return count.ToString();
        }
        public String GetActiveTasksCount()
        {

            return Global.EngineFactory.GetTaskEngine().GetTaskCount(Milestone.ID, TaskStatus.NotAccept, TaskStatus.Open).ToString();

        }
        public String GetDeadLineDate()
        {
            DateTime deadline = Milestone.DeadLine;

            string color = "";
            if (Milestone.Status == MilestoneStatus.Closed)
                color = "pm-greenText";
            else if (Milestone.Status == MilestoneStatus.Open)
            {
                color = "pm-blueText";
                if (Milestone.DeadLine.AddDays(1) < ASC.Core.Tenants.TenantUtil.DateTimeNow())
                    color = "pm-redText";
            }

            return string.Format("<dd class='headerBaseMedium {0}'>{1}</dd>", color, deadline.ToString(DateTimeExtension.DateFormatPattern));
        }
        public String GetDaysBeforeDeadLine()
        {
            DateTime deadline = Milestone.DeadLine;
            int count = deadline.Subtract(ASC.Core.Tenants.TenantUtil.DateTimeNow()).Days;

            if (deadline.Date == ASC.Core.Tenants.TenantUtil.DateTimeNow().Date)
                return ProjectsCommonResource.Today;
            if (deadline < ASC.Core.Tenants.TenantUtil.DateTimeNow())
            {
                return Math.Abs(count) + " " +
                    string.Format(MilestoneResource.DaysAfterDeadLine,
                    GrammaticalHelper.ChooseNumeralCase(Math.Abs(count), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural));
            }

            return count + 1 + " " +
                string.Format(MilestoneResource.DaysBeforeDeadLine,
                    GrammaticalHelper.ChooseNumeralCase(count + 1, GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural));
        }
        public String GetMilestoneStatus()
        {
            StringBuilder innerHTML = new StringBuilder();

            MilestoneStatus status = Milestone.Status;
            DateTime deadline = Milestone.DeadLine;
            innerHTML.AppendLine(RenderStatus(status, deadline));
            if (status != MilestoneStatus.Disable && status != MilestoneStatus.Closed)
            {
                innerHTML.AppendLine(", " + GetDaysBeforeDeadLine());
            }
            return innerHTML.ToString();

        }
        public String GrammaticalHelperTotalTasksCount()
        {
            string count = GetTotalTasksCount();
            return count + " " + GrammaticalHelper.ChooseNumeralCase(Convert.ToInt32(count), GrammaticalResource.TaskNominative, GrammaticalResource.TaskGenitiveSingular, GrammaticalResource.TaskGenitivePlural);
        }
        public String GrammaticalHelperActiveTasksCount()
        {
            string count = GetActiveTasksCount();
            return count + " " + GrammaticalHelper.ChooseNumeralCase(Convert.ToInt32(count), GrammaticalResource.TaskNotDoneNominative, GrammaticalResource.TaskNotDoneGenitiveSingular, GrammaticalResource.TaskNotDoneGenitivePlural);
        }
        protected String RenderStatus(MilestoneStatus milestoneStatus, DateTime deadline)
        {

            StringBuilder innerHTML = new StringBuilder();

            switch (milestoneStatus)
            {

                case MilestoneStatus.Open:
                    if (deadline.AddDays(1) < ASC.Core.Tenants.TenantUtil.DateTimeNow())
                    {
                        innerHTML.AppendFormat(@"<img align='absmiddle' src='{0}' alt='{1}' title='{1}' style='border:0px;margin-right:5px;' />",
                                            WebImageSupplier.GetAbsoluteWebPath("milestone_status_late_24.png", ProductEntryPoint.ID),
                                            ResourceEnumConverter.ConvertToString(MilestoneStatus.Late));
                        innerHTML.AppendLine(" " + ResourceEnumConverter.ConvertToString(MilestoneStatus.Late));
                    }
                    else
                    {
                        innerHTML.AppendFormat(@"<img align='absmiddle' src='{0}' alt='{1}' title='{1}' style='border:0px;margin-right:5px;' />",
                                                WebImageSupplier.GetAbsoluteWebPath("milestone_status_active_24.png", ProductEntryPoint.ID),
                                                ResourceEnumConverter.ConvertToString(MilestoneStatus.Open));
                        innerHTML.AppendLine(" " + ResourceEnumConverter.ConvertToString(MilestoneStatus.Open));
                    }
                    break;

                case MilestoneStatus.Closed:
                    innerHTML.AppendFormat(@"<img align='absmiddle' src='{0}'  alt='{1}' title='{1}' style='border:0px;margin-right:5px;' />",
                                            WebImageSupplier.GetAbsoluteWebPath("milestone_status_completed_24.png", ProductEntryPoint.ID),
                                            ResourceEnumConverter.ConvertToString(MilestoneStatus.Closed));
                    innerHTML.AppendLine(" " + ResourceEnumConverter.ConvertToString(MilestoneStatus.Closed));
                    break;

                default:
                    break;

            }

            return innerHTML.ToString().Trim();

        }
        protected String RenderAddTaskButton()
        {
            var sb = new StringBuilder();

            if (ProjectSecurity.CanCreateTask(ProjectFat.Project, false) && Milestone.Status != MilestoneStatus.Closed)
            {
                var milestoneID = Milestone.ID.ToString();
                var userID = "null";
                var onclick = string.Format("javascript:ASC.Projects.TaskActionPage.init(-1,{0},{1}); ASC.Projects.TaskActionPage.show()", milestoneID, userID);
                sb.AppendFormat("<a class='grayButton' href='javascript:void(0)' onclick='{0}'><div></div>{1}</a>", onclick, TaskResource.AddNewTask);
            }
            return sb.ToString();
        }
        public String GetActions()
        {
            StringBuilder innerHTML = new StringBuilder();
            MilestoneStatus status = Milestone.Status;

            if (ProjectSecurity.CanCreateComment() && status != MilestoneStatus.Closed)
            {
                innerHTML.AppendFormat("<a onclick='javascript:CommentsManagerObj.AddNewComment();' class='baseLinkButton promoAction' id='add_comment_btn0'>{0}</a><span class='splitter'></span>", ProjectsCommonResource.AddComment);
            }

            if (status != MilestoneStatus.Disable)
            {
                if (ProjectSecurity.CanEdit(Milestone))
                {

                    var tasks = Global.EngineFactory.GetTaskEngine().GetMilestoneTasks(Milestone.ID);

                    int allTasksComplete = 1;

                    foreach (Task task in tasks)
                    {
                        if (task.Status == TaskStatus.Open)
                            allTasksComplete = 0;
                    }
                    if (status != MilestoneStatus.Closed)
                        innerHTML.AppendFormat("<a href='javascript:void(0);' class='baseLinkButton' onclick='ASC.Projects.Milestones.finishMilestone({1});'>{0}</a>", MilestoneResource.FinishMilestone, allTasksComplete);
                    if (status == MilestoneStatus.Closed)
                        innerHTML.AppendFormat("<a href='javascript:void(0);' class='baseLinkButton' onclick='ASC.Projects.Milestones.resumeMilestone();'>{0}</a>", MilestoneResource.ResumeMilestone);

                }
            }
            else
            {
                innerHTML.AppendFormat("<a href='javascript:void(0);' onclick='ASC.Projects.Milestones.restoreFromTrash();'>{0}</a>", MilestoneResource.RestoreFromTrash);
            }

            if (innerHTML.ToString() != string.Empty)
                return string.Format("<div class='pm-headerPanel-splitter'><div class='pm-h-line'><!– –></div><div>{0}</div></div>", innerHTML.ToString());

            return string.Empty;

        }

        [AjaxMethod]
        public String AddNewMilestone(int projectID, int milestoneID, string title, string deadline, bool notifyManager, bool isKey, bool shift, bool moveOutWeekend)
        {

            Project project = Global.EngineFactory.GetProjectEngine().GetByID(projectID);
            DateTime deadLine = DateTime.Parse(deadline);

            var milestone = new Milestone();

            if (milestoneID != 0)
            {
                milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(milestoneID);

                if (shift)
                {
                    if (moveOutWeekend)
                    {
                        List<Milestone> milestones = Global.EngineFactory.GetMilestoneEngine().GetByProject(project.ID)
                            .FindAll(item => item.ID != milestoneID && item.Status != MilestoneStatus.Closed);
                        int count = deadLine.Subtract(milestone.DeadLine).Days;
                        DateTime milestoneDeadLine = milestone.DeadLine;
                        foreach (Milestone mil in milestones)
                        {
                            milestoneDeadLine = mil.DeadLine;
                            milestoneDeadLine = milestoneDeadLine.AddDays(count);

                            if (milestoneDeadLine.DayOfWeek == DayOfWeek.Sunday) milestoneDeadLine = milestoneDeadLine.AddDays(1);
                            if (milestoneDeadLine.DayOfWeek == DayOfWeek.Saturday) milestoneDeadLine = milestoneDeadLine.AddDays(2);

                            mil.DeadLine = milestoneDeadLine;
                            Global.EngineFactory.GetMilestoneEngine().SaveOrUpdate(mil);
                        }
                    }
                    else
                    {
                        List<Milestone> milestones = Global.EngineFactory.GetMilestoneEngine().GetByProject(project.ID)
                            .FindAll(item => item.ID != milestoneID && item.Status != MilestoneStatus.Closed);
                        int count = deadLine.Subtract(milestone.DeadLine).Days;
                        DateTime milestoneDeadLine = milestone.DeadLine;
                        foreach (Milestone mil in milestones)
                        {
                            milestoneDeadLine = mil.DeadLine;
                            milestoneDeadLine = milestoneDeadLine.AddDays(count);
                            mil.DeadLine = milestoneDeadLine;
                            Global.EngineFactory.GetMilestoneEngine().SaveOrUpdate(mil);
                        }
                    }

                }
            }

            milestone.Project = project;
            milestone.Title = title.Trim();
            milestone.DeadLine = deadLine;
            milestone.IsKey = isKey;
            milestone.Status = MilestoneStatus.Open;
            milestone.IsNotify = notifyManager;

            Global.EngineFactory.GetMilestoneEngine().SaveOrUpdate(milestone);
            return milestone.ID.ToString();

        }

        [AjaxMethod]
        public void FinishMilestone(int milestoneID)
        {
            Milestone milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(milestoneID);
            milestone.Status = MilestoneStatus.Closed;
            Global.EngineFactory.GetMilestoneEngine().SaveOrUpdate(milestone);
        }
        [AjaxMethod]
        public void ResumeMilestone(int milestoneID)
        {
            Milestone milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(milestoneID);
            milestone.Status = MilestoneStatus.Open;
            Global.EngineFactory.GetMilestoneEngine().SaveOrUpdate(milestone);
        }
        [AjaxMethod]
        public void DeleteMilestone(int milestoneID)
        {
            Milestone milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(milestoneID);
            //milestone.Status = MilestoneStatus.Disable;
            //Global.EngineFactory.GetMilestoneEngine().SaveOrUpdate(milestone);
            Global.EngineFactory.GetMilestoneEngine().Delete(milestone);

        }
        [AjaxMethod]
        public void RestoreFromTrash(int milestoneID)
        {
            Milestone milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(milestoneID);
            milestone.Status = MilestoneStatus.Open;
            Global.EngineFactory.GetMilestoneEngine().SaveOrUpdate(milestone);
        }
        [AjaxMethod]
        public String ChangeSubscribe(int milestoneID)
        {
            ProjectSecurity.DemandAuthentication();

            Milestone milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(milestoneID);

            String objectID = String.Format("{0}_{1}", milestone.UniqID, milestone.Project.ID);
            //NotifyConstants.Event_NewCommentForMilestone
            var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
              NotifyConstants.Event_NewCommentForMilestone,
              NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                  SecurityContext.CurrentAccount.ID.ToString())
              ));

            bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, objectID, true) == 0));

            if (subscribed)
            {
                NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForMilestone, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));

                return ProjectsCommonResource.SubscribeOnNewComment;


            }
            else
            {
                NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForMilestone, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));

                return ProjectsCommonResource.UnSubscribeOnNewComment;
            }
        }

        #endregion

        #region AjaxPro

        #region Comment Block Managment

        [AjaxMethod]
        public AjaxResponse AddComment(string parrentCommentID, string milestoneID, string text, string pid)
        {
            if (!ProjectSecurity.CanCreateComment())
                throw ProjectSecurity.CreateSecurityException();

            AjaxResponse resp = new AjaxResponse();

            Comment comment = new Comment
            {
                Content = text,
                TargetUniqID = ProjectEntity.BuildUniqId<Milestone>(Convert.ToInt32(milestoneID))
            };

            resp.rs1 = parrentCommentID;

            if (!String.IsNullOrEmpty(parrentCommentID))
                comment.Parent = new Guid(parrentCommentID);
            Milestone milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(Convert.ToInt32(milestoneID));
            Global.EngineFactory.GetMilestoneEngine().SaveMilestoneComment(milestone, comment);
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
                IsEditPermissions = CanEditComment(comment),
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
            var comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
            if (comment != null)
            {
                var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
                var targetProject = Global.EngineFactory.GetMilestoneEngine().GetByID(targetID).Project;
                if (!ProjectSecurity.CanEditComment(targetProject, comment))
                    throw ProjectSecurity.CreateSecurityException();

                comment.Content = text;
                Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);
                resp.rs2 = text + ASC.Web.Controls.CodeHighlighter.GetJavaScriptLiveHighlight(true);
            }
            return resp;
        }

        [AjaxMethod]
        public string RemoveComment(string commentID, string pid)
        {
            var comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
            if (comment != null)
            {
                var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
                var targetProject = Global.EngineFactory.GetMilestoneEngine().GetByID(targetID).Project;
                if (!ProjectSecurity.CanEditComment(targetProject, comment))
                    throw ProjectSecurity.CreateSecurityException();

                comment.Inactive = true;
                Global.EngineFactory.GetCommentEngine().SaveOrUpdate(comment);
            }
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
            return finded != null ? finded.Content : string.Empty;
        }


        private string GetHTMLComment(Comment comment, bool isPreview)
        {
            var info = new CommentInfo();

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

            return CommentsHelper.GetOneCommentHtmlWithContainer(defComment, info, comment.Parent == Guid.Empty, false);
        }

        private static void ConfigureComments(CommentsList commentList, Milestone messageToUpdate)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.IsShowAddCommentBtn = ProjectSecurity.CanCreateComment();

            commentList.CommentsCountTitle = messageToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(messageToUpdate).ToString() : "";

            commentList.ObjectID = messageToUpdate != null ? messageToUpdate.ID.ToString() : "";
            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "AjaxPro.MilestoneDetailsView.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "AjaxPro.MilestoneDetailsView.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "AjaxPro.MilestoneDetailsView.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "AjaxPro.MilestoneDetailsView.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "AjaxPro.MilestoneDetailsView.UpdateComment";
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
            ConfigureComments(commentList, Milestone);
        }

        public bool CanEditComment(Comment comment)
        {
            var result = comment.CreateBy == SecurityContext.CurrentAccount.ID || Global.IsAdmin;
            if (!result)
            {
                try
                {
                    var mid = int.Parse(comment.TargetUniqID.Split('_')[1]);
                    var m = Global.EngineFactory.GetMilestoneEngine().GetByID(mid);
                    result = m.Project.Responsible == SecurityContext.CurrentAccount.ID;
                }
                catch { }
            }
            return result;
        }

        private IList<CommentInfo> BuilderCommentInfo()
        {
            var container = new List<CommentInfo>();
            var milestone = Milestone;
            var comments = Global.EngineFactory.GetCommentEngine().GetComments(milestone);
            comments.Sort((x, y) => DateTime.Compare(x.CreateOn, y.CreateOn));

            foreach (var comment in comments)
                if (comment.Parent == Guid.Empty)
                    container.Add(GetCommentInfo(comments, comment));

            container.Sort((x, y) => DateTime.Compare(y.TimeStamp, x.TimeStamp));
            return container;
        }

        private CommentInfo GetCommentInfo(List<Comment> allComments, Comment parent)
        {
            var commentInfo = new CommentInfo()
            {
                IsRead = true,
                TimeStamp = parent.CreateOn,
                TimeStampStr = parent.CreateOn.Ago(),
                Inactive = parent.Inactive,
                IsResponsePermissions = ProjectSecurity.CanCreateComment(),
                IsEditPermissions = CanEditComment(parent),
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