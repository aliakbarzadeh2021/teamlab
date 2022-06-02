using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.UserControls.Wiki.Handlers;
using System.Web.Configuration;
using System.Web.Hosting;
using ASC.Web.UserControls.Wiki;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiStatisticProvider : IStatisticProvider
    {

        private WikiSection section;


        private int TenantId
        {
            get
            {
                return TenantProvider.CurrentTenantID;
            }
        }

        public WikiStatisticProvider()
        {
            section = (WikiSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath + WikiManager.WikiSectionConfig).GetSection("wikiSettings");
            PagesProvider.SetConnectionStringName(section.DB.ConnectionStringName);
        }

        private int GetPostCount(Guid userID)
        {
            return PagesProvider.PagesGetCountByCreatedUserId(userID, TenantId);
        }


        #region IStatisticProvider Members

        
        public List<StatisticItem> GetAllStatistic(Guid userID)
        {
            return new List<StatisticItem>();
        }

        
        public StatisticItem GetMainStatistic(Guid userID)
        {

            return new StatisticItem()
            {
                Count = GetPostCount(userID),
                URL = VirtualPathUtility.ToAbsolute(string.Format("{0}/ListPages.aspx", WikiManager.BaseVirtualPath)).ToLower() + "?uid=" + userID,
                Name = WikiResource.WikiPageCount
            };
        }

        #endregion

    }
}
