using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Projects.Controls.ProjectTemplates;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Projects.Core.Domain;
using ASC.Web.Studio.Controls.Users;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Projects.Controls.Projects;
using AjaxPro;
using ASC.Web.Projects.Controls;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core.Users;
using ASC.Projects.Engine;

namespace ASC.Web.Projects.TProject
{
	[AjaxNamespace("ProjectTemplateTeam")]
	public partial class Team : BasePage
	{

		protected TemplateProject Template { get; set; }

		protected override void PageLoad()
		{
			Utility.RegisterTypeForAjax(this.GetType());
			int projectID = ProjectTemplatesUtil.ProjectId();
			Template = Global.EngineFactory.GetTemplateEngine().GetTemplateProject(projectID);
			ProjectTemplatesUtil.InitProjectTemplatesBreadcrumbs(Master, Template, Resources.ProjectResource.Team, "Team");

			Title = HeaderStringHelper.GetPageTitle(Resources.ProjectResource.Team, Master.BreadCrumbs);

			List<Participant> team = new List<Participant>();
			foreach (Guid prt in Template.Team)
			{
				if (ASC.Core.CoreContext.UserManager.UserExists(prt) && (CoreContext.UserManager.GetUsers(prt).Status != EmployeeStatus.Terminated))
				{
					team.Add(new Participant(prt));
				}
			}
			if (team.Count > 0)
			{
				_ltlTeam.Text = RenderProjectTeam(team, team);
			}


			ASC.Web.Studio.UserControls.Users.UserSelector userSelector = (ASC.Web.Studio.UserControls.Users.UserSelector)LoadControl(ASC.Web.Studio.UserControls.Users.UserSelector.Location);
			userSelector.BehaviorID = "UserSelector";
			userSelector.DisabledUsers.Add(Template.Responsible);
			userSelector.Title = ProjectResource.ManagmentTeam;
			//userSelector.SelectedUserListTitle = ProjectResource.Team;
			//userSelector.CustomBottomHtml = string.Format("<div style='padding-top:10px'><input id='notify' type='checkbox'/><label for='notify' style='padding-left:10px' >{0}</label></div>", ProjectResource.NotifyProjectTeam);

			var selectedUsers = new List<Guid>();

			foreach (Guid participant in Template.Team)
			{
				selectedUsers.Add(participant);
			}

			userSelector.SelectedUsers = selectedUsers;

			_phUserSelector.Controls.Add(userSelector);

			ProjectTemplatesUtil.AddCreateProjectFromTemplateActions(SideActionsPanel, Template);


			if (ProjectTemplatesUtil.CheckEditPermission(Template))
			{
				EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
				emptyScreenControl.HeaderContent = String.Format("<a onclick='javascript:UserSelector.ShowDialog();' href='javascript:void(0)' >{0}</a>", ProjectResource.EmptyScreenProjectTemplateTeam);
				emptyScreenControl.HeaderDescribe = ProjectResource.EmptyScreenProjectTemplateTeamDescription;
				EmptyScreenContainer.Controls.Add(emptyScreenControl);
			}
			else
			{
				EmptyScreenContainer.Controls.Add(new NotFoundControl());
			}

			if (!string.IsNullOrEmpty(Request.QueryString["initTeam"]))
			{
				Page.ClientScript.RegisterClientScriptBlock(typeof(string), "initTeam", "; jq(function(){UserSelector.ShowDialog();}); ", true);
			}
		}


		protected String RenderProjectTeam(IList<Participant> participants, IList<Participant> oldTeam)
		{
			Page page = new Page();

			ProjectTeamView oProjectTeamView = (ProjectTeamView)page.LoadControl(PathProvider.GetControlVirtualPath("ProjectTeamView.ascx"));
			oProjectTeamView.TemplateMode = true;

			oProjectTeamView.Team = participants;
			oProjectTeamView.OldTeam = new List<Participant>(oldTeam);
			oProjectTeamView.TeamLeader = new Participant(Template.Responsible);

			page.Controls.Add(oProjectTeamView);

			System.IO.StringWriter writer = new System.IO.StringWriter();

			HttpContext.Current.Server.Execute(page, writer, false);

			string output = writer.ToString();

			writer.Close();

			return output;
		}

		protected String RenderProjectTeam(IList<Guid> participants, IList<Guid> oldTeam)
		{
			return RenderProjectTeam(ConvertToParticipants(participants), ConvertToParticipants(oldTeam));
		}

		private static IList<Participant> ConvertToParticipants(IList<Guid> list)
		{
			var participants = new List<Participant>();
			foreach (var item in list)
			{
				participants.Add(new Participant(item));
			}
			return participants;
		}

		[AjaxMethod]
		public String UserManager(String value, bool notifyIsChecked)
        {
            ProjectSecurity.DemandAuthentication();

			var templateEngine = Global.EngineFactory.GetTemplateEngine();

			var checkedParticipant = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			var shapeTeam = new List<Guid>();

			for (int index = 0; index < checkedParticipant.Length; index++)
				shapeTeam.Add(Global.EngineFactory.GetParticipantEngine().GetByID(new Guid(checkedParticipant[index])).ID);

            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                var queryString = HttpContext.Current.Request.UrlReferrer.Query;
                char[] separators = { '=', '&' };
                var tempArray = queryString.Split(separators);
                Template = templateEngine.GetTemplateProject(int.Parse(tempArray[1]));
            }

			var oldTeam = Template.Team;

			var removeFromTeam = new List<Guid>();
			var inviteToTeam = new List<Guid>();

			foreach (var participant in oldTeam)
				if (!shapeTeam.Contains(participant))
					if (participant != Template.Responsible)
						removeFromTeam.Add(participant);

			foreach (var participant in shapeTeam)
				if (!oldTeam.Contains(participant))
				{
					Template.Team.Remove(participant);
					inviteToTeam.Add(participant);

				}


			foreach (var participant in inviteToTeam)
			{
				Template.Team.Add(participant);
			}

			foreach (var participant in removeFromTeam)
			{
				Template.Team.Remove(participant);
			}
			templateEngine.SaveTemplateProject(Template);

			if (shapeTeam.Count == 0 && oldTeam.Count == 0)
			{
				return string.Empty;
			}

			return RenderProjectTeam(shapeTeam, oldTeam);

		}

	}
}
