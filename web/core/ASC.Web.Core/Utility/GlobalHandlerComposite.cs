using System;
using System.Collections.Generic;

namespace ASC.Web.Core.Utility
{
    public class GlobalHandlerComposite : IGlobalHandler
    {
        public List<IGlobalHandler> ChildGlobalHandlers { get; private set; }

        public GlobalHandlerComposite() : this(null) {}

        public GlobalHandlerComposite(List<IGlobalHandler> childGlobalHandlers)
        {
            this.ChildGlobalHandlers = childGlobalHandlers;
            if (this.ChildGlobalHandlers == null)
                this.ChildGlobalHandlers = new List<IGlobalHandler>();
        }

        #region IGlobalHandler Members

        public void ApplicationBeginRequest(object sender, EventArgs e)
        {
            this.ChildGlobalHandlers.ForEach(gh =>
            {
                try
                {
                    gh.ApplicationBeginRequest(sender, e);
                }
                catch (Exception exc)
                {
                    ASC.Common.Utils.LogHolder.Log("ASC.Web").Error(String.Format("ApplicationBeginRequest GlobalHandler Error"), exc);
                }
            });            
        }

        public void Login(Guid userID)
        {
            this.ChildGlobalHandlers.ForEach(gh =>
            {
                try
                {
                    gh.Login(userID);
                }
                catch (Exception exc)
                {
                    ASC.Common.Utils.LogHolder.Log("ASC.Web").Error(String.Format("Login GlobalHandler Error"), exc);
                }
            });
        }

        public void Logout(Guid userID)
        {
            this.ChildGlobalHandlers.ForEach(gh =>
            {
                try
                {
                    gh.Logout(userID);
                }
                catch (Exception exc)
                {
                    ASC.Common.Utils.LogHolder.Log("ASC.Web").Error(String.Format("Logout GlobalHandler Error"), exc);
                }
            });
        }

        public void SessionEnd(object sender, EventArgs e)
        {
            this.ChildGlobalHandlers.ForEach(gh =>
            {
                try
                {
                    gh.SessionEnd(sender, e);
                }
                catch (Exception exc)
                {
                    ASC.Common.Utils.LogHolder.Log("ASC.Web").Error(String.Format("SessionEnd GlobalHandler Error"), exc);
                }
            });
        }
       

        public void InitItemsComplete(IWebItem[] items)
        {
            this.ChildGlobalHandlers.ForEach(gh =>
            {
                try
                {
                    gh.InitItemsComplete(items);
                }
                catch (Exception exc)
                {
                    ASC.Common.Utils.LogHolder.Log("ASC.Web").Error(String.Format("InitProductsComplete GlobalHandler Error"), exc);
                }
            });
        }
    

        public void ApplicationEndRequest(object sender, EventArgs e)
        {
            this.ChildGlobalHandlers.ForEach(gh =>
            {
                try
                {
                    gh.ApplicationEndRequest(sender, e);
                }
                catch (Exception exc)
                {
                    ASC.Common.Utils.LogHolder.Log("ASC.Web").Error(String.Format("ApplicationEndRequest GlobalHandler Error"), exc);
                }
            });
        }

        #endregion
    }
}
