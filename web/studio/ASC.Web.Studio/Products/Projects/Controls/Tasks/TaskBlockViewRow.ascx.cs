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

#endregion

namespace ASC.Web.Projects.Controls.Tasks
{
    public partial class TaskBlockViewRow : BaseUserControl
    {

        #region Property

        public ProjectFat ProjectFat { get; set; }
        public Task Target { get; set; }
        public int RowIndex { get; set; }

        public bool MultiSelect {get;set;}

        public String Status { get; set; }
        public bool HasTime { get; set; }

        public Participant TeamLeader { get; set; }
        public bool HaveFullPermission { get; set; }
        public bool HavePermission { get; set; }

        public bool IsAllMyTasks { get; set; }

        public bool OneList { get; set; }

        public string MilestoneName { get; set; }
        public string ProjectName { get { return (Target.Project != null ? Target.Project.HtmlTitle : ""); } }
        
        #endregion

        #region Methods

        protected String RenderHighlightRow(Task task, int rowIndex)
        {
            
            String taskStatusClass = String.Empty;
          

            switch (task.Status)
            {
                
                case TaskStatus.Open:
                    taskStatusClass = "pm-task-open";
                    break;
                case TaskStatus.Closed:
                    taskStatusClass = "pm-task-closed";
                    break;
                case TaskStatus.Disable:
                    taskStatusClass = "pm-task-disable";
                    break;
                case TaskStatus.NotAccept:
                    taskStatusClass = "pm-task-notaccept";
                    break;
                case TaskStatus.Unclassified:
                    taskStatusClass = "pm-task-unclassified";
                    break;
            }

            return String.Format("class='{0}'", taskStatusClass);

        }

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

            string cssClass = Target.Status != TaskStatus.Closed ? "linkHeaderLightMedium" : "pm-linkClosedTask";

            innerHTML.AppendFormat("<a id='taskTitle_{1}' class='{4}' title=\"{3}\" href='tasks.aspx?prjID={0}&id={1}'>{2}</a>",
                Target.Project.ID,
                Target.ID,
                Target.Title,
                Target.Description.ReplaceSingleQuote().Replace("\n", "<br/>").HtmlEncode().Replace("'", "-"),
                cssClass
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
                if (ProjectFat != null)
                {
                    if (ProjectFat.IsInTeam() || Global.IsAdmin)
                        return String.Format("<div style='width:120px'><a onclick='javascript: ASC.Projects.TaskPage.execTaskBeResponsible({0});' href='javascript:void(0)' class='grayLinkButton'>{1}</a></div>", Target.ID, TaskResource.Accept);
                    else
                        return String.Format("<div class='textBigDescribe' style='width:120px'>{0}</div>", TaskResource.WithoutResponsible);
                }
                else
                {
                    if (Global.IsAdmin)
                        return String.Format("<div style='width:120px'><a onclick='javascript: ASC.Projects.TaskPage.execTaskBeResponsible({0});' href='javascript:void(0)' class='grayLinkButton'>{1}</a></div>", Target.ID, TaskResource.Accept);
                    else
                        return String.Format("<div class='textBigDescribe' style='width:120px'>{0}</div>", TaskResource.WithoutResponsible);
                }

            }
        }

        protected string GetUserName()
        {
            return Target.Responsible != Guid.Empty ? CoreContext.UserManager.GetUsers(Target.Responsible).DisplayUserName(true).ReplaceSingleQuote() : string.Empty;
        }

        protected string GetTimeTrackingAction()
        {
            if(ASC.Core.SecurityContext.CheckPermissions(Target, SecurityProvider, ASC.Projects.Core.AuthorizationConstants.Action_Task_Update))
                if(Target.Status!=TaskStatus.Unclassified)
                    return string.Format("ASC.Projects.TimeSpendActionPage.ViewTimeLogPanel({0})",Target.ID);
            return string.Empty;
        }

        protected string GetNotifyAction()
        {
            if (ASC.Core.SecurityContext.CheckPermissions(Target, SecurityProvider, ASC.Projects.Core.AuthorizationConstants.Action_Task_Update))
                if (Target.Status != TaskStatus.Unclassified)
                    return string.Format("ASC.Projects.TaskActionPage.notifyResponsible({0},\"{1}\")", Target.ID, WebImageSupplier.GetAbsoluteWebPath("ico_bell_ani.gif", ProductEntryPoint.ID));
            return string.Empty;
        }

        protected string GetRowCssClass()
        {
            //string cssClass = "tintLight";
            //if(Target.Status!=TaskStatus.Closed)
            string cssClass = RowIndex % 2 == 0 ? "tintMedium" : "tintLight";
            return string.Format("borderBase pm_task_item {0}", cssClass );
        }

        protected string GetTaskDeadline()
        {
            if (Target.Deadline == null || Target.Deadline == DateTime.MinValue)
                return string.Empty;

            string style = Target.Status == TaskStatus.Closed ? "style='display:none'" : string.Empty;

            int count = Target.Deadline.Subtract(TenantUtil.DateTimeNow()).Days;
            //string days = GrammaticalHelper.ChooseNumeralCase(Math.Abs(count), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural);

            if (count > 0)
                return string.Format("<span id='taskDeadline_{2}' {3}>{0} {1}</span>", Math.Abs(count), TaskResource.DaysLeft, Target.ID, style);

            if (count < 0)
                return string.Format("<span id='taskDeadline_{2}' {3} class='pm-taskDeadlineLateInfoContainer'>{0} {1}</span>", TaskResource.TaskDeadlineLateInfo, Target.Deadline.ToString(DateTimeExtension.DateFormatPattern), Target.ID, style);

            return string.Format("<span id='taskDeadline_{1}' {2} class='pm-redText'>{0}</span>", CommonResource.Today, Target.ID, style);
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            HaveFullPermission = true;
            if (!Global.IsAdmin && !ProjectFat.IsResponsible())
                HaveFullPermission = false;

            HavePermission = ASC.Core.SecurityContext.CheckPermissions(Target, SecurityProvider, ASC.Projects.Core.AuthorizationConstants.Action_Task_Update);
            if (Target.Status == TaskStatus.Unclassified || Target.Responsible == Guid.Empty)
                if (Global.IsAdmin || ProjectFat.IsResponsible() ||ProjectFat.IsInTeam())
                    HavePermission = true;

        }

        #endregion

    }
}