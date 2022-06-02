using ASC.Notify.Model;
using ASC.Notify.Patterns;

namespace ASC.Projects.Core.Services.NotifyService
{
    public static class NotifyConstants
    {
        public static INotifyAction Event_NewCommentForMessage = new NotifyAction("NewCommentForMessage", "new comment for message");
        public static INotifyAction Event_NewCommentForTask = new NotifyAction("NewCommentForTask", "new comment for task");
        public static INotifyAction Event_NewCommentForMilestone = new NotifyAction("NewCommentForMilestone", "new comment for milestone");
        public static INotifyAction Event_NewCommentForIssue = new NotifyAction("NewCommentForIssue", "new comment for issue");
        
        public static INotifyAction Event_UploadFiles = new NotifyAction("UploadFiles", "upload files");
        public static INotifyAction Event_TaskClosed = new NotifyAction("TaskClosed", "task closed");
        public static INotifyAction Event_TaskCreated = new NotifyAction("TaskCreated", "task created");
        public static INotifyAction Event_MilestoneDeadline = new NotifyAction("MilestoneDeadline", "milestone deadline");
        public static INotifyAction Event_ResponsibleForProject = new NotifyAction("ResponsibleForProject", "responsible for project");
        public static INotifyAction Event_ResponsibleForTask = new NotifyAction("ResponsibleForTask", "responsible for task");
        public static INotifyAction Event_ReminderAboutTask = new NotifyAction("ReminderAboutTask", "reminder about task");
        public static INotifyAction Event_ReminderAboutTaskDeadline = new NotifyAction("ReminderAboutTaskDeadline", "reminder about task deadline");
        public static INotifyAction Event_InviteToProject = new NotifyAction("InviteToProject", "invite to project");
        public static INotifyAction Event_RemoveFromProject = new NotifyAction("RemoveFromProject", "remove from project");

        public static INotifyAction Event_ProjectCreateRequest = new NotifyAction("ProjectCreateRequest", "project create request");
        public static INotifyAction Event_ProjectEditRequest = new NotifyAction("ProjectEditRequest", "project edit request");
        public static INotifyAction Event_ProjectRemoveRequest = new NotifyAction("ProjectRemoveRequest", "project remove request");
        public static INotifyAction Event_ProjectAcceptRequest = new NotifyAction("ProjectAcceptRequest", "project accept request");
        public static INotifyAction Event_ProjectRejectRequest = new NotifyAction("ProjectRejectRequest", "project reject request");

        public static INotifyAction Event_MessageCreated = new NotifyAction("NewMessage", "message created");
        public static INotifyAction Event_MessageEdited = new NotifyAction("EditMessage", "message edited");

        public static INotifyAction Event_ImportData = new NotifyAction("ImportData", "import data");


        public static ITag Tag_ProjectTitle = new Tag("ProjectTitle");
        public static ITag Tag_ProjectID = new Tag("ProjectID");
        public static ITag Tag_AdditionalData = new Tag("AdditionalData");
        public static ITag Tag_EntityID = new Tag("EntityID");
        public static ITag Tag_EntityTitle = new Tag("EntityTitle");
        public static ITag Tag_EventType = new Tag("EventType");
    }
}
