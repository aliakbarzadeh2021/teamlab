using System;
using System.Collections.Generic;
using ASC.Forum;
using ASC.Web.Controls;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    public partial class Default : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.Default);

            List<ThreadCategory> categories = null;
            List<Thread> threads = null;

            ForumDataProvider.GetThreadCategories(TenantProvider.CurrentTenantID, true, out categories, out threads);


            if (categories.Count > 0)
            {
                var categoryListControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/ThreadCategoryListControl.ascx") as ASC.Web.UserControls.Forum.ThreadCategoryListControl;
                categoryListControl.Categories = categories;
                categoryListControl.Threads = threads;
                categoryListControl.SettingsID = ForumManager.Settings.ID;
                this.forumListHolder.Controls.Add(categoryListControl);
            }
            else
            {
                _headerHolder.Visible = false;
                this.forumListHolder.Controls.Add(new ForumNotFoundControl(false));
            }


            var breadCrumbs = (this.Master as ForumMasterPage).BreadCrumbs;
            breadCrumbs.Add(new BreadCrumb() { Caption = Resources.ForumResource.ForumsBreadCrumbs, NavigationUrl = "default.aspx" });

            Title = HeaderStringHelper.GetPageTitle(Resources.ForumResource.AddonName, breadCrumbs);
        }
    }
}
