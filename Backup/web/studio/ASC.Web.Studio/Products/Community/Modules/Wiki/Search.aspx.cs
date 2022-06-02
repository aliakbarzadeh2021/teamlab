using System;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.Controls;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.Community.Wiki
{
    public partial class Search : WikiBasePage
    {
        protected string searchContent
        {
            get
            {
                return Request["Search"];
            }
        }

        protected bool PageNameOnly
        {
            get
            {
                return Request["pn"] != null;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            (Master as WikiMaster).GetNavigateActionsVisible += new WikiMaster.GetNavigateActionsVisibleHandle(Search_GetNavigateActionsVisible);

            BreadCrumb.Add(new BreadCrumb() { Caption = WikiResource.wikiSearchCaption, NavigationUrl = this.ResolveUrlLC("Search.aspx") });
            if (!string.IsNullOrEmpty(searchContent))
            {
                BreadCrumb.Add(new BreadCrumb() { Caption = string.Format(WikiResource.wikiSearchContentFormat, searchContent) });
            }

            if (!IsPostBack)
            {
                BindRepeater();
            }

        }

        WikiNavigationActionVisible Search_GetNavigateActionsVisible()
        {
            return WikiNavigationActionVisible.AddNewPage;
        }

        private void BindRepeater()
        {

            if (PageNameOnly)
            {
                rptPageList.DataSource = PagesProvider.PagesSearchByName(searchContent, TenantId);
            }
            else
            {
                rptPageList.DataSource = PagesProvider.PagesSearchAllByContentEntry(searchContent, TenantId);
            }
            
            rptPageList.DataBind();
        }


        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            var pageName = ((LinkButton)sender).CommandName;
            foreach (var comment in CommentsProvider.GetAllComments(pageName, TenantId))
            {
                CommonControlsConfigurer.FCKUploadsRemoveForItem("wiki_comments", comment.Id.ToString());
            }
            PagesProvider.PagesDelete(pageName, TenantId);
            BindRepeater();
        }


        protected new string GetPageName(Pages page)
        {
            if (string.IsNullOrEmpty(page.PageName))
                return WikiResource.MainWikiCaption;
            return page.PageName;
        }

        protected new string GetPageViewLink(Pages page)
        {
            return ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), page.PageName);
        }

        protected string GetPageEditLink(Pages page)
        {
            return ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), page.PageName);
        }

        protected string GetPageInfo(Pages page)
        {
            if (page.UserID.Equals(Guid.Empty))
            {
                return string.Empty;
            }

            return ProcessVersionInfo(page.PageName, page.UserID, page.Date, page.Version, false);
        }

        protected string GetAuthor(Pages page)
        {
            return CoreContext.UserManager.GetUsers(page.UserID).RenderProfileLink(ASC.Web.Community.Product.CommunityProduct.ID);
        }

        protected string GetDate(Pages page)
        {
            return string.Format("<span class=\"wikiDateTime\">{0}</span> {1}", page.Date.ToString("t"), page.Date.ToString("d"));
        }

    }
}
