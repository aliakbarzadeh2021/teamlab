using System;
using System.Configuration;
using System.Web;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty;

namespace ASC.FederatedLogin.LoginProviders
{
    public interface ILoginProvider
    {
        LoginProfile ProcessAuthoriztion(HttpContext context);
    }
}