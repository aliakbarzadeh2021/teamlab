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

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Tasks
{
    public partial class TaskBlockViewTemplateRow : BaseUserControl
    {

        #region Property

        public TemplateProject Template { get; set; }
        public TemplateTask Target { get; set; }
        public int RowIndex { get; set; }

        public bool MultiSelect {get;set;}

        public String Status { get; set; }
        public bool HasTime { get; set; }

        public Participant TeamLeader { get; set; }
        public bool ExecuteHtmlEncode { get; set; }

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

			string cssClass = "linkHeaderLightMedium";

			innerHTML.AppendFormat("<a id='taskTitle_{1}' class='{4}' title=\"{3}\" href='tasks.aspx?prjID={0}&id={1}'>{2}</a>",
				Target.ProjectId,
				Target.Id,
                ExecuteHtmlEncode ? Target.Title.HtmlEncode() : Target.Title,
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
            }
            else
            {
				//        return String.Format("<div style='width:120px'><a onclick='javascript:ASC.Projects.TaskPage.execTaskBeResponsible({0});' href='javascript:void(0)' class='grayLinkButton'>{1}</a></div>", Target.Id, TaskResource.Accept);
				return String.Format("<div class='textBigDescribe' style='width:120px'>{0}</div>", TaskResource.WithoutResponsible);
            }
        }

		protected string GetUserName()
		{
			var userName = string.Format("<img src='{0}' style='margin-bottom: -1px; margin-right: 6px;' />", WebImageSupplier.GetAbsoluteWebPath("profile.gif"));
			return Target.Responsible != Guid.Empty ? userName + CoreContext.UserManager.GetUsers(Target.Responsible).DisplayUserName(true).ReplaceSingleQuote() : TaskResource.WithoutResponsible;
		}

		protected string GetRowCssClass()
		{
			string cssClass = "tintLight";
			cssClass = RowIndex % 2 == 0 ? "tintMedium" : "tintLight";
			return string.Format("borderBase pm-task-row pm_task_item {0}", cssClass);
		}

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #endregion
    }
}