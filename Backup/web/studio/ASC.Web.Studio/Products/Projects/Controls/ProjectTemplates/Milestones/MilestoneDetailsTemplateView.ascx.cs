#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using AjaxPro;
using ASC.Core.Users;
using ASC.Notify.Patterns;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
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
using ASC.Projects.Core;
using ASC.Core;
using ASC.Notify;
using ASC.Notify.Recipients;
using ASC.Core.Tenants;
using ASC.Web.Projects.Controls.ProjectTemplates.Tasks;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates
{
	[AjaxNamespace("AjaxPro.MilestoneDetailsView")]
	public partial class MilestoneDetailsTemplateView : BaseUserControl
	{
		#region Members

		public string status { get; set; }
		public int countTasks
		{
			get
			{
				return Milestone.TasksCount;
			}
		}

		public TemplateProject Template;
		public TemplateMilestone Milestone;

		#endregion

		#region Events

		protected void Page_Load(object sender, EventArgs e)
		{
			AjaxPro.Utility.RegisterTypeForAjax(typeof(MilestoneDetailsTemplateView));
			AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockTemplateView));

			TaskBlockTemplateView cntrlTaskBlockView = (TaskBlockTemplateView)LoadControl(PathProvider.GetControlVirtualPath("TaskBlockTemplateView.ascx"));

			cntrlTaskBlockView.Template = Template;
			cntrlTaskBlockView.MakeChangeStatus = true;
			cntrlTaskBlockView.BlockMilestone = Milestone;



			var list = Global.EngineFactory.GetTemplateEngine().GetTemplateTasks(Template.Id).FindAll(item => item.MilestoneId == Milestone.Id);
			cntrlTaskBlockView.Items = list;

			cntrlTaskBlockView.MakeChangeStatus = false;

			milestoneTasksContent.Controls.Add(cntrlTaskBlockView);

			EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
			emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", TaskResource.AddTask, "javascript:ASC.Projects.TaskActionPage.init(-1," + Milestone.Id + ", null); ASC.Projects.TaskActionPage.show();");
			emptyScreenControl.HeaderDescribe = TaskResource.EmptyScreenHeaderDescribe;
			taskEmptyContent.Controls.Add(emptyScreenControl);

			if (ProjectTemplatesUtil.CheckEditPermission(Template))
			{

				TaskActionTemplateView cntrlTaskActionView = (TaskActionTemplateView)LoadControl(PathProvider.GetControlVirtualPath("TaskActionTemplateView.ascx"));
				cntrlTaskActionView.Template = Template;
				phAddTaskPanel.Controls.Add(cntrlTaskActionView);

				MoveTaskTemplateView cntrlMoveTaskView = (MoveTaskTemplateView)LoadControl(PathProvider.GetControlVirtualPath("MoveTaskTemplateView.ascx"));
				cntrlMoveTaskView.Template = Template;
				phMoveTaskPanel.Controls.Add(cntrlMoveTaskView);
			}

			Global.EngineFactory.GetParticipantEngine().Read(Page.Participant.ID, Milestone.Id.ToString(), TenantUtil.DateTimeNow());

			group_manager_container.Options.IsPopup = true;
		}

		#endregion

		#region Methods


		public String GetMilestoneName()
		{
			return Milestone.Title;
		}
		public String GetTotalTasksCount()
		{
			return Milestone.TasksCount.ToString();
		}
		public String GetActiveTasksCount()
		{
			return Milestone.TasksCount.ToString();
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

		[AjaxMethod]
		public String AddNewMilestone(int projectID, int milestoneID, string title, string deadline, bool notifyManager, bool isKey, bool shift, bool moveOutWeekend)
		{
            ProjectSecurity.DemandAuthentication();
            
            var engine = Global.EngineFactory.GetTemplateEngine();

			TemplateProject project = engine.GetTemplateProject(projectID);

			TemplateMilestone milestone = null;

			if (milestoneID != 0)
			{
				milestone = engine.GetTemplateMilestone(milestoneID);
			}
			if (milestone == null)
			{
				new TemplateMilestone(project.Id, title);
			}

			milestone.ProjectId = project.Id;
			milestone.Title = title.Trim();
			milestone.IsKey = isKey;
			milestone.IsNotify = notifyManager;

			engine.SaveTemplateMilestone(milestone);
			return milestone.Id.ToString();

		}


		[AjaxMethod]
		public void DeleteMilestone(int milestoneID)
		{
            ProjectSecurity.DemandAuthentication();
			
            Global.EngineFactory.GetTemplateEngine().RemoveTemplateMilestone(milestoneID);
		}



		protected String RenderAddTaskButton()
		{
			var sb = new StringBuilder();

			if (Global.IsAdmin)
			{
					var milestoneID = Milestone.Id.ToString();
					var userID = "null";
					var onclick = string.Format("javascript:ASC.Projects.TaskActionPage.init(-1,{0},{1}); ASC.Projects.TaskActionPage.show()",
												milestoneID, userID);
					sb.AppendFormat("<a class='grayButton' href='javascript:void(0)' onclick='{0}'><div></div>{1}</a>",
									onclick, TaskResource.AddNewTask);
			}

			return sb.ToString();
		}




		#endregion
	}
}