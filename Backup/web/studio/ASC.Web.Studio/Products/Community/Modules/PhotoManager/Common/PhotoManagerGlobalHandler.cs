using System;
using System.Web.Configuration;
using ASC.Common.Data;
using ASC.PhotoManager;
using ASC.PhotoManager.Data;
using ASC.PhotoManager.Model;
using ASC.Web.Community.Product;
using ASC.Web.Core;

namespace ASC.Web.Community.PhotoManager
{
	public class PhotoManagerGlobalHandler : IGlobalHandler
	{

		#region IGlobalHandler Members

		public void ApplicationBeginRequest(object sender, EventArgs e)
		{
		}

        public void InitItemsComplete(IWebItem[] items)
		{
			if (!DbRegistry.IsDatabaseRegistered(StorageFactory.Id))
			{
				DbRegistry.RegisterDatabase(StorageFactory.Id, WebConfigurationManager.ConnectionStrings[StorageFactory.Id]);
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

		public void ApplicationEndRequest(object sender, EventArgs e)
		{

		}

		#endregion
	}
}
