#region Import

using System;
using System.Web;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Controls;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Projects.Classes;
using ASC.Core;
using ASC.Web.Projects.Resources;
using ASC.Data.Storage;
using AjaxPro;
using ASC.Web.Core.Users;
using System.Linq;
using ASC.Projects.Core.Domain;

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Tasks
{
	public partial class TaskActionTemplateView : BaseUserControl
	{
		#region Property

		public TemplateTask Target { get; set; }

		public TemplateProject Template { get; set; }

		public List<TemplateMilestone> Milestones { get { return Global.EngineFactory.GetTemplateEngine().GetTemplateMilestones(Template.Id); } }

		public Milestone SelectedMilestone { get; set; }
        public bool Permission { get; set; }

        public bool isMobileVersion { get { return ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context); } }

		#endregion

		#region Events

		protected void Page_Load(object sender, EventArgs e)
		{
			hfTaskId.Value = "-1";

			//AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskActionTemplateView));

			Permission = true;

			_taskContainer.Options.IsPopup = true;

			RegisterClientScript();

            //init milestone selector
            if (isMobileVersion)
            {
                rptMilestoneSelectorMobile.DataSource = Milestones;
                rptMilestoneSelectorMobile.DataBind();
            }
            else
            {
                rptMilestoneSelector.DataSource = Milestones;
                rptMilestoneSelector.DataBind();
            }
			//init responsible selector
			var users = new List<UserInfo>();
			users.Add(new UserInfo());
			users.AddRange(Template.Team.Select(p => CoreContext.UserManager.GetUsers(p)).OrderBy(u => u, UserInfoComparer.Default).ToList());
            if (isMobileVersion)
            {
                rptUserSelectorMobile.DataSource = users;
                rptUserSelectorMobile.DataBind();
            }
            else
            {
                rptUserSelector.DataSource = users;
                rptUserSelector.DataBind();
            }
			//init task deadline
			//taskDeadline.Text = string.Empty;

			
		}

		private void RegisterClientScript()
		{
			UserInfo userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

			var havePermissions = true;
			Page.ClientScript.RegisterClientScriptBlock(typeof(TaskActionTemplateView), "D1F5CC2D-0B7C-41fd-809F-42A64EDB95F1", "CurrUser = " +
																		  JavaScriptSerializer.Serialize(
																			  new
																			  {
																				  UserName = userInfo.DisplayUserName(true),
																				  UserID = userInfo.ID.ToString(),
																				  EmptyGuid = Guid.Empty.ToString(),
																				  HavePermission = havePermissions
																			  }) + "; ", true);
		}

		protected string GetUserAvatarUrl(UserInfo userInfo) { return userInfo.GetSmallPhotoURL(); }

		protected string GetUserName(UserInfo userInfo) { return userInfo.ID != Guid.Empty ? userInfo.DisplayUserName(true).ReplaceSingleQuote() : TaskResource.WithoutResponsible; }

		protected string GetUserTitle(UserInfo userInfo) { return userInfo.ID != Guid.Empty ? userInfo.Title : string.Empty; }

		protected string GetMilestoneTitle(TemplateMilestone milestone)
        {
            return String.Format("[{0}] {1}", ProjectTemplatesUtil.GetMilestoneDeadLine(milestone), HtmlUtility.GetText(milestone.Title, 150));
        }

		#endregion

	}
}