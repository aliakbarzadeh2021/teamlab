using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using ASC.Web.Community.Product;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Web.UserControls.Wiki.UC;
using ASC.Web.Controls;


namespace ASC.Web.Community.Wiki.Common
{
    public class WikiSearchHandler : BaseSearchHandlerEx
    {

        private int TenantId
        {
            get
            {
                return TenantProvider.CurrentTenantID;
            }
        }

        #region ISearchHandler Members

        public override SearchResultItem[] Search(string text)
        {
            List<SearchResultItem> list = new List<SearchResultItem>();
            WikiSection section = (WikiSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath + WikiManager.WikiSectionConfig).GetSection("wikiSettings");
            string rootPage = HttpContext.Current.Request.PhysicalApplicationPath.TrimEnd('\\');
            string defPageHref = VirtualPathUtility.ToAbsolute(WikiManager.ViewVirtualPath);
            PagesProvider.SetConnectionStringName(section.DB.ConnectionStringName);

            string pageName;


            foreach (Pages page in PagesProvider.PagesSearchAllByContentEntry(text, TenantId))
            {
                pageName = page.PageName;
                if (string.IsNullOrEmpty(pageName))
                {
                    pageName = WikiResource.MainWikiCaption;
                }

                list.Add(new SearchResultItem()
                {
                    Name = pageName,
                    Description = HtmlUtility.GetText(
                        EditPage.ConvertWikiToHtml(page.PageName, page.Body, defPageHref,
                            section.ImageHangler.UrlFormat, TenantId), 120),
                    URL = ActionHelper.GetViewPagePath(defPageHref, page.PageName)
                });
            }
            return list.ToArray();
        }
       
        #endregion

        public override string AbsoluteSearchURL
        {
            get { return VirtualPathUtility.ToAbsolute(WikiManager.BaseVirtualPath + "/Search.aspx").ToLower(); }
        }

        public override ImageOptions Logo
        {
            get 
            {
                return new ImageOptions() { ImageFileName = "WikiLogo16.png", PartID = WikiManager.ModuleId };
            }
        }

        public override string SearchName
        {
            get { return WikiManager.SearchDefaultString; }
        }

        public override Guid ModuleID
        {
            get { return WikiManager.ModuleId; }
        }

        public override string PlaceVirtualPath
        {
            get { return WikiManager.BaseVirtualPath; }
        }

        public override Guid ProductID
        {
            get { return CommunityProduct.ID; }
        }
    }
}
