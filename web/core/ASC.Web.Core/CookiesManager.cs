using System;
using System.Web;
using System.Linq;

namespace ASC.Web.Core
{
    public enum CookiesType
    {
        AuthKey,
        UserID,
        EmployeeViewMode,
        MinimizedNavpanel,
        NoMobile
    }

    public class CookiesManager
    {
        private const string AuthCookiesName = "asc_auth_key";
        private const string NoMobileCookiesName = "asc_nomobile";
        private const string UidCookiesName = "asc_selectuid_key";
        private const string EmpViewModeCookiesName = "asc_empviewmode_key";
        private const string MinimizedNavpanel = "asc_minimized_np";

        private static string GetCookiesName(CookiesType type)
        {
            switch (type)
            {
                case CookiesType.AuthKey: return AuthCookiesName;
                case CookiesType.UserID: return UidCookiesName;
                case CookiesType.EmployeeViewMode: return EmpViewModeCookiesName;
                case CookiesType.MinimizedNavpanel: return MinimizedNavpanel;
                case CookiesType.NoMobile:
                    return NoMobileCookiesName;
            }

            return string.Empty;
        }

        public static bool IsMobileBlocked()
        {
            return !string.IsNullOrEmpty(GetRequestVar(CookiesType.NoMobile));
        }

        public static string GetRequestVar(CookiesType type)
        {
            if (HttpContext.Current!=null)
            {
                var cookie = GetCookies(type);
                return string.IsNullOrEmpty(cookie) ? HttpContext.Current.Request[GetCookiesName(type)] : cookie;
            }
            return "";
        }

        public static void SetCookies(CookiesType type, string value)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Value = value;
                HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = DateTime.Now.AddDays(365);
            }
        }

        public static string GetCookies(CookiesType type)
        {
            if (HttpContext.Current != null)
            {
                var cookieName = GetCookiesName(type);
                if (HttpContext.Current.Response.Cookies.AllKeys.Contains(cookieName))
                    return HttpContext.Current.Response.Cookies[cookieName].Value ?? "";

                if (HttpContext.Current.Request.Cookies[cookieName] != null)
                    return HttpContext.Current.Request.Cookies[cookieName].Value ?? "";
            }
            return "";
        }

        public static void ClearCookies(CookiesType type)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Request.Cookies[GetCookiesName(type)] != null)
                    HttpContext.Current.Response.Cookies[GetCookiesName(type)].Expires = DateTime.Now.AddDays(-3);
            }
        }
    }

}