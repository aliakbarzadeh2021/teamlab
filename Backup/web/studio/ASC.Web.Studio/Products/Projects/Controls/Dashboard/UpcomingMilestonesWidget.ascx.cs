using System;
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
    public class UpcomingMilestonesWidgetSettings : ISettings
    {
        public int MilestonesCount { get; set; }
        public bool ShowOnlyItemsInFollowingProjects { get; set; }

        public Guid ID
        {
            get { return new Guid("{299B9E33-D81B-47f6-BCE9-2C2D8AC84D76}"); }
        }

        public ISettings GetDefault()
        {
            return new UpcomingMilestonesWidgetSettings() { MilestonesCount = 5, ShowOnlyItemsInFollowingProjects = false };
        }
    }

    public partial class UpcomingMilestonesWidget : BaseUserControl
    {
        public static Guid WidgetID { get { return new Guid("{86A18C6E-9753-4c61-B763-954C02E1795B}"); } }

        public bool HasData { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            var settings = SettingsManager.Instance.LoadSettingsFor<UpcomingMilestonesWidgetSettings>(SecurityContext.CurrentAccount.ID);
            var projects = settings.ShowOnlyItemsInFollowingProjects ?
                Global.EngineFactory.GetParticipantEngine().GetInterestedProjects(SecurityContext.CurrentAccount.ID).ToArray() :
                null;
            var upcomingMilestones = Global.EngineFactory.GetMilestoneEngine().GetUpcomingMilestones(settings.MilestonesCount, projects);
            HasData = 0 < upcomingMilestones.Count;
            MilestonesRepeater.DataSource = upcomingMilestones;
            MilestonesRepeater.DataBind();
        }


        public string GetMilestoneDate(Milestone milestone)
        {
            return milestone.DeadLine.ToString(DateTimeExtension.DateFormatPattern);
        }

        public string GetCountDaysLate(Milestone milestone)
        {
            var deadline = milestone.DeadLine;
            var count = deadline.Subtract(TenantUtil.DateTimeNow()).Days;

            if (deadline.Date == TenantUtil.DateTimeNow().Date)
            {
                return ProjectsCommonResource.Today;
            }
            if (deadline < TenantUtil.DateTimeNow())
            {
                return Math.Abs(count) + " " + GrammaticalHelper.ChooseNumeralCase(Math.Abs(count), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural);
            }
            return count + 1 + " " + GrammaticalHelper.ChooseNumeralCase(Math.Abs(count + 1), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural);
        }

        public string GetProjectLeader(Milestone milestone)
        {
            return Global.EngineFactory.GetParticipantEngine().GetByID(milestone.Project.Responsible).UserInfo.RenderProfileLink(ProductEntryPoint.ID);
        }

        public string IsRootMilestone(bool isKey)
        {
            return isKey ?
                string.Format("<img title='{0}' src='{1}' alt='{0}' align='absmiddle'>", MilestoneResource.RootMilestone, WebImageSupplier.GetAbsoluteWebPath("key.png", ProductEntryPoint.ID)) :
                string.Empty;
        }

        public string GetReportUri()
        {
            var date = TenantUtil.DateTimeNow().Date;
            var filter = new ReportFilter
            {
                TimeInterval = ReportTimeInterval.Relative,
                FromDate = date,
                ToDate = date.AddDays(5 * 7),
            };
            return string.Format("reports.aspx?action=generate&reportType=1&{0}", filter.ToUri());
        }
    }
}