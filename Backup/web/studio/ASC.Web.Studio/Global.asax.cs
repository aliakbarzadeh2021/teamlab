using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ASC.Common.Security.Authorizing;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Notify.Patterns;
using ASC.Web.Core;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Security.Ajax;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using AjaxPro.Security;
using log4net.Config;
using TMResourceData;

namespace ASC.Web.Studio
{
    public class Global : HttpApplication
    {
        private static bool _profileEachRequest = false;


        public class StudioSessionEventArgs : EventArgs
        {
            public HttpSessionState Session { get; set; }
        }

        private class FirstRequestInitialization
        {
            private static bool notInitialized = true;

            public static void Initialize(HttpContext context)
            {
                CommonLinkUtility.Initialize();

                if (notInitialized)
                {
                    lock (typeof(FirstRequestInitialization))
                    {
                        if (notInitialized)
                        {
                            HttpContext.Current = context;

                            NotifyConfiguration.Configure();

                            ProductManager.Instance.RegisterProduct(new PeopleProduct());
                            WebItemManager.Instance.LoadItems(new StuidoGlobalHandler());

                            notInitialized = false;
                        }
                    }
                }
            }
        }


        protected void Application_Start(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();
            
            //Handling ajax calls
            AjaxSecurityChecker.Instance.CheckMethodPermissions+=AjaxMethodChecker;

            AppDomain.CurrentDomain.SetData("DataDirectory", Server.MapPath("~/"));
            _profileEachRequest = ConfigurationManager.AppSettings["web.profile.requests"] == "true";

            if (ConfigurationManager.AppSettings["ResourcesFromDb"] == "true")
            {
                XmlPatternProvider.CreateResourceManager = (baseKey, assembly) =>new DBResourceManager(baseKey.Substring(baseKey.LastIndexOf('.')+1)+".resx",new ResourceManager(baseKey,assembly));
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            }
        }

        private bool AjaxMethodChecker(MethodInfo method)
        {
            bool authorized = false;
            if (HttpContext.Current!=null)
            {
                //First check custom authorization
                authorized =
                    method.GetCustomAttributes(typeof(AjaxSecurityAttribute), true).Cast<AjaxSecurityAttribute>().Any(
                        attribute => attribute.CheckAuthorization(HttpContext.Current));
                if (!authorized)
                {
                    //Try Authentificate by cookie here for all ajax request
                    authorized = SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey));                    
                }
                
            }
            return authorized;
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            var wizardSettings = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
            if (!SecurityContext.IsAuthenticated && wizardSettings.Completed)
            {
                if (SetupInfo.WorkMode == WorkMode.Promo)
                    MainPage.AutoAuthByPromo();
                else
                    MainPage.AutoAuthByCookies();
            }
            else if (Request.Url.AbsoluteUri.IndexOf("ajaxpro") < 0 && SecurityContext.IsAuthenticated)
            {
                WebItemManager.Instance.ItemGlobalHandlers.Login(SecurityContext.CurrentAccount.ID);
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            FirstRequestInitialization.Initialize(((HttpApplication)sender).Context);

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (currentTenant == null)
            {
                Response.Redirect(SetupInfo.NoTenantRedirectURL, true);
            }
            else if (currentTenant.Status != ASC.Core.Tenants.TenantStatus.Active)
            {
                var ind = Request.Url.AbsoluteUri.IndexOf(VirtualPathUtility.ToAbsolute("~/confirm.aspx"), StringComparison.InvariantCultureIgnoreCase);
                if (currentTenant.Status == TenantStatus.RemovePending || !(ind >= 0 && currentTenant.Status == TenantStatus.Suspended))
                {
                    Response.Redirect(SetupInfo.NoTenantRedirectURL, true);
                }
            }
          
            Thread.CurrentThread.CurrentCulture = currentTenant.GetCulture();
            Thread.CurrentThread.CurrentUICulture = currentTenant.GetCulture();

            CheckBasicAuth(((HttpApplication)sender).Context);

            WebItemManager.Instance.ItemGlobalHandlers.ApplicationBeginRequest(sender, e);

            FixFlashPlayerCookieBug();

            if (HttpContext.Current != null && _profileEachRequest)
            {
                var extensions = new[] { ".aspx", /*".ashx",*/ ".asmx" };
                if (Array.Exists(extensions, ext => ext.Equals(Path.GetExtension(HttpContext.Current.Request.Path), StringComparison.InvariantCultureIgnoreCase)))
                {
                    var sw = Stopwatch.StartNew();
                    HttpContext.Current.Items.Add("http-profiler-watch", sw);
                    sw.Start();
                }
            }
        }

        private void CheckBasicAuth(HttpContext context)
        {
            string authCookie;
            if (AuthorizationHelper.ProcessBasicAuthorization(context, out authCookie))
            {
                //Authorized through basic
                //Set cookie
                CookiesManager.SetCookies(CookiesType.AuthKey,authCookie);
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current != null && _profileEachRequest)
            {
                var sw = HttpContext.Current.Items["http-profiler-watch"] as Stopwatch;
                if (sw != null)
                {
                    sw.Stop();
                    log4net.ThreadContext.Properties["duration"] = sw.Elapsed.TotalMilliseconds;
                    LogHolder.Log("HttpProfiler").Info(HttpContext.Current.Request.Path);
                }
            }

            if (WebItemManager.Instance.ItemGlobalHandlers != null)
            {
                WebItemManager.Instance.ItemGlobalHandlers.ApplicationEndRequest(sender, e);
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            WebItemManager.Instance.ItemGlobalHandlers.SessionEnd(sender, new StudioSessionEventArgs() { Session = this.Session });
        }

        private void FixFlashPlayerCookieBug()
        {
            /* Fix for the Flash Player Cookie bug in Non-IE browsers.
         * Since Flash Player always sends the IE cookies even in FireFox
         * we have to bypass the cookies by sending the values as part of the POST or GET
         * and overwrite the cookies with the passed in values.
         * 
         * The theory is that at this point (BeginRequest) the cookies have not been read by
         * the Session and Authentication logic and if we update the cookies here we'll get our
         * Session and Authentication restored correctly
         */

            try
            {
                string session_param_name = "ASPSESSID";
                string session_cookie_name = "ASP.NET_SESSIONID";

                if (HttpContext.Current.Request.Form[session_param_name] != null)
                {
                    UpdateCookie(session_cookie_name, HttpContext.Current.Request.Form[session_param_name]);
                }
                else if (HttpContext.Current.Request.QueryString[session_param_name] != null)
                {
                    UpdateCookie(session_cookie_name, HttpContext.Current.Request.QueryString[session_param_name]);
                }
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                Response.Write("Error Initializing Session");
            }

            try
            {
                string auth_param_name = "AUTHID";
                string auth_cookie_name = FormsAuthentication.FormsCookieName;

                if (HttpContext.Current.Request.Form[auth_param_name] != null)
                {
                    UpdateCookie(auth_cookie_name, HttpContext.Current.Request.Form[auth_param_name]);
                }
                else if (HttpContext.Current.Request.QueryString[auth_param_name] != null)
                {
                    UpdateCookie(auth_cookie_name, HttpContext.Current.Request.QueryString[auth_param_name]);
                }

            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                Response.Write("Error Initializing Forms Authentication");
            }
        }

        private void UpdateCookie(string cookie_name, string cookie_value)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(cookie_name);
            if (cookie == null)
            {
                cookie = new HttpCookie(cookie_name);
                HttpContext.Current.Request.Cookies.Add(cookie);
            }
            cookie.Value = cookie_value;
            HttpContext.Current.Request.Cookies.Set(cookie);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            AssemblyWork.UploadResourceData(AppDomain.CurrentDomain.GetAssemblies());
        }

    }
}