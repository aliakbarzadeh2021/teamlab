using System;
using System.Collections.Generic;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;


namespace ASC.Web.Projects.Controls.Dashboard
{
    [Serializable]
    public class LateMilestonesWidgetSettings : ISettings
    {
        public int MilestonesCount { get; set; }
        public bool ShowOnlyItemsInFollowingProjects { get; set; }

        public Guid ID
        {
            get { return new Guid("{C15F4506-24D8-4fb5-8990-4F2277EA83E1}"); }
        }

        public ISettings GetDefault()
        {
            return new LateMilestonesWidgetSettings() { MilestonesCount = 5, ShowOnlyItemsInFollowingProjects = false };
        }
    }

    public partial class LateMilestonesWidget : BaseUserControl
    {
        public static Guid WidgetID { get { return new Guid("{411E5079-B2D3-4b1b-99AE-AFCB3DC4D57A}"); } }

        public bool HasData { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            var settings = SettingsManager.Instance.LoadSettingsFor<LateMilestonesWidgetSettings>(SecurityContext.CurrentAccount.ID);
            var projectsID = settings.ShowOnlyItemsInFollowingProjects ?
                Global.EngineFactory.GetParticipantEngine().GetInterestedProjects(SecurityContext.CurrentAccount.ID).ToArray() :
                null;
            var lateMilestones = GetRepeaterDataSourceList(Global.EngineFactory.GetMilestoneEngine().GetLateMilestones(settings.MilestonesCount, projectsID));
            HasData = 0 < lateMilestones.Count;
            MilestonesRepeater.DataSource = lateMilestones;
            MilestonesRepeater.DataBind();
        }

        public string GetProjectLeader(UserInfo projectLeader)
        {
            return projectLeader.RenderProfileLink(ProductEntryPoint.ID);
        }

        public List<WrapperLateMilestonesWidget> GetRepeaterDataSourceList(List<Milestone> list)
        {
            var result = new List<WrapperLateMilestonesWidget>();
            var daysLate = 0;
            foreach (var milestone in list)
            {
                daysLate = CountDaysLate(milestone.DeadLine);
                result.Add(new WrapperLateMilestonesWidget
                {
                    projectID = milestone.Project.ID,
                    projectTitle = milestone.Project.Title,
                    projectLeader = Global.EngineFactory.GetParticipantEngine().GetByID(milestone.Project.Responsible).UserInfo,
                    milestoneID = milestone.ID,
                    milestoneTitle = milestone.Title,
                    isRoot = milestone.IsKey,
                    daysLate = daysLate
                });
            }

            return result;

        }

        public string GrammaticalHelperDays(int count)
        {
            return GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural);
        }

        public int CountDaysLate(DateTime deadline)
        {
            return Math.Abs(deadline.Subtract(TenantUtil.DateTimeNow()).Days);
        }

        public string IsRootMilestone(bool isKey)
        {
            return isKey ?
                string.Format("<img title='{0}' src='{1}' alt='{0}' align='absmiddle'>", MilestoneResource.RootMilestone, WebImageSupplier.GetAbsoluteWebPath("key.png", ProductEntryPoint.ID)) :
                string.Empty;
        }

        public string GetReportUri()
        {
            var filter = new ReportFilter { TimeInterval = ReportTimeInterval.Absolute };
            return string.Format("reports.aspx?action=generate&reportType=0&{0}", filter.ToUri());
        }
    }

    public class WrapperLateMilestonesWidget
    {
        public int projectID;
        public string projectTitle;
        public UserInfo projectLeader;
        public int milestoneID;
        public string milestoneTitle;
        public bool isRoot;
        public int daysLate;
    }
}