using System;
using System.Web;
using AjaxPro;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    [AjaxNamespace("Wizard")]
    public partial class Wizard : MainPage
    {
        protected TenantInfoSettings _tenantInfoSettings;
        protected string _defaultFirstName = string.Empty;
        protected string _defaultLastName = string.Empty;


        protected void Page_Load(object sender, EventArgs e)
        {
            ((IStudioMaster)Master).DisabledSidePanel = true;
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
            var wizardSettings = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
            if (!wizardSettings.Completed)
            {
                Title = HeaderStringHelper.GetPageTitle(Resources.Resource.SetAdminPageTitle, null, null);
                try
                {
                    var setupInfo = SetupInfo.ReadFromFile(SetupInfo.SetupXMLPath);
                    var index = setupInfo.LogonUser.IndexOf('.');
                    if (index == -1) index = setupInfo.LogonUser.IndexOf(' ');
                    if (index != -1)
                    {
                        _defaultFirstName = setupInfo.LogonUser.Substring(0, index);
                        _defaultLastName = setupInfo.LogonUser.Substring(index + 1);
                    }
                    else
                    {
                        _defaultFirstName = setupInfo.LogonUser;
                    }
                }
                catch { };
            }
            else
            {
                Response.Redirect("~/");
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse CreateAdmin(string firstName, string lastName, string email, string pwd)
        {
            var wizardSettings = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
            var resp = new AjaxResponse() { rs1 = "0" };

            if (wizardSettings.Completed)
            {
                resp.rs2 = Resources.Resource.ErrorWizardAlreadyCompleted;
                return resp;
            }
            if (String.IsNullOrEmpty(firstName))
            {
                resp.rs2 = Resources.Resource.ErrorEmptyUserFirstName;
                return resp;
            }
            if (String.IsNullOrEmpty(lastName))
            {
                resp.rs2 = Resources.Resource.ErrorEmptyUserLastName;
                return resp;
            }
            if (!email.TestEmailRegex())
            {
                resp.rs2 = Resources.Resource.ErrorNotCorrectEmail;
                return resp;
            }
            if (String.IsNullOrEmpty(pwd))
            {
                resp.rs2 = Resources.Resource.ErrorNotCorrectPassword;
                return resp;
            }

            try
            {
                var ui = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                ui.FirstName = firstName;
                ui.LastName = lastName;
                ui.Email = email;
                ui.UserName = email.Substring(0, email.IndexOf('@'));

                UserManagerWrapper.SaveUserInfo(ui, pwd);
                wizardSettings.Completed = true;
                SettingsManager.Instance.SaveSettings<WizardSettings>(wizardSettings, TenantProvider.CurrentTenantID);

                resp.rs1 = "1";
                resp.rs2 = VirtualPathUtility.ToAbsolute("~/");

                SecurityContext.Logout();
                CookiesManager.SetCookies(CookiesType.UserID, ui.ID.ToString());
                CookiesManager.SetCookies(CookiesType.AuthKey, SecurityContext.AuthenticateMe(ui.ID.ToString(), pwd));
            }
            catch (Exception e)
            {
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }
            return resp;
        }
    }
}
