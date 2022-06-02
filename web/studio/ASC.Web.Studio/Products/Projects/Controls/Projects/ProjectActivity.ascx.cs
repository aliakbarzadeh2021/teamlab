#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using ASC.Projects.Core.Domain;
using ASC.Core;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Core.Users;
using ASC.Web.Core.Users.Activity;
using ASC.Projects.Engine;
using ASC.Web.Projects.Resources;
using System.Text;
using ASC.Web.Studio.Utility;
using ASC.Web.Controls;
using ASC.Projects.Core.Domain.Reports;
using ASC.Core.Tenants;
#endregion

namespace ASC.Web.Projects.Controls.Projects
{
    public partial class ProjectActivity : BaseUserControl, IUserActivityControlLoader
    {
        #region Members

        public Guid userID { get; set; }
        public string HistoryRangeParams { get; set; }
        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (userID == Guid.Empty)
                userID = SecurityContext.CurrentAccount.ID;

            //load projects stats
            //
            var projects = Global.EngineFactory.GetProjectEngine().GetByParticipant(userID);
            var projectsvm = projects.ConvertAll(p => new ProjectVM(p));

            var project_user_activities = UserActivityManager.GetUserActivities(
                TenantProvider.CurrentTenantID,
                userID,
                ProductEntryPoint.ID,
                new[] { ProductEntryPoint.ID },
                UserActivityConstants.AllActionType,
                projects.Select(x => x.ID.ToString()),
                0, 0);

            var grouped = project_user_activities
                .GroupBy(act => act.ContainerID)
                .Select(q => new { ProjectID = Int32.Parse(q.Key), ActivityCount = q.Count() });

            var query = project_user_activities
                            .Select(act => act.Date)
                            .OrderBy(date => date);

            DateTime minADate = TenantUtil.DateTimeNow().Date;
            DateTime maxADate = TenantUtil.DateTimeNow().Date;
            if (query.Count() > 0)
            {
                minADate = query.First().Date;
                maxADate = query.Last().Date;
            }
            HistoryRangeParams = String.Format("startDate={0}&finishDate={1}&timeRange=8", minADate.Ticks, maxADate.Ticks);

            foreach (var proj in projectsvm)
            {
                var p_activity = grouped.SingleOrDefault(ga => ga.ProjectID == proj.Project.ID);
                if (p_activity != null) proj.ActivityCount = p_activity.ActivityCount;

                //project tasks stats
                //
                var p_tasks = Global.EngineFactory.GetTaskEngine().GetByProject(proj.ProjectId, null, userID);
                proj.OpenedTasksCount = p_tasks.Where(t => t.Status == TaskStatus.Open || t.Status == TaskStatus.NotAccept).Count();
                proj.ClosedTasksCount = p_tasks.Where(t => t.Status == TaskStatus.Closed).Count();
            }

            ProjectsRepeater.DataSource = projectsvm;
            ProjectsRepeater.DataBind();

            //load user activity
            //
            var activities = UserActivityManager.GetUserActivities(
                TenantProvider.CurrentTenantID,
                userID,
                ProductEntryPoint.ID,
                new[] { ProductEntryPoint.ID },
                UserActivityConstants.AllActionType,
                null,
                0, 10);

            LastActivityRepeater.DataSource = activities.ConvertAll(a => new ActivityVM(a));
            LastActivityRepeater.DataBind();
        }

        #endregion

        #region Methods
        public string GetOpenedTasksString(int count)
        {
            return GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.ActiveTaskNominative, GrammaticalResource.ActiveTaskGenitiveSingular, GrammaticalResource.ActiveTaskGenitivePlural);
        }

        public string GetClosedTasksString(int count)
        {
            return GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.ClosedTaskNominative, GrammaticalResource.ClosedTaskGenitiveSingular, GrammaticalResource.ClosedTaskGenitivePlural);
        }


        public string GetActivitiesString(int count)
        {
            return GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.ActionNominative, GrammaticalResource.ActionGenitiveSingular, GrammaticalResource.ActionGenitivePlural);
        }


        public string GetActivityReportUri()
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.UserId = userID;
            return string.Format("{0}?action=generate&reportType=5&{1}",
                VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "reports.aspx"),
                filter.ToUri());
        }

        public string GetTasksReportUri()
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.UserId = userID;
            filter.TaskStatuses.Add(TaskStatus.Open);
            filter.TaskStatuses.Add(TaskStatus.Closed);
            return string.Format("{0}?action=generate&reportType=10&{1}",
                VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "reports.aspx"),
                filter.ToUri());
        }

        #endregion

        #region IUserActivityControlLoader Members

        public Control LoadControl(Guid userID)
        {
            var cntrl = (ProjectActivity)LoadControl(PathProvider.GetControlVirtualPath("ProjectActivity.ascx"));
            cntrl.userID = userID;
            return cntrl;
        }

        #endregion

        #region VM
        public class ProjectVM
        {
            public ProjectVM(Project project)
            {
                Project = project;
            }

            public Project Project;
            public int ProjectId { get { return Project.ID; } }
            public string ProjectTitle { get { return Project.Title; } }

            public int OpenedTasksCount { get; set; }
            public int ClosedTasksCount { get; set; }
            public int ActivityCount { get; set; }
        }

        public class ActivityVM
        {
            public ActivityVM(UserActivity activity)
            {
                Activity = activity;
            }
            public UserActivity Activity;

            public string DateString { get { return Activity.Date.ToString(DateTimeExtension.DateFormatPattern); } }
            public string TimeString { get { return Activity.Date.ToString("HH:mm"); } }

            public string EntityPlate
            {
                get
                {
                    EntityType _timeLineType;
                    var AdditionalDataParts = Activity.AdditionalData.Split(new[] { '|' });
                    _timeLineType = (EntityType)Enum.Parse(typeof(EntityType), AdditionalDataParts[0]);
                    return Global.RenderEntityPlate(_timeLineType, true);
                }
            }
            public string EntityType
            {
                get
                {
                    EntityType _timeLineType;
                    var AdditionalDataParts = Activity.AdditionalData.Split(new[] { '|' });
                    _timeLineType = (EntityType)Enum.Parse(typeof(EntityType), AdditionalDataParts[0]);

                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("<a style='padding-right:10px' href='{0}'>{1}</a>",
                        Activity.URL,
                        ASC.Web.Controls.HtmlUtility.GetText(Activity.Title, 70));
 
                    sb.AppendFormat("<span>{0}</span>", Activity.ActionText.ToLower());

                    return sb.ToString();
                }
            }
            public string EntityParentContainers
            {
                get
                {
                    var innerHTML = new StringBuilder();
                    var activity = Activity;
                    var AdditionalDataParts = activity.AdditionalData.Split(new[] { '|' });
                    var timeLineType = (EntityType)Enum.Parse(typeof(EntityType), AdditionalDataParts[0]);

                    var parent = string.Empty;

                    if (timeLineType == ASC.Projects.Core.Domain.EntityType.Comment)
                    {
                        if (activity.URL.IndexOf("tasks") != -1)
                        {
                            if (AdditionalDataParts[1] != string.Empty)
                                parent = string.Format("<span class='pm-grayText'>{0}</span><a style='padding-left:10px' href='{1}'>{2}</a>",
                                    TaskResource.Task,
                                    activity.URL,
                                    HtmlUtility.GetText(AdditionalDataParts[1], 50));
                        }
                        if (activity.URL.IndexOf("milestones") != -1)
                        {
                            if (AdditionalDataParts[1] != string.Empty)
                                parent = string.Format("<span class='pm-grayText'>{0}</span><a style='padding-left:10px' href='{1}'>{2}</a>",
                                    MilestoneResource.Milestone,
                                    activity.URL,
                                    HtmlUtility.GetText(AdditionalDataParts[1], 50));
                        }
                        if (activity.URL.IndexOf("messages") != -1)
                        {
                            if (AdditionalDataParts[1] != string.Empty)
                                parent = string.Format("<span class='pm-grayText'>{0}</span><a style='padding-left:10px' href='{1}'>{2}</a>",
                                    MessageResource.Message,
                                    activity.URL,
                                    HtmlUtility.GetText(AdditionalDataParts[1], 50));
                        }
                    }
                    else if (timeLineType == ASC.Projects.Core.Domain.EntityType.Project)
                    {

                    }
                    else
                    {

                        parent = string.Format("<span class='pm-grayText'>{0}</span><a style='padding-left:10px' href='{3}?prjID={1}'>{2}</a>",
                            ProjectResource.Project, activity.ContainerID,
                            HtmlUtility.GetText(AdditionalDataParts[2], 50),
                            VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "projects.aspx"));

                        if (AdditionalDataParts[1] != string.Empty)
                            parent = string.Format("<span class='pm-grayText'>{0}</span><a style='padding-left:10px' href='{4}?prjID={1}&id={2}'>{3}</a>",
                                MilestoneResource.Milestone, activity.ContainerID, activity.ContentID,
                                HtmlUtility.GetText(AdditionalDataParts[1], 50),
                                VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "milestones.aspx"));
                    }
                    innerHTML.AppendLine(parent);

                    return innerHTML.ToString();
                }
            }
        }

        #endregion
    }

}