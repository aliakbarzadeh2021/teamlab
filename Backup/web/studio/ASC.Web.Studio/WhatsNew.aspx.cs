using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Controls;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Web.Studio
{
	public partial class WhatsNew : MainPage
	{
		#region Members

		static internal string PageUrl { get { return "~/whatsnew.aspx"; } }
		protected Guid ProductID { get; set; }
		protected List<Guid> ModuleIDs { get; set; }		

		#endregion
		#region Property

		static public string GetUrlForModule(Guid productId, Guid? moduleId)
		{
			string url = string.Format("{0}?{1}", VirtualPathUtility.ToAbsolute(PageUrl), CommonLinkUtility.GetProductParamsPair(productId));
			if (moduleId.HasValue)
				url = string.Concat(url, "#mid", moduleId.Value, "mid");
			return url;
		}

		#endregion
		#region Events

		protected override void OnPreInit(EventArgs e)
		{
			base.OnPreInit(e);
			base.SetProductMasterPage();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			ProductID = base.GetProductID();
			if (ProductID.Equals(Guid.Empty))
				Response.Redirect(CommonLinkUtility.GetDefault());

			IStudioMaster master = this.Master as IStudioMaster;
			if (master == null) return;
			
			Container container = new Container() { Body = new PlaceHolder(), Header = new PlaceHolder() };
			master.ContentHolder.Controls.Add(container);

			container.BreadCrumbs.Add(new BreadCrumb() { Caption = Resources.Resource.MainTitle, NavigationUrl = VirtualPathUtility.ToAbsolute(ProductManager.Instance[ProductID].StartURL) });
            container.BreadCrumbs.Add(new BreadCrumb() { Caption = Resources.Resource.RecentActivity });

            Title = HeaderStringHelper.GetPageTitle(Resources.Resource.RecentActivity, container.BreadCrumbs);

			InitBody(container.Body);

			SideNavigator navigate = new SideNavigator();
			navigate.Controls.Add(new NavigationItem() { Name = Resources.Resource.MainTitle, URL = VirtualPathUtility.ToAbsolute(ProductManager.Instance[ProductID].StartURL) });
			master.SideHolder.Controls.Add(navigate);
		}

		#endregion
		#region Methods

		private void InitBody(PlaceHolder bodyHolder)
		{
			ASC.Web.Studio.UserControls.Common.WhatsNewBody whatsNewBody = 
								((ASC.Web.Studio.UserControls.Common.WhatsNewBody)LoadControl(ASC.Web.Studio.UserControls.Common.WhatsNewBody.Location));
			whatsNewBody.ProductId = ProductID;			
			bodyHolder.Controls.Add(whatsNewBody);
		}

		#endregion
	}
}