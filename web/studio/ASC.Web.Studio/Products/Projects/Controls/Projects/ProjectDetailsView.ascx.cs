#region Import

using System;
using System.Collections.Generic;
using System.Text;
using AjaxPro;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Common;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Controls.Milestones;
using ASC.Core;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Projects
{
    [AjaxNamespace("AjaxPro.ProjectDetailsView")]
    public partial class ProjectDetailsView : BaseUserControl
    {

        
        public ProjectFat ProjectFat { get; set; }
        public List<UserActivity> LastActivity;

        
        [AjaxMethod]
        public String CheckAsFollowing(int projectID)
        {
            ProjectSecurity.DemandAuthentication();
            
            var participant = ASC.Core.SecurityContext.CurrentAccount.ID;
            if (Global.EngineFactory.GetParticipantEngine().GetFollowingProjects(participant).Contains(projectID))
            {
                Global.EngineFactory.GetParticipantEngine().RemoveFromFollowingProjects(projectID, participant);
                return ProjectResource.FollowingProjects;
            }
            else
            {
                Global.EngineFactory.GetParticipantEngine().AddToFollowingProjects(projectID, participant);
                return ProjectResource.UnFollowingProjects;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockView));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ProjectDetailsView));

            InitMilestonesContent();

            EmployeeUserCard cntrl = new EmployeeUserCard();
            cntrl.EmployeeInfo = ProjectFat.Responsible;
            cntrl.EmployeeUrl = ProjectFat.Responsible.GetUserProfilePageURL();
            cntrl.EmployeeDepartmentUrl = CommonLinkUtility.GetUserDepartment(ProjectFat.Responsible.ID);
            user_content.Controls.Add(cntrl);

            TaskActionView cntrlTaskActionView = (TaskActionView)LoadControl(PathProvider.GetControlVirtualPath("TaskActionView.ascx"));
            cntrlTaskActionView.ProjectFat = ProjectFat;
            phAddTaskPanel.Controls.Add(cntrlTaskActionView);
            InitTimeLine();
        }

        public string GetProjectDescription()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class='pm-headerPanel-splitter'>");

            sb.Append(ProjectFat.Project.Description.HtmlEncode().Replace("\n", "<br/>"));

            sb.Append("</div>");

            return sb.ToString();
        }

        public void InitMilestonesContent()
        {
            var listMilestones = ProjectFat.GetMilestones(MilestoneStatus.Open);

            List<Milestone> lateMilestones = listMilestones.FindAll(item => item.DeadLine.Date < ASC.Core.Tenants.TenantUtil.DateTimeNow().Date);
            List<Milestone> comingMilestone = listMilestones.FindAll(item => item.DeadLine.Date >= ASC.Core.Tenants.TenantUtil.DateTimeNow().Date);

            lateMilestones.Sort((oX, oY) =>
            {
                return DateTime.Compare(oX.DeadLine, oY.DeadLine);
            });

            if (comingMilestone.Count > 2)
            {
                comingMilestone.RemoveRange(2, comingMilestone.Count - 2);
            }

            listMilestones = lateMilestones;
            foreach (Milestone milestone in comingMilestone)
                listMilestones.Add(milestone);

            if (listMilestones.Count > 0)
            {

                ListMilestonesView cntrlListMilestonesView = (ListMilestonesView)LoadControl(PathProvider.GetControlVirtualPath("ListMilestonesView.ascx"));
                cntrlListMilestonesView.status = "Active";
                cntrlListMilestonesView.ProjectFat = ProjectFat;
                cntrlListMilestonesView.Milestones = listMilestones;
                content.Controls.Add(cntrlListMilestonesView);

            }

        }
        public string GetGrammaticalHelperCountTasks()
        {
            int count = Global.EngineFactory.GetProjectEngine().GetTaskCount(ProjectFat.Project.ID, TaskStatus.Closed, TaskStatus.NotAccept, TaskStatus.Open);
            return count + " " + GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.TaskNominative, GrammaticalResource.TaskGenitiveSingular, GrammaticalResource.TaskGenitivePlural);
        }
        public string GetGrammaticalHelperCountActiveTasks()
        {
            int count = Global.EngineFactory.GetProjectEngine().GetTaskCount(ProjectFat.Project.ID, TaskStatus.NotAccept, TaskStatus.Open);
            return count + " " + GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.TaskNotDoneNominative, GrammaticalResource.TaskNotDoneGenitiveSingular, GrammaticalResource.TaskNotDoneGenitivePlural);
        }
        public string GetGrammaticalHelperParticipantCount()
        {
            int count = ProjectFat.GetActiveTeam().Count;
            return count + " " + GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.ParticipantNominative, GrammaticalResource.ParticipantGenitiveSingular, GrammaticalResource.ParticipantGenitivePlural);
        }
        public int GetActiveMilestonesCount()
        {
            return ProjectFat.GetMilestones(MilestoneStatus.Open).Count;
        }
        public string MilestonesStyleView()
        {
            if (GetActiveMilestonesCount() == 0)
                return "display:none";
            else return "display:block";
        }

        public void InitTimeLine()
        {
            LastActivity = UserActivityManager.GetUserActivities(
                TenantProvider.CurrentTenantID,
                null,
                ProductEntryPoint.ID,
                new[] { ProductEntryPoint.ID },
                UserActivityConstants.AllActionType,
                new[] { ProjectFat.Project.ID.ToString() },
                0, 3);

            TimeLineView cntrlTimeLineView = (TimeLineView)LoadControl(PathProvider.GetControlVirtualPath("TimeLineView.ascx"));
            cntrlTimeLineView.Activities = LastActivity;
            cntrlTimeLineView.ShowActivityDate = true;
            phTimeLine.Controls.Add(cntrlTimeLineView);
        }

    }
}