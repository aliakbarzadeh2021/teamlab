using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using ASC.Common.Utils;
using ASC.Core;

namespace ASC.Web.Core.Mobile
{
    public class MobileDetector:IHttpModule
    {
        private const string TenantReplacePattern = "%tenant%";

        public static void RedirectToMobileVersionIfNeeded(string mobileAddress, HttpContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(mobileAddress) && IsRequestMatchesMobile(context) && !CookiesManager.IsMobileBlocked())
                {
                    //TODO: check user status to display desktop or mobile version
                    if (mobileAddress.StartsWith("~"))
                    {
                        //Resolve to current
                        mobileAddress = VirtualPathUtility.ToAbsolute(mobileAddress);
                    }
                    if (mobileAddress.Contains(TenantReplacePattern))
                    {
                        var tennant = CoreContext.TenantManager.GetCurrentTenant();
                        mobileAddress = mobileAddress.Replace(TenantReplacePattern, tennant.TenantAlias);
                    }

                    var redirectUri = Uri.IsWellFormedUriString(mobileAddress, UriKind.Absolute) ? new Uri(mobileAddress) : new Uri(context.Request.Url, mobileAddress);
                    if (!redirectUri.Equals(context.Request.Url))
                    {
                        LogHolder.Log("ASC.Mobile.Redirect").DebugFormat("Redirecting url:'{1}' to mobile. UA={0}",context.Request.UserAgent,context.Request.Url);
                        context.Response.Redirect(redirectUri.ToString(), true);
                    }
                }

            }
            catch(ThreadAbortException)
            {
                //Don't do nothing
            }
            catch (Exception e)
            {
                //If error happens it's not so bad as you may think. We won't redirect user to mobile version.
                LogHolder.Log("ASC.Mobile.Redirect").Error("failed to redirect user to mobile",e);
            }
        }

        private static volatile Regex _userAgentRegex;
        private static readonly object SyncLock = new object();

        private const string UserAgentRegexDefaultPattern =
            @"android|avantgo|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od|ad)|iris|kindle|lge |maemo|midp|mmp|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\\/|plucker|pocket|psp|symbian|treo|up\\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino";

        private static Regex UserAgentRegex
        {
            get
            {
                if (_userAgentRegex==null)
                {
                    lock (SyncLock)
                    {
                        if (_userAgentRegex==null)
                        {
                            _userAgentRegex = new Regex(System.Configuration.ConfigurationManager.AppSettings["web.mobile.regex"] ?? UserAgentRegexDefaultPattern, 
                                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                        }
                    }
                }
                return _userAgentRegex;
            }
        }

        public static bool IsRequestMatchesMobile(HttpContext context)
        {
            //Check user agent
            var u = context.Request.UserAgent;
            return IsUserAgentMatchesMobile(u);
        }

        public static bool IsUserAgentMatchesMobile(string userAgnet)
        {
            return !string.IsNullOrEmpty(userAgnet) && (UserAgentRegex.IsMatch(userAgnet));
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest+=BeginRequest;
        }

        private static void BeginRequest(object sender, EventArgs e)
        {
            //Detect mobile support on begin request
            RedirectToMobileVersionIfNeeded(System.Configuration.ConfigurationManager.AppSettings["web.mobile.redirect"],((HttpApplication)sender).Context);
        }

        public void Dispose()
        {
            
        }
    }


}