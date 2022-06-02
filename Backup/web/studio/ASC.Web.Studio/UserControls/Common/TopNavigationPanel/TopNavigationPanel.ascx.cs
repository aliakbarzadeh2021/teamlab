using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WebZones;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SearchHandlers;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class TopNavigationPanel : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Common/TopNavigationPanel/TopNavigationPanel.ascx"; } }

        #region SearchHandlerItem
        private class SearchHandlerItem : IComparable<SearchHandlerItem>
        {
            public Guid ID { get; private set; }

            public ISearchHandlerEx Handler { get; set; }

            public bool Active { get; set; }

            public string LogoURL { get; set; }

            public SearchHandlerItem()
            {
                this.ID = Guid.NewGuid();
            }

            #region IComparable<SearchHandlerItem> Members

            public int CompareTo(SearchHandlerItem other)
            {
                if (other == null)
                    return -1;

                if (this.Handler != null && other.Handler != null)
                {
                    if (this.Handler.GetType().Equals(typeof(StudioSearchHandler)) && !other.Handler.GetType().Equals(typeof(StudioSearchHandler)))
                        return -1;
                    if (!this.Handler.GetType().Equals(typeof(StudioSearchHandler)) && other.Handler.GetType().Equals(typeof(StudioSearchHandler)))
                        return 1;

                    return String.Compare(this.Handler.SearchName, other.Handler.SearchName);
                }

                return 0;
            }

            #endregion
        }
        #endregion

        protected Guid _currentProductID;
        protected UserInfo _currentUser;
        protected IProduct _currentProduct;

        public bool DisableSearch { get; set; }
        public Type SingleSearchHandlerType { get; set; }
        public bool DisableProductNavigation { get; set; }
        public List<NavigationItem> NavigationItems = new List<NavigationItem>();

        public string CustomTitle { get; set; }

        public string CustomTitleIconURL { get; set; }

        public string CustomInfoHTML { get; set; }

        public bool? DisableUserInfo { get; set; }

        public string CustomTitleURL { get; set; }

        private List<SearchHandlerItem> _handlerItems = new List<SearchHandlerItem>();

        private List<IWebItem> _customNavItems = null;

        protected bool _singleSearch;

        /// <summary>
        /// Number of products to be displayed on the header of the site. Other products will be under the More link.
        /// </summary>
        protected const int ProductsDisplayNum = 3;

        protected bool NeedDrawMoreProducts = false;

        private void InitScripts()
        {
            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "topnavpanel_script", WebPath.GetPath("usercontrols/common/topnavigationpanel/js/topnavigator.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "topnavpanel_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/common/topnavigationpanel/css/<theme_folder>/topnavigationpanel.css") + "\">", false);

            if (!DisableSearch)
            {
                #region search scripts
                StringBuilder sb = new StringBuilder();

                string searchText = Page.Request["search"];
                var activeHandler = SearchHandlerManager.GetActiveHandlerEx();

                var allHandlers = SearchHandlerManager.GetHandlersExForProduct(_currentProductID);

                if (_currentProductID.Equals(Guid.Empty) || !allHandlers.Exists(sh => !sh.ProductID.Equals(Guid.Empty)))
                {
                    allHandlers.RemoveAll(sh => sh is StudioSearchHandler);
                    _singleSearch = true;
                }

                if (SingleSearchHandlerType != null)
                {
                    allHandlers.RemoveAll(sh => !sh.GetType().Equals(SingleSearchHandlerType));
                    _singleSearch = true;
                }


                bool isFirst = true;
                foreach (var sh in allHandlers)
                {
                    if (sh == null)
                        continue;

                    var module = WebItemManager.Instance[sh.ModuleID];
                    if (module != null && module.IsDisabled())
                        continue;

                    var shi = new SearchHandlerItem()
                    {
                        Handler = sh,
                        LogoURL = (sh.Logo != null) ? WebImageSupplier.GetAbsoluteWebPath(sh.Logo.ImageFileName, sh.Logo.PartID) : "",
                        Active = String.IsNullOrEmpty(searchText) ? (sh.GetType().Equals(typeof(StudioSearchHandler)) || _singleSearch) :
                                                    (sh.Equals(activeHandler) || (activeHandler == null && isFirst))
                    };

                    _handlerItems.Add(shi);



                    string absoluteSearchURL = sh.AbsoluteSearchURL;

                    if (sh.ProductID.Equals(Guid.Empty) && !this._currentProductID.Equals(Guid.Empty))
                        absoluteSearchURL = absoluteSearchURL + (absoluteSearchURL.IndexOf("?") != -1 ? "&" : "?") + CommonLinkUtility.GetProductParamsPair(this._currentProductID);


                    sb.Append(" Searcher.AddHandler(new SearchHandler('" + shi.ID + "','" + sh.SearchName.HtmlEncode().ReplaceSingleQuote() + "','" + shi.LogoURL + "'," + (shi.Active ? "true" : "false") + ",'" + absoluteSearchURL + "')); ");

                    isFirst = false;
                }


                _handlerItems.Sort((h1, h2) =>
                {
                    return h1.CompareTo(h2);
                });


                //script
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "topnavpanel_init_script", sb.ToString(), true);
                #endregion
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Page is Auth || this.Page is _Default)
                _currentProductID = Guid.Empty;
            else
                _currentProductID = CommonLinkUtility.GetProductID();

            _currentProduct = ProductManager.Instance[_currentProductID];

            InitScripts();

            if (!DisableUserInfo.HasValue)
            {
                _guestInfoHolder.Visible =  !(Page is Auth) && CoreContext.TenantManager.GetCurrentTenant().Public && !SecurityContext.IsAuthenticated;
                _userInfoHolder.Visible = SecurityContext.IsAuthenticated && !(Page is Wizard);

            }
            else 
            {
                _guestInfoHolder.Visible = !DisableUserInfo.Value && !(Page is Auth) && CoreContext.TenantManager.GetCurrentTenant().Public && !SecurityContext.IsAuthenticated;
                _userInfoHolder.Visible = !DisableUserInfo.Value && SecurityContext.IsAuthenticated && !(Page is Wizard);
            }

            if (SecurityContext.IsAuthenticated)
                _currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (this.Page is Auth || this.Page is _Default)
                DisableProductNavigation = true;


            if (DisableProductNavigation)
                _productListHolder.Visible = false;
            else
            {


                var productsList = WebItemManager.Instance.GetItems(WebZoneType.TopNavigationProductList);

                if (productsList.Count >= ProductsDisplayNum)
                {
                    NeedDrawMoreProducts = true;
                    _productRepeater.DataSource = productsList.GetRange(0, ProductsDisplayNum);
                    MoreProductsRepeater.DataSource = productsList.GetRange(ProductsDisplayNum, productsList.Count - ProductsDisplayNum);
                }
                else
                {
                    //all products fake product in top navigation
                    //
                    NavigationWebItem allProducts = new NavigationWebItem(false);
                    allProducts._ModuleName = Resources.UserControlsCommonResource.AllProductsTitle;
                    allProducts._StartURL = CommonLinkUtility.GetDefault();
                    allProducts._ModuleID = Guid.NewGuid();
                    productsList.Add(allProducts);

                    _productRepeater.DataSource = productsList;
                    MoreProductsRepeater.DataSource = new List<IWebItem>();
                }

                _productRepeater.DataBind();
                MoreProductsRepeater.DataBind();
            }

            DisableSearch = (DisableSearch || _handlerItems == null || _handlerItems.Count == 0);
            _searchHolder.Visible = !DisableSearch;

            if (NavigationItems.Count == 0)
                _navItemRepeater.Visible = false;
            else
            {
                _navItemRepeater.DataSource = NavigationItems;
                _navItemRepeater.DataBind();
            }

            if (String.IsNullOrEmpty(_title) && DisableSearch)
                _contentSectionHolder.Visible = false;


            _customNavItems = WebItemManager.Instance.GetItems(WebZoneType.CustomProductList);
            foreach (var item in _customNavItems)
            {
                var render = WebItemManager.Instance[item.ID] as IRenderCustomNavigation;
                if (render != null)
                {
                    try
                    {
                        var control = render.LoadCustomNavigationControl(this.Page);
                        if (control != null)
                        {
                            _customNavControls.Controls.Add(control);
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.LogManager.GetLogger("ASC.Web.Studio").Error(ex);
                    }
                }
            }

            myToolsItemRepeater.DataSource = WebItemManager.Instance.GetItems(WebZoneType.MyTools).OfType<IRenderMyTools>();
            myToolsItemRepeater.DataBind();
        }

        protected bool _canLogout
        {
            get
            {
                return (SetupInfo.WorkMode != WorkMode.Promo);
            }
        }

        protected string _searchText
        {
            get { return (Page.Request["search"] ?? "").HtmlEncode().ReplaceSingleQuote(); }
        }

        protected string _searchLogoUrl
        {
            get
            {
                if (DisableSearch)
                    return "";

                var sh = _handlerItems.Find(h => h.Active);
                return sh != null ? sh.LogoURL : "";
            }
        }

        protected string _title
        {
            get
            {
                if (String.IsNullOrEmpty(this.CustomTitle))
                    return _currentProduct != null ? _currentProduct.ProductName : "";
                else
                    return this.CustomTitle;

            }
        }

        protected string _titleURL
        {
            get
            {
                if (String.IsNullOrEmpty(this.CustomTitleURL))
                    return _currentProduct != null ? VirtualPathUtility.ToAbsolute(_currentProduct.StartURL) : "";
                else
                    return this.CustomTitleURL;

            }
        }

        protected string _titleIconURL
        {
            get
            {
                if (String.IsNullOrEmpty(this.CustomTitleIconURL))
                    return _currentProduct != null ? _currentProduct.GetIconAbsoluteURL() : "";
                else
                    return this.CustomTitleIconURL;

            }
        }

        protected string RenderCustomNavigation()
        {
            var sb = new StringBuilder();
            _customNavItems.Reverse();
            foreach (var item in _customNavItems)
            {
                var render = WebItemManager.Instance[item.ID] as IRenderCustomNavigation;
                if (render != null)
                {
                    var rendered = render.RenderCustomNavigation();
                    if (string.IsNullOrEmpty(rendered))
                    {
                        //Try another way
                        var ctrlRenderer = WebItemManager.Instance[item.ID] as IRenderControlNavigation;
                        if (ctrlRenderer != null)
                        {
                            rendered = ctrlRenderer.RenderCustomNavigation(Page);
                        }
                    }
                    if (!string.IsNullOrEmpty(rendered))
                    {
                        
                        sb.Append("<li style=\"float:right\"><span class=\"spacer\">|</span></li>");
                        sb.Append(rendered);
                    }
                }
                else
                {
                    sb.Append("<li class=\"mailBox\" style=\"float:right;\">");
                    
                    sb.Append("<a href=\"" + VirtualPathUtility.ToAbsolute(item.StartURL) + "\">" + item.Name.HtmlEncode() + "</a>");
                    sb.Append("<span class=\"spacer\">|</span>");
                    sb.Append("</li>");
                }
            }

            return sb.ToString();
        }

        protected string RenderSearchHandlers()
        {
            if (DisableSearch)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (var shi in _handlerItems)
            {
                sb.Append("<div id='studio_shItem_" + shi.ID + "' onclick=\"Searcher.SelectSearchPlace('" + shi.ID + "');\" class='" + (shi.Active ? "searchHandlerActiveItem" : "searchHandlerItem") + "'>");
                if (!String.IsNullOrEmpty(shi.LogoURL))
                    sb.Append("<img alt='' align='absmiddle' src='" + shi.LogoURL + "' style='margin-right:5px;'/>");

                sb.Append(shi.Handler.SearchName.HtmlEncode());
                sb.Append("</div>");
            }
            return sb.ToString();
        }

        #region Render Navigation Item

        protected string RenderNavigationItem(NavigationItem item)
        {
            var hasSubItems = (item.SubItems != null && item.SubItems.Count > 0);
            Guid popupId = Guid.NewGuid();
            var navItem = string.Format(@"
<a class='{0}' href='{1}' {2} {3}>                        
	<span>{4}</span>{5}{6}
</a>",
         item.Selected ? "selectedTab" : "tab",	//class
         hasSubItems ? "javascript:void(0);" : item.URL,	//Url
         item.RightAlign ? "style='float: right;'" : string.Empty,	//float right if item.RigthAlign
         hasSubItems ? string.Format(@"onclick=""PopupPanelHelper.ShowPanel(jq(this), '{0}', 'auto', true, event); return false;""", popupId) : string.Empty,
         HttpUtility.HtmlEncode(item.Name),	//item name
         string.IsNullOrEmpty(item.ModuleStatusIconFileName) ?			//ModuleStatusIconFileName
                                    string.Empty :
                                    string.Format(@"<img src='{0}' style='border: 0px solid White; margin-left: 7px; padding-top:-2px;'/>", item.ModuleStatusIconFileName),
         hasSubItems ? string.Format(@"<img src='{0}' style='border: 0px solid White; margin-left: 6px;'/>", WebImageSupplier.GetAbsoluteWebPath("tri.png")) : string.Empty			//Arrow icon
         );
            if (!hasSubItems)
            {
                return navItem;
            }
            StringBuilder popup = new StringBuilder(navItem);

            popup.AppendFormat("<div id='{0}' class='subMenuItemsPopupBox switchBox'>", popupId);
            foreach (var subItem in item.SubItems)
            {
                popup.AppendFormat("<a href='{0}' class='subMenuItemsPopupBoxItem'>{1}</a>", subItem.URL, HttpUtility.HtmlEncode(subItem.Name));
            }
            popup.Append("</div>");
            return popup.ToString();
        }
        #endregion

    }
}