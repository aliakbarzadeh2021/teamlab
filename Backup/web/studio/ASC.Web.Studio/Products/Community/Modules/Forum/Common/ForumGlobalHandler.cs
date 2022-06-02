using System;
using ASC.Web.Community.Product;
using ASC.Web.Core;
using ASC.Forum;

namespace ASC.Web.Community.Forum
{
    public class ForumGlobalHandler : IGlobalHandler
    {
        #region IGlobalHandler Members

        public void Login(Guid userID)
        {
            ForumDataProvider.InitFirstVisit();
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

        public void InitItemsComplete(IWebItem[] items)
        {
            foreach (var item in items)
            {
                if (item.ID.Equals(ForumManager.Settings.ModuleID)
                    && item.Context != null
                    && item.Context.UserActivityPublisher != null)
                {
                    (item.Context.UserActivityPublisher as ForumActivityPublisher).InitSettings(ForumManager.Settings);
                    break;
                }
            }
        }

        #endregion
    }
}
