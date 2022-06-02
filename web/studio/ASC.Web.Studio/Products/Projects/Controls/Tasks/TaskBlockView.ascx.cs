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
using ASC.Core.Tenants;
using ASC.Data.Storage;
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using System.Collections;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Tasks
{

    [AjaxNamespace("AjaxPro.TaskBlockView")]
    public partial class TaskBlockView : BaseUserControl
    {

        #region Members

        public ProjectFat ProjectFat;
        protected Milestone _milestone;

        #endregion

        #region Property

        public Milestone BlockMilestone { get; set; }

        public List<Milestone> OtherMilestones { get; set; }

        public bool IsAllMyTasks { get; set; }

        public bool OneList { get; set; }

        public Dictionary<Int32, bool> TaskHasTimeSpend { get; set; }

        public List<Task> Items { get; set; }

        public bool ShowAllTasks { get; set; }

        protected bool IsVisible { get; set; }

        #endregion

        #region Methods

        protected void rptContentOpen_OnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {

            if (!(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)) return;


            OpenTaskBlockViewRow cntrlOpenTaskBlockViewRow = (OpenTaskBlockViewRow)e.Item.FindControl("_openTaskBlockViewRow");
            Task taskItem = (Task)e.Item.DataItem;

            taskItem.Title = taskItem.Title.HtmlEncode().Replace("'", @"&#039;");
            taskItem.Description = taskItem.Description.HtmlEncode().Replace("'", @"&#039;");

            cntrlOpenTaskBlockViewRow.ProjectFat = ProjectFat;
            cntrlOpenTaskBlockViewRow.Target = taskItem;
            cntrlOpenTaskBlockViewRow.Status = RenderStatus(cntrlOpenTaskBlockViewRow.Target);
            cntrlOpenTaskBlockViewRow.RowIndex = e.Item.ItemIndex;
            cntrlOpenTaskBlockViewRow.IsAllMyTasks = this.IsAllMyTasks;
            cntrlOpenTaskBlockViewRow.OneList = this.OneList;
            cntrlOpenTaskBlockViewRow.HasTime = TaskHasTimeSpend[taskItem.ID];
            cntrlOpenTaskBlockViewRow.MilestoneName = (BlockMilestone != null ? BlockMilestone.Title : "");
            cntrlOpenTaskBlockViewRow.ExecuteHtmlEncode = false;

        }

        protected void rptContentClosed_OnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {

            if (!(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)) return;


            ClosedTaskBlockViewRow cntrlClosedTaskBlockViewRow = (ClosedTaskBlockViewRow)e.Item.FindControl("_closedTaskBlockViewRow");
            Task taskItem = (Task)e.Item.DataItem;

            taskItem.Title = taskItem.Title.HtmlEncode().Replace("'", @"&#039;");
            taskItem.Description = taskItem.Description.HtmlEncode().Replace("'", @"&#039;");

            cntrlClosedTaskBlockViewRow.ProjectFat = ProjectFat;
            cntrlClosedTaskBlockViewRow.Target = taskItem;
            cntrlClosedTaskBlockViewRow.Status = RenderStatus(cntrlClosedTaskBlockViewRow.Target);
            cntrlClosedTaskBlockViewRow.RowIndex = e.Item.ItemIndex;
            cntrlClosedTaskBlockViewRow.IsAllMyTasks = this.IsAllMyTasks;
            cntrlClosedTaskBlockViewRow.OneList = this.OneList;
            cntrlClosedTaskBlockViewRow.HasTime = TaskHasTimeSpend[taskItem.ID];
            cntrlClosedTaskBlockViewRow.MilestoneName = (BlockMilestone != null ? BlockMilestone.Title : "");
            cntrlClosedTaskBlockViewRow.ExecuteHtmlEncode = false;

        }

        protected String RenderStatus(Task task)
        {
            var html = new StringBuilder();
            var statusTitle = string.Empty;
            var canUpdate = ProjectSecurity.CanEdit(task);

            switch (task.Status)
            {
                case TaskStatus.Open:
                    statusTitle = ResourceEnumConverter.ConvertToString(TaskStatus.Open);
                    html.AppendFormat("<img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />", WebImageSupplier.GetAbsoluteWebPath(canUpdate ? "status_open_combo.png" : "status_open.png", ProductEntryPoint.ID), statusTitle);
                    break;

                case TaskStatus.Closed:
                    statusTitle = ResourceEnumConverter.ConvertToString(TaskStatus.Closed);
                    html.AppendFormat("<img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />", WebImageSupplier.GetAbsoluteWebPath(canUpdate ? "status_closed_combo.png" : "status_closed.png", ProductEntryPoint.ID), statusTitle);
                    break;

                case TaskStatus.NotAccept:
                    statusTitle = ResourceEnumConverter.ConvertToString(TaskStatus.NotAccept);
                    html.AppendFormat(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />", WebImageSupplier.GetAbsoluteWebPath(canUpdate ? "status_wait_combo.png" : "status_wait.png", ProductEntryPoint.ID), statusTitle);
                    break;

                case TaskStatus.Unclassified:
                    statusTitle = ResourceEnumConverter.ConvertToString(TaskStatus.Unclassified);
                    html.AppendFormat(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px;  float:left;' />", WebImageSupplier.GetAbsoluteWebPath(canUpdate ? "status_unclass_border.png" : "status_unclass.png", ProductEntryPoint.ID), statusTitle);
                    break;
            }
            if (canUpdate)
            {
                return task.Status == TaskStatus.Unclassified ?
                    String.Format(@"{0}", html) :
                    String.Format(@"<a  style='float:left;' href='javascript:void(0)' class='pm-taskStatus-switch' onclick='javascript:ASC.Projects.TaskPage.execShowTaskActionPanel({0}, this)'>{1}</a>", (int)task.Status, html);
            }
            return html.ToString();
        }

        protected void InitControls()
        {
            var openTasks = Items.Where(t => t.Status == TaskStatus.Open || t.Status == TaskStatus.NotAccept || t.Status == TaskStatus.Unclassified).ToList();
            var closedTasks = Items.Where(t => t.Status == TaskStatus.Closed).ToList();

            if (ProjectFat.Project.Private)
            {
                openTasks = openTasks.Where(t => ProjectSecurity.CanRead(t)).ToList();
                closedTasks = closedTasks.Where(t => ProjectSecurity.CanRead(t)).ToList();
            }

            if (!ShowAllTasks)
            {
                string tasksID = string.Empty;
                for (int i = 0; i < closedTasks.Count; i++)
                {
                    tasksID += closedTasks[i].ID;
                    tasksID += i != closedTasks.Count - 1 ? "," : string.Empty;
                }

                hfTaskList.Value = tasksID;

                int tasksCount;
                if (!Int32.TryParse(WebConfigurationManager.AppSettings["showClosedTasksCount"], out tasksCount))
                {
                    tasksCount = 3;
                }

                if (closedTasks.Count > tasksCount)
                    IsVisible = true;
                else IsVisible = false;

                hfCountTask.Value = tasksCount.ToString();

                if (closedTasks.Count > tasksCount)
                    closedTasks.RemoveRange(tasksCount, closedTasks.Count - tasksCount);

                hfLastTask.Value = closedTasks.Count > 1 ? closedTasks[closedTasks.Count - 1].ID.ToString() : string.Empty;

                rptContentOpen.DataSource = openTasks;
                rptContentClosed.DataSource = closedTasks;
            }
            else
            {
                rptContentOpen.DataSource = openTasks;
                rptContentClosed.DataSource = closedTasks;
            }
            rptContentOpen.DataBind();
            rptContentClosed.DataBind();
        }

        [AjaxMethod]
        public void SetTaskSortOrder(int milestoneID, int taskID, int prevTaskID, int nextTaskID, bool moveToAnotherMilestone)
        {
            int? prevID = prevTaskID;
            int? nextID = nextTaskID;

            if (prevTaskID == -1)
                prevID = null;

            if (nextTaskID == -1)
                nextID = null;

            if (moveToAnotherMilestone)
            {
                var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
                task.Milestone = milestoneID;
                Global.EngineFactory.GetTaskEngine().SaveOrUpdate(task,null,false);
            }

            Global.EngineFactory.GetTaskEngine().SetTaskOrders(milestoneID, taskID, prevID, nextID);
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
            var task = new Task();

            if (taskID > 0)
            {
                task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
                ProjectFat = new ProjectFat(task.Project);
            }
            else
            {
                task.Status = TaskStatus.Open;
                task.Project = Global.EngineFactory.GetProjectEngine().GetByID(projectID);
                ProjectFat = new ProjectFat(task.Project);
            }

            task.Milestone = milestoneID;
            task.Title = title;
            task.Description = description;
            task.Responsible = responsible;
            if (deadline.Trim() != string.Empty)
                task.Deadline = DateTime.Parse(deadline);
            else task.Deadline = DateTime.MinValue;

            var fileIds = !string.IsNullOrEmpty(attachmentList)
                              ? attachmentList.Split(';').Select(x => Convert.ToInt32(x.Split('_')[0]))
                              : null;
            Global.EngineFactory.GetTaskEngine().SaveOrUpdate(task,fileIds,notifyResponsible);

            ////NOTE: leave it here since it's specific to web
            //var uploadedFiles = new List<ASC.Files.Core.File>();
            //if (attachmentList.Length > 0)
            //    foreach (String fileInfo in attachmentList.Split(';'))
            //    {
            //        FileEngine2.AttachFileToTask(task.ID, Convert.ToInt32(fileInfo.Split('_')[0]));
            //        uploadedFiles.Add(FileEngine2.GetFile(Convert.ToInt32(fileInfo.Split('_')[0]), Convert.ToInt32(fileInfo.Split('_')[1])));
            //    }

            //if (task.Responsible.Equals(Guid.Empty))
            //{
            //    Global.EngineFactory.GetTaskEngine().ChangeStatus(task, TaskStatus.Unclassified);
            //}
            //else
            //{
            //    if (task.Status == TaskStatus.Unclassified)
            //        Global.EngineFactory.GetTaskEngine().ChangeStatus(task, TaskStatus.Open);
            //}

            //var senders = new List<Guid>();

            //if (notifyResponsible && task.Responsible != Guid.Empty && task.Responsible != SecurityContext.CurrentAccount.ID)
            //{
            //    NotifyClient.Instance.SendAboutResponsibleByTask(task.Responsible, task, GetFileListInfoHashtable(uploadedFiles));
            //}
            //if (SecurityContext.CurrentAccount.ID != task.Project.Responsible)
            //{
            //    var users = new List<Guid>();
            //    users.Add(task.Project.Responsible);
            //    NotifyClient.Instance.SendAboutTaskCreating(users, task, GetFileListInfoHashtable(uploadedFiles));
            //}

            var page = new Page();

            var cntrlOpenTaskBlockViewRow = (OpenTaskBlockViewRow)LoadControl(PathProvider.GetControlVirtualPath("OpenTaskBlockViewRow.ascx"));

            cntrlOpenTaskBlockViewRow.ProjectFat = ProjectFat;
            cntrlOpenTaskBlockViewRow.Target = task;

            cntrlOpenTaskBlockViewRow.Status = RenderStatus(task);
            cntrlOpenTaskBlockViewRow.RowIndex = -1;
            cntrlOpenTaskBlockViewRow.HasTime = Global.EngineFactory.GetTimeTrackingEngine().HasTime(task.ID);
            cntrlOpenTaskBlockViewRow.ExecuteHtmlEncode = true;
            page.Controls.Add(cntrlOpenTaskBlockViewRow);

            System.IO.StringWriter writer = new System.IO.StringWriter();
            HttpContext.Current.Server.Execute(page, writer, false);

            string output = writer.ToString();
            writer.Close();

            String infoBlockText = String.Empty;

            if (SecurityContext.CurrentAccount.ID != task.Project.Responsible)
                infoBlockText = String.Format(TaskResource.NotifyManagerAboutTaskCreate, task.Title);

            //String objectID = task.UniqID + "_" + projectID;

            //senders = new List<Guid> { ASC.Core.SecurityContext.CurrentAccount.ID };

            //if (SecurityContext.CurrentAccount.ID != task.Responsible && !task.Responsible.Equals(Guid.Empty))
            //    senders.Add(task.Responsible);

            //foreach (var recipientID in senders)
            //    NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForTask,
            //                                              objectID,
            //                                              NotifySource.Instance.GetRecipientsProvider().
            //                                              GetRecipient(recipientID.ToString()));

            return new NameValueCollection
                                 {
                                     {"ID", task.ID.ToString()},
                                     {"HTML", output}, 
                                     {"InfoBlockText", infoBlockText}
                                 };
        }

        [AjaxMethod]
        public string GetLastCompletedTasks(int[] allTasks, int lastTask, int tasksCount)
        {
            ProjectSecurity.DemandAuthentication();

            var sb = new StringBuilder();
            var task = Global.EngineFactory.GetTaskEngine().GetByID(lastTask);
            ProjectFat = new ProjectFat(task.Project);
            var lastIndex = 0;
            var count = 0;

            for (int i = 0; i < allTasks.Length; i++)
            {
                if (allTasks[i] == lastTask)
                    lastIndex = i + 1;
            }

            for (int i = 0; i < allTasks.Length; i++)
            {
                if (i >= lastIndex && count < tasksCount)
                {
                    task = Global.EngineFactory.GetTaskEngine().GetByID(allTasks[i]);
                    var page = new Page();
                    var cntrlClosedTaskBlockViewRow = (ClosedTaskBlockViewRow)LoadControl(PathProvider.GetControlVirtualPath("ClosedTaskBlockViewRow.ascx"));

                    cntrlClosedTaskBlockViewRow.ProjectFat = ProjectFat;
                    cntrlClosedTaskBlockViewRow.Target = task;

                    cntrlClosedTaskBlockViewRow.Status = RenderStatus(task);
                    cntrlClosedTaskBlockViewRow.RowIndex = i;
                    page.Controls.Add(cntrlClosedTaskBlockViewRow);

                    System.IO.StringWriter writer = new System.IO.StringWriter();
                    HttpContext.Current.Server.Execute(page, writer, false);

                    string output = writer.ToString();
                    writer.Close();

                    sb.Append(output);
                    count++;
                }
            }

            return sb.ToString();
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

            TaskStatus status = (TaskStatus)Enum.ToObject(typeof(TaskStatus), newStatus);

            Task task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);

            Global.EngineFactory.GetTaskEngine().ChangeStatus(task, status);

            return RenderStatus(task);
        }

        [AjaxMethod]
        public void TaskManager(int[] ids, string actionString)
        {

            int actionID = int.Parse(actionString.Split('_')[0]);
            int entityID = int.Parse(actionString.Split('_')[1]);

            switch (actionID)
            {

                case 1:

                    foreach (var id in ids)
                        Global.EngineFactory.GetTaskEngine().ChangeStatus(Global.EngineFactory.GetTaskEngine().GetByID(id), TaskStatus.Open);


                    break;
                case 2:

                    foreach (var id in ids)
                        Global.EngineFactory.GetTaskEngine().ChangeStatus(Global.EngineFactory.GetTaskEngine().GetByID(id), TaskStatus.Closed);

                    break;
                case 3:

                    foreach (var id in ids)
                        Global.EngineFactory.GetTaskEngine().Delete(Global.EngineFactory.GetTaskEngine().GetByID(id));

                    break;
                case 4:

                    foreach (var id in ids)
                    {

                        Task task = Global.EngineFactory.GetTaskEngine().GetByID(id);

                        task.Milestone = entityID;

                        Global.EngineFactory.GetTaskEngine().SaveOrUpdate(task,null,false);

                    }

                    break;
                case 5:
                    foreach (var id in ids)
                        Global.EngineFactory.GetTaskEngine().ChangeStatus(Global.EngineFactory.GetTaskEngine().GetByID(id), TaskStatus.NotAccept);


                    break;
                case 6:
                    foreach (var id in ids)
                    {

                        Task task = Global.EngineFactory.GetTaskEngine().GetByID(id);

                        task.Milestone = 0;

                        Global.EngineFactory.GetTaskEngine().SaveOrUpdate(task,null,false);


                    }

                    break;
            }

        }

        [AjaxMethod]
        public string ExecBeResponsible(int taskID)
        {
            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
            ProjectFat = new ProjectFat(task.Project);
            task.Responsible = SecurityContext.CurrentAccount.ID;
            Global.EngineFactory.GetTaskEngine().SaveOrUpdate(task,null,true);
            var page = new Page();
            ProjectFat = new ProjectFat(task.Project);
            Global.EngineFactory.GetTaskEngine().ChangeStatus(task, TaskStatus.Open);
            var cntrlOpenTaskBlockViewRow = (OpenTaskBlockViewRow)LoadControl(PathProvider.GetControlVirtualPath("OpenTaskBlockViewRow.ascx"));
            cntrlOpenTaskBlockViewRow.ProjectFat = ProjectFat;
            cntrlOpenTaskBlockViewRow.Target = task;
            cntrlOpenTaskBlockViewRow.Status = RenderStatus(task);
            cntrlOpenTaskBlockViewRow.RowIndex = 0;
            cntrlOpenTaskBlockViewRow.ExecuteHtmlEncode = true;
            page.Controls.Add(cntrlOpenTaskBlockViewRow);
            System.IO.StringWriter writer = new System.IO.StringWriter();
            HttpContext.Current.Server.Execute(page, writer, false);
            var output = writer.ToString();
            writer.Close();

            return output;
        }

        [AjaxMethod]
        public string ChangeTaskDaedline(int daysCount)
        {
            ProjectSecurity.DemandAuthentication();

            return TenantUtil.DateTimeNow().AddDays(daysCount).ToString(DateTimeExtension.DateFormatPattern);
        }

        [AjaxMethod]
        public object GetCurrUser(int taskID)
        {
            ProjectSecurity.DemandAuthentication();
            
            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);

            string deadline = task.Deadline != DateTime.MinValue ? task.Deadline.ToString(DateTimeExtension.DateFormatPattern) : string.Empty;

            return new
                  {
                      Title = task.Title,
                      task.ID,
                      ResponsibleID = task.Responsible,
                      task.Priority,
                      Description = task.Description,
                      MilestoneID = task.Milestone,
                      Deadline = deadline
                  };
        }

        [AjaxMethod]
        public object GetUploadedFiles(int taskID)
        {
            ProjectSecurity.DemandAuthentication();
            
            if (taskID == -1) return null;

            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
            var files = FileEngine2.GetTaskFiles(task);
            var uploadedFiles = new List<Object>();

            if (files.Count > 0)
            {
                foreach (var fileInfo in files)
                {
                    uploadedFiles.Add(new
                    {
                        Name = fileInfo.Title,
                        Size = fileInfo.ContentLength,
                        URL = fileInfo.FileUri,
                        TargerContainerID = "task_uploadedContainer",
                        RemoveHandler = "function (fileInfo) {return ASC.Projects.TaskActionPage.deleteFile(" + String.Format("{0},{1}, '{2}'", fileInfo.ID, fileInfo.Version, fileInfo.Title) + " );}"

                    });
                }
            }

            return uploadedFiles;
        }

        [AjaxMethod]
        public string ChangeTaskStatus(int taskID, bool isClosed, bool isAllMyTasks, bool isOneList)
        {
            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
            ProjectFat = new ProjectFat(task.Project);
            var output = string.Empty;

            if (task.Status == TaskStatus.Unclassified || task.Responsible == Guid.Empty)
            {
                task.Responsible = SecurityContext.CurrentAccount.ID;
            }

            var page = new Page();

            if (isClosed)
            {
                Global.EngineFactory.GetTaskEngine().ChangeStatus(task, TaskStatus.Closed);
                var cntrlClosedTaskBlockViewRow = (ClosedTaskBlockViewRow)LoadControl(PathProvider.GetControlVirtualPath("ClosedTaskBlockViewRow.ascx"));
                cntrlClosedTaskBlockViewRow.ProjectFat = ProjectFat;
                cntrlClosedTaskBlockViewRow.Target = task;
                cntrlClosedTaskBlockViewRow.Status = RenderStatus(task);
                cntrlClosedTaskBlockViewRow.RowIndex = 0;
                cntrlClosedTaskBlockViewRow.IsAllMyTasks = isAllMyTasks;
                cntrlClosedTaskBlockViewRow.OneList = isOneList;
                cntrlClosedTaskBlockViewRow.ExecuteHtmlEncode = true;
                if (task.Milestone != 0)
                    cntrlClosedTaskBlockViewRow.MilestoneName = Global.EngineFactory.GetMilestoneEngine().GetByID(task.Milestone).Title;
                page.Controls.Add(cntrlClosedTaskBlockViewRow);
                System.IO.StringWriter writer = new System.IO.StringWriter();
                HttpContext.Current.Server.Execute(page, writer, false);
                output = writer.ToString();
                writer.Close();
            }
            else
            {
                Global.EngineFactory.GetTaskEngine().ChangeStatus(task, TaskStatus.Open);
                var cntrlOpenTaskBlockViewRow = (OpenTaskBlockViewRow)LoadControl(PathProvider.GetControlVirtualPath("OpenTaskBlockViewRow.ascx"));
                cntrlOpenTaskBlockViewRow.ProjectFat = ProjectFat;
                cntrlOpenTaskBlockViewRow.Target = task;
                cntrlOpenTaskBlockViewRow.Status = RenderStatus(task);
                cntrlOpenTaskBlockViewRow.RowIndex = 0;
                cntrlOpenTaskBlockViewRow.IsAllMyTasks = isAllMyTasks;
                cntrlOpenTaskBlockViewRow.OneList = isOneList;
                cntrlOpenTaskBlockViewRow.ExecuteHtmlEncode = true;
                if (task.Milestone != 0)
                    cntrlOpenTaskBlockViewRow.MilestoneName = Global.EngineFactory.GetMilestoneEngine().GetByID(task.Milestone).Title;
                cntrlOpenTaskBlockViewRow.HasTime = Global.EngineFactory.GetTimeTrackingEngine().HasTime(task.ID);
                page.Controls.Add(cntrlOpenTaskBlockViewRow);
                System.IO.StringWriter writer = new System.IO.StringWriter();
                HttpContext.Current.Server.Execute(page, writer, false);
                output = writer.ToString();
                writer.Close();
            }

            return output;
        }

        [AjaxMethod]
        public void MoveTaskToMilestone(int taskID, int milestoneID)
        {
            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
            task.Milestone = milestoneID;
            Global.EngineFactory.GetTaskEngine().SaveOrUpdate(task,null,false);
        }

        [AjaxMethod]
        public string NotifyTaskResponsible(int taskID)
        {
            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);

            ProjectSecurity.DemandEdit(task);

            Global.EngineFactory.GetTaskEngine().NotifyResponsible(task);
            return string.Format(TaskResource.NotifyTaskResponsible,
                                CoreContext.UserManager.GetUsers(task.Responsible).DisplayUserName(true).ReplaceSingleQuote(),
                                task.Title);
        }

        //protected Hashtable GetFileListInfoHashtable(List<ASC.Files.Core.File> uploadedFiles)
        //{
        //    var fileListInfoHashtable = new Hashtable();

        //    foreach (var file in uploadedFiles)
        //    {
        //        String fileInfo = String.Format("{0} ({1}, {2})", file.Title, Global.GetFileExtension(file.Title).ToUpper(), Global.ContentLengthToString(file.ContentLength));
        //        fileListInfoHashtable.Add(fileInfo, file.PreviewUrl);
        //    }

        //    return fileListInfoHashtable;
        //}

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskDetailsView));
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

            //if (OtherMilestones != null && OtherMilestones.Count > 0)
            //{
            //    _milestoneDropdownContainer.DataSource = OtherMilestones;
            //    _milestoneDropdownContainer.DataBind();
            //}

            InitControls();

        }

        #endregion

    }
}