using System;
using System.Web;

namespace ASC.Data.Storage
{
    public static class SecureHelper
    {
        public static bool IsSecure()
        {
            return HttpContext.Current != null
                   && !string.IsNullOrEmpty(HttpContext.Current.Request.Url.Scheme)
                   && string.Equals(HttpContext.Current.Request.Url.Scheme, "https", StringComparison.OrdinalIgnoreCase);
        }
    }
}