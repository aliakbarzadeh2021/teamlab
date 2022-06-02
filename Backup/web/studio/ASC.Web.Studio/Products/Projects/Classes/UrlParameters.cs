#region Import

using System;
using System.Web;

#endregion

namespace ASC.Web.Projects.Classes
{

    public static class UrlParameters
    {

        public static String ProjectsFilter
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.ProjectsFilter];
                return result == null ? "" : result;
            }
        }
        public static String ProjectsTag
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.ProjectsTag];
                return result == null ? "" : result;
            }
        }

        public static String ActionType
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Action];
                return result == null ? "" : result;
            }
        }

        public static String Search
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Search];
                return result == null ? "" : result;
            }
        }

        public static String EntityID
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.EntityID];
                return result == null ? "" : result;
            }
        }

        public static String ProjectID
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.ProjectID];
                return result == null ? "" : result;
            }
        }

        public static Int32 PageNumber
        {
            get
            {
                int result;

                return int.TryParse(HttpContext.Current.Request[UrlConstant.PageNumber], out result) ? result : 1;
            }
        }

        public static Guid UserID
        {

            get
            {
                string result = HttpContext.Current.Request[UrlConstant.UserID];
                return result == null ? Guid.Empty : new Guid(result);
            }

        }

        public static int? Version
        {

            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Version];

                int version;

                if (!int.TryParse(result, out version))
                    return null;

                return version;
            }

        }

        public static String ReportType
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.ReportType];
                return result == null ? "" : result;
            }
        }

        public static String CategoryID
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.CategoryID];
                return result == null ? "" : result;
            }
        }

    }

}