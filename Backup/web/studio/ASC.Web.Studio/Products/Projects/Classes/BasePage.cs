using System;
using System.Runtime.Remoting.Messaging;
using ASC.Common.Security;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Studio;
using System.Web.Configuration;

namespace ASC.Web.Projects
{
    public abstract class BasePage : MainPage
    {
        protected BasePage()
        {
            if (CallContext.GetData("CURRENT_ACCOUNT") == null && SecurityContext.IsAuthenticated)
            {
                CallContext.SetData("CURRENT_ACCOUNT", SecurityContext.CurrentAccount.ID);
            }
        }

        public Participant Participant
        {
            get { return ASC.Web.Projects.Classes.Global.EngineFactory.GetParticipantEngine().GetByID(SecurityContext.CurrentAccount.ID); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            PageLoad();
        }

        protected abstract void PageLoad();
    }
}
