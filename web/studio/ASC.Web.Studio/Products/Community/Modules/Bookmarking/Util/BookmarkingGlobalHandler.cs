using System;
using System.Collections.Generic;
using System.Web;
using ASC.Bookmarking.Common;
using ASC.Common.Data;
using System.Web.Configuration;
using ASC.Web.Core;

namespace ASC.Web.Community.Bookmarking.Util
{
	public class BookmarkingGlobalHandler : IGlobalHandler
	{
		#region IGlobalHandler Members

		public void ApplicationBeginRequest(object sender, EventArgs e)
		{
		}

		public void ApplicationEndRequest(object sender, EventArgs e)
		{
		}

        public void InitItemsComplete(IWebItem[] items)
		{
			var dbId = BookmarkingBusinessConstants.BookmarkingDbID;
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

		#endregion
	}
}
