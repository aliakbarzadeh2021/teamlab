using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("StudioSettings")]
    public partial class StudioSettings : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/StudioSettings.ascx"; } }

        public Guid ProductID { get; set; }

        protected StudioViewSettings _studioViewSettings;

        public bool DevelopMode { get; set; }

        protected Tenant _currentTenant;

        protected static bool EnableDnsChange
        {
            get
            {
                return !string.IsNullOrEmpty(ASC.Core.CoreContext.TenantManager.GetCurrentTenant().MappedDomain);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());

            _studioViewSettings = SettingsManager.Instance.LoadSettings<StudioViewSettings>(TenantProvider.CurrentTenantID);
            _currentTenant = CoreContext.TenantManager.GetCurrentTenant();

            _enterSettingsHolder.Visible = DevelopMode;            
            _studioViewSettingsHolder.Visible = DevelopMode;

            //portal settings
            if(DevelopMode)
                _portalSettingsHolder.Controls.Add(LoadControl(PortalSettings.Location));
            
            //main domain settings
            _mailDomainSettings.Controls.Add(LoadControl(MailDomainSettings.Location));
        }

        protected string RenderLanguageSelector()
        {
            var allCultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures));
            var enabledCultures = SetupInfo.EnabledCultures;
            allCultures.RemoveAll(c => enabledCultures.Find(ec => String.Equals(ec.Name, c.Name)) == null);

            StringBuilder sb = new StringBuilder();
            sb.Append("<select id=\"studio_lng\" class=\"comboBox\">");
            foreach (var ci in allCultures)
            {
                sb.AppendFormat("<option " + (String.Equals(_currentTenant.Language, ci.Name) ? "selected" : "") + " value=\"{0}\">{1}</option>", ci.Name, ci.DisplayName);
            }
            sb.Append("</select>");

            return sb.ToString();
        }

        protected string RenderTimeZoneSelector()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<select id=\"studio_timezone\" class=\"comboBox\">");
            foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                sb.AppendFormat("<option " + (timeZone.Equals(_currentTenant.TimeZone) ? "selected" : "") + " value=\"{0}\">{1}</option>", timeZone.Id, timeZone.DisplayName);
            }
            sb.Append("</select>");

            return sb.ToString();


        }

        private bool CheckTrustedDomain(string domain)
        {
            return !string.IsNullOrEmpty(domain) && new Regex("^[a-z0-9]([a-z0-9-.]){1,98}[a-z0-9]$").IsMatch(domain);
        }


        #region Check custom domain name
        /// <summary>
        /// Custom domain name shouldn't ends with tenant base domain name.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private static bool CheckCustomDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return false;
            }
            if (string.IsNullOrEmpty(TenantBaseDomain))
            {
                return true;
            }
            if (domain.EndsWith(TenantBaseDomain))
            {
                return false;
            }
            if (domain.Equals(TenantBaseDomain.TrimStart('.')))
            {
                return false;
            }
            return true;
        }
        #endregion

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveDnsSettings(string dnsName, string alias, bool enableDns)
        {
            var resp = new AjaxResponse() { rs1 = "1" };
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                if (!enableDns || string.IsNullOrEmpty(dnsName))
                {
                    dnsName = null;
                }
                if (dnsName == null || (CheckTrustedDomain(dnsName) && CheckCustomDomain(dnsName)))
                {
                    if (CoreContext.Configuration.Standalone)
                    {
                        var tenant = CoreContext.TenantManager.GetCurrentTenant();
                        tenant.MappedDomain = dnsName;
                        CoreContext.TenantManager.SaveTenant(tenant);

                        return resp;
                    }
                    else
                    {
                        if (!CoreContext.TenantManager.GetCurrentTenant().TenantAlias.Equals(alias))
                        {
                            CoreContext.TenantManager.CheckTenantAddress(alias);
                        }
                    }

                    var portalAddress = string.Format("http://{0}.{1}", alias ?? String.Empty, SetupInfo.BaseDomain);

                    var u = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
                    StudioNotifyService.Instance.SendMsgDnsChange(CoreContext.TenantManager.GetCurrentTenant(), GenerateDnsChangeConfirmUrl(u.Email, dnsName, alias, ConfirmType.DnsChange), portalAddress, dnsName);
                    resp.rs2 = string.Format(Resources.Resource.DnsChangeMsg, string.Format("<a href='mailto:{0}'>{0}</a>", u.Email));
                }
                else
                {
                    resp.rs1 = "0";
                    resp.rs2 = "<div class='errorBox'>" + Resources.Resource.ErrorNotCorrectTrustedDomain + "</div>";
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div class='errorBox'>" + e.Message.HtmlEncode() + "</div>";
            }
            return resp;
        }

        private string GenerateDnsChangeConfirmUrl(string email, string dnsName, string tenantAlias, ConfirmType confirmType)
        {
            var key = string.Join(string.Empty, new string[] { email.ToLower(), confirmType.ToString().ToLower(), dnsName, tenantAlias });
            var validationKey = EmailValidationKeyProvider.GetEmailKey(key);

            var sb = new StringBuilder();
            sb.Append(CommonLinkUtility.GetFullAbsolutePath("~/confirm.aspx"));
            sb.AppendFormat("?email={0}&key={1}&type={2}", email, validationKey, confirmType);
            if (!string.IsNullOrEmpty(dnsName))
            {
                sb.AppendFormat("&dns={0}", dnsName);
            }
            if (!string.IsNullOrEmpty(tenantAlias))
            {
                sb.AppendFormat("&alias={0}", tenantAlias);
            }
            return sb.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveLanguageTimeSettings(string lng, string timeZoneID)
        {
            var resp = new AjaxResponse();
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var culture = CultureInfo.GetCultureInfo(lng);

                var changelng = false;
                if (SetupInfo.EnabledCultures.Find(c => String.Equals(c.Name, culture.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
                {
                    if (!String.Equals(tenant.Language, culture.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tenant.Language = culture.Name;
                        changelng = true;
                    }
                }

                tenant.TimeZone = new List<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones()).Find(tz => String.Equals(tz.Id, timeZoneID));

                CoreContext.TenantManager.SaveTenant(tenant);
                if (changelng)
                {
                    resp.rs1 = "1";
                }
                else
                {
                    resp.rs1 = "2";
                    resp.rs2 = "<div class=\"okBox\">" + Resources.Resource.SuccessfullySaveSettingsMessage + "</div>";
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div class=\"errorBox\">" + e.Message.HtmlEncode() + "</div>";
            }
            return resp;
        }       

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveEnterSettings(bool enabled)
        {
            AjaxResponse resp = new AjaxResponse();
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                CoreContext.Configuration.DemoAccountEnabled = enabled;
                resp.rs1 = "1";
                resp.rs2 = "<div class=\"okBox\">" + Resources.Resource.SuccessfullySaveEnterSettingsMessage + "</div>";
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div class=\"errorBox\">" + e.Message.HtmlEncode() + "</div>";
            }

            return resp;
        }
        
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveStudioViewSettings(bool leftSidePanel)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            AjaxResponse resp = new AjaxResponse();
            _studioViewSettings = SettingsManager.Instance.LoadSettings<StudioViewSettings>(TenantProvider.CurrentTenantID);
            _studioViewSettings.LeftSidePanel = leftSidePanel;
            if (SettingsManager.Instance.SaveSettings<StudioViewSettings>(_studioViewSettings, TenantProvider.CurrentTenantID))
            {
                resp.rs1 = "1";
                resp.rs2 = "<div class=\"okBox\">" + Resources.Resource.SuccessfullySaveSettingsMessage + "</div>";
            }
            else
            {
                resp.rs1 = "0";
                resp.rs2 = "<div class=\"errorBox\">" + Resources.Resource.UnknownError + "</div>";
            }

            return resp;
        }

        protected static string TenantBaseDomain
        {
            get
            {
                if (String.IsNullOrEmpty(SetupInfo.BaseDomain))
                    return String.Empty;
                else
                    return String.Format(".{0}", SetupInfo.BaseDomain);
            }
        }


        public static string ModifyHowToAdress(string adr)
        {
            var lang = CoreContext.TenantManager.GetCurrentTenant().Language;
            if (lang.Contains("-"))
            {
                lang = lang.Split('-')[0];
            }
            if (lang != "en") lang += "/";
            else lang = string.Empty;
            return string.Format("{0}/{1}{2}", "http://www.teamlab.com", lang, adr ?? string.Empty);
        }
    }
}