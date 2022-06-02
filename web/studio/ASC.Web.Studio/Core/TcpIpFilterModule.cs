using System;
using System.Web;
using ASC.Core;
using ASC.Common.Security.Authorizing;
using System.Net;

namespace ASC.Web.Studio.Core
{
    public class TcpIpFilterModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostAcquireRequestState += OnPostAcquireRequestState;
        }

        public void OnPostAcquireRequestState(Object source, EventArgs e)
        {
            if (SecurityContext.CurrentAccount.IsAuthenticated)
            {
                var context = ((HttpApplication)source).Context;
                if (context.Request.Url.LocalPath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(context.Request.UserHostName))
                {
                    try
                    {
                        SecurityContext.DemandPermissions(new TcpIpFilterSecurityObject(context.Request.UserHostName), TcpIpFilterActions.TcpIpFilterAction);
                    }
                    catch (AuthorizingException error)
                    {
                        throw new HttpException((int)HttpStatusCode.Forbidden, HttpStatusCode.Forbidden.ToString(), error);
                    }
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
