using System;
using System.Web.Configuration;
using ASC.Core;
using ASC.Xmpp.Common;

namespace ASC.Web.Talk
{
    class TalkConfiguration
    {
        public string ServerAddress
        {
            get;
            private set;
        }

        public string UpdateInterval
        {
            get;
            private set;
        }

        public string OverdueInterval
        {
            get;
            private set;
        }

        public string ServerName
        {
            get;
            private set;
        }

        public string ServerPort
        {
            get;
            private set;
        }

        public string BoshUri
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public string Jid
        {
            get;
            private set;
        }

        public string FileTransportType
        {
            get;
            private set;
        }

        public string RequestTransportType
        {
            get;
            private set;
        }

        public bool EnabledFirebugLite
        {
          get;
          private set;
        }

        public bool EnabledHistory
        {
            get;
            private set;
        }

        public bool EnabledConferences
        {
            get;
            private set;
        }

        public bool EnabledMassend
        {
            get;
            private set;
        }

        public String ValidSymbols
        {
            get;
            private set;
        }

        public String HistoryLength
        {
            get;
            private set;
        }

        public String ResourcePriority
        {
            get;
            private set;
        }

        public String ClientInactivity
        {
            get;
            private set;
        }

        public TalkConfiguration()
        {
            JabberClientConfiguration cfg = null;
            try
            {
                cfg = new JabberServiceClient().GetClientConfiguration(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            }
            catch { }

            ServerAddress = WebConfigurationManager.AppSettings["JabberAddress"] ?? (cfg != null ? cfg.Domain : string.Empty);
            ServerName = cfg != null ? cfg.Domain : ServerAddress;
            ServerPort = WebConfigurationManager.AppSettings["JabberPort"] ?? (cfg != null ? cfg.Port : 5222).ToString();
            if (WebConfigurationManager.AppSettings["BoshPath"] != null)
            {
                BoshUri = string.Format(WebConfigurationManager.AppSettings["BoshPath"], ServerAddress);
            }
            else if (cfg != null && cfg.BoshUri != null)
            {
                BoshUri = cfg.BoshUri.ToString();
            }
            else
            {
                BoshUri = string.Empty;
            }
            try
            {
                UserName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName.ToLowerInvariant();
            }
            catch
            {
                UserName = string.Empty;
            }
            Jid = string.Format("{0}@{1}", UserName, ServerName).ToLowerInvariant();
            RequestTransportType = WebConfigurationManager.AppSettings["RequestTransportType"] ?? "flash";
            FileTransportType = WebConfigurationManager.AppSettings["FileTransportType"] ?? "flash";
            // in seconds
            UpdateInterval = WebConfigurationManager.AppSettings["UpdateInterval"] ?? 3600.ToString();
            OverdueInterval = WebConfigurationManager.AppSettings["OverdueInterval"] ?? 300.ToString();

            EnabledHistory = (WebConfigurationManager.AppSettings["History"] ?? "off") == "on";
            EnabledMassend = (WebConfigurationManager.AppSettings["Massend"] ?? "off") == "on";
            EnabledConferences = (WebConfigurationManager.AppSettings["Conferences"] ?? "off") == "on";
            EnabledFirebugLite = (WebConfigurationManager.AppSettings["FirebugLite"] ?? "off") == "on";
            ValidSymbols = WebConfigurationManager.AppSettings["ValidSymbols"] ?? "";
            HistoryLength = WebConfigurationManager.AppSettings["HistoryLength"] ?? 10.ToString();
            ResourcePriority = WebConfigurationManager.AppSettings["ResourcePriority"] ?? 60.ToString();
            ClientInactivity = WebConfigurationManager.AppSettings["ClientInactivity"] ?? 90.ToString();
        }
    }
}