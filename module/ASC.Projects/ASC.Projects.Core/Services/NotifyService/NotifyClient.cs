using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Projects.Core.Domain;
using ASC.Core.Tenants;

namespace ASC.Projects.Core.Services.NotifyService
{
    public class NotifyClient
    {
        private static NotifyClient instance;
        private readonly INotifyClient client;
        private readonly INotifySource source;

        public static NotifyClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(NotifyClient))
                    {
                        if (instance == null) instance = new NotifyClient(WorkContext.NotifyContext.NotifyService.RegisterClient(NotifySource.Instance), NotifySource.Instance);
                    }
                }
                return instance;
            }
        }

        public INotifyClient Client
        {
            get { return client; }
        }


        private NotifyClient(INotifyClient client, INotifySource source)
        {
            this.client = client;
            this.source = source;
        }


        public void SendInvaiteToProjectTeam(Guid userId, Project project)
        {
            var recipient = ToRecipient(userId);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_InviteToProject,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title));
            }
        }

        public void SendRemovingFromProjectTeam(Guid userId, Project project)
        {
            var recipient = ToRecipient(userId);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_RemoveFromProject,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title));
            }
        }

        public void SendMilestoneDeadline(Guid userID, Milestone milestone)
        {
            var recipient = ToRecipient(userID);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_MilestoneDeadline,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, milestone.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, milestone.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, milestone.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, milestone.ID));
            }
        }

        public void SendAboutResponsibleByProject(Guid responsible, Project project)
        {
            var recipient = ToRecipient(responsible);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ResponsibleForProject,
                    project.UniqID,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title),
                    new TagValue(NotifyConstants.Tag_AdditionalData, project.Description));
            }
        }

        public void SendAboutResponsibleByTask(Guid responsible, Task task, Hashtable fileListInfoHashtable)
        {
            var recipient = ToRecipient(responsible);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ResponsibleForTask,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", HttpUtility.HtmlEncode(task.Description) }, { "Files", fileListInfoHashtable } }));
            }
        }

        public void SendReminderAboutTask(Guid responsible, Task task, Hashtable fileListInfoHashtable)
        {
            var recipient = ToRecipient(responsible);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ReminderAboutTask,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", HttpUtility.HtmlEncode(task.Description) }, { "Files", fileListInfoHashtable } }));
            }
        }

        public void SendReminderAboutTaskDeadline(Guid responsible, Task task)
        {
            var recipient = ToRecipient(responsible);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ReminderAboutTaskDeadline,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", HttpUtility.HtmlEncode(task.Description) }, { "TaskDeadline", task.Deadline.ToString() } }));
            }
        }

        public void SendAboutFileUploaded(Guid user, Project project, Hashtable additional)
        {
            var recipient = ToRecipient(user);
            if (recipient != null)
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_UploadFiles,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title),
                    new TagValue(NotifyConstants.Tag_AdditionalData, additional));
            }
        }

        public void SendNewComment(ProjectEntity entity, string comment)
        {
            INotifyAction action = null;
            if (entity.GetType() == typeof(Issue)) action = NotifyConstants.Event_NewCommentForIssue;
            else if (entity.GetType() == typeof(Message)) action = NotifyConstants.Event_NewCommentForMessage;
            else if (entity.GetType() == typeof(Milestone)) action = NotifyConstants.Event_NewCommentForMilestone;
            else if (entity.GetType() == typeof(Task)) action = NotifyConstants.Event_NewCommentForTask;
            else return;

            var interceptor = new InitiatorInterceptor(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            try
            {
                client.AddInterceptor(interceptor);
                client.SendNoticeAsync(
                    action,
                    entity.NotifyId,
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectID, entity.Project.ID),
                    new TagValue(NotifyConstants.Tag_ProjectTitle, entity.Project.Title),
                    new TagValue(NotifyConstants.Tag_EntityTitle, entity.Title),
                    new TagValue(NotifyConstants.Tag_EntityID, entity.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, comment));
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
            }
        }

        public void SendProjectCreateRequest(ProjectChangeRequest request)
        {
            var additional = new Hashtable
            {
                { "Description",   request.Description },
                { "ProjectLeader", GetUserName(request.Responsible) }
            };
            foreach (var recipient in GetAdminsAsRecipient())
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ProjectCreateRequest,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectTitle, request.Title),
                    new TagValue(NotifyConstants.Tag_AdditionalData, additional));
            }
        }

        public void SendProjectEditRequest(Project project, ProjectChangeRequest request)
        {
            var additional = new Hashtable
            {
                { "Title_Old", project.Title },
                { "Title_NEW", request.Title }, 
                { "Description_Old", project.Description },
                { "Description_NEW", request.Description },
                { "ProjectLeader_Old", GetUserName(project.Responsible) },
                { "ProjectLeader_NEW", GetUserName(request.Responsible) }
            };
            foreach (var recipient in GetAdminsAsRecipient())
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ProjectEditRequest,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectTitle, project.Title),
                    new TagValue(NotifyConstants.Tag_ProjectID, project.ID),
                    new TagValue(NotifyConstants.Tag_AdditionalData, additional));
            }
        }

        public void SendProjectRemoveRequest(ProjectChangeRequest request)
        {
            var additional = new Hashtable
            {
                { "Description",   request.Description },
                { "ProjectLeader", GetUserName(request.Responsible) }
            };
            foreach (var recipient in GetAdminsAsRecipient())
            {
                client.SendNoticeToAsync(
                    NotifyConstants.Event_ProjectRemoveRequest,
                    null,
                    new[] { recipient },
                    GetDefaultSenders(recipient),
                    null,
                    new TagValue(NotifyConstants.Tag_ProjectTitle, request.Title),
                    new TagValue(NotifyConstants.Tag_AdditionalData, additional));
            }
        }

        public void SendProjectAcceptRequest(ProjectChangeRequest request)
        {
            var recipient = new DirectRecipient(request.CreateBy.ToString(), GetUserName(request.CreateBy));
            client.SendNoticeToAsync(
                NotifyConstants.Event_ProjectAcceptRequest,
                null,
                new[] { recipient },
                GetDefaultSenders(recipient),
                null,
                new TagValue(NotifyConstants.Tag_ProjectTitle, request.Title),
                new TagValue(NotifyConstants.Tag_ProjectID, request.ProjectID));
        }

        public void SendProjectRejectRequest(ProjectChangeRequest request)
        {
            var recipient = new DirectRecipient(request.CreateBy.ToString(), GetUserName(request.CreateBy));
            client.SendNoticeToAsync(
                NotifyConstants.Event_ProjectRejectRequest,
                null,
                new[] { recipient },
                GetDefaultSenders(recipient),
                null,
                new TagValue(NotifyConstants.Tag_ProjectTitle, request.Title),
                new TagValue(NotifyConstants.Tag_ProjectID, request.ProjectID));
        }

        public void SendAboutTaskClosing(List<Guid> users, Task task)
        {
            client.BeginSingleRecipientEvent("task closed");
            var interceptor = new InitiatorInterceptor(new DirectRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString(), ""));
            client.AddInterceptor(interceptor);
            try
            {
                foreach (var user in users)
                {
                    var recipient = ToRecipient(user);
                    if (recipient != null)
                    {
                        client.SendNoticeToAsync(
                           NotifyConstants.Event_TaskClosed,
                           task.NotifyId,
                           new[] { recipient },
                           GetDefaultSenders(recipient),
                           null,
                           new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                           new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                           new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                           new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                           new TagValue(NotifyConstants.Tag_AdditionalData, HttpUtility.HtmlEncode(task.Description)));
                    }
                }
            }
            finally
            {
                client.RemoveInterceptor(interceptor.Name);
                client.EndSingleRecipientEvent("task closed");
            }
        }

        public void SendAboutTaskCreating(List<Guid> users, Task task, Hashtable fileListInfoHashtable)
        {
            foreach (var user in users)
            {
                var recipient = ToRecipient(user);
                if (recipient != null)
                {
                    client.SendNoticeToAsync(
                        NotifyConstants.Event_TaskCreated,
                        null,
                        new[] { recipient },
                        GetDefaultSenders(recipient),
                        null,
                        new TagValue(NotifyConstants.Tag_ProjectID, task.Project.ID),
                        new TagValue(NotifyConstants.Tag_ProjectTitle, task.Project.Title),
                        new TagValue(NotifyConstants.Tag_EntityTitle, task.Title),
                        new TagValue(NotifyConstants.Tag_EntityID, task.ID),
                        new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "TaskDescription", HttpUtility.HtmlEncode(task.Description) }, { "Files", fileListInfoHashtable } }));
                }
            }
        }

        public void SendAboutMessageAction(Message message, string realAction, Hashtable fileListInfoHashtable)
        {
            client.SendNoticeAsync(
                NotifyConstants.Event_NewCommentForMessage,
                message.NotifyId,
                null,
                new TagValue(NotifyConstants.Tag_ProjectID, message.Project.ID),
                new TagValue(NotifyConstants.Tag_ProjectTitle, message.Project.Title),
                new TagValue(NotifyConstants.Tag_EntityTitle, message.Title),
                new TagValue(NotifyConstants.Tag_EntityID, message.ID),
                new TagValue(NotifyConstants.Tag_EventType, realAction),
                new TagValue(NotifyConstants.Tag_AdditionalData, new Hashtable { { "MessagePreview", message.Content }, { "Files", fileListInfoHashtable } }));
        }

        public void SendAboutImportComplite(Guid user)
        {
            var recipient = ToRecipient(user);
            if (recipient != null)
            {
                client.SendNotice(NotifyConstants.Event_ImportData, null, ToRecipient(user));
            }
        }

        private IRecipient[] GetAdminsAsRecipient()
        {
            return CoreContext.UserManager
                .GetUsersByGroup(Constants.GroupAdmin.ID)
                .Select(u => new DirectRecipient(u.ID.ToString(), UserFormatter.GetUserName(u)))
                .ToArray();
        }

        private string[] GetDefaultSenders(IRecipient recipient)
        {
            return source.GetSubscriptionProvider().GetSubscriptionMethod(
                    NotifyConstants.Event_NewCommentForMessage,
                    recipient);
        }

        private IRecipient ToRecipient(Guid userID)
        {
            return source.GetRecipientsProvider().GetRecipient(userID.ToString());
        }

        private string GetUserName(Guid id)
        {
            return UserFormatter.GetUserName(CoreContext.UserManager.GetUsers(id));
        }
    }
}