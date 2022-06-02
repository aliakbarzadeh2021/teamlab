using System;
using System.Collections.Generic;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TariffSettingsController")]
    public partial class TariffSettings : System.Web.UI.UserControl
    {
        protected UserInfo _owner = null;
        protected bool _canOwnerEdit;
        protected string _tariffPlanName = "";
        protected TariffPlan _tariff;
        protected DateTime _tariffEndDate;
        protected bool _expired;

        public static string Location { get { return "~/UserControls/Management/TariffSettings/TariffSettings.ascx"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());

            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "tariffsettings_script", WebPath.GetPath("usercontrols/management/tariffsettings/js/tariffsettings.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "tariffsettings_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/management/tariffsettings/css/<theme_folder>/tariffsettings.css") + "\">", false);

            var curTenant = CoreContext.TenantManager.GetCurrentTenant();

            var t = CoreContext.TenantManager.GetTariff(curTenant.TenantId);
            _tariff = t.Plan;
            _tariffEndDate = t.DueDate;
            _expired = t.Expired;


            var payments = new List<PaymentInfo>(CoreContext.TenantManager.GetTariffPayments(curTenant.TenantId));
           
            _paymentsRepeater.Visible = (payments.Count > 0);
            _paymentsRepeater.DataSource = payments;
            _paymentsRepeater.DataBind();
        }
    }
}