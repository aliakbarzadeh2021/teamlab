using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
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
    [AjaxNamespace("PortalSettingsController")]
    public partial class PortalSettings : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/PortalSettings/PortalSettings.ascx"; } }

        protected bool _isPublic = false;        
        
        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "portalsettings_script", WebPath.GetPath("usercontrols/management/portalsettings/js/portalsettings.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "portalsettings_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/management/portalsettings/css/<theme_folder>/portalsettings.css") + "\">", false);

            _isPublic = CoreContext.TenantManager.GetCurrentTenant().Public;

            var publicItems = CoreContext.TenantManager.GetCurrentTenant().PublicVisibleProducts;

            var items = WebItemManager.Instance.GetItems(WebZoneType.All, ItemAvailableState.All);
            var itemSettings = SettingsManager.Instance.LoadSettings<WebItemSettings>(TenantProvider.CurrentTenantID);

            var data = new List<object>();
            foreach (var wi in items.Where(i => !i.IsSubItem()))
            {
                data.Add(new
                {
                    Id = wi.ID,
                    Name = wi.Name,
                    Public = publicItems.Exists(id => string.Equals(id, wi.ID.ToString(), StringComparison.InvariantCultureIgnoreCase))
                });
            }

            _productRepeater.DataSource = data;
            _productRepeater.DataBind();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SavePortalSettings(bool isPublic, List<string> productIDs)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var curTenant = CoreContext.TenantManager.GetCurrentTenant();
                curTenant.Public = isPublic;
                if(isPublic)
                {
                    curTenant.PublicVisibleProducts.Clear();
                    productIDs.ForEach(id=> curTenant.PublicVisibleProducts.Add(id));
                }

                CoreContext.TenantManager.SaveTenant(curTenant);

                return new { Status = 1, Message = Resources.Resource.SuccessfullySaveSettingsMessage };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }
    }
}