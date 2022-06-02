#region Import

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using AjaxPro;
using System.Collections.Specialized;
using ASC.Web.Studio.Utility;
using ASC.Core.Users;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Tasks
{

    [AjaxNamespace("AjaxPro.TaskBlockView")]
    public partial class TaskBlockTemplateView : BaseUserControl
    {

        #region Members

        public TemplateProject Template;

        #endregion

        #region Property

        public TemplateMilestone BlockMilestone { get; set; }
        public List<TemplateMilestone> OtherMilestones { get; set; }

        public bool MakeChangeStatus { get; set; }
        

        public Dictionary<Int32, bool> TaskHasTimeSpend { get; set; }

        public List<TemplateTask> Items {get; set;}
        
        #endregion


        protected void rptContent_OnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {

            if (!(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)) return;


			TaskBlockViewTemplateRow cntrlTaskBlockViewRow = (TaskBlockViewTemplateRow)e.Item.FindControl("_taskBlockViewRow");
			var taskItem = (TemplateTask)e.Item.DataItem;

            taskItem.Title       = taskItem.Title.HtmlEncode().Replace("'", @"&#039;");
            taskItem.Description = taskItem.Description.HtmlEncode().Replace("'", @"&#039;");

            cntrlTaskBlockViewRow.Template = Template;
            cntrlTaskBlockViewRow.Target = taskItem;
            cntrlTaskBlockViewRow.RowIndex = e.Item.ItemIndex;
			//cntrlTaskBlockViewRow.HasTime = TaskHasTimeSpend[taskItem.Id];
            cntrlTaskBlockViewRow.ExecuteHtmlEncode = false;

        }
        

        [AjaxMethod]
        public void SetTaskSortOrder(int milestoneID, int taskID, int prevTaskID, int nextTaskID, bool moveToAnotherMilestone)
        {
            ProjectSecurity.DemandAuthentication();
            
            int? prevID = prevTaskID;
            int? nextID = nextTaskID;

            if (prevTaskID == -1)
                prevID = null;

            if (nextTaskID == -1)
                nextID = null;

			var engine = Global.EngineFactory.GetTemplateEngine();
            if (moveToAnotherMilestone)
            {

				var task = engine.GetTemplateTask(taskID);
                task.MilestoneId = milestoneID;
				engine.SaveTemplateTask(task);				
            }

			Global.EngineFactory.GetTemplateEngine().SetTaskOrders(milestoneID, taskID, prevID, nextID);
        }

        [AjaxMethod]
        public NameValueCollection SaveOrUpdateTask(int projectID,
                                                    int taskID,
                                                    int milestoneID,
                                                    String title,
                                                    String description,
                                                    Guid responsible,
													String deadline,
													bool notifyResponsible,
                                                    string attachmentList)
        {
            ProjectSecurity.DemandAuthentication();
            
            TemplateTask task = null;

            if (taskID > 0)
            {
                task = Global.EngineFactory.GetTemplateEngine().GetTemplateTask(taskID);
            }
            else
            {
				task = new TemplateTask(projectID, title);
            }

            task.MilestoneId = milestoneID;
            task.Title = title;
            task.Description = description;
            task.Responsible = responsible;
			//task.Priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), priority.ToString());

            Global.EngineFactory.GetTemplateEngine().SaveTemplateTask(task);

            var senders = new List<Guid>();

            var page = new Page();

			var cntrlTaskBlockViewRow = (TaskBlockViewTemplateRow)LoadControl(PathProvider.GetControlVirtualPath("TaskBlockViewTemplateRow.ascx"));

            cntrlTaskBlockViewRow.Template = Template;
            cntrlTaskBlockViewRow.Target = task;

            cntrlTaskBlockViewRow.RowIndex = 0;
            cntrlTaskBlockViewRow.ExecuteHtmlEncode = true;
            page.Controls.Add(cntrlTaskBlockViewRow);

            var writer = new System.IO.StringWriter();
            HttpContext.Current.Server.Execute(page, writer, false);

            string output = writer.ToString();
            writer.Close();

            String infoBlockText = String.Empty;

            return new NameValueCollection
                                 {
                                     {"ID", task.Id.ToString()},
                                     {"HTML", output}, 
                                     {"InfoBlockText", infoBlockText}
                                 };
        }

        [AjaxMethod]
        public String TaskActionPanelHTML(int taskID, int taskStatus)
        {
            ProjectSecurity.DemandAuthentication();

            TaskStatus status = (TaskStatus)Enum.ToObject(typeof(TaskStatus), taskStatus);

            StringBuilder innerHTML = new StringBuilder();

            innerHTML.AppendLine("<dl class='cornerAll pm-taskStatus pm-flexible'>");

            switch (status)
            {

                case TaskStatus.NotAccept:
                    innerHTML.AppendFormat(
                                       @"<dt>
                                           <img src='{0}' title='{2}' alt='{2}' />
                                      </dt>
                                      <dd  class='textMediumDescribe' onclick='javascript:ASC.Projects.TaskPage.execTaskChangeStatus({4},{3})'>
                                            {1}
                                      </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID), TaskResource.TaskOpen.ToLower(), TaskResource.TaskOpen, (int)TaskStatus.Open, taskID);

                    innerHTML.AppendFormat(
                         @"<dt>
                                <img src='{0}' title='{2}' alt='{2}' />
                          </dt>
                          <dd  class='textMediumDescribe' onclick='javascript:ASC.Projects.TaskPage.execTaskChangeStatus({4},{3})'>
                                {1}
                          </dd>",
                           WebImageSupplier.GetAbsoluteWebPath("status_closed.png", ProductEntryPoint.ID), TaskResource.TaskClosed.ToLower(), TaskResource.TaskClosed, (int)TaskStatus.Closed, taskID);

                    break;
                case TaskStatus.Closed:
                    innerHTML.AppendFormat(
                                       @"<dt>
                                           <img src='{0}' title='{2}' alt='{2}' />
                                      </dt>
                                      <dd  class='textMediumDescribe'  onclick='javascript:ASC.Projects.TaskPage.execTaskChangeStatus({4},{3})'>
                                            {1}
                                      </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID), TaskResource.TaskReopen.ToLower(), TaskResource.TaskReopen, (int)TaskStatus.Open, taskID);

                    break;
                case TaskStatus.Open:
                    innerHTML.AppendFormat(
                         @"<dt>
                                <img src='{0}' title='{2}' alt='{2}' />
                          </dt>
                          <dd  class='textMediumDescribe'  onclick='javascript:ASC.Projects.TaskPage.execTaskChangeStatus({4},{3})'>
                                {1}
                          </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_wait.png", ProductEntryPoint.ID), TaskResource.TaskStopExecute.ToLower(), TaskResource.TaskStopExecute, (int)TaskStatus.NotAccept, taskID);

                    innerHTML.AppendFormat(
                         @"<dt>
                                <img src='{0}' title='{2}' alt='{2}' />
                          </dt>
                          <dd  class='textMediumDescribe'  onclick='javascript:ASC.Projects.TaskPage.execTaskChangeStatus({4},{3})'>
                                {1}
                          </dd>",
                           WebImageSupplier.GetAbsoluteWebPath("status_closed.png", ProductEntryPoint.ID), TaskResource.TaskClosed.ToLower(), TaskResource.TaskClosed, (int)TaskStatus.Closed, taskID);

                    break;
            }


            innerHTML.AppendLine("</dl>");

            return innerHTML.ToString();

        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public String TaskChangeStatus(int taskID, int newStatus)
        {
			return string.Empty;
			//TaskStatus status = (TaskStatus)Enum.ToObject(typeof(TaskStatus), newStatus);
			//TemplateTask task = Global.EngineFactory.GetTemplateEngine().GetTemplateTask(taskID);
			//MakeChangeStatus = true;			
        }

        [AjaxMethod]
        public void TaskManager(int[] ids, string actionString)
        {
            ProjectSecurity.DemandAuthentication();

            int actionID = int.Parse(actionString.Split('_')[0]);
            int entityID = int.Parse(actionString.Split('_')[1]);
			var engine = Global.EngineFactory.GetTemplateEngine();
            switch (actionID)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
					foreach (var id in ids)
					{
						engine.RemoveTemplateTask(id);
					}
                    break;
                case 4:
                    foreach (var id in ids)
                    {
						TemplateTask task = engine.GetTemplateTask(id);
                        task.MilestoneId = entityID;
						engine.SaveTemplateTask(task);
                    }
                    break;
                case 5:
                    break;
                case 6:
                    foreach (var id in ids)
                    {
						TemplateTask task = engine.GetTemplateTask(id);
						task.MilestoneId = 0;
						engine.SaveTemplateTask(task);
                    }
                    break;
            }

        }

        [AjaxMethod]
        public string ExecBeResponsible(int taskID)
        {
            ProjectSecurity.DemandAuthentication();
            
            var engine = Global.EngineFactory.GetTemplateEngine();
			TemplateTask target = engine.GetTemplateTask(taskID);
			target.Responsible = SecurityContext.CurrentAccount.ID;
			engine.SaveTemplateTask(target);
            
			//return StudioUserInfoExtension.RenderProfileLink(CoreContext.UserManager.GetUsers(target.Responsible), ProductEntryPoint.ID);

            var Responsible = new Participant(target.Responsible);

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
                                </div>", ASC.Core.Users.UserInfoExtension.GetSmallPhotoURL(((ASC.Core.Users.UserInfo)Responsible.UserInfo)),
                                   ASC.Web.Studio.Utility.CommonLinkUtility.GetUserProfile(((ASC.Core.Users.UserInfo)Responsible.UserInfo).ID, ProductEntryPoint.ID),
                                   ASC.Web.Core.Users.DisplayUserSettings.GetFullUserName(((ASC.Core.Users.UserInfo)Responsible.UserInfo)));

        }

		[AjaxMethod]
		public object GetCurrUser(int taskID)
		{
            ProjectSecurity.DemandAuthentication();
			
            var task = Global.EngineFactory.GetTemplateEngine().GetTemplateTask(taskID);

			string deadline = string.Empty;

			return new
			{
				Title = HttpUtility.HtmlDecode(task.Title),
				task.Id,
				ResponsibleID = task.Responsible,
				task.Priority,
				Description = task.Description,
				MilestoneID = task.MilestoneId,
				Deadline = deadline
			};
		}

		[AjaxMethod]
		public void MoveTaskToMilestone(int taskID, int milestoneID)
		{
            ProjectSecurity.DemandAuthentication();
            
            var engine = Global.EngineFactory.GetTemplateEngine();
			var task = Global.EngineFactory.GetTemplateEngine().GetTemplateTask(taskID);
			task.MilestoneId = milestoneID;
			engine.SaveTemplateTask(task);
		}

		[AjaxMethod]
		public void ChangeTaskStatus(int taskID, bool isClosed)
		{			
		}

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
			AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskDetailsTemplateView));
			AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            String script = @"jq(document).click(function(event) 
                              {

                                 if (!jq((event.target) ? event.target : event.srcElement)
                                      .parents()
                                      .andSelf()
                                      .is('.pm-taskStatus-switch, .pm-taskStatus'))
                                      jq('.pm-taskStatus').hide();                                     

                               });";

            if (!Page.ClientScript.IsStartupScriptRegistered(typeof(Page), "216528BD-3421-4a2f-ADCF-E7B5C8C5CA3D"))
                Page.ClientScript.RegisterStartupScript(typeof(Page), "216528BD-3421-4a2f-ADCF-E7B5C8C5CA3D", script, true);

			rptContent.DataSource = Items;
            rptContent.DataBind();
        }

        #endregion

    }
}