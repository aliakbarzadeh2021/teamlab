using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using ASC.Core;
using ASC.Web.Core.Utility;

namespace ASC.Web.Studio.Core
{
    public enum WorkMode
    {
        Default,
        Develop,
        Promo
    }

    [Serializable]
    [XmlRoot]
    public class SetupInfo
    {
        [XmlElement]
        public string LogonUser
        {
            get;
            set;
        }

        [XmlElement]
        public string LogonUserSID
        {
            get;
            set;
        }

        [XmlElement]
        public bool PartOfDomain
        {
            get;
            set;
        }

        [XmlElement]
        public string Domain
        {
            get;
            set;
        }

        public static string SecureFilter
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["SecureFilter"];
            }
        }

        public static string SslPort
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["SslPort"]??"443";
            }
        }

        public static string HttpPort
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["HttpPort"] ?? "80";
            }
        }

        public static string ImportDomain
        {
            get
            {
                string importDomain = System.Configuration.ConfigurationManager.AppSettings["ImportDomain"];
                return string.IsNullOrEmpty(importDomain) ? HttpContext.Current.Request.Url.Host : importDomain;
            }
        }

        public static WorkMode WorkMode
        {
            get
            {
                string workMode = System.Configuration.ConfigurationManager.AppSettings["WorkMode"];
                if (String.Equals(workMode, "develop", StringComparison.InvariantCultureIgnoreCase))
                    return WorkMode.Develop;

                if (String.Equals(workMode, "promo", StringComparison.InvariantCultureIgnoreCase))
                    return WorkMode.Promo;

                return WorkMode.Default; ;
            }
        }

        public static string PromoActionURL
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["PromoActionURL"];
            }
        }


        public static string StatisticTrackURL
        {
            get
            {
                return ConfigurationManager.AppSettings["stat.trackurl"] ?? string.Empty;
            }
        }

        public static bool StatisticEnable
        {
            get
            {
                if (WorkMode != WorkMode.Default)
                    return false;

                if (String.Equals(System.Configuration.ConfigurationManager.AppSettings["stat.enable"], "false", StringComparison.InvariantCultureIgnoreCase))
                    return false;

                return true;
            }
        }

        public static Guid[] DevelopItems
        {
            get
            {
                List<Guid> ids = new List<Guid>();
                var items = System.Configuration.ConfigurationManager.AppSettings["developitems"] ?? "";
                foreach (var s in items.Split(','))
                {
                    try
                    {
                        var id = new Guid(s);
                        ids.Add(id);
                    }
                    catch { }

                }
                return ids.ToArray();
            }
        }

        public static string SetupXMLPath
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Server != null)
                    return HttpContext.Current.Server.MapPath("setupinfo.xml");

                return "";
            }
        }

        public static SetupInfo ReadFromFile(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                return (SetupInfo)GetSerializer().Deserialize(reader);
            }
        }

        public static void WriteToFile(string fileName, SetupInfo setupInfo)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                GetSerializer().Serialize(writer, setupInfo);
            }
        }

        public static List<CultureInfo> EnabledCultures
        {
            get
            {
                return (ConfigurationManager.AppSettings["enabledcultures"] ?? "en-US")
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => CultureInfo.GetCultureInfo(l.Trim()))
                    .OrderBy(l => l.Name)
                    .ToList();
            }
        }

        public static long MaxTextFileUploadSize
        {
            get { return 5 * 1024 * 1024; }
        }

        public static long MaxImageUploadSize
        {
            get { return 1024 * 1024; }
        }
             

        public static long MaxUploadSize
        {
            get
            {
                long size = 0;
                var diskQuota = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                var usedSize = ASC.Web.Studio.UserControls.Statistics.TenantStatisticsProvider.GetUsedSize();
                if (diskQuota != null)
                {                   
                    var freeSize = diskQuota.MaxTotalSize - usedSize;
                    if(freeSize<diskQuota.MaxFileSize)
                        return freeSize<0? 0: freeSize;
                    else 
                        return diskQuota.MaxFileSize;                    
                }

                return 0 < size ? size : 25 * 1024 * 1024;
            }
        }

        public static long MaxImageUploadSizeInKB
        {
            get { return MaxImageUploadSize / 1024; }
        }

        public static string MaxImageUploadSizeToMBFormat
        {
            get
            {
                var size = MaxImageUploadSize;
                if (size > 0)
                    return (size / (float)(1024 * 1024)).ToString("0.##");

                return "0";
            }
        }

        public static long MaxUploadSizeInKB
        {
            get { return MaxUploadSize / 1024; }
        }

        public static string MaxUploadSizeToMBFormat
        {
            get
            {
                var size = MaxUploadSize;
                if (size > 0)
                    return (size / (float)(1024 * 1024)).ToString("0.##");

                return "0";
            }
        }

        public static bool ThirdPartyAuthEnabled
        {
            get
            {
                return String.Equals(System.Configuration.ConfigurationManager.AppSettings["thirdparty.auth.enabled"], "true");
            }
        }

        private static XmlSerializer GetSerializer()
        {
            return new XmlSerializer(typeof(SetupInfo));
        }

        public static string NoTenantRedirectURL
        {
            get
            {
                var url = System.Configuration.ConfigurationManager.AppSettings["NoTenantRedirectURL"];
                if (string.IsNullOrEmpty(url))
                    return "http://teamlab.com";

                return url;
            }
        }

        public static string[] CustomScripts
        {
            get
            {
                return (ConfigurationManager.AppSettings["customscripts"] ?? string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string NotifyAddress
        {
            get
            {
                return (System.Configuration.ConfigurationManager.AppSettings["notifyaddress"]) ?? "";
            }
        }

        private const string CultureGetKey = "culture";
        public static string GetImportServiceUrl()
        {
            var url = System.Configuration.ConfigurationManager.AppSettings["web.import-contacts.url"];
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            var urlSeparatorChar = "?";
            if (url.Contains(urlSeparatorChar))
            {
                urlSeparatorChar = "&";
            }
            var cultureName = HttpUtility.HtmlEncode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            return UrlSwitcher.SelectCurrentUriScheme(string.Format("{0}{3}{1}={2}", url, CultureGetKey, cultureName, urlSeparatorChar));
        }

        /// <summary>
        /// If mass add enabled - show Add Emloyees import/invite window
        /// If no enabled - whow simple invite
        /// </summary>
        /// <returns></returns>
        public static bool IsMassAddEnabled()
        {
            return !String.IsNullOrEmpty(GetImportServiceUrl());
        }

        public static string BaseDomain
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["asc.core.tenants.base-domain"];
            }
        }

        public static string MobileRedirect
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["web.mobile.redirect"];
            }
        }
    }

}
