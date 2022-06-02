﻿using System;
using System.Web;

namespace ASC.FederatedLogin.Profile
{
    public static class LoginProfileExtensions
    {
        public static Uri AddProfile(this Uri uri, LoginProfile profile)
        {
            return profile.AppendProfile(uri);
        }
        public static Uri AddProfileSession(this Uri uri, LoginProfile profile, HttpContext context)
        {
            return profile.AppendSessionProfile(uri, context);
        }

        public static Uri AddProfileCache(this Uri uri, LoginProfile profile)
        {
            return profile.AppendCacheProfile(uri);
        }

        public static LoginProfile GetProfile(this Uri uri)
        {
            var profile = new LoginProfile();
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(queryString[LoginProfile.QuerySessionParamName]) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                return (LoginProfile)HttpContext.Current.Session[queryString[LoginProfile.QuerySessionParamName]];
            }
            if (!string.IsNullOrEmpty(queryString[LoginProfile.QueryParamName]))
            {
                profile.ParseFromUrl(uri);
            }
            if (!string.IsNullOrEmpty(queryString[LoginProfile.QueryCacheParamName]))
            {
                return (LoginProfile)HttpRuntime.Cache.Get(queryString[LoginProfile.QuerySessionParamName]);
            }
            return null;
        }

        public static bool HasProfile(this Uri uri)
        {
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            return !string.IsNullOrEmpty(queryString[LoginProfile.QueryParamName]) || !string.IsNullOrEmpty(queryString[LoginProfile.QuerySessionParamName]) || !string.IsNullOrEmpty(queryString[LoginProfile.QueryCacheParamName]);
        }


    }
}