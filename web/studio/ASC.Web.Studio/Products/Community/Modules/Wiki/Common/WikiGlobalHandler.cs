using System;
using ASC.Web.Core;
using ASC.Web.UserControls.Wiki.Handlers;
using System.Web.Configuration;
using System.Web.Hosting;
using ASC.Common.Data;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiGlobalHandler : IGlobalHandler
    {
        public void InitItemsComplete(IWebItem[] items)
        {
            WikiSection section = (WikiSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath + WikiManager.WikiSectionConfig).GetSection("wikiSettings");
            string dbId = section.DB.ConnectionStringName;
            if (!DbRegistry.IsDatabaseRegistered(dbId))
            {
                DbRegistry.RegisterDatabase(dbId, WebConfigurationManager.ConnectionStrings[dbId]);
            }
        }

        public void Login(Guid userID)
        {
        }

        public void Logout(Guid userID)
        {

        }

        public void SessionEnd(object sender, EventArgs e)
        {

        }

        public void ApplicationBeginRequest(object sender, EventArgs e)
        {

        }

        public void ApplicationEndRequest(object sender, EventArgs e)
        {

        }


        
    }
}
