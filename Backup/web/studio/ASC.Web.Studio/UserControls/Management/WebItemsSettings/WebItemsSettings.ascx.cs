using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WebZones;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("WebItemsSettingsController")]
    public partial class WebItemsSettings : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/WebItemsSettings/WebItemsSettings.ascx"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "webitemssettings_script", WebPath.GetPath("usercontrols/management/webitemssettings/js/webitemssettings.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "webitemssettings_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/management/webitemssettings/css/<theme_folder>/webitemssettings.css") + "\">", false);

            _itemsRepeater.DataSource = GetDataSource();
            _itemsRepeater.ItemDataBound += new System.Web.UI.WebControls.RepeaterItemEventHandler(_itemsRepeater_ItemDataBound);
            _itemsRepeater.DataBind();
        }

        void _itemsRepeater_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var subItemsRepeater = (Repeater)e.Item.FindControl("_subItemsRepeater");

                subItemsRepeater.DataSource = ((Item)e.Item.DataItem).SubItems;
                subItemsRepeater.DataBind();
            }
        }
        private class Item
        {
            public string Name { get; set; }
            public Guid ID { get; set; }
            public bool Disabled { get; set; }
            public bool DisplayedAlways { get; set; }
            public bool SortDisabled { get; set; }
            public List<Item> SubItems { get; set; }
            public int SortOrder { get; set; }
        }

        private List<Item> GetDataSource()
        {
            var data = new List<Item>();

            var items = WebItemManager.Instance.GetItems(WebZoneType.All, ItemAvailableState.All);
            var itemSettings = SettingsManager.Instance.LoadSettings<WebItemSettings>(TenantProvider.CurrentTenantID);

            foreach (var wi in items)
            {
                if (wi.IsSubItem()) continue;

                var item = new Item
                {
                    ID = wi.ID,
                    Name = wi.Name,
                    Disabled = wi.IsDisabled(itemSettings),
                    SubItems = new List<Item>(),
                    SortDisabled = wi.Context != null && wi.Context.SortDisabled
                };
                data.Add(item);

                if (wi is IProduct)
                {
                    foreach (var m in ((IProduct)wi).Modules)
                    {
                        var si = items.Find(i => i.ID == m.ModuleID);
                        if (si != null && si.ID != item.ID)
                        {
                            var sumitem = new Item
                            {
                                ID = si.ID,
                                Name = si.Name,
                                Disabled = si.IsDisabled(itemSettings),
                                DisplayedAlways = (si is NavigationWebItem) && ((si as NavigationWebItem).DisplayedAlways),
                                SortDisabled = si.Context != null && si.Context.SortDisabled,
                                SortOrder = items.IndexOf(si),
                            };
                            item.SubItems.Add(sumitem);
                        }
                    }
                }
                item.SubItems.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));
            }

            return data;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveSettings(List<ASC.Web.Core.Utility.Settings.WebItemSettings.WebItemOption> options)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var settings = new WebItemSettings();
                foreach (var opt in options.Select((o, i) => { o.SortOrder = i; return o; }))
                    settings.SettingsCollection.Add(opt);

                SettingsManager.Instance.SaveSettings(settings, TenantProvider.CurrentTenantID);
                return new { Status = 1, Message = Resources.Resource.SuccessfullySaveSettingsMessage };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };

            }
        }

    }
}