using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Users.Activity;
using IDaoFactory = ASC.Projects.Core.DataInterfaces.IDaoFactory;

namespace ASC.Projects.Engine
{
    public class TaskEngine
    {
        private readonly EngineFactory _factory;
        private readonly ITaskDao taskDao;
        private readonly IMilestoneDao milestoneDao;
        private readonly FileEngine fileEngine;

        public TaskEngine(IDaoFactory daoFactory, EngineFactory factory)
        {
            _factory = factory;
            taskDao = daoFactory.GetTaskDao();
            milestoneDao = daoFactory.GetMilestoneDao();
            this.fileEngine = factory.GetFileEngine();
        }


        public List<Task> GetByProject(int projectId, TaskStatus? status, Guid participant)
        {
            return taskDao.GetByProject(projectId, status, participant).Where(t => CanRead(t)).ToList();
        }

        public List<Task> GetByResponsible(Guid responsibleId)
        {
            return taskDao.GetByResponsible(responsibleId, null);
        }

        public List<Task> GetByResponsible(Guid responsibleId, params TaskStatus[] statuses)
        {
            return taskDao.GetByResponsible(responsibleId, statuses);
        }

        public List<Task> GetLastTasks(Guid participant, int max)
        {
            return taskDao.GetLastTasks(participant, max);
        }

        public List<Task> GetMilestoneTasks(int milestoneId)
        {
            return taskDao.GetMilestoneTasks(milestoneId);
        }

        public Task GetByID(int id)
        {
            return taskDao.GetById(id);
        }

        public List<Task> GetByID(ICollection<int> ids)
        {
            return taskDao.GetById(ids);

        }

        public bool IsExists(int id)
        {
            return taskDao.IsExists(id);
        }

        public int GetTaskCount(int milestoneId, params TaskStatus[] statuses)
        {
            return taskDao.GetTaskCount(milestoneId, statuses);
        }

        public void SetTaskOrders(int? milestoneId, int taskID, int? prevTaskID, int? nextTaskID)
        {
            var task = taskDao.GetById(taskID);
            if (task != null)
            {
                ProjectSecurity.DemandEdit(task);
                taskDao.SetTaskOrders(milestoneId, taskID, prevTaskID, nextTaskID);
            }
        }

        public Task SaveOrUpdate(Task task, IEnumerable<int> attachedFileIds, bool notifyResponsible)
        {
            return SaveOrUpdate(task, attachedFileIds, notifyResponsible, false);
        }

        public Task SaveOrUpdate(Task task, IEnumerable<int> attachedFileIds, bool notifyResponsible, bool isImport)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (task.Project == null) throw new Exception("task.Project");

            var milestone = task.Milestone != 0 ? milestoneDao.GetById(task.Milestone) : null;

            task.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            task.LastModifiedOn = TenantUtil.DateTimeNow();

            var isNew = task.ID == default(int);//Task is new

            if (isNew)
            {
                if (task.CreateBy == default(Guid)) task.CreateBy = SecurityContext.CurrentAccount.ID;
                if (task.CreateOn == default(DateTime)) task.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreateTask(task.Project);
                task = taskDao.Save(task);

                if (isImport)
                {
                    TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_Imported, UserActivityConstants.ContentActionType, UserActivityConstants.NormalContent, true);
                }
                else
                {
                    TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_Create, UserActivityConstants.ContentActionType, UserActivityConstants.NormalContent, true);
                }
            }
            else
            {
                //changed task
                ProjectSecurity.DemandEdit(GetByID(new[] { task.ID }).FirstOrDefault());
                task = taskDao.Save(task);
                TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_Update, UserActivityConstants.ActivityActionType, UserActivityConstants.NormalActivity);
            }

            //NOTE: We calling this here because the ChangeStatus() do many usefull things. And we need a task saved in DB to this things
            if (task.Responsible.Equals(Guid.Empty))
            {
                ChangeStatus(task, TaskStatus.Unclassified);
            }
            else
            {
                if (task.Status == TaskStatus.Unclassified) ChangeStatus(task, TaskStatus.Open);
            }

            if (attachedFileIds != null && attachedFileIds.Count() > 0)
            {
                foreach (var attachedFileId in attachedFileIds)
                {
                    fileEngine.AttachFileToTask(task.ID, attachedFileId);
                }
            }

            var uploadedFiles = fileEngine.GetTaskFiles(task);
            NotifyTask(task, notifyResponsible, isNew, uploadedFiles);

            var senders = new List<Guid> { SecurityContext.CurrentAccount.ID };

            if (SecurityContext.CurrentAccount.ID != task.Responsible && !task.Responsible.Equals(Guid.Empty))
                senders.Add(task.Responsible);

            //Object id for sender
            var objectId = task.UniqID + "_" + task.Project.ID;
            foreach (var recipientId in senders)
            {
                NotifySource.Instance.GetSubscriptionProvider().Subscribe(
                    NotifyConstants.Event_NewCommentForTask,
                    objectId,
                    NotifySource.Instance.GetRecipientsProvider().
                    GetRecipient(recipientId.ToString()));
            }
            return task;
        }

        private void NotifyTask(Task task, bool notifyResponsible, bool isNew, IEnumerable<Files.Core.File> uploadedFiles)
        {
            //Don't send anything if notifications are disabled
            if (_factory.DisableNotifications) return;

            if (notifyResponsible && task.Responsible != Guid.Empty && task.Responsible != SecurityContext.CurrentAccount.ID)
            {
                NotifyClient.Instance.SendAboutResponsibleByTask(task.Responsible, task, FileEngine.GetFileListInfoHashtable(uploadedFiles));
            }
            if (SecurityContext.CurrentAccount.ID != task.Project.Responsible && isNew)
            {
                var users = new List<Guid> { task.Project.Responsible };
                NotifyClient.Instance.SendAboutTaskCreating(users, task, FileEngine.GetFileListInfoHashtable(uploadedFiles));
            }
        }

        public void NotifyResponsible(Task task)
        {
            //Don't send anything if notifications are disabled
            if (_factory.DisableNotifications) return;

            var files = fileEngine.GetTaskFiles(task);
            if (task.Status != TaskStatus.Unclassified && task.Responsible != Guid.Empty && task.Responsible != SecurityContext.CurrentAccount.ID)
                NotifyClient.Instance.SendReminderAboutTask(task.Responsible, task, FileEngine.GetFileListInfoHashtable(files));
        }



        public void Delete(Task task)
        {
            if (task == null) throw new ArgumentNullException("task");

            ProjectSecurity.DemandEdit(task);
            taskDao.Delete(task.ID);

            var milestone = task.Milestone != 0 ? milestoneDao.GetById(task.Milestone) : null;
            TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_Delete, UserActivityConstants.ActivityActionType, UserActivityConstants.NormalActivity);
        }

        public Task ChangeStatus(Task task, TaskStatus newStatus)
        {
            ProjectSecurity.DemandEdit(task);

            if (task == null) throw new ArgumentNullException("task");
            if (task.Project == null) throw new Exception("Project can be null.");
            if (task.Status == newStatus) return task;

            var objectID = String.Format("{0}_{1}", task.UniqID, task.Project.ID);
            var context = HttpContext.Current;
            var milestone = task.Milestone != 0 ? milestoneDao.GetById(task.Milestone) : null;

            switch (newStatus)
            {
                case TaskStatus.Closed:
                    TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_Closed, UserActivityConstants.ActivityActionType, UserActivityConstants.ImportantActivity);
                    break;

                case TaskStatus.Disable:
                    TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_MoveToTrash, UserActivityConstants.ActivityActionType, UserActivityConstants.NormalActivity);
                    NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForTask, objectID);
                    break;

                case TaskStatus.Open:
                    if (task.Status == TaskStatus.NotAccept)
                    {
                        TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_Accept, UserActivityConstants.ActivityActionType, UserActivityConstants.SmallActivity);
                    }
                    else
                    {
                        TimeLinePublisher.Task(task, milestone, EngineResource.ActionText_Reopen, UserActivityConstants.ActivityActionType, UserActivityConstants.NormalActivity);
                    }
                    break;
            }

            var senders = new List<Guid> { task.Project.Responsible, task.CreateBy };

            if (!task.Responsible.Equals(Guid.Empty)) senders.Add(task.Responsible);

            if (newStatus == TaskStatus.Closed && !_factory.DisableNotifications)
                NotifyClient.Instance.SendAboutTaskClosing(senders, task);

            taskDao.TaskTrace(task.ID, (Guid)CallContext.GetData("CURRENT_ACCOUNT"), TenantUtil.DateTimeNow(), newStatus);

            task.Status = newStatus;
            task.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            task.LastModifiedOn = TenantUtil.DateTimeNow();

            return taskDao.Save(task);
        }

        public Comment SaveOrUpdateTaskComment(Task task, Comment comment)
        {
            _factory.GetCommentEngine().SaveOrUpdate(comment);

            NotifyNewComment(comment, task);
            TimeLinePublisher.Comment(task, EngineResource.ActionText_Add);
            return comment;
        }

        private void NotifyNewComment(Comment comment, Task task)
        {
            //Don't send anything if notifications are disabled
            if (_factory.DisableNotifications) return;

            foreach (var prt in _factory.GetProjectEngine().GetTeam(task.Project.ID))
            {
                if (prt.ID == SecurityContext.CurrentAccount.ID)
                {
                    var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
                        NotifyConstants.Event_NewCommentForTask,
                        NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                            SecurityContext.CurrentAccount.ID.ToString())
                                                       ));
                    var subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, task.NotifyId, true) == 0));
                    if (!subscribed)
                    {
                        NotifySource.Instance.GetSubscriptionProvider().Subscribe(
                            NotifyConstants.Event_NewCommentForTask,
                            task.NotifyId,
                            NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));
                    }
                }
            }
            NotifyClient.Instance.SendNewComment(task, comment.Content);
        }

        private bool CanRead(Task t)
        {
            return ProjectSecurity.CanRead(t);
        }
    }
}