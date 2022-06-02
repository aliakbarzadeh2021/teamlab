#region Import

using System;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Studio.Utility;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Text;
using ASC.Web.Projects.Resources;
using AjaxPro;
using ASC.Web.Projects.Classes;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Milestones
{
    [AjaxNamespace("AjaxPro.MilestoneActionView")]
    public partial class MilestoneActionView : BaseUserControl
    {
        #region Members

        public Milestone Milestone { get; set; }
        public ProjectFat ProjectFat { get; set; }

        public string ActionType { get; set; }
        public string[] Days { get; set; }
        public DateTime ChoosenDate { get; set; }
        public string IsKey { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MilestoneDetailsView));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MilestoneActionView));

            DateTime date = ASC.Core.Tenants.TenantUtil.DateTimeNow();

            if (ActionType == "add")
            {
                var milestones = ProjectFat.GetMilestones();
                while (milestones.Exists(m => m.DeadLine.Date == date.Date)) date = date.AddDays(1);

                Date.Text = date.ToString(DateTimeExtension.DateFormatPattern);
                tbxTitle.Text = string.Empty;
                ChoosenDate = date;
                IsKey = string.Empty;
            }
            else
            {

                Date.Text = Milestone.DeadLine.ToString(DateTimeExtension.DateFormatPattern);
                tbxTitle.Text = HttpUtility.HtmlDecode(Milestone.Title);
                ChoosenDate = Milestone.DeadLine;
                IsKey = Milestone.IsKey ? "checked='checked'" : string.Empty;
            }

            List<string> months = new List<string>();
            for (int i = 1; i < 13; i++)
            {
                date = new DateTime(2009, i, 1);
                months.Add(date.ToString("MMMM"));
            }

            Days = new string[7];

            CultureInfo info = Thread.CurrentThread.CurrentCulture;
            DayOfWeek firstday = info.DateTimeFormat.FirstDayOfWeek;

            if (firstday == DayOfWeek.Monday)
            {
                for (int i = 1; i < 8; i++)
                {
                    date = new DateTime(2009, 12, 6 + i);
                    Days[i - 1] = date.ToString("ddd");
                }
            }

            if (firstday == DayOfWeek.Sunday)
            {
                for (int i = 1; i < 8; i++)
                {
                    date = new DateTime(2009, 12, 5 + i);
                    Days[i - 1] = date.ToString("ddd");
                }
            }

            const string jTemplateJavaScript = "jTemplateJavaScript";
            if (!Page.ClientScript.IsClientScriptBlockRegistered(jTemplateJavaScript))
            {
                Page.ClientScript.RegisterClientScriptInclude(jTemplateJavaScript, ASC.Data.Storage.WebPath.GetPath("js/jquery-jtemplates.js"));
            }

            //const string daysJavaScript = "daysJavaScript";
            //if (!Page.ClientScript.IsClientScriptBlockRegistered(daysJavaScript))
            //{
            //    Page.ClientScript.RegisterClientScriptBlock(typeof(DayContainer), daysJavaScript, "Days = " +
            //                                                    JavaScriptSerializer.Serialize(GetMonthDays(ChoosenDate.Year, ChoosenDate.Month, ProjectFat,ChoosenDate.Year, ChoosenDate.Month, ChoosenDate.Day)) + ";", true);
            //}

            const string monthsJavaScript = "monthsJavaScript";
            if (!Page.ClientScript.IsClientScriptBlockRegistered(monthsJavaScript))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(string), monthsJavaScript, "month_names = " +
                                                                JavaScriptSerializer.Serialize(months) + ";", true);
            }

            const string choosenDate = "choosenDate";
            var cd = new List<int>(); cd.Add(ChoosenDate.Year); cd.Add(ChoosenDate.Month); cd.Add(ChoosenDate.Day);
            if (!Page.ClientScript.IsClientScriptBlockRegistered(choosenDate))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(int), choosenDate, "ChoosenDay = " +
                                                                JavaScriptSerializer.Serialize(cd) + ";", true);
            }

            RegisterFirstDayOfWeekInCurrentCulture();

            const string eventsJavaScript = "eventsJavaScript";
            if (!Page.ClientScript.IsClientScriptBlockRegistered(eventsJavaScript))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(DayContainer), eventsJavaScript, "Events = " +
                                                                JavaScriptSerializer.Serialize(GetEvents()) + ";", true);
            }

        }

        public void RegisterFirstDayOfWeekInCurrentCulture()
        {
            CultureInfo info = Thread.CurrentThread.CurrentCulture;
            DayOfWeek firstday = info.DateTimeFormat.FirstDayOfWeek;
            int FirstDayOfWeekInCurrentCulture = (int)firstday;

            if (!Page.ClientScript.IsClientScriptBlockRegistered("F31E6F6F-B6D2-40e3-8B2B-A82A0B5E9DCA"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(int), "F31E6F6F-B6D2-40e3-8B2B-A82A0B5E9DCA", "firstDayOfWeekInCurrentCulture = " +
                                                                JavaScriptSerializer.Serialize(FirstDayOfWeekInCurrentCulture) + ";", true);
            }
        }

        public string GetImageByDeadline(Milestone milestone)
        {

            string image = string.Empty;

            if (milestone.Status == MilestoneStatus.Closed)
                image = string.Format("<img align=\"absmiddle\" src=\"{0}\" alt=\"{1}\" title=\"{2}\">",
                    WebImageSupplier.GetAbsoluteWebPath("milestone_status_completed_16.png", ProductEntryPoint.ID),
                    MilestoneResource.ClosedMilestone, MilestoneResource.ClosedMilestone);

            if (milestone.Status == MilestoneStatus.Open)
                if (milestone.DeadLine.AddDays(1) < ASC.Core.Tenants.TenantUtil.DateTimeNow())
                    image = string.Format("<img align=\"absmiddle\" src=\"{0}\" alt=\"{1}\" title=\"{2}\">",
                        WebImageSupplier.GetAbsoluteWebPath("milestone_status_late_16.png", ProductEntryPoint.ID),
                        MilestoneResource.LateMilestone, MilestoneResource.LateMilestone);
                else image = string.Format("<img align=\"absmiddle\" src=\"{0}\" alt=\"{1}\" title=\"{2}\">",
                        WebImageSupplier.GetAbsoluteWebPath("milestone_status_active_16.png", ProductEntryPoint.ID),
                        MilestoneResource.OpenMilestone, MilestoneResource.OpenMilestone);

            return image;
        }

        public List<DayContainer> GetMonthDays(int year, int month, Project prj, int choosenYear, int choosenMonth, int choosenDay)
        {
            List<DayContainer> result = new List<DayContainer>();

            DateTime choosenDate = new DateTime(choosenYear, choosenMonth, choosenDay);
            DateTime date = new DateTime(year, month, 1);
            int dayOfWeek = (int)date.DayOfWeek;

            CultureInfo info = Thread.CurrentThread.CurrentCulture;
            DayOfWeek firstDayOfWeekInCurrentCulture = info.DateTimeFormat.FirstDayOfWeek;

            dayOfWeek = dayOfWeek - (int)firstDayOfWeekInCurrentCulture;

            int countDaysInWeekBeforeDate = 7 - (7 - dayOfWeek);

            DateTime currentDate = date.AddDays(-countDaysInWeekBeforeDate);

            List<Milestone> milestones = Global.EngineFactory.GetMilestoneEngine().GetByProject(prj.ID);

            for (int i = 0; i < 42; i++)
            {
                bool hasEvent = false;

                DayContainer dc = new DayContainer();

                dc.dayNumber = currentDate.Day;
                dc.monthNumber = currentDate.Month;
                dc.yearNumber = currentDate.Year;
                dc.index = i + 1;
                dc.cssClass = string.Empty;

                if (currentDate.Date == ASC.Core.Tenants.TenantUtil.DateTimeNow().Date)
                    dc.cssClass += "current";

                if (currentDate.Date < date.Date || currentDate.Date >= date.AddMonths(1).Date)
                    dc.cssClass += " pm-grayText";

                if (currentDate.Date == choosenDate)
                    dc.cssClass += " choose";

                StringBuilder sb = new StringBuilder();

                foreach (Milestone milestone in milestones)
                {
                    if (milestone.DeadLine.Date == currentDate.Date)
                    {
                        sb.AppendFormat("<div class='pm-dashboard-fade'></div><div>{0}<span class='splitter'></span>{1}</div>",
                        GetImageByDeadline(milestone),
                        milestone.Title.HtmlEncode()
                        );

                        if (!hasEvent)
                            dc.cssClass += " date_has_event";

                        hasEvent = true;
                    }
                }

                dc.popupEvents = hasEvent ? sb.ToString() : string.Empty;

                currentDate = currentDate.AddDays(1);

                dc.cssClass = dc.cssClass.Trim();

                result.Add(dc);
            }

            return result;
        }

        //TODO: %) need to rewrite!
        public List<DayEvents> GetEvents()
        {
            List<DayEvents> result = new List<DayEvents>();

            List<int> ids = new List<int>();

            var milestones = ProjectFat.GetMilestones();
            foreach (var milestone in milestones)
            {
                bool isAlreadyWrite = false;
                foreach (int id in ids)
                {
                    if (id == milestone.ID) isAlreadyWrite = true;
                }
                if (!isAlreadyWrite)
                {
                    DayEvents de = new DayEvents();

                    StringBuilder sb = new StringBuilder();

                    de.dayNumber = milestone.DeadLine.Day;
                    de.monthNumber = milestone.DeadLine.Month - 1;
                    de.yearNumber = milestone.DeadLine.Year;

                    sb.AppendFormat("<div class='pm-dashboard-fade'></div><div>{0}<span class='splitter'></span>{1}</div>",
                                GetImageByDeadline(milestone),
                                milestone.Title.HtmlEncode()
                                );

                    foreach (var m in milestones)
                    {
                        if (milestone.DeadLine.Date == m.DeadLine.Date && milestone.ID != m.ID)
                        {
                            sb.AppendFormat("<div class='pm-dashboard-fade'></div><div>{0}<span class='splitter'></span>{1}</div>",
                                    GetImageByDeadline(m),
                                    m.Title.HtmlEncode()
                                    );
                            ids.Add(m.ID);
                        }
                    }

                    de.popupEvents = sb.ToString();

                    result.Add(de);
                }
            }

            return result;
        }

        [AjaxMethod]
        public string GetMonthDays(int prjID, int year, int month, int choosenYear, int choosenMonth, int choosenDay)
        {
            ProjectSecurity.DemandAuthentication();

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            Project project = Global.EngineFactory.GetProjectEngine().GetByID(prjID);

            List<DayContainer> monthDays = GetMonthDays(year, month, project, choosenYear, choosenMonth, choosenDay);

            return serializer.Serialize(monthDays);
        }

    }

    public class DayContainer
    {
        public int index;
        public int dayNumber;
        public int monthNumber;
        public int yearNumber;
        public string cssClass;
        public string popupEvents;
    }

    public class DayEvents
    {
        public int dayNumber;
        public int monthNumber;
        public int yearNumber;
        public string popupEvents;
    }

}