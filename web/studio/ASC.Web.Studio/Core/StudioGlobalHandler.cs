using System;
using System.Web.Configuration;
using ASC.Web.Core;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Controls.Dashboard.Dao;
using ASC.Web.Studio.Core.SearchHandlers;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core
{
	internal class StuidoGlobalHandler : IGlobalHandler
	{
		public void ApplicationBeginRequest(object sender, EventArgs e)
		{

		}

		public void InitItemsComplete(IWebItem[] items)
		{
			SettingsManager.Instance.Configure("webstudio", WebConfigurationManager.ConnectionStrings["webstudio"]);
			UserActivityManager.Configure("webstudio", WebConfigurationManager.ConnectionStrings["webstudio"]);
			WidgetManager.Configure("webstudio", WebConfigurationManager.ConnectionStrings["webstudio"]);
            FCKUploadsDBManager.Configure("webstudio", WebConfigurationManager.ConnectionStrings["webstudio"]);
			StatisticManager.Configure("webstat", WebConfigurationManager.ConnectionStrings["webstat"]);
            
            
			new StudioSearchHandler();

			new EmployeeSearchHendler();
		}

		public void Login(Guid userID)
		{			
		}

		public void Logout(Guid userID)
		{			
		}

		public void SessionEnd(object sender, EventArgs e)
		{
            var args = (Global.StudioSessionEventArgs)e;
            CommonControlsConfigurer.FCKClearTempStore(args.Session);
		}

		public void ApplicationEndRequest(object sender, EventArgs e)
		{

		}
	}
}
