using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Web.Core;
using ASC.Core.Common.Publisher;
using ASC.Web.Community.Product;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core.Publisher;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.Studio;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.Community
{   

    public partial class CommunityMasterPage : PublisherMasterPage, IStudioMaster
    {
        private PublishZone _topBannerZone = new PublishZone(new Zone(Constants.TopBannerZoneID));
        private PublishZone _underProfileZone = new PublishZone(new Zone(Constants.UnderProfileZoneID));
        private List<PublishZone> _zoneCollection = new List<PublishZone>();

        protected void Page_Load(object sender, EventArgs e)
        {   
            var topNavPanel = (TopNavigationPanel)LoadControl(TopNavigationPanel.Location);

            var dashboardItem = new NavigationItem() {
                Name = Resources.CommunityResource.Dashboard,
                URL = VirtualPathUtility.ToAbsolute("~/products/community/"),
                Selected = (this.Page is ASC.Web.Community._Default)};

            topNavPanel.NavigationItems.Add(dashboardItem);
            

            var employeesItem = new NavigationItem()
            {
                Name = CustomNamingPeople.Substitute<Resources.CommunityResource>("Employees"),
                Selected = UserOnlineManager.Instance.IsEmployeesPage() || UserOnlineManager.Instance.IsUserProfilePage(),
                URL = CommonLinkUtility.GetEmployees(CommunityProduct.ID)
            };
            topNavPanel.NavigationItems.Add(employeesItem);
            
            var product = ProductManager.Instance[CommunityProduct.ID];

            var currentModule = UserOnlineManager.Instance.GetCurrentModule();
            foreach (var item in WebItemManager.Instance.GetSubItems(product.ProductID))
            {
                if ((item is Module) == false)
                    continue;

                var module = item as Module;

				string moduleStatusIconFileName = string.Empty;
				switch (module.Status)
				{
					case ModuleStatus.Alpha:
						moduleStatusIconFileName = ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("alpha.png");
						break;
					case ModuleStatus.Beta:
						moduleStatusIconFileName = ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("beta.png");
						break;
				}

                var moduleItem = new NavigationItem()
                {
                    URL = VirtualPathUtility.ToAbsolute(module.MainShortcutCategory.StartURL),
                    Name = module.Name,
                    Description = module.Description,
					ModuleStatusIconFileName = moduleStatusIconFileName,
                };

                if (currentModule != null && currentModule.ModuleID.Equals(module.Guid))
                    moduleItem.Selected = true;

                topNavPanel.NavigationItems.Add(moduleItem);               
            }

            _topNavigatorPlaceHolder.Controls.Add(topNavPanel);
            
            BottomNavigator bottomNavigator = new BottomNavigator();
            _bottomNavigatorPlaceHolder.Controls.Add(bottomNavigator);

            OnlineUsers onlineUsersControl = (OnlineUsers)LoadControl(OnlineUsers.Location);
            onlineUsersControl.ProductId = CommunityProduct.ID;
            phOnlineUsers.Controls.Add(onlineUsersControl);

            if (this.Page is ASC.Web.Community._Default)
            {
                VisitedUsers VisitedUsersControl = (VisitedUsers)LoadControl(VisitedUsers.Location);
                VisitedUsersControl.ProductId = CommunityProduct.ID;
                phVisitedUsers.Controls.Add(VisitedUsersControl);
            }
           
            LoadZoneContents();


        }

        private void TopBannerZoneLoadHandler(object sender, ZoneLoadEventArgs e)
        {
            _topBannerZoneHolder.Controls.Add(new Literal() { Text = e.Article.Html});
        }

        private void UnderProfileZoneLoadHandler(object sender, ZoneLoadEventArgs e)
        {
            if(!String.IsNullOrEmpty(e.Article.Html))
                _underProfileZoneHolder.Controls.Add(new Literal() { Text = "<div style='margin-bottom:20px;'>" + e.Article.Html + "</div>" });
        }

        private void LoadZoneContents()
        {
            _zoneCollection = new List<PublishZone>();

            if (!DisabledSidePanel && !(this.Page is ASC.Web.Community._Default))
            {
                _underProfileZone.Load += new EventHandler<ZoneLoadEventArgs>(UnderProfileZoneLoadHandler);
                _zoneCollection.Add(_underProfileZone);
            }

            _topBannerZone.Load += new EventHandler<ZoneLoadEventArgs>(TopBannerZoneLoadHandler);
            _zoneCollection.Add(_topBannerZone);

            base.InitPageZones();

            base.LoadZoneContent(new RequestContext()
                    {
                        ProductID = ASC.Web.Community.Product.CommunityProduct.ID,
                        User = SecurityContext.IsAuthenticated ? (CoreContext.UserManager.UserExists(SecurityContext.CurrentAccount.ID) ? CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID) : null) : null
                    });
        }
       
        
        #region IStudioMaster Members

        public PlaceHolder ContentHolder
        {
            get { return this._contentHolder; }
        }

        public PlaceHolder SideHolder
        {
            get { return this._sideHolder; }
        }

        public PlaceHolder TitleHolder
        {
            get { return (this.Master as IStudioMaster).TitleHolder; }
        }

        public PlaceHolder FooterHolder
        {
            get { return (this.Master as IStudioMaster).FooterHolder; }
        }

        public bool DisabledSidePanel
        {
            get
            {
                return (this.Master as IStudioMaster).DisabledSidePanel;
            }
            set
            {
                (this.Master as IStudioMaster).DisabledSidePanel = value;
            }
        }

        public bool? LeftSidePanel
        {
            get
            {
                return (this.Master as IStudioMaster).LeftSidePanel;
            }
            set
            {
                (this.Master as IStudioMaster).LeftSidePanel = value;
            }
        }

        #endregion

        public override List<PublishZone> PublishZones
        {
            get { return _zoneCollection; }
        }
    }
}
