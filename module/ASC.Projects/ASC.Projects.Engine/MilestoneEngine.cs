using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Users.Activity;

namespace ASC.Projects.Engine
{
    public class MilestoneEngine
    {
        private readonly EngineFactory _engineFactory;
        private readonly IMilestoneDao milestoneDao;


        public MilestoneEngine(IDaoFactory daoFactory, EngineFactory engineFactory)
        {
            _engineFactory = engineFactory;
            milestoneDao = daoFactory.GetMilestoneDao();
        }


        public List<Milestone> GetByProject(int projectId)
        {
            var milestones = milestoneDao.GetByProject(projectId).Where(m => CanRead(m)).ToList();
            milestones.Sort((x, y) =>
            {
                if (x.Status != y.Status) return x.Status.CompareTo(y.Status);
                if (x.Status == MilestoneStatus.Open) return x.DeadLine.CompareTo(y.DeadLine);
                return y.DeadLine.CompareTo(x.DeadLine);
            });
            return milestones;
        }

        public List<Milestone> GetUpcomingMilestones(int max, params int[] projects)
        {
            var offset = 0;
            var milestones = new List<Milestone>();
            while (true)
            {
                var packet = milestoneDao.GetUpcomingMilestones(offset, 2 * max, projects);
                milestones.AddRange(packet.Where(m => CanRead(m)));
                if (max <= milestones.Count || packet.Count() < 2 * max)
                {
                    break;
                }
                offset += 2 * max;
            }
            return milestones.Count <= max ? milestones : milestones.GetRange(0, max);
        }

        public List<Milestone> GetLateMilestones(int max, params int[] projects)
        {
            var offset = 0;
            var milestones = new List<Milestone>();
            while (true)
            {
                var packet = milestoneDao.GetLateMilestones(offset, 2 * max, projects);
                milestones.AddRange(packet.Where(m => CanRead(m)));
                if (max <= milestones.Count || packet.Count() < 2 * max)
                {
                    break;
                }
                offset += 2 * max;
            }
            return milestones.Count <= max ? milestones : milestones.GetRange(0, max);
        }

        public List<Milestone> GetByDeadLine(DateTime deadline)
        {
            return milestoneDao.GetByDeadLine(deadline).Where(m => CanRead(m)).ToList();
        }

        public Milestone GetByID(int id)
        {
            var m = milestoneDao.GetById(id);
            return CanRead(m) ? m : null;
        }

        public bool IsExists(int id)
        {
            return GetByID(id) != null;
        }

        public Milestone SaveOrUpdate(Milestone milestone)
        {
            return SaveOrUpdate(milestone, false);
        }

        public Milestone SaveOrUpdate(Milestone milestone, bool import)
        {
            if (milestone == null) throw new ArgumentNullException("milestone");
            if (milestone.Project == null) throw new Exception("milestone.project is null");

            milestone.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            milestone.LastModifiedOn = TenantUtil.DateTimeNow();

            if (milestone.ID == default(int))
            {
                if (milestone.CreateBy == default(Guid)) milestone.CreateBy = SecurityContext.CurrentAccount.ID;
                if (milestone.CreateOn == default(DateTime)) milestone.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreateMilestone(milestone.Project);
                milestone = milestoneDao.Save(milestone);
                TimeLinePublisher.Milestone(milestone, import ? EngineResource.ActionText_Imported : EngineResource.ActionText_Create, UserActivityConstants.ContentActionType, UserActivityConstants.ImportantContent);
            }
            else
            {
                ProjectSecurity.DemandEdit(milestone);
                milestone = milestoneDao.Save(milestone);
                TimeLinePublisher.Milestone(milestone, EngineResource.ActionText_Update, UserActivityConstants.ActivityActionType, UserActivityConstants.ImportantActivity);
            }
            var objectId = String.Format("{0}_{1}", milestone.UniqID, milestone.Project.ID);
            NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForMilestone, objectId, NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));
            return milestone;
        }

        public void Delete(Milestone milestone)
        {
            if (milestone == null) throw new ArgumentNullException("milestone");

            ProjectSecurity.DemandEdit(milestone);
            milestoneDao.Delete(milestone.ID);
        }

        public Comment SaveMilestoneComment(Milestone milestone, Comment comment)
        {
            _engineFactory.GetCommentEngine().SaveOrUpdate(comment);
            NotifyNewComment(comment, milestone);
            TimeLinePublisher.Comment(milestone, EngineResource.ActionText_Add);
            return comment;
        }

        private void NotifyNewComment(Comment comment, Milestone milestone)
        {
            //Don't send anything if notifications are disabled
            if (_engineFactory.DisableNotifications) return;

            foreach (Participant prt in _engineFactory.GetProjectEngine().GetTeam(milestone.Project.ID))
            {
                if (prt.ID == SecurityContext.CurrentAccount.ID)
                {
                    var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
                        NotifyConstants.Event_NewCommentForMilestone,
                        NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                            SecurityContext.CurrentAccount.ID.ToString())));
                    bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, milestone.NotifyId, true) == 0));
                    if (!subscribed)
                    {
                        NotifySource.Instance.GetSubscriptionProvider().Subscribe(
                            NotifyConstants.Event_NewCommentForMilestone,
                            milestone.NotifyId,
                            NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()));
                    }
                }
            }
            NotifyClient.Instance.SendNewComment(milestone, comment.Content);
        }

        private bool CanRead(Milestone m)
        {
            return ProjectSecurity.CanRead(m);
        }
    }
}