using System;
using System.Web.Configuration;
using ASC.Common.Data;
using ASC.Files.Core;
using ASC.Web.Core;

namespace ASC.Web.Files.Configuration
{
    public class GlobalHandler : ASC.Web.Core.IGlobalHandler
    {
      
        public void InitItemsComplete(IWebItem[] products)
        {
            if (!IsExistProduct(products)) return;

            if (!DbRegistry.IsDatabaseRegistered(FileConst.DatabaseId))
            {
                DbRegistry.RegisterDatabase(FileConst.DatabaseId, WebConfigurationManager.ConnectionStrings[FileConst.DatabaseId]);
            }
        }

        protected bool IsExistProduct(IWebItem[] products)
        {
            foreach (var product in products)
            {
                if (product.ID == ProductEntryPoint.ID) return true;
            }
            return false;
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
