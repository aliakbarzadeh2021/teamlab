using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement.Common;

namespace ASC.Web.Studio.UserControls.Users
{    
    public partial class UserStatistics : System.Web.UI.UserControl
    {

        public static string Location
        {
            get { return "~/UserControls/Users/UserStatistics.ascx"; }
        }

        public Guid UserID { get; set; }
        public Guid ProductID { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
           
        }

        private class StatItem
        {
            public string ModuleName{get; set;}
            public StatisticItem Data{get; set;}            
            
        }

        protected string RenderUserStatistic()
        {     
            IProduct product = ProductManager.Instance[this.ProductID];
            if (product == null) return "";

            float width = 46;
            double k = 0;
            double fullCount = .0;
            double max = 0;

            List<StatItem> statistic = new List<StatItem>();
            foreach (var item in WebItemManager.Instance.GetSubItems(product.ProductID))
            {
                if (item is IModule == false)
                    continue;

                var module = item as IModule;

                var stat = new StatItem()
                {
                    ModuleName = module.ModuleName,
                    Data = new StatisticItem()
                    {
                        URL = VirtualPathUtility.ToAbsolute(module.StartURL),
                        Count = 0,
                        Name = module.ModuleName
                    }
                };

                if (module.Context.StatisticProvider != null)
                {
                    stat.Data = module.Context.StatisticProvider.GetMainStatistic(this.UserID);

                    fullCount += stat.Data.Count;
                    if (max < stat.Data.Count)
                        max = stat.Data.Count;
                }
                statistic.Add(stat);
            }

            if (max!=0)
                k = width / max;

            StringBuilder sb = new StringBuilder();
            foreach (var stat in statistic)
            {
             
                sb.AppendFormat(@"<div class=""{1}"">{0}</div>", stat.ModuleName, "VariantTextCSSClass");
                sb.Append("<div class='clearFix' style='padding-bottom:15px;'>");
                sb.AppendFormat(@"<div class=""{1}"" style=""width:{0}%;"">&nbsp;</div>", Math.Round(k * stat.Data.Count), max == stat.Data.Count ? "statisticBar liaderBar" : "statisticBar");
                sb.AppendFormat(@"<div class=""{2}"" style=""width:20%; float:left;""><span id=""strong"">{0}</span><span> ({1}%)</span></div>", stat.Data.Count, fullCount != 0 ? Math.Round((stat.Data.Count * 100) / fullCount) : 0, "pollSmallDescribe");
                sb.AppendFormat(
                        "<div style='text-align:right; float:right; width:30%;'><a href=\"{0}\">{1}:&nbsp;{2}</a></div>",
                        stat.Data.URL,
                        stat.Data.Name,
                        stat.Data.Count);
                sb.Append("</div>");
            }
           
            return sb.ToString();
        }
    }
}