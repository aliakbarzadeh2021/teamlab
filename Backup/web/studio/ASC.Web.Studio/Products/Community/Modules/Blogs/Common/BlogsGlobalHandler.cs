using System;
using System.Web;
using ASC.Blogs.Core;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Common.Data;
using ASC.Blogs.Core.Data;

namespace ASC.Web.Community.Blogs
{
    public class BlogsGlobalHandler : IGlobalHandler
    {

        #region IGlobalHandler Members

        public void ApplicationBeginRequest(object sender, EventArgs e)
        {
        }

        public void InitItemsComplete(IWebItem[] items)
        {
            DbRegistry.RegisterDatabase(
                BlogsStorageFactory.DbRegistryKey, 
                System.Web.Configuration.WebConfigurationManager.ConnectionStrings["blogs"]);

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

        public void ApplicationEndRequest(object sender, EventArgs e)
        {

        }

        #endregion
    }
}
