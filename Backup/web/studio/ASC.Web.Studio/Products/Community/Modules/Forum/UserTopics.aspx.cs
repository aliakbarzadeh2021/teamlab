using System;
using System.Collections.Generic;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Forum;
using ASC.Web.Controls;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    [AjaxNamespace("SearchResult")]
    public partial class UserTopics : MainPage
    {
        protected bool _isFind;
        private Guid _userID;

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.Search);

            if (!String.IsNullOrEmpty(Request["uid"]))
            {
                try
                {
                    _userID = new Guid(Request["uid"]);
                }
                catch
                {
                    _userID = Guid.Empty;
                }
            }

            var userInfo = CoreContext.UserManager.GetUsers(_userID);
          
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
            int topicCount = 0;

            if (!String.IsNullOrEmpty(Request["uid"]))
            {
                try
                {
                    _userID = new Guid(Request["uid"]);
                }
                catch
                {
                    _userID = Guid.Empty;
                }

                if (_userID != Guid.Empty)
                {
                   findTopicList =  ForumDataProvider.SearchTopicsByUser(TenantProvider.CurrentTenantID, _userID, currentPageNumber, ForumManager.Settings.TopicCountOnPage, out topicCount);
                }
            }

            #region navigators

            var pageNavigator = new PageNavigator()
            {   
                CurrentPageNumber = currentPageNumber,
                EntryCountOnPage = ForumManager.Settings.TopicCountOnPage,
                VisiblePageCount = 5,
                EntryCount = topicCount,
                PageUrl = "usertopics.aspx?uid=" + _userID.ToString()
            };
                

            bottomPageNavigatorHolder.Controls.Add(pageNavigator);

            #endregion

            int i = 0;
            foreach (Topic topic in findTopicList)
            {
                _isFind = true;
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

            //bread crumbs
            var breadCrumbs = (this.Master as ForumMasterPage).BreadCrumbs;
            breadCrumbs.Add(new BreadCrumb() { Caption = Resources.ForumResource.ForumsBreadCrumbs, NavigationUrl = "default.aspx" });
            breadCrumbs.Add(new BreadCrumb() { Caption = userInfo.DisplayUserName()});

            this.Title = HeaderStringHelper.GetPageTitle(Resources.ForumResource.AddonName, breadCrumbs);
        }
    }
}
