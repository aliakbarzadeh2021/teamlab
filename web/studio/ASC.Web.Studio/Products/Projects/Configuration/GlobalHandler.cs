using System;
using System.Runtime.Remoting.Messaging;
using ASC.Projects.Data;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Configuration
{
    public class GlobalHandler : IGlobalHandler
    {
        public void ApplicationBeginRequest(object sender, EventArgs e)
        {
            if (CommonLinkUtility.GetProductID() != ProductEntryPoint.ID) return;

            CallContext.SetData("CURRENT_TENANT_ID", TenantProvider.CurrentTenantID);
        }

        public void ApplicationEndRequest(object sender, EventArgs e)
        {
        }
        
        public void InitItemsComplete(IWebItem[] items)
        {
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
    }
}
