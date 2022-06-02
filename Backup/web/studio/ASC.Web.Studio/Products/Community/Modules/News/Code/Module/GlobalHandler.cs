using System;
using System.Web.Configuration;
using ASC.Common.Data;
using ASC.Web.Community.News.Code.DAO;
using ASC.Web.Core;

namespace ASC.Web.Community.News.Code.Module
{
	public class GlobalHandler : IGlobalHandler
	{
        public void InitItemsComplete(IWebItem[] items)
		{
			var dbId = FeedStorageFactory.Id;
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