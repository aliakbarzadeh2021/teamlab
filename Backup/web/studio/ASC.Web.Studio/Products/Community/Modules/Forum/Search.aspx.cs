using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using AjaxPro;
using ASC.Forum;
using ASC.Web.Controls;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{    
    public partial class Search : MainPage
    {
        protected bool _isFind;
        private bool _isFindByTag;
        private int _tagID;

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.Search);
           
            int currentPageNumber = 0;
            if (!String.IsNullOrEmpty(Request["p"]))
            {
                try
                {
                    currentPageNumber = Convert.ToInt32(Request["p"]);
                }
                catch { currentPageNumber = 0; }
            }
            if (currentPageNumber <= 0)
                currentPageNumber = 1;

            var findTopicList = new List<Topic>();
            int topicCount =0;

            string searchText = "";
            _tagID = 0;
            _isFindByTag = false;

            string tagName = "";

            if (!String.IsNullOrEmpty(Request["tag"]))
            {
                _isFindByTag = true;
                try
                {
                    _tagID = Convert.ToInt32(Request["tag"]);
                }
                catch 
                { 
                    _tagID = 0;
                }

                findTopicList = ForumDataProvider.SearchTopicsByTag(TenantProvider.CurrentTenantID, _tagID,currentPageNumber, ForumManager.Settings.TopicCountOnPage, out topicCount);

            }
            else if (!String.IsNullOrEmpty(Request["search"]))
            {
                searchText = Request["search"];
                findTopicList = ForumDataProvider.SearchTopicsByText(TenantProvider.CurrentTenantID, searchText, currentPageNumber, ForumManager.Settings.TopicCountOnPage, out topicCount);
            }


            var pageNavigator = new PageNavigator()
            {   
                CurrentPageNumber = currentPageNumber,
                EntryCountOnPage = ForumManager.Settings.TopicCountOnPage,
                VisiblePageCount = 5,
                EntryCount = topicCount
            };

            if (_isFindByTag)
                pageNavigator.PageUrl = "search.aspx?tag=" + _tagID.ToString();
            else
                pageNavigator.PageUrl = "search.aspx?search=" + HttpUtility.UrlEncode(searchText, Encoding.UTF8);

            bottomPageNavigatorHolder.Controls.Add(pageNavigator);

            int i = 0;
            foreach (Topic topic in findTopicList)
            {
                _isFind = true;
                if (i == 0)
                {
                    foreach (var tag in topic.Tags)
                    {
                        if (tag.ID == _tagID)
                        {
                            tagName = tag.Name;
                            break;
                        }
                    }
                }

                TopicControl topicControl = (TopicControl)LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/TopicControl.ascx");
                topicControl.SettingsID = ForumManager.Settings.ID;
                topicControl.Topic = topic;
                topicControl.IsShowThreadName = true;
                topicControl.IsEven = (i % 2 == 0);
                this.topicListHolder.Controls.Add(topicControl);
                i++;
            }
            
            if (!_isFind)
            {
                this.topicListHolder.Controls.Add(new ForumNotFoundControl(true));

               
            }
            else if (_isFindByTag)
            {
            }

            Utility.RegisterTypeForAjax(this.GetType());

            var breadCrumbs = (this.Master as ForumMasterPage).BreadCrumbs;
            breadCrumbs.Add(new BreadCrumb() { Caption = Resources.ForumResource.ForumsBreadCrumbs, NavigationUrl = "default.aspx" });
            
            if(_isFindByTag)
                breadCrumbs.Add(new BreadCrumb() { Caption = HeaderStringHelper.GetHTMLSearchByTagHeader(tagName)});
            else
                breadCrumbs.Add(new BreadCrumb() { Caption = HeaderStringHelper.GetHTMLSearchHeader(searchText) });                

            this.Title = HeaderStringHelper.GetPageTitle(Resources.ForumResource.AddonName, breadCrumbs);
        }
       
    }
}
