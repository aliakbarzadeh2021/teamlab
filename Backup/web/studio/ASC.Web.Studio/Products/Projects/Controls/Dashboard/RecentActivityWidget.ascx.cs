#region Import

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility;
using ASC.Web.Projects.Classes;
using ASC.Core.Users;
using ASC.Core;
using ASC.Web.Projects.Resources;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Projects.Configuration;
using ASC.Projects.Engine;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Settings;
using System.Linq;
using ASC.Projects.Core.Domain.Reports;
using ASC.Core.Tenants;
#endregion

namespace ASC.Web.Projects.Controls.Dashboard
{
    [Serializable]
    public class RecentActivityWidgetSettings : ISettings
    {
        public int ActivityCount { get; set; }
        public bool ShowOnlyItemsInFollowingProjects { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{7CEDC501-2DEB-4a84-BFE4-284B6EF468A9}"); }
        }

        public ISettings GetDefault()
        {
            return new RecentActivityWidgetSettings() { ActivityCount = 5, ShowOnlyItemsInFollowingProjects = false };
        }

        #endregion
    }

    public partial class RecentActivityWidget : BaseUserControl
    {
        #region Properties

        public static Guid WidgetID { get { return new Guid("{B2E3B652-49F3-4113-AED1-0BB80C41EA27}"); } }

        public bool HasData { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {

            var widgetSettings = SettingsManager.Instance.LoadSettingsFor<RecentActivityWidgetSettings>(SecurityContext.CurrentAccount.ID);

            var projectsID = new List<int>();

            if (widgetSettings.ShowOnlyItemsInFollowingProjects)
            {
                projectsID.AddRange(Global.EngineFactory
                         .GetParticipantEngine()
                         .GetInterestedProjects(SecurityContext.CurrentAccount.ID));
            }
            var activity = UserActivityManager.GetUserActivities(
                TenantProvider.CurrentTenantID,
                null,
                ProductEntryPoint.ID,
                new[] { ProductEntryPoint.ID },
                UserActivityConstants.AllActionType,
                projectsID.Select(x => x.ToString()),
                0, widgetSettings.ActivityCount);

            HasData = activity.Count > 0 ? true : false;

            LastActivityRepeater.DataSource = activity;
            LastActivityRepeater.DataBind();
        }

        #endregion

        #region Methods

        public string EntityType(UserActivity activity)
        {
            var AdditionalDataParts = activity.AdditionalData.Split(new[] { '|' });

            var timeLineType = (EntityType)Enum.Parse(typeof(EntityType), AdditionalDataParts[0]);

            return ResourceEnumConverter.ConvertToString(timeLineType);
        }

        public string EntityParentContainers(UserActivity activity)
        {
            int prjID;
            if (Int32.TryParse(activity.ContainerID, out prjID))
            {
                var prj = Global.EngineFactory.GetProjectEngine().GetByID(prjID);
                if (prj != null)
                {
                    return string.Format("<span>{0}</span>:<a title='{2}' class='linkAction pm-dashboard-leftIndent-small' href='projects.aspx?prjID={1}'>{2}</a>",
                        ProjectResource.Project, prj.ID.ToString(), prj.Title.HtmlEncode().ReplaceSingleQuote());
                }
                return string.Empty;
            }
            return string.Empty;
        }

        public string GetReportUri()
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            return string.Format("reports.aspx?action=generate&reportType=5&{0}", filter.ToUri());
        }

        #endregion
    }
}