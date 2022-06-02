using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Studio.UserControls.Statistics
{
    public partial class ProductQuotes : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Statistics/ProductQuotes/ProductQuotes.ascx"; } }
        protected Tariff _tariff;        

        public long CurrentSize
        {
            get;
            set;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "productquotes_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/statistics/productquotes/css/<theme_folder>/productquotes_style.css") + "\">", false);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            _tariff = CoreContext.TenantManager.GetTariff(tenant.TenantId);

            var data = new List<object>();
            foreach (var item in WebItemManager.Instance.GetItems(ASC.Web.Core.WebZones.WebZoneType.All, ItemAvailableState.All))
            {
                if (item.Context == null || item.Context.SpaceUsageStatManager == null)
                    continue;

                data.Add(new Product() { Id = item.ID, Name = item.Name, Icon = item.GetIconAbsoluteURL() });
            }

            _itemsRepeater.ItemDataBound += new System.Web.UI.WebControls.RepeaterItemEventHandler(_itemsRepeater_ItemDataBound);
            _itemsRepeater.DataSource = data;
            _itemsRepeater.DataBind();
            
        }

        void _itemsRepeater_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {

            var product = e.Item.DataItem as Product;
            var webItem = WebItemManager.Instance[product.Id];

            var data = new List<object>();
            var items = webItem.Context.SpaceUsageStatManager.GetStatData();
            foreach (var it in items)
            {
                data.Add(new { Name = it.Name, Icon = it.ImgUrl, Size = SizeToString(it.SpaceUsage), Url = it.Url });
            }
            
            var repeater = (Repeater)e.Item.FindControl("_usageSpaceRepeater");
            repeater.DataSource = data;
            repeater.DataBind();

            e.Item.FindControl("_showMorePanel").Visible = (items.Count > 10);
        }

        protected String RenderCreatedDate()
        {
            return String.Format("{0}", TenantStatisticsProvider.GetCreationDate().ToShortDateString());
        }

        protected String RenderUsersTotal()
        {
            return TenantStatisticsProvider.GetUsersCount().ToString();
        }

        protected String GetMaxTotalSpace()
        {
            return SizeToString(CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).MaxTotalSize);
        }

        protected String RenderUsedSpace()
        {            
            var used = TenantStatisticsProvider.GetUsedSize();
            return SizeToString(used);
        }

        private String SizeToString(long size)
        {
            var mn = 1024;
            var posit = 0;
            var values = new List<String>(Resources.Resource.FileSizePostfix.Split(','));
            while (size > Math.Exp((posit + 1) * Math.Log(mn)) && posit < values.Count) posit++;
            return String.Format("{0} {1}", Math.Floor(((double)size / Math.Exp((posit) * Math.Log(mn))) * 10) / 10, values[posit]);
        }

        protected sealed class Product
        {
            public Guid Id { get; set; }
            public String Name { get; set; }
            public String Icon { get; set; }
            public long Size { get; set; }
        }
    }
}
