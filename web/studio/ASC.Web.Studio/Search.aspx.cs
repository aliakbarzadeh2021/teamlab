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
using ASC.Web.Studio.Core.SearchHandlers;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Web.Studio
{
    public partial class Search : MainPage
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            base.SetProductMasterPage();
        }

        protected string _searchText;
        const int MaxResultCount = 5;
        private class SearchResult
        {
            public string LogoURL { get; set; }
            public string Name { get; set; }
            public Guid ModuleID { get; set; }
            public List<SearchResultItem> Items { get; set; }
            public string MoreURL { get; set; }

            public SearchResult()
            {
                this.Items = new List<SearchResultItem>();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Guid productID = base.GetProductID();     

            IStudioMaster master = this.Master as IStudioMaster;
            if (master == null) return;


            master.DisabledSidePanel = true;

            _searchText = Request["search"] ?? "";
            List<SearchResult> searchResult = SearchByModules(productID, _searchText);


            Container container = new Container() { Body = new PlaceHolder(), Header = new PlaceHolder() };
            master.ContentHolder.Controls.Add(container);

            container.BreadCrumbs.Add(new BreadCrumb() { Caption = Resources.Resource.MainTitle, NavigationUrl = productID.Equals(Guid.Empty)? CommonLinkUtility.GetDefault():VirtualPathUtility.ToAbsolute(ProductManager.Instance[productID].StartURL) });
            container.BreadCrumbs.Add(new BreadCrumb() { Caption = HeaderStringHelper.GetHTMLSearchHeader(_searchText) });

            container.Body.Controls.Add(GetSearchResult(searchResult));

            Title = HeaderStringHelper.GetPageTitle(Resources.Resource.Search, container.BreadCrumbs);
        }

        private List<SearchResult> SearchByModules(Guid productID, string _searchText)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            if (!String.IsNullOrEmpty(_searchText))
            {
                foreach (var sh in SearchHandlerManager.GetHandlersExForProduct(productID))
                {                    
                    var module = WebItemManager.Instance[sh.ModuleID];
                    if (module != null && module.IsDisabled())
                        continue;
                    
                    var items = sh.Search(_searchText);
                    if (items.Length > 0)
                    {                        
                        var searchResult = new SearchResult()
                        {
                            ModuleID = sh.ModuleID
                        };

                        if (sh.GetType().Equals(typeof(EmployeeSearchHendler)))
                            searchResult.Name = ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("Employees");
                        else if (module != null)
                            searchResult.Name = module.Name;
                        else
                            searchResult.Name = sh.SearchName;


                        if (sh.GetType().Equals(typeof(EmployeeSearchHendler)))
                            searchResult.LogoURL = WebImageSupplier.GetAbsoluteWebPath("home.png");
                        else if (module != null)
                            searchResult.LogoURL = module.GetIconAbsoluteURL();
                        else if (sh.Logo != null)
                            searchResult.LogoURL = WebImageSupplier.GetAbsoluteWebPath(sh.Logo.ImageFileName, sh.Logo.PartID);

                        searchResult.MoreURL = sh.AbsoluteSearchURL;

                        if (sh.ProductID.Equals(Guid.Empty) && !productID.Equals(Guid.Empty))
                            searchResult.MoreURL = sh.AbsoluteSearchURL + (sh.AbsoluteSearchURL.IndexOf("?") != -1 ? "&" : "?") + CommonLinkUtility.GetProductParamsPair(productID);

                        searchResult.MoreURL = searchResult.MoreURL + (searchResult.MoreURL.IndexOf("?") != -1 ? "&search=" : "?search=") + HttpUtility.UrlEncode(_searchText, Encoding.UTF8);


                        searchResult.Items.AddRange(items);

                        searchResults.Add(searchResult);
                    }
                }
            }

            return searchResults;
        }

        private LiteralControl GetSearchResult(List<SearchResult> searchResult)
        {

            if (searchResult.Count <= 0)
            {
                return new NotFoundControl();
            }

            StringBuilder sb = new StringBuilder();
            foreach (var result in searchResult)
            {
                //head
                sb.AppendFormat("<div style=\"padding: 10px;\" class=\"clearFix\">");
                sb.AppendFormat("<div style=\"vertical-align: middle; float: left;\" class=\"headerBase\">");
                sb.AppendFormat("<img align=\"absmiddle\" alt=\"{1}\" style=\"margin-right: 5px\" src=\"{0}\" />", result.LogoURL, result.Name);
                sb.Append(result.Name.HtmlEncode());
                sb.AppendFormat("</div>");
                sb.AppendFormat("<div style=\"float:right;padding-top:10px;\">");
                sb.AppendFormat("{0}: {1}&nbsp;&nbsp;|&nbsp;&nbsp;<a href=\"{3}\" >{2}</a>"
                    , Resources.Resource.TotalFinded, result.Items.Count, Resources.Resource.ShowAllSearchResult, result.MoreURL);
                sb.AppendFormat("</div>");
                sb.AppendFormat("</div>");

                //results
                sb.AppendFormat(" <div class=\"borderBase tintMedium\" style=\"border-left:none;border-right:none\">");
                foreach (var item in result.Items.GetRange(0, (MaxResultCount < result.Items.Count) ? MaxResultCount : result.Items.Count))
                {
                    sb.AppendFormat("<div style=\"padding: 5px 5px 5px 30px\">");
                    sb.AppendFormat("<a href=\"{1}\">{0}</a><br /><span class=\"textBigDescribe\">{2}</span>",
                            item.Name.HtmlEncode(), item.URL, item.Description.HtmlEncode());
                    sb.AppendFormat("</div>");
                }
                sb.AppendFormat("</div>");
            }
            

            return new LiteralControl(sb.ToString());
        }
    }
}
