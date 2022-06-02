﻿using System;
using System.Linq;
using System.Web;

namespace ASC.Web.Core.Utility
{
    public static class UrlSwitcher
    {
        public static string SelectCurrentUriScheme(string uri)
        {
            return HttpContext.Current != null ? SelectUriScheme(uri, HttpContext.Current.Request.Url.Scheme) : uri;
        }

        public static string SelectUriScheme(string uri, string scheme)
        {
            return Uri.IsWellFormedUriString(uri,UriKind.Absolute) ? SelectUriScheme(new Uri(uri, UriKind.Absolute),scheme).ToString() : uri;
        }

        public static Uri SelectCurrentUriScheme(Uri uri)
        {
            if (HttpContext.Current!=null)
            {
                return SelectUriScheme(uri, HttpContext.Current.Request.Url.Scheme);
            }
            return uri;
        }

        public static Uri SelectUriScheme(Uri uri, string scheme)
        {
            if (!string.IsNullOrEmpty(scheme) && !scheme.Equals(uri.Scheme,StringComparison.OrdinalIgnoreCase))
            {
                //Switch
                var builder = new UriBuilder(uri) { Scheme = scheme.ToLowerInvariant(), Port = scheme.Equals("https",StringComparison.OrdinalIgnoreCase)?443:80};//Set proper port!
                return builder.Uri;
            }
            return uri;
        }

        public static Uri ToCurrentScheme(this Uri uri)
        {
            return SelectCurrentUriScheme(uri);
        }

        public static Uri ToScheme(this Uri uri, string scheme)
        {
            return SelectUriScheme(uri,scheme);
        }
    }
}