using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Common.Publisher;
using ASC.Web.Controls;
using ASC.Web.Core.Utility;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Configuration;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core.Publisher;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Users;

namespace ASC.Web.Files.Masters
{
    public partial class BasicTemplate : PublisherMasterPage, IStudioMaster
    {
        #region Methods

        public List<BreadCrumb> BreadCrumbs
        {
            get
            {
                if (_commonContainer.BreadCrumbs == null) _commonContainer.BreadCrumbs = new List<BreadCrumb>();
                return _commonContainer.BreadCrumbs;
            }
        }

        protected void InitControls()
        {
            var searchHandler = (BaseSearchHandlerEx)(SearchHandlerManager.GetHandlersExForProduct(ProductEntryPoint.ID)).Find(sh => sh is SearchHandler);

            if (searchHandler != null)
            {
                searchHandler.AbsoluteSearchURL = (VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "/search.aspx"));
            }

            RenderHeader();

            BottomNavigator bottomNavigator = new BottomNavigator();
            _bottomNavigatorPlaceHolder.Controls.Add(bottomNavigator);

            base.InitPageZones();

            base.LoadZoneContent(new RequestContext
            {
                ProductID = ProductEntryPoint.ID,
                User = SecurityContext.IsAuthenticated ? (CoreContext.UserManager.UserExists(SecurityContext.CurrentAccount.ID) ? CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID) : null) : null
            });
        }


        #endregion

        protected void RenderHeader()
        {
            TopNavigationPanel topNavigationPanel = (TopNavigationPanel)LoadControl(TopNavigationPanel.Location);

            topNavigationPanel.SingleSearchHandlerType = typeof(SearchHandler);

            _topNavigationPanelPlaceHolder.Controls.Add(topNavigationPanel);

        }


        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            OnlineUsers onlineUsersControl = (OnlineUsers)LoadControl(OnlineUsers.Location);
            onlineUsersControl.ProductId = ProductEntryPoint.ID;
            phOnlineUsers.Controls.Add(onlineUsersControl);

            VisitedUsers VisitedUsersControl = (VisitedUsers)LoadControl(VisitedUsers.Location);
            VisitedUsersControl.ProductId = ProductEntryPoint.ID;

            phVisitedUsers.Controls.Add(VisitedUsersControl);
        }

        #region Overrides of PublisherMasterPage

        public bool CommonContainerHeaderVisible
        {
            get { return _commonContainer.Options.HeadStyle.Contains("display:none"); }
            set { _commonContainer.Options.HeadStyle = value ? String.Empty : "display:none"; }
        }

        public override List<PublishZone> PublishZones
        {
            get { return new List<PublishZone>(); }
        }

        #endregion

        #region Implementation of IStudioMaster

        public PlaceHolder ContentHolder
        {
            get
            {
                _commonContainer.Visible = false;
                return _contentHolder;
            }
        }

        public PlaceHolder SideHolder
        {
            get { return (Master as IStudioMaster).SideHolder; }
        }

        public PlaceHolder TitleHolder
        {
            get { return (Master as IStudioMaster).TitleHolder; }
        }

        public PlaceHolder FooterHolder
        {
            get { return (Master as IStudioMaster).FooterHolder; }
        }


        public bool DisabledSidePanel
        {
            get { return (this.Master as IStudioMaster).DisabledSidePanel; }
            set { (this.Master as IStudioMaster).DisabledSidePanel = value; }
        }

        public bool? LeftSidePanel
        {
            get { return (this.Master as IStudioMaster).LeftSidePanel; }
            set { (this.Master as IStudioMaster).LeftSidePanel = value; }
        }

        #endregion
    }
}