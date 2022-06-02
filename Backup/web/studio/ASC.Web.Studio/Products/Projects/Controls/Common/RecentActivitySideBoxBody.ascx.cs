using System;
using ASC.Core.Tenants;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;


namespace ASC.Web.Projects.Controls.Common
{
    public partial class RecentActivitySideBoxBody : BaseUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            rptContent.DataSource = UserActivityManager.GetUserActivities(
                TenantProvider.CurrentTenantID,
                null,
                ProductEntryPoint.ID,
                new[] { ProductEntryPoint.ID },
                UserActivityConstants.AllActionType,
                RequestContext.IsInConcreteProject() ? new[] { RequestContext.GetCurrentProjectId().ToString() } : null,
                0, 5);
            rptContent.DataBind();
        }

        public string GetEntityType(UserActivity activity)
        {
            EntityType _timeLineType;
            var AdditionalDataParts = activity.AdditionalData.Split(new[] { '|' });
            _timeLineType = (EntityType)Enum.Parse(typeof(EntityType), AdditionalDataParts[0]);

            switch (_timeLineType)
            {
                case EntityType.Team: return ProjectResource.Team.ToLower();
                case EntityType.Comment: return ProjectsCommonResource.Comment.ToLower();
                case EntityType.Task: return TaskResource.Task.ToLower();
                case EntityType.Project: return ProjectResource.Project.ToLower();
                case EntityType.Milestone: return MilestoneResource.Milestone.ToLower();
                case EntityType.Message: return MessageResource.Message.ToLower();
                case EntityType.File: return ProjectsFileResource.Documents.ToLower();
                case EntityType.TimeSpend: return ProjectsCommonResource.Time.ToLower();
                default: return string.Empty;
            }
        }
    }
}