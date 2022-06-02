using System;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    public partial class Auth : MainPage
    {
        protected string _loginMessage;
        protected string _login;
        protected string _password;
        protected TenantInfoSettings _tenantInfoSettings;

        protected void Page_Load(object sender, EventArgs e)
        {
            _login = "";
            _password = "";

            //Account link control
            AccountLinkControl accountLink = null;
            if (SetupInfo.ThirdPartyAuthEnabled)
            {
                accountLink = (AccountLinkControl)LoadControl(AccountLinkControl.Location);
                associateAccount.Visible = true;
                associateAccount.Text = Resources.Resource.LoginWithAccount;
                accountLink.ClientCallback = "authCallback";
                accountLink.SettingsView = false;
                signInPlaceholder.Controls.Add(accountLink);
            }

            ((IStudioMaster)this.Master).DisabledSidePanel = true;

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);

            this.Title = HeaderStringHelper.GetPageTitle(Resources.Resource.Authorization, null, null);

            pwdReminderHolder.Controls.Add(LoadControl(PwdTool.Location));
            pwdReminderHolder.Controls.Add(LoadControl(InviteEmployeeControl.Location));

            _joinBlock.Visible = EnabledJoin;

            if (this.IsPostBack && !SecurityContext.IsAuthenticated)
            {
                if (!String.IsNullOrEmpty(Request["login"]))
                    _login = Request["login"];

                if (!String.IsNullOrEmpty(Request["pwd"]))
                    _password = Request["pwd"];

                bool isDemo = false;
                if (!String.IsNullOrEmpty(Request["authtype"]))
                    isDemo = Request["authtype"] == "demo";

                string hashId = string.Empty;
                if (!string.IsNullOrEmpty(Request["__EVENTARGUMENT"]) && Request["__EVENTTARGET"] == "signInLogin" && accountLink != null)
                {
                    //Login from open id
                    hashId = Request["__EVENTARGUMENT"];
                }

                if (isDemo)
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.Demo);
                }
                else
                {
                    try
                    {
                        string cookiesKey = string.Empty;
                        if (!string.IsNullOrEmpty(hashId))
                        {
                            var accounts = accountLink.GetLinker().GetLinkedObjectsByHashId(hashId);

                            foreach (var account in accounts.Select(x =>
                            {
                                try
                                {
                                    return new Guid(x);
                                }
                                catch
                                {
                                    return Guid.Empty;
                                }
                            }))
                            {
                                if (CoreContext.UserManager.UserExists(account) && account != Guid.Empty)
                                {
                                    var coreAcc = CoreContext.UserManager.GetUsers(account);
                                    cookiesKey = SecurityContext.AuthenticateMe(coreAcc.Email, CoreContext.Authentication.GetUserPasswordHash(coreAcc.ID));
                                }
                            }
                            if (string.IsNullOrEmpty(cookiesKey))
                            {
                                _loginMessage = "<div class=\"errorBox\">" + HttpUtility.HtmlEncode(Resources.Resource.LoginWithAccountNotFound) + "</div>";
                                return;
                            }
                        }
                        else
                        {

                            cookiesKey = SecurityContext.AuthenticateMe(_login, _password);
                        }

                        CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
                    }
                    catch (System.Security.SecurityException)
                    {
                        _loginMessage = "<div class=\"errorBox\">" + HttpUtility.HtmlEncode(Resources.Resource.InvalidUsernameOrPassword) + "</div>";
                        return;
                    }
                    catch (Exception exception)
                    {
                        _loginMessage = "<div class=\"errorBox\">" + HttpUtility.HtmlEncode(exception.Message) + "</div>";
                        return;
                    }
                }

                UserOnlineManager.Instance.RegistryOnlineUser(SecurityContext.CurrentAccount.ID);

                WebItemManager.Instance.ItemGlobalHandlers.Login(SecurityContext.CurrentAccount.ID);

                string refererURL = (string)Session["refererURL"];
                if (String.IsNullOrEmpty(refererURL))
                    Response.Redirect("~/");
                else
                {
                    Session["refererURL"] = null;
                    Response.Redirect(refererURL);
                }

                return;

            }
            else if (SecurityContext.IsAuthenticated && base.IsLogout)
            {
                try
                {
                    WebItemManager.Instance.ItemGlobalHandlers.Logout(SecurityContext.CurrentAccount.ID);

                }
                finally
                {
                    //logout
                    UserOnlineManager.Instance.UnRegistryOnlineUser(SecurityContext.CurrentAccount.ID);

                    if (!SecurityContext.DemoMode)
                        CookiesManager.ClearCookies(CookiesType.AuthKey);

                    SecurityContext.Logout();
                    Response.Redirect("~/auth.aspx");
                }
            }
        }

        protected bool EnabledJoin
        {
            get
            {
                var t = CoreContext.TenantManager.GetCurrentTenant();
                if ((t.TrustedDomainsType == ASC.Core.Tenants.TenantTrustedDomainsType.Custom && t.TrustedDomains.Count > 0) ||
                    t.TrustedDomainsType == ASC.Core.Tenants.TenantTrustedDomainsType.All)
                    return true;
                return false;
            }
        }

        protected string RenderDemoEnter()
        {
            if (!CoreContext.Configuration.DemoAccountEnabled)
                return "";

            return "<a class=\"linkHeaderLightMedium\" href=\"./\" onclick=\"AuthManager.Login('demo'); return false;\">" + Resources.Resource.DemoEnterButton + "</a>";
        }
    }
}
