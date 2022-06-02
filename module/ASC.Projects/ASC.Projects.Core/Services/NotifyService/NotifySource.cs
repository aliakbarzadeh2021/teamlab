using System;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using NotifySourceBase = ASC.Core.Notify.NotifySource;

namespace ASC.Projects.Core.Services.NotifyService
{
    public class NotifySource : NotifySourceBase, IDependencyProvider
    {
        private static NotifySource instance;


        public static NotifySource Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(NotifySource))
                    {
                        if (instance == null) instance = new NotifySource();
                    }
                }
                return instance;
            }
        }

        public NotifySource()
            : base(new Guid("{6045B68C-2C2E-42db-9E53-C272E814C4AD}"))
        {

        }

        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                NotifyConstants.Event_UploadFiles,
                NotifyConstants.Event_NewCommentForMessage,
                NotifyConstants.Event_NewCommentForTask,
                NotifyConstants.Event_NewCommentForMilestone,
                NotifyConstants.Event_NewCommentForIssue,
                NotifyConstants.Event_MilestoneDeadline,
                NotifyConstants.Event_ResponsibleForProject,
                NotifyConstants.Event_ResponsibleForTask,
                NotifyConstants.Event_ReminderAboutTask,
                NotifyConstants.Event_ReminderAboutTaskDeadline,
                NotifyConstants.Event_TaskClosed,
                NotifyConstants.Event_TaskCreated,
                NotifyConstants.Event_InviteToProject,
                NotifyConstants.Event_RemoveFromProject,
                NotifyConstants.Event_ProjectCreateRequest,
                NotifyConstants.Event_ProjectEditRequest,
                NotifyConstants.Event_ProjectRemoveRequest,
                NotifyConstants.Event_ProjectAcceptRequest,
                NotifyConstants.Event_ProjectRejectRequest,
                NotifyConstants.Event_MessageCreated,
                NotifyConstants.Event_MessageEdited,
                NotifyConstants.Event_ImportData);
        }

        protected override IActionPatternProvider CreateActionPatternProvider()
        {
            return new XmlActionPatternProvider(
                GetType().Assembly,
                "ASC.Projects.Core.Services.NotifyService.action_pattern.xml",
                ActionProvider,
                PatternProvider
            ) { GetPatternMethod = ChoosePattern };
        }

        private IPattern ChoosePattern(INotifyAction action, string senderName, ASC.Notify.Engine.NotifyRequest request)
        {
            if (action == NotifyConstants.Event_NewCommentForMessage)
            {
                var tag = request.Arguments.Find(tv => tv.Tag.Name == "EventType");
                if (tag != null) return ActionPatternProvider.GetPattern(new NotifyAction(Convert.ToString(tag.Value), ""), senderName);
            }
            return null;
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider(Resources.ProjectsPatternResource.patterns);
        }
    }
}
