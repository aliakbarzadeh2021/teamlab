using System;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WebZones;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;
using System.Reflection;

namespace ASC.Web.Studio.Masters
{
    public partial class StudioTemplate : System.Web.UI.MasterPage, IStudioMaster
    {
        public TopNavigationPanel TopNavigationPanel; 

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            TopNavigationPanel= (TopNavigationPanel)LoadControl(TopNavigationPanel.Location);             
        }
       
        protected void Page_Load(object sender, EventArgs e)
        {
            var topNavPanel = TopNavigationPanel;
            
            if (this.Page is Auth || this.Page is Error404 || this.Page is ServerError)
            {
                if (this.Page is Error404 == false)
                    topNavPanel.DisableProductNavigation = true;

                topNavPanel.DisableSearch = true;
            }
            else if (this.Page is Wizard || this.Page is confirm)
            {
                topNavPanel.DisableProductNavigation = true;
                topNavPanel.DisableUserInfo = true;
                topNavPanel.DisableSearch = true;
            }
            else if (this.Page is _Default)
            {
                topNavPanel.DisableProductNavigation = true;
                topNavPanel.DisableSearch = true;
            }
            else if (this.Page is Management)
            {
                //topNavPanel.CustomTitle = Resources.Resource.Administration;
                //topNavPanel.CustomTitleURL = CommonLinkUtility.GetAdministration();
                //topNavPanel.CustomTitleIconURL = WebImageSupplier.GetAbsoluteWebPath("settings.png");
            }                      
            else if (this.Page is Employee || this.Page is UserProfile)
            {
                topNavPanel.CustomTitle = CustomNamingPeople.Substitute<Resources.Resource>("Employees");
                topNavPanel.CustomTitleURL = CommonLinkUtility.GetEmployees();
                topNavPanel.CustomTitleIconURL = WebImageSupplier.GetAbsoluteWebPath("home.png");
            }
            else if (this.Page is Search)
            {
                topNavPanel.CustomTitle = Resources.Resource.Search;
                topNavPanel.CustomTitleIconURL = WebImageSupplier.GetAbsoluteWebPath("search.gif");
            }
            else if (this.Page is MyStaff)
            {
                bool isDefault;
                MyStaffType type = typeof(MyStaffType).TryParseEnum<MyStaffType>(Request["type"] ?? "", MyStaffType.General, out isDefault);
                if (String.IsNullOrEmpty(Request["type"]))
                    isDefault = false;

                topNavPanel.CustomTitle = Resources.Resource.MyStaff;
                topNavPanel.CustomTitleURL = CommonLinkUtility.GetMyStaff();
                topNavPanel.CustomTitleIconURL = WebImageSupplier.GetAbsoluteWebPath("staff.gif");

                //profile
                topNavPanel.NavigationItems.Add(new NavigationItem()
                {
                    Name = Resources.Resource.Profile,
                    URL = CommonLinkUtility.GetMyStaff(),
                    Selected = (type == MyStaffType.General && !isDefault)
                });
                

                //activity
                topNavPanel.NavigationItems.Add(new NavigationItem()
                {
                    Name = Resources.Resource.RecentActivity,
                    URL = CommonLinkUtility.GetMyStaff(MyStaffType.Activity),
                    Selected = (type == MyStaffType.Activity)
                });

                //subscriptions
                topNavPanel.NavigationItems.Add(new NavigationItem()
                {
                    Name = Resources.Resource.Subscriptions,
                    URL = CommonLinkUtility.GetMyStaff(MyStaffType.Subscriptions),
                    Selected = (type == MyStaffType.Subscriptions)
                });

                var myToolsItems = WebItemManager.Instance.GetItems(ASC.Web.Core.WebZones.WebZoneType.MyTools);
                foreach (var item in myToolsItems)
                {
                    var render = WebItemManager.Instance[item.ID] as IRenderMyTools;
                    if (render == null)
                        continue;

                    topNavPanel.NavigationItems.Add(new NavigationItem()
                    {
                        Name = render.TabName,
                        URL = CommonLinkUtility.GetMyStaff(render.ParameterName),
                        Selected = String.Equals(Request["type"] ?? "", render.ParameterName, StringComparison.InvariantCultureIgnoreCase)
                    });
                }

            }
            
            _topNavigatorPlaceHolder.Controls.Add(topNavPanel);
          

            BottomNavigator bottomNavigator = new BottomNavigator();
            _bottomNavigatorPlaceHolder.Controls.Add(bottomNavigator);
        }

        #region IStudioMaster Members

        public System.Web.UI.WebControls.PlaceHolder ContentHolder
        {
            get { return _contentHolder; }
        }

        public System.Web.UI.WebControls.PlaceHolder SideHolder
        {
            get { return _sideHolder; }
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


        public PlaceHolder TitleHolder
        {
            get { return (this.Master as IStudioMaster).TitleHolder; }
        }

        public PlaceHolder FooterHolder
        {
            get { return (this.Master as IStudioMaster).FooterHolder; }
        }

        
        #endregion
    }   
    
}
