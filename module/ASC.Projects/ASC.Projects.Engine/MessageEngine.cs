using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Projects.Core;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Users.Activity;
using IDaoFactory = ASC.Projects.Core.DataInterfaces.IDaoFactory;

namespace ASC.Projects.Engine
{
    public class MessageEngine
    {
        private readonly EngineFactory _engineFactory;
        private readonly IMessageDao messageDao;


        public MessageEngine(IDaoFactory daoFactory, EngineFactory engineFactory)
        {
            _engineFactory = engineFactory;
            messageDao = daoFactory.GetMessageDao();
        }


        public Message GetByID(int id)
        {
            return messageDao.GetById(id);
        }

        public List<Message> GetByProject(int projectID)
        {
            return messageDao.GetByProject(projectID);
        }

        public List<Message> GetMessages(int startIndex,int maxResult)
        {
            return messageDao.GetMessages(startIndex,maxResult);
        }

        public List<Message> GetRecentMessages(int maxResult)
        {
            int offset = 0;
            var recentMessages = new List<Message>();
            var messages = messageDao.GetRecentMessages(offset, maxResult)
                .Where(m => CanRead(m.Project))
                .ToList();

            recentMessages.AddRange(messages);

            if (recentMessages.Count < maxResult)
            {
                do
                {
                    offset = offset + maxResult;
                    messages = messageDao.GetRecentMessages(offset, maxResult);

                    if (messages.Count == 0)
                        return recentMessages;
                    else messages = messages
                        .Where(m => CanRead(m.Project))
                        .ToList();

                    recentMessages.AddRange(messages);
                }
                while (recentMessages.Count < maxResult);
            }

            return recentMessages.Count == maxResult ? recentMessages : recentMessages.GetRange(0, maxResult);
        }

        public List<Message> GetRecentMessages(int maxResult, params int[] projectID)
        {
            int offset = 0;
            var recentMessages = new List<Message>();
            var messages = messageDao.GetRecentMessages(offset, maxResult, projectID)
                .Where(m => CanRead(m.Project))
                .ToList();

            recentMessages.AddRange(messages);

            if (recentMessages.Count < maxResult)
            {
                do
                {
                    offset = offset + maxResult;
                    messages = messageDao.GetRecentMessages(offset, maxResult, projectID);

                    if (messages.Count == 0)
                        return recentMessages;
                    else messages = messages
                        .Where(m => CanRead(m.Project))
                        .ToList();

                    recentMessages.AddRange(messages);
                }
                while (recentMessages.Count < maxResult);
            }

            return recentMessages.Count == maxResult ? recentMessages : recentMessages.GetRange(0, maxResult);
        }

        public bool IsExists(int id)
        {
            return messageDao.IsExists(id);
        }

        public Message SaveOrUpdate(Message message,bool notify, IEnumerable<Guid> participant, IEnumerable<int > fileIds)
        {
            return SaveOrUpdate(message, notify, participant, fileIds, false);
        }

        public Message SaveOrUpdate(Message message, bool notify, IEnumerable<Guid> participant, IEnumerable<int> fileIds, bool isImport)
        {
            if (message == null) throw new ArgumentNullException("message");

            var isNew = true;

            message.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            message.LastModifiedOn = TenantUtil.DateTimeNow();
            
            if (message.ID == default(int))
            {
                if (message.CreateBy == default(Guid)) message.CreateBy = SecurityContext.CurrentAccount.ID;
                if (message.CreateOn == default(DateTime)) message.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreateMessage(message.Project);
                messageDao.Save(message);

                if (isImport)
                {
                    TimeLinePublisher.Message(message, EngineResource.ActionText_Imported, UserActivityConstants.ActivityActionType, UserActivityConstants.NormalActivity);
                }
                else
                {
                    TimeLinePublisher.Message(message, EngineResource.ActionText_Create, UserActivityConstants.ActivityActionType, UserActivityConstants.NormalActivity);
                }
            }
            else
            {
                ProjectSecurity.DemandEdit(message);
                messageDao.Save(message);
                isNew = false;
                TimeLinePublisher.Message(message, EngineResource.ActionText_Update, UserActivityConstants.ContentActionType, UserActivityConstants.NormalContent,true);
            }
            var fileEngine = _engineFactory.GetFileEngine();
            if (fileIds != null)
            {
                foreach (var fileId in fileIds)
                {
                    fileEngine.AttachFileToMessage(message.ID,fileId);
                }
            }

            NotifyParticipiant(message, isNew, participant, fileEngine.GetMessageFiles(message), notify);

            return message;
        }

        public void Delete(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.Project == null) throw new Exception("Project");

            ProjectSecurity.DemandEdit(message);
            TimeLinePublisher.Message(message, EngineResource.ActionText_Delete, UserActivityConstants.ActivityActionType, UserActivityConstants.SmallActivity);

            messageDao.Delete(message.ID);

            String objectID = String.Format("{0}_{1}", message.UniqID, message.Project.ID);
            NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForMessage, objectID);
        }

        protected void NotifyParticipiant(Message message, bool isMessageNew, IEnumerable<Guid> participant, IEnumerable<Files.Core.File> uploadedFiles, bool sendNotify)
        {
            //Don't send anything if notifications are disabled
            if (_engineFactory.DisableNotifications) return;

            var objectId = message.NotifyId;
            var subscriptionRecipients = NotifySource.Instance.GetSubscriptionProvider().GetRecipients(NotifyConstants.Event_NewCommentForMessage, objectId);
            var recipients = new List<Guid>(participant);

            foreach (var subscriptionRecipient in subscriptionRecipients)
            {
                var subscriptionRecipientId = new Guid(subscriptionRecipient.ID);
                if (!recipients.Contains(subscriptionRecipientId))
                {
                    NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForMessage, objectId, subscriptionRecipient);
                }
                else
                {
                    recipients.Remove(subscriptionRecipientId);
                }
            }

            recipients
                .Select(r => NotifySource.Instance.GetRecipientsProvider().GetRecipient(r.ToString()))
                .Where(r => r != null)
                .ToList()
                .ForEach(r => NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForMessage, objectId, r));

            if (sendNotify)
                NotifyClient.Instance.SendAboutMessageAction(message, isMessageNew?"NewMessage":"EditMessage", FileEngine.GetFileListInfoHashtable(uploadedFiles));
        }

        public Comment SaveMessageComment(Message message, Comment comment)
        {
            _engineFactory.GetCommentEngine().SaveOrUpdate(comment);
            NotifyComment(comment, message);
            TimeLinePublisher.Comment(message, EngineResource.ActionText_Add);
            return comment;
        }

        private void NotifyComment(Comment comment, Message message)
        {
            //Don't send anything if notifications are disabled
            if (_engineFactory.DisableNotifications) return;

            foreach (Participant prt in _engineFactory.GetProjectEngine().GetTeam(message.Project.ID))
            {
                if (prt.ID == SecurityContext.CurrentAccount.ID)
                {
                    var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
                        NotifyConstants.Event_NewCommentForMessage,
                        NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                            SecurityContext.CurrentAccount.ID.ToString())
                                                       ));
                    bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, message.NotifyId, true) == 0));
                    if (!subscribed)
                    {
                        NotifySource.Instance.GetSubscriptionProvider().Subscribe(
                            NotifyConstants.Event_NewCommentForMessage,
                            message.NotifyId,
                            NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));
                    }
                }
            }
            //////////////////////////////////////////

            NotifyClient.Instance.SendNewComment(message, comment.Content);
        }

        public bool CanRead(Project project)
        {
            if (ProjectSecurity.CanRead(project))
                return ProjectSecurity.CanReadMessages(project);
            return false;
        }
    }
}