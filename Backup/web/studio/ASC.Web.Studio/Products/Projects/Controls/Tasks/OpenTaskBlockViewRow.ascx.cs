#region Import

using System;
using System.Text;
using System.Web;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using System.Collections.Generic;
using ASC.Web.Projects.Classes;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Tasks
{
    public partial class OpenTaskBlockViewRow : BaseUserControl
    {

        #region Property

        public ProjectFat ProjectFat { get; set; }
        public Task Target { get; set; }
        public int RowIndex { get; set; }

        public String Status { get; set; }
        public bool HasTime { get; set; }

        public Participant TeamLeader { get; set; }
        public bool HaveFullPermission { get; set; }
        public bool HavePermission { get; set; }

        public bool IsAllMyTasks { get; set; }

        public bool OneList { get; set; }

        public bool ExecuteHtmlEncode { get; set; }

        public string MilestoneName { get; set; }
        public string ProjectName { get { return (Target.Project != null ? Target.Project.HtmlTitle : ""); } }

        #endregion

        #region Methods

        public string GetTimeTrackingImagePath()
        {

            if (HasTime)
                return WebImageSupplier.GetAbsoluteWebPath("clock_active.png", ProductEntryPoint.ID);

            return WebImageSupplier.GetAbsoluteWebPath("clock_noactive.png", ProductEntryPoint.ID);

        }

        protected string GetTitle()
        {

            StringBuilder innerHTML = new StringBuilder();

            switch (Target.Priority)
            {

                case TaskPriority.High:
                    innerHTML.AppendFormat("<img title='{0}' src='{1}' align='top' alt='{0}' style='margin-right:5px;margin-top:1px; ' />", TaskResource.HighPriority_Title, WebImageSupplier.GetAbsoluteWebPath("prior_hi.png", ProductEntryPoint.ID));
                    break;
                case TaskPriority.Low:
                    innerHTML.AppendFormat("<img title='{0}' src='{1}' align='top' alt='{0}' style='margin-right:5px;margin-top:1px; ' />", TaskResource.LowPriority_Title, WebImageSupplier.GetAbsoluteWebPath("prior_lo.png", ProductEntryPoint.ID));
                    break;
            }

            innerHTML.AppendFormat("<a id='taskTitle_{1}' class='linkHeaderLightMedium' title=\"{3}\" href='tasks.aspx?prjID={0}&id={1}'>{2}</a>",
                Target.Project.ID,
                Target.ID,
                ExecuteHtmlEncode ? Target.Title.HtmlEncode() : Target.Title,
                Target.Description.ReplaceSingleQuote().Replace("\n", "<br/>").HtmlEncode().Replace("'", "-")
                );

            return innerHTML.ToString();
        }

        protected string GetTaskResponsible()
        {
            if (!Target.Responsible.Equals(Guid.Empty))
            {
                TeamLeader = new Participant(Target.Responsible);

                return String.Format(@"<div class='pm-projectTeam-userInfoContainer' style='width:120px'>
                                    <table cellpadding='0' cellspacing='0'>
                                        <tr>
                                            <td class='pm-projectTeam-userPhotoContainer'>
                                                <img src='{0}' />
                                            </td>
                                            <td>
                                                <a class='linkDescribe' href='{1}'>
                                                    {2}
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
                                </div>", ASC.Core.Users.UserInfoExtension.GetSmallPhotoURL(((ASC.Core.Users.UserInfo)TeamLeader.UserInfo)),
                                       ASC.Web.Studio.Utility.CommonLinkUtility.GetUserProfile(((ASC.Core.Users.UserInfo)TeamLeader.UserInfo).ID, ProductEntryPoint.ID),
                                       ASC.Web.Core.Users.DisplayUserSettings.GetFullUserName(((ASC.Core.Users.UserInfo)TeamLeader.UserInfo)));

                //return StudioUserInfoExtension.RenderProfileLink(CoreContext.UserManager.GetUsers(Target.Responsible), ProductEntryPoint.ID);
            }
            else
            {
                return ProjectSecurity.CanEdit(Target) ?
                    String.Format("<div style='width:120px'><a onclick='javascript: ASC.Projects.TaskPage.execTaskBeResponsible({0});' href='javascript:void(0)' class='grayLinkButton'>{1}</a></div>", Target.ID, TaskResource.Accept) :
                    String.Format("<div class='textBigDescribe' style='width:120px'>{0}</div>", TaskResource.WithoutResponsible);
            }
        }

        protected string GetUserName()
        {
            if (Target.Responsible != Guid.Empty)
                return CoreContext.UserManager.GetUsers(Target.Responsible).DisplayUserName(true).ReplaceSingleQuote();
            else
                if (HavePermission)
                    return string.Format("<a class='linkShowLastCompletedTasks' style='margin-left:0px;' href='javascript:void(0)' onclick='javascript:ASC.Projects.TaskPage.execTaskBeResponsible({0},{1});'>{2}</a>", Target.ID, Target.Milestone, TaskResource.Accept);
                else
                    return TaskResource.WithoutResponsible;
        }

        protected string GetTimeTrackingAction()
        {
            if (ProjectSecurity.CanEdit(Target) && Target.Status != TaskStatus.Unclassified)
            {
                return string.Format("ASC.Projects.TimeSpendActionPage.ViewTimeLogPanel({0},{1})", Target.Project.ID, Target.ID);
            }
            return string.Empty;
        }

        protected string GetNotifyAction()
        {
            if (ProjectSecurity.CanEdit(Target) && Target.Status != TaskStatus.Unclassified)
            {
                return string.Format("ASC.Projects.TaskActionPage.notifyResponsible({0},\"{1}\")", Target.ID, WebImageSupplier.GetAbsoluteWebPath("ico_bell_ani.gif", ProductEntryPoint.ID));
            }
            return string.Empty;
        }

        protected string GetRowCssClass()
        {
            string cssClass = string.Empty;
            if (RowIndex != -1)
                cssClass = RowIndex % 2 == 0 ? "tintMedium" : "tintLight";
            return string.Format("borderBase pm-task-row pm_task_item {0}", cssClass);
        }

        protected string GetTaskDeadline()
        {
            if (Target.Deadline == null || Target.Deadline == DateTime.MinValue)
                return string.Empty;

            string style = Target.Status == TaskStatus.Closed ? "style='display:none'" : string.Empty;

            int count = Target.Deadline.Date.Subtract(TenantUtil.DateTimeNow().Date).Days;
            //string days = GrammaticalHelper.ChooseNumeralCase(Math.Abs(count), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural);

            if (count > 0)
                return string.Format("<span class=\"splitter\"></span><span id='taskDeadline_{2}' {3}>{0} {1}</span>", Math.Abs(count), TaskResource.DaysLeft, Target.ID, (IsAllMyTasks ? "class='describeText'" : style));

            if (count < 0)
                return string.Format("<span class=\"splitter\"></span><span id='taskDeadline_{2}' {3} class='pm-taskDeadlineLateInfoContainer'>{0} {1}</span>", TaskResource.TaskDeadlineLateInfo, Target.Deadline.ToString(DateTimeExtension.DateFormatPattern), Target.ID, style);

            return string.Format("<span class=\"splitter\"></span><span id='taskDeadline_{1}' {2} class='pm-redText'>{0}</span>", ProjectsCommonResource.Today, Target.ID, style);
        }

        protected string GetTitleContainerWidth()
        {
            int width = 490;

            if (IsAllMyTasks)
            {
                if (OneList)
                    width = 500;
                else width = 830;
            }
            return string.Format("style='width:{0}px;'", width);
        }

        public string GetLoaderImgURL()
        {
            return WebImageSupplier.GetAbsoluteWebPath("loader_12.gif", ProductEntryPoint.ID);
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            HaveFullPermission = ProjectSecurity.CanEdit(ProjectFat.Project);
            HavePermission = ProjectSecurity.CanEdit(Target);
        }

        #endregion

    }
}