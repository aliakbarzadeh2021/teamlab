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
using ASC.Web.Projects.Controls;
using ASC.Web.Controls;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Studio.Controls.Common;
using ASC.Projects.Core;
using ASC.Core;
using ASC.Web.Projects.Controls.ProjectTemplates.Tasks;

namespace ASC.Web.Projects.TProject
{
	public partial class Tasks : BasePage
	{

		#region Init template
		protected TemplateProject Template { get; set; }

		private void InitProjectTemplate()
		{
			Template = ProjectTemplatesUtil.GetProjectTemplate();
		}
		#endregion

		protected override void PageLoad()
		{
			InitProjectTemplate();
			ProjectTemplatesUtil.InitProjectTemplatesBreadcrumbs(Master, Template, Resources.TaskResource.Tasks, "Tasks");
			Title = HeaderStringHelper.GetPageTitle(Resources.TaskResource.Tasks, Master.BreadCrumbs);

			int taskID;

			if (Int32.TryParse(UrlParameters.EntityID, out taskID))
			{
				TemplateTask task = Global.EngineFactory.GetTemplateEngine().GetTemplateTask(taskID);
				if (task != null)
				{
					if (task.ProjectId == Template.Id)
						ExecTaskDetailsView(task);
					else
						ElementNotFoundControlView(Template.Id);
				}
				else
					ElementNotFoundControlView(Template.Id);
			}
			else
			{
				int filter;
				Int32.TryParse(UrlParameters.ActionType, out filter);

				var enumValue = (ListTaskTemplateView.TaskFilter)Enum.ToObject(typeof(ListTaskTemplateView.TaskFilter), filter);

				if (!Enum.IsDefined(typeof(ListTaskTemplateView.TaskFilter), enumValue))
				{
					if (Template.Responsible == ASC.Core.SecurityContext.CurrentAccount.ID)
						enumValue = ListTaskTemplateView.TaskFilter.AllTasks;
					else if (Global.EngineFactory.GetProjectEngine().IsInTeam(Template.Id, SecurityContext.CurrentAccount.ID))
						enumValue = ListTaskTemplateView.TaskFilter.MyTask;
					else
						enumValue = ListTaskTemplateView.TaskFilter.AllTasks;
				}

				if (enumValue == ListTaskTemplateView.TaskFilter.ByUser && (String.IsNullOrEmpty(Request["userID"])))
					enumValue = ListTaskTemplateView.TaskFilter.AllTasks;

				ExecListTaskView(enumValue);
			}

			Title = HeaderStringHelper.GetPageTitle(TaskResource.Task, Master.BreadCrumbs);
			SideActionsPanel.Visible = true;
			ProjectTemplatesUtil.AddCreateProjectFromTemplateActions(SideActionsPanel, Template);

			if (ProjectTemplatesUtil.CheckEditPermission(Template) && !string.IsNullOrEmpty(Request.QueryString["addTask"]))
			{
				Page.ClientScript.RegisterClientScriptBlock(typeof(string), "initTask", @"; jq(function(){
ASC.Projects.TaskActionPage.init(-1, null, null);
ASC.Projects.TaskActionPage.show();
jq('#userSelector_dropdown').hide();
jq('#milestoneSelector_dropdown').hide();
}); ", true);
			}
		}



		#region Methods

		protected void ExecTaskDetailsView(TemplateTask task)
		{
			TaskDetailsTemplateView cntrlTaskDetailsView = (TaskDetailsTemplateView)LoadControl(PathProvider.GetControlVirtualPath("TaskDetailsTemplateView.ascx"));
			cntrlTaskDetailsView.Template = Template;

			cntrlTaskDetailsView.Task = task;

			_content.Controls.Add(cntrlTaskDetailsView);

			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = TaskResource.AddTask,
				URL = "javascript:ASC.Projects.TaskActionPage.init(-1, null, null);ASC.Projects.TaskActionPage.show();"
			});
			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = ProjectsCommonResource.Edit,
				URL = String.Format("javascript:ASC.Projects.TaskActionPage.init({0}, null, null);ASC.Projects.TaskActionPage.show({0});", task.Id)
			});
			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = ProjectsCommonResource.Delete,
				URL = String.Format("javascript:ASC.Projects.TaskDetailsView.execTaskDelete({0})", task.Id)
			});

			Master.BreadCrumbs.Add(new BreadCrumb
			{
				Caption = task.Title.HtmlEncode(),
				NavigationUrl = String.Format("tasks.aspx?prjID={0}&id={1}", task.ProjectId, task.Id)
			});

			Master.CommonContainerHeader = Global.RenderCommonContainerHeader(task.Title.HtmlEncode(), EntityType.Task);
		}

		protected void ExecListTaskView(ListTaskTemplateView.TaskFilter filter)
		{
			AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockTemplateView));

			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = TaskResource.AddNewTask,
				URL = "javascript:ASC.Projects.TaskActionPage.init(-1, null, null); ASC.Projects.TaskActionPage.show()"
			});

			if (Template.TasksCount > 0)
			{
				ListTaskTemplateView cntrlListTaskView = (ListTaskTemplateView)LoadControl(PathProvider.GetControlVirtualPath("ListTaskTemplateView.ascx"));
				cntrlListTaskView.Template = Template;
				cntrlListTaskView.CurrentFilter = filter;
				_content.Controls.Add(cntrlListTaskView);
			}
			else
			{
				if (ProjectTemplatesUtil.CheckEditPermission(Template))
				{
					EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
					emptyScreenControl.HeaderContent = String.Format("<a onclick='{1}' href='javascript:void(0)' >{0}</a>", TaskResource.EmptyScreenTaskTemplate, "javascript:ASC.Projects.TaskActionPage.init(-1, null, null); ASC.Projects.TaskActionPage.show()");
					emptyScreenControl.HeaderDescribe = TaskResource.EmptyScreenTaskTemplateDescription;
					_content.Controls.Add(emptyScreenControl);
				}
				else
				{
					_content.Controls.Add(new NotFoundControl());
				}

				if (ProjectTemplatesUtil.CheckEditPermission(Template))
				{
                    TaskActionTemplateView taskActionView = (TaskActionTemplateView)LoadControl(PathProvider.GetControlVirtualPath("TaskActionTemplateView.ascx"));
					taskActionView.Template = Template;
					_content.Controls.Add(taskActionView);
				}
			}

			switch (filter)
			{

				case ListTaskTemplateView.TaskFilter.AllTasks:
					Master.BreadCrumbs.Add(new BreadCrumb
					{
						Caption = TaskResource.AllTasks,
						NavigationUrl = String.Format("tasks.aspx?{1}={0}", Template.Id, UrlParameters.ProjectID)
					});
					break;
				case ListTaskTemplateView.TaskFilter.MyTask:
					Master.BreadCrumbs.Add(new BreadCrumb
					{
						Caption = TaskResource.MyTasks,
						NavigationUrl = String.Format("tasks.aspx?{2}={0}&{1}=2", Template.Id, UrlParameters.ActionType, UrlParameters.ProjectID)
					});
					break;
				case ListTaskTemplateView.TaskFilter.ByUser:
					Master.BreadCrumbs.Add(new BreadCrumb
					{
						Caption = String.Format(TaskResource.TasksByParticipantHeader, ASC.Core.CoreContext.UserManager.GetUsers(new Guid(Request["userID"]))),
						NavigationUrl = String.Format("tasks.aspx?prjID={0}&{1}=3", Template.Id, UrlParameters.ActionType)
					});
					break;
			}
		}

		protected void ElementNotFoundControlView(int prjID)
		{

			_content.Controls.Add(new ElementNotFoundControl
			{
				Header = TaskResource.TaskNotFound_Header,
				Body = TaskResource.TaskNotFound_Body,
				RedirectURL = String.Format("tasks.aspx?prjID={0}", prjID),
				RedirectTitle = TaskResource.TaskNotFound_RedirectTitle
			});

			SideActionsPanel.Visible = false;

		}

		#endregion
	}
}
