using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.Utility;
using System.Globalization;
using ASC.Core;
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio.UserControls.Statistics
{
  public sealed class ChartPoint
  {
    public String DisplayDate { get; set; }
    public DateTime Date { get; set; }
    public Int32 Hosts { get; set; }
    public Int32 Hits { get; set; }
  }

  [AjaxPro.AjaxNamespace("VisitorsChart")]
  public partial class VisitorsChart : System.Web.UI.UserControl
  {
    public static string Location { get { return "~/UserControls/Statistics/VisitorsChart/VisitorsChart.ascx"; } }

    protected void Page_Load(object sender, EventArgs e)
    {
      AjaxPro.Utility.RegisterTypeForAjax(this.GetType());

      Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "visitorschart_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/statistics/visitorschart/css/<theme_folder>/visitorschart_style.css") + "\">", false);
      Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "excanvaslib_script", WebPath.GetPath("usercontrols/statistics/visitorschart/js/excanvas.min.js"));
      Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "flotlib_script", WebPath.GetPath("usercontrols/statistics/visitorschart/js/jquery.flot.js"));
      Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "tooltip_script", WebPath.GetPath("usercontrols/statistics/visitorschart/js/tooltip.js"));
      Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "visitorschart_script", WebPath.GetPath("usercontrols/statistics/visitorschart/js/visitorschart.js"));

      StringBuilder jsResources = new StringBuilder();
      jsResources.Append("if(typeof window.ASC==='undefined')window.ASC={};");
      jsResources.Append("if(typeof window.ASC.Resources==='undefined')window.ASC.Resources={};");
      jsResources.Append("window.ASC.Resources.chartDateFormat='" + Resources.Resource.ChartDateFormat + "';");
      jsResources.Append("window.ASC.Resources.chartMonthNames='" + Resources.Resource.ChartMonthNames + "';");
      jsResources.Append("window.ASC.Resources.hitLabel='" + Resources.Resource.VisitorsChartHitLabel + "';");
      jsResources.Append("window.ASC.Resources.hostLabel='" + Resources.Resource.VisitorsChartHostLabel + "';");
      jsResources.Append("window.ASC.Resources.visitsLabel='" + Resources.Resource.VisitorsChartVisitsLabel + "';");

      Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "StatisticsResources", jsResources.ToString(), true);

      studio_chart_FromDate.Text = DateTime.Now.AddMonths(-6).ToString(DateTimeExtension.ShortDatePattern);
      studio_chart_ToDate.Text = DateTime.Now.ToString(DateTimeExtension.ShortDatePattern);

      jQueryDateMask.Value = DateTimeExtension.DateMaskForJQuery;
    }

    [AjaxPro.AjaxMethod(AjaxPro.HttpSessionStateRequirement.ReadWrite)]
    public List<ChartPoint> GetVisitStatistics(DateTime from, DateTime to)
    {

        SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

      List<ChartPoint> points = new List<ChartPoint>();
      if (from.CompareTo(to) >= 0)
        return points;
      for (DateTime d = new DateTime(from.Ticks); d.Date.CompareTo(to.Date) <= 0; d = d.AddDays(1))
        points.Add(new ChartPoint() { Hosts = 0, Hits = 0, Date = TenantUtil.DateTimeFromUtc(d.Date), DisplayDate = TenantUtil.DateTimeFromUtc(d.Date).ToShortDateString() });

      List<Hit> hits = StatisticManager.GetHitsByPeriod(TenantProvider.CurrentTenantID, from, to);
      List<Host> hosts = StatisticManager.GetHostsByPeriod(TenantProvider.CurrentTenantID, from, to);
      if (hits.Count == 0 || hosts.Count == 0)
        return points;
      hits.Sort(SortByVisitDate);
      hosts.Sort(SortByVisitDate);

      ChartPoint point = new ChartPoint() { Hosts = 0, Hits = 0, Date = from.Date };
      for (Int32 i = 0, n = points.Count, hitsNum = 0, hostsNum = 0; i < n; i++)
      {
        while (hitsNum < hits.Count && points[i].Date.Date.CompareTo(hits[hitsNum].VisitDate.Date) == 0)
        {
          points[i].Hits += hits[hitsNum].Count;
          hitsNum++;
        }
        while (hostsNum < hosts.Count && points[i].Date.Date.CompareTo(hosts[hostsNum].VisitDate.Date) == 0)
        {
          points[i].Hosts++;
          hostsNum++;
        }
      }

      return points;
    }

    private int SortByVisitDate(Visit x, Visit y)
    {
      return x.VisitDate.CompareTo(y.VisitDate);
    }
  }
}
