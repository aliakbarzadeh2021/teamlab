using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.SessionState;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.FederatedLogin.Profile;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace ASC.FederatedLogin
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Login : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            ReturnUrl = context.Request["returnurl"] ?? FormsAuthentication.LoginUrl;
            Callback = context.Request["callback"] ?? "loginCallback";
            if (!string.IsNullOrEmpty("mode"))
            {
                Mode = (LoginMode) Enum.Parse(typeof(LoginMode),context.Request["mode"],true);
            }

            var auth = context.Request["auth"];
            if (!string.IsNullOrEmpty(auth))
            {
                try
                {
                    var profile = ProviderManager.Process(auth, context);
                    if (profile != null)
                    {
                        SendClientData(context, profile);
                    }
                }
                catch (ThreadAbortException)
                {
                    //Thats is responce ending
                }
                catch (Exception ex)
                {
                    SendClientData(context,LoginProfile.FromError(ex));
                }
            }
            else
            {
                //Render xrds
                RenderXrds(context);
            }
        }

        protected string Callback { get; set; }

        private void RenderXrds(HttpContext context)
        {
            var xrdsloginuri = new Uri(context.Request.Url,
                                       new Uri(context.Request.Url.AbsolutePath, UriKind.Relative)) + "?auth=openid&returnurl=" + ReturnUrl;
            var xrdsimageuri = new Uri(context.Request.Url,
                                       new Uri(context.Request.ApplicationPath, UriKind.Relative)) + "openid.gif";
            XrdsHelper.RenderXrds(context.Response, xrdsloginuri, xrdsimageuri);
        }

        protected LoginMode Mode { get; set; }

        protected string ReturnUrl { get; set; }

        public bool IsReusable
        {
            get { return false; }
        }


        private void SendClientData(HttpContext context, LoginProfile profile)
        {
            if (Mode==LoginMode.Redirect)
            {
                RedirectToReturnUrl(context,profile);
            }
            else if (Mode==LoginMode.Popup)
            {
                SendJsCallback(context, profile);
            }
        }

        private void SendJsCallback(HttpContext context, LoginProfile profile)
        {
            //Render a page
            context.Response.ContentType = "text/html";
            context.Response.Write(JsCallbackHelper.GetCallbackPage().Replace("%PROFILE%", profile.ToJson()).Replace("%CALLBACK%", Callback));
        }

        private void RedirectToReturnUrl(HttpContext context, LoginProfile profile)
        {
            if (context.Session != null)
            {
                //Store in session
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfileSession(profile, context).ToString(), true);
            }
            else if (HttpRuntime.Cache != null)
            {
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfileCache(profile).ToString(), true);
            }
            else
            {
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfile(profile).ToString(), true);
            }
        }
    }
}
