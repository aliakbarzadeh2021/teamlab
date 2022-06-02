#region Import

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Projects.Core;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Controls;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls;
using ASC.Web.Projects.Resources;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Data.Storage;
using ASC.Web.Projects.Controls.TimeSpends;
using ASC.Core.Users;

#endregion

namespace ASC.Web.Projects
{
    public partial class Tasks : BasePage
    {

        #region Events



        protected override void PageLoad()
        {
            RequestContext.EnsureCurrentProduct();

            int projectID = RequestContext.GetCurrentProjectId();
            int taskID;

            Project project = RequestContext.GetCurrentProject();

            if (Int32.TryParse(UrlParameters.EntityID, out taskID))
            {
                Task task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
                if (ProjectSecurity.CanRead(task))
                {
                    if (task.Project.ID == projectID)
                        ExecTaskDetailsView(task);
                    else
                        ElementNotFoundControlView(projectID);
                }
                else
                    ElementNotFoundControlView(projectID);
            }
            else
            {
                int filter;
                Int32.TryParse(UrlParameters.ActionType, out filter);

                var enumValue = (ListTaskView.TaskFilter)Enum.ToObject(typeof(ListTaskView.TaskFilter), filter);

                if (!Enum.IsDefined(typeof(ListTaskView.TaskFilter), enumValue))
                {
                    if (project.Responsible == ASC.Core.SecurityContext.CurrentAccount.ID)
                        enumValue = ListTaskView.TaskFilter.AllTasks;
                    else if (Global.EngineFactory.GetProjectEngine().IsInTeam(project.ID, SecurityContext.CurrentAccount.ID))
                        enumValue = ListTaskView.TaskFilter.MyTask;
                    else
                        enumValue = ListTaskView.TaskFilter.AllTasks;
                }

                if (enumValue == ListTaskView.TaskFilter.ByUser && (String.IsNullOrEmpty(Request["userID"])))
                    enumValue = ListTaskView.TaskFilter.AllTasks;

                ExecListTaskView(enumValue);
            }


            if (SideActionsPanel.Controls.Count == 0)
                SideActionsPanel.Visible = false;


            Title = HeaderStringHelper.GetPageTitle(TaskResource.Task, Master.BreadCrumbs);

        }

        #endregion

        #region Methods

        protected void ExecTaskDetailsView(Task task)
        {
            Project project = RequestContext.GetCurrentProject();

            TaskDetailsView cntrlTaskDetailsView = (TaskDetailsView)LoadControl(PathProvider.GetControlVirtualPath("TaskDetailsView.ascx"));

            cntrlTaskDetailsView.Task = task;

            _content.Controls.Add(cntrlTaskDetailsView);

            if (ProjectSecurity.CanCreateTask(task.Project,false))
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = TaskResource.AddTask,
                    URL = "javascript:ASC.Projects.TaskActionPage.init(-1, null, null);ASC.Projects.TaskActionPage.show();"
                });

            if (ProjectSecurity.CanEdit(task))
            {
                if (task.Status == TaskStatus.NotAccept || task.Status == TaskStatus.Open || task.Status == TaskStatus.Unclassified)
                {
                    SideActionsPanel.Controls.Add(new NavigationItem
                   {
                       Name = ProjectsCommonResource.Edit,
                       URL = String.Format("javascript:ASC.Projects.TaskActionPage.init({0}, null, null);ASC.Projects.TaskActionPage.show({0});", task.ID)
                   });
                }

                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = ProjectsCommonResource.Delete,
                    URL = String.Format("javascript:ASC.Projects.TaskDetailsView.execTaskDelete({0})", task.ID)
                });
            }
            String objectID = String.Format("{0}_{1}", task.UniqID, task.Project.ID);

            var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
              NotifyConstants.Event_NewCommentForTask,
              NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                  ASC.Core.SecurityContext.CurrentAccount.ID.ToString())
              ));

            bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, objectID, true) == 0));

            StringBuilder innerHTML = new StringBuilder();

            innerHTML.Append("<div id='task_subscriber'>");
            innerHTML.AppendFormat("<a  href='javascript:void(0)' onclick='javascript:ASC.Projects.TaskActionPage.changeSubscribe({1})' class=\"linkAction\">{0}</a>",
                  subscribed ? ProjectsCommonResource.UnSubscribeOnNewComment : ProjectsCommonResource.SubscribeOnNewComment,
                  task.ID
                );
            innerHTML.Append("</div>");

            if (Global.EngineFactory.GetProjectEngine().IsInTeam(project.ID, Participant.ID))
                SideActionsPanel.Controls.Add(new HtmlMenuItem(innerHTML.ToString()));

            if (SideActionsPanel.Controls.Count == 0)
                SideActionsPanel.Visible = false;


            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = task.Project.HtmlTitle.HtmlEncode(),
                NavigationUrl = "projects.aspx?prjID=" + task.Project.ID

            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {

                Caption = TaskResource.AllTasks,
                NavigationUrl = String.Format("tasks.aspx?prjID={0}&action=2", task.Project.ID)

            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {

                Caption = task.Title.HtmlEncode(),
                NavigationUrl = String.Format("tasks.aspx?prjID={0}&id={1}", task.Project.ID, task.ID)

            });

            string closedBy = string.Empty;
            if (task.Status == TaskStatus.Closed)
            {
                UserInfo user = CoreContext.UserManager.GetUsers(task.LastModifiedBy);
                closedBy = string.Format("<div class='pm_taskTitleClosedByPanel'>{0}<div>",
                    string.Format(TaskResource.TitleClosedByPanel, user.DisplayUserName(true), task.LastModifiedOn.ToString(DateTimeExtension.DateFormatPattern)));
            }

            string taskTitle = string.Format("{0}{1}",
                task.Title.HtmlEncode(),
                closedBy
                );

            Master.CommonContainerHeader = Global.RenderCommonContainerHeader(taskTitle, EntityType.Task);
        }

        protected void ExecListTaskView(ListTaskView.TaskFilter filter)
        {
            Project project = RequestContext.GetCurrentProject();
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockView));


            if (Global.EngineFactory.GetProjectEngine().GetTaskCount(project.ID) > 0)
            {

                ListTaskView cntrlListTaskView = (ListTaskView)LoadControl(PathProvider.GetControlVirtualPath("ListTaskView.ascx"));

                cntrlListTaskView.ProjectFat = RequestContext.GetCurrentProjectFat();
                cntrlListTaskView.CurrentFilter = filter;

                if (ProjectSecurity.CanCreateTask(project,false))
                    SideActionsPanel.Controls.Add(new NavigationItem
                    {
                        Name = TaskResource.AddTask,
                        URL = "javascript:ASC.Projects.TaskActionPage.init(-1, null, null); ASC.Projects.TaskActionPage.show()"
                    });

                _content.Controls.Add(cntrlListTaskView);

                if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking))
                {
                    TimeSpendActionView _timeSpendActionView = (TimeSpendActionView)LoadControl(PathProvider.GetControlVirtualPath("TimeSpendActionView.ascx"));
                    timeSpendPlaceHolder.Controls.Add(_timeSpendActionView);
                }

            }
            else if (ProjectSecurity.CanCreateTask(project,false))
            {

                EmptyScreenControl emptyScreenControl = new EmptyScreenControl();

                emptyScreenControl.HeaderContent = String.Format("<a onclick='{1}' href='javascript:void(0)' >{0}</a>", TaskResource.EmptyScreenHeaderContent, "javascript:ASC.Projects.TaskActionPage.init(-1, null, null); ASC.Projects.TaskActionPage.show()");

                emptyScreenControl.HeaderDescribe = TaskResource.EmptyScreenHeaderDescribe;

                _content.Controls.Add(emptyScreenControl);

                var taskActionView = (TaskActionView)LoadControl(PathProvider.GetControlVirtualPath("TaskActionView.ascx"));

                taskActionView.ProjectFat = RequestContext.GetCurrentProjectFat();
                _content.Controls.Add(taskActionView);

                SideActionsPanel.Visible = false;

            }
            else
            {

                _content.Controls.Add(new EmptyScreenControl
                                                            {
                                                                HeaderContent = TaskResource.EmptyScreenAllTasksHeaderContent,
                                                                HeaderDescribe = String.Empty
                                                            });

            }

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = project.HtmlTitle.HtmlEncode(),
                NavigationUrl = "projects.aspx?prjID=" + project.ID

            });


            switch (filter)
            {

                case ListTaskView.TaskFilter.AllTasks:
                    Master.BreadCrumbs.Add(new BreadCrumb
                    {
                        Caption = TaskResource.AllTasks,
                        NavigationUrl = String.Format("tasks.aspx?{1}={0}", project.ID, UrlParameters.ProjectID)

                    });
                    break;
                case ListTaskView.TaskFilter.MyTask:
                    Master.BreadCrumbs.Add(new BreadCrumb
                    {
                        Caption = TaskResource.MyTasks,
                        NavigationUrl = String.Format("tasks.aspx?{2}={0}&{1}=2", project.ID, UrlParameters.ActionType, UrlParameters.ProjectID)

                    });
                    break;
                case ListTaskView.TaskFilter.ByUser:
                    Master.BreadCrumbs.Add(new BreadCrumb
                    {
                        Caption = String.Format(TaskResource.TasksByParticipantHeader, ASC.Core.CoreContext.UserManager.GetUsers(new Guid(Request["userID"])).DisplayUserName(true).ReplaceSingleQuote()),
                        NavigationUrl = String.Format("tasks.aspx?prjID={0}&{1}=3", project.ID, UrlParameters.ActionType)

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