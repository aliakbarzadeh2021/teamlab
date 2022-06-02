using System;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Users
{
    public partial class VisitedUsers : System.Web.UI.UserControl
    {
		public int UserVisitedCount
		{
			get;
			private set;
		}

		public Guid ProductId
		{
			get;
			set;
		}

        public static string Location
        {
            get { return "~/UserControls/Users/VisitedUsers.ascx"; }
        }

        protected string RenderProfileLink(UserInfo userInfo)
        {
            return userInfo.RenderProfileLink(ProductId);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
			var users = StatisticManager
				.GetVisitorsToday(TenantProvider.CurrentTenantID, ProductId)
				.ConvertAll(id => CoreContext.UserManager.GetUsers(id))
				.FindAll(u => u.ID != ASC.Core.Users.Constants.LostUser.ID);
			rpVisitedUsers.DataSource = users;
			UserVisitedCount = users.Count;
            rpVisitedUsers.DataBind();
        }
    }
}