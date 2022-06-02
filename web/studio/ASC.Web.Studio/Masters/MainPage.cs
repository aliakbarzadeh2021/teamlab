﻿using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using ASC.Common.Security.Authentication;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Publisher;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using log4net;

namespace ASC.Web.Studio
{

    /// <summary>
    /// Base page for all pages in projects
    /// </summary>
    public partial class MainPage : System.Web.UI.Page, IPublishZoneCollection
    {
        protected static ILog Log { get { return LogHolder.Log("ASC.Web"); } }

        public WebSkin CurrentSkin { get; set; }

        public WebItemSettings WebItemSettings { get; set; }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            //check if cookie from this portal
            if (SecurityContext.CurrentAccount is IUserAccount &&
                ((IUserAccount)SecurityContext.CurrentAccount).Tenant != CoreContext.TenantManager.GetCurrentTenant().TenantId)
            {
                SecurityContext.Logout();
                Response.Redirect("~/");
            }

            ProcessSecureFilter();

            var wizardSettings = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
            WebItemSettings = SettingsManager.Instance.LoadSettings<WebItemSettings>(TenantProvider.CurrentTenantID);

            if (!wizardSettings.Completed)
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    var cookiesKey = SecurityContext.AuthenticateMe(UserManagerWrapper.AdminID.ToString(), "admin");
                    CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
                    WebItemManager.Instance.ItemGlobalHandlers.Login(SecurityContext.CurrentAccount.ID);
                }

                if ((this is Wizard) == false)
                {
                    Response.Redirect("~/wizard.aspx");
                    return;
                }
            }

            else if (!SecurityContext.IsAuthenticated && wizardSettings.Completed && !(this is confirm))
            {
                //for demo
                if (SetupInfo.WorkMode == WorkMode.Promo)
                {
                    if (AutoAuthByPromo())
                    {
                        UserOnlineManager.Instance.RegistryOnlineUser(SecurityContext.CurrentAccount.ID);

                        Response.Redirect("~/");
                        return;
                    }
                }

                if (this is Auth && Session["refererURL"] == null && !string.IsNullOrEmpty(HttpContext.Current.Request.Params["id"]))
                {
                    var authCookie = HttpContext.Current.Request.Params["id"];
                    if (AuthByCookies(authCookie))
                    {
                        CookiesManager.SetCookies(CookiesType.AuthKey, authCookie);
                        var first = Request["first"] == "1";
                        if (first)
                        {
                            try
                            {
                                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                                tenant.Name = Resources.Resource.StudioWelcomeHeader;
                                CoreContext.TenantManager.SaveTenant(tenant);
                            }
                            catch { }
                        }
                        Response.Redirect(VirtualPathUtility.ToAbsolute("~/") + (first ? "?first=1" : ""));
                        return;
                    }
                }

                if (!(this is Auth))
                {
                    var refererURL = Request.Url.AbsoluteUri;
                    if (!ValidateRefererUrl(refererURL))
                        refererURL = (string)Session["refererURL"];
                    
                    if (!AutoAuthByCookies() && !CoreContext.TenantManager.GetCurrentTenant().Public)
                    {
                        Session["refererURL"] = refererURL;
                        Response.Redirect("~/auth.aspx");
                        return;
                    }
                }
            }

            else if (SecurityContext.IsAuthenticated && this is Auth && !this.IsLogout)
            {
                Response.Redirect("~/");
                return;
            }

            else if (this is Wizard && wizardSettings.Completed)
            {
                Response.Redirect("~/");
                return;
            }


            IProduct currentProduct = null;
            IModule currentModule = null;
            CommonLinkUtility.GetLocationByRequest(this.Request, out currentProduct, out currentModule);

            //check disable and public 
            if (currentProduct != null && (
                (currentProduct as IWebItem).IsDisabled() || 
                (currentProduct as IWebItem).IsDisabledForPublic()
                ))
            {
                Response.Redirect("~/");
                return;
            }


            if (SecurityContext.IsAuthenticated)
            {
                UserOnlineManager.Instance.RegistryOnlineUser(SecurityContext.CurrentAccount.ID);

                try
                {
                    StatisticManager.SaveUserVisit(TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID,
                                                                     (currentProduct == null ? Guid.Empty : currentProduct.ProductID),
                                                                     (currentModule == null ? Guid.Empty : currentModule.ModuleID));
                }
                catch (Exception exc)
                {
                    Log.Error("failed save user visit", exc);
                }

            }

            try
            {
                WebLog.WriteLog("{0};{1};{2}", Request.UserHostAddress, CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email, Request.Url);
            }
            catch { }

            CurrentSkin = WebSkin.GetUserSkin();
            this.Theme = CurrentSkin.ASPTheme;


            #region Init common javascript resources
            var commonJavascriptResources = "CommonJavascriptResources";
            if (!Page.ClientScript.IsClientScriptBlockRegistered(commonJavascriptResources))
            {
                var script = string.Format(@"
var CommonJavascriptResources = {{
	CancelConfirmMessage : '{0}'
}};", Resources.Resource.CancelConfirmMessage.ReplaceSingleQuote());
                Page.ClientScript.RegisterClientScriptBlock(typeof(string), commonJavascriptResources, script, true);
            }
            #endregion

        }

        private void ProcessSecureFilter()
        {
            var filter = SetupInfo.SecureFilter;
            if (HttpContext.Current != null)
            {
                //ssl enable only on subdomain of basedomain
                if (HttpContext.Current.Request.Url.Host.EndsWith("." + SetupInfo.BaseDomain))
                    SecureFilter.GetInstance(filter).ProcessRequest(Request.Url,SetupInfo.SslPort, SetupInfo.HttpPort);
            }
        }

        protected override void OnPreLoad(EventArgs e)
        {
            foreach (var css in CurrentSkin.CSSFileNames)
            {
                RegisterCSSLink(CurrentSkin.GetCSSFileAbsoluteWebPath(css));
            }
            RegisterCSSLink(CurrentSkin.GetCSSFileAbsoluteWebPath(Path.GetFileNameWithoutExtension(CurrentSkin.BaseCSSFileName) +
                "." + CoreContext.TenantManager.GetCurrentTenant().GetCulture().Name.ToLower() + Path.GetExtension(CurrentSkin.BaseCSSFileName)));
            base.OnPreLoad(e);
        }

        public static bool AutoAuthByPromo()
        {   
            try
            {
                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.Demo);
                WebItemManager.Instance.ItemGlobalHandlers.Login(SecurityContext.CurrentAccount.ID);
                
                return true;
            }
            catch { }

            return false;
        }

        public static bool AutoAuthByCookies()
        {
            return AuthByCookies(CookiesManager.GetCookies(CookiesType.AuthKey));
        }


        public static bool AuthByCookies(string cookiesKey)
        {
            var result = false;

            if (!string.IsNullOrEmpty(cookiesKey))
            {
                try
                {
                    if (SecurityContext.AuthenticateMe(cookiesKey))
                    {
                        result = true;
                        WebItemManager.Instance.ItemGlobalHandlers.Login(SecurityContext.CurrentAccount.ID);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("AutoAuthByCookies Error {0}", ex);
                }
            }

            return result;
        }

        private bool ValidateRefererUrl(string refererURL)
        {
            if (String.IsNullOrEmpty(refererURL)
               || (this is _Default)
               || (this is Error404)
               || (this is ServerError)
               || refererURL.IndexOf("Subgurim_FileUploader", StringComparison.InvariantCultureIgnoreCase) != -1
               || refererURL.IndexOf("servererror.aspx", StringComparison.InvariantCultureIgnoreCase) != -1
               || refererURL.IndexOf("error404.aspx", StringComparison.InvariantCultureIgnoreCase) != -1
            )
            {
                return false;
            }

            return true;
        }

        protected void RegisterCSSLink(string url)
        {
            HtmlLink link = new HtmlLink();
            Page.Header.Controls.Add(link);
            link.EnableViewState = false;
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            link.Href = url;
        }

        protected void SetProductMasterPage()
        {
            Guid productID = this.GetProductID();

            if (productID.Equals(Guid.Empty))
                this.MasterPageFile = "~/masters/studiotemplate.master";
            else
            {
                string productMasterPageFile = ProductManager.Instance[productID].Context.MasterPageFile;
                if (!String.IsNullOrEmpty(productMasterPageFile))
                    this.MasterPageFile = productMasterPageFile;
                else
                    this.MasterPageFile = "~/masters/studiotemplate.master";
            }
        }

        protected Guid GetProductID()
        {
            return CommonLinkUtility.GetProductID();
        }

        protected bool IsLogout
        {
            get
            {
                if (this is Auth)
                {
                    string logoutParam = Request["t"];
                    if (String.IsNullOrEmpty(logoutParam))
                        logoutParam = "";

                    return logoutParam.ToLower() == "logout";
                }

                return false;
            }
        }

        #region IPublishZoneCollection Members

        public virtual System.Collections.Generic.List<PublishZone> PublishZones
        {
            get { return new System.Collections.Generic.List<PublishZone>(); }
        }

        #endregion
    }
}
