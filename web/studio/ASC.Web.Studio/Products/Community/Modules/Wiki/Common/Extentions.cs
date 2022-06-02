﻿using System.Web;
using System.Web.UI;
namespace ASC.Web.Community.Wiki.Common
{
    public static class ResponseExtention
    {
        public static void RedirectLC(this HttpResponse response, string url, Page page)
        {
            response.Redirect(page.ResolveUrlLC(url));
        }

        public static void RedirectLC(this HttpResponse response, string url, Page page, bool endResponse)
        {
            response.Redirect(page.ResolveUrlLC(url), endResponse);
        }
    }

    public static class ControlsExtention
    {
        public static string ResolveUrlLC(this Control control, string url)
        {
            if (!url.Contains("?"))
            {
                return control.ResolveUrl(url).ToLower();
            }

            string sUrl = url.Split('?')[0];
            string sParams = url.Split('?')[1];

            return  string.Format("{0}?{1}", control.ResolveUrl(sUrl).ToLower(), sParams);
            
        }

        
        

    }
}