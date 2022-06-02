using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class ViewPage : BaseUserControl
    {
        private string _pageName = string.Empty;
        public string PageName
        {
            get
            {
                return PageNameUtil.Encode(_pageName);
            }
            set
            {
                _pageName = PageNameUtil.Decode(value);
            }
        }
        public int Version { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
           
        }

        public bool CanEditPage
        {
            get
            {
                if (ViewState["CanEditPage"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["CanEditPage"]);
            }
            set
            {
                ViewState["CanEditPage"] = value;
            }
        }

        protected string RenderPageContent()
        {
            Pages page;


            if (_pageName == null)
            {
                _pageName = string.Empty;
            }

            if (Version > 0)
            {
                page = wikiDAO.PagesHistGetByNameVersion(_pageName, Version);
            }
            else
            {
                page = wikiDAO.PagesGetByName(_pageName);
            }
            
            if (page == null)
            {
                return RenderEmptyPage();
            }

            RiseWikiPageLoaded(page);
            RisePublishVersionInfo(page);



            return HtmlWikiUtil.WikiToHtml(page.PageName, page.Body, Page.ResolveUrl(Request.AppRelativeCurrentExecutionFilePath), PagesProvider.GetExistingPagesFilesListByBody(page.Body, TenantId), Page.ResolveUrl(ImageHandlerUrlFormat), TenantId, CanEditPage && Version == 0 ? ConvertType.Editable : ConvertType.NotEditable);
        }


        protected string RenderEmptyPage()
        {
            RisePageEmptyEvent();
            return string.Empty;//HtmlWikiUtil.WikiToHtml(WikiUCResource.MainPage_EmptyPage);
        }

    }
}