using System;
using System.Collections.Generic;
using System.Web.UI;
using AjaxPro;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Core;
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("SkinSettingsController")]
    public partial class SkinSettings : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/SkinSettings/SkinSettings.ascx"; } }

        protected WebSkin _currentSkin;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "skinsettings_script", WebPath.GetPath("usercontrols/management/skinsettings/js/skinsettings.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "skinsettings_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/management/skinsettings/css/<theme_folder>/skinsettings.css") + "\">", false);


            //Skin Settings
            var skins = WebSkin.GetSkinCollection();
            _currentSkin = WebSkin.GetUserSkin();

            var items = new List<object>();
            foreach (var s in skins)
            {
                items.Add(new
                {
                    Id = s.ID,
                    Name = s.Name,
                    Checked = string.Equals(s.ID, _currentSkin.ID, StringComparison.InvariantCultureIgnoreCase),
                    Folder = s.FolderName,
                    Path = WebImageSupplier.GetAbsoluteWebPath("skins/" + s.ID + ".png")
                });
            }

            skinRepeater.DataSource = items;
            skinRepeater.DataBind();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveSkinSettings(string skinID)
        {
            var skins = WebSkin.GetSkinCollection();

            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var skin = skins.Find(s => string.Equals(s.ID, skinID, StringComparison.InvariantCultureIgnoreCase));

                var settings = SettingsManager.Instance.LoadSettings<WebSkinSettings>(TenantProvider.CurrentTenantID);
                if (skin != null)
                    settings.WebSkin = skin;
                SettingsManager.Instance.SaveSettings<WebSkinSettings>(settings, TenantProvider.CurrentTenantID);


                return new { Status = 1 };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }
    }
}