using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Forum;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    public class ForumShortcutProvider : IShortcutProvider
    {
        public static string GetCreateContentPageUrl()
        {
            if (ValidateCreateTopicOrPoll(false))
                return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?m=0";

            return null;
        }

        private Hashtable _eventHandlerHash = Hashtable.Synchronized(new Hashtable());

        #region IShortcutProvider Members

        public string GetAbsoluteWebPathForShortcut(Guid shortcutID, string currentUrl)
        {
            if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34"))
                || shortcutID.Equals(new Guid("84DF7BE7-315B-4ba3-9BE1-1E348F6697A5")))
            {
                if (ForumManager.Instance.CurrentPage.Page == ForumPage.TopicList || ForumManager.Instance.CurrentPage.Page == ForumPage.NewPost)
                {
                    var threadID = GetThreadID();
                    if (threadID != -1)
                    {
                        if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")))
                            return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?f=" + threadID.ToString() + "&m=0";
                        else
                            return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?f=" + threadID.ToString() + "&m=1";
                    }

                    if (ForumManager.Instance.CurrentPage.Page == ForumPage.NewPost)
                    {
                        var topic = GetTopicByID();
                        if (topic != null)
                        {
                            if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")))
                                return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?f=" + topic.ThreadID.ToString() + "&m=0";
                            else
                                return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?f=" + topic.ThreadID.ToString() + "&m=1";
                        }
                    }
                }

                else if (ForumManager.Instance.CurrentPage.Page == ForumPage.PostList ||
                         ForumManager.Instance.CurrentPage.Page == ForumPage.EditTopic)
                {
                    Topic topic = GetTopicByID();
                    if (topic != null)
                    {
                        if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")))
                            return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?f=" + topic.ThreadID + "&m=0";
                        else
                            return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?f=" + topic.ThreadID + "&m=1";
                    }
                }

                if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")))
                    return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?m=0";
                else
                    return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?m=1";
            }

            else if (shortcutID.Equals(new Guid("FA5C4BD5-25E7-41c8-A0DC-64DC2A977391")))
            {
                if (ForumManager.Instance.CurrentPage.Page == ForumPage.PostList ||
                    ForumManager.Instance.CurrentPage.Page == ForumPage.NewPost ||
                    ForumManager.Instance.CurrentPage.Page == ForumPage.EditTopic)
                {
                    int topicID = GetTopicID();
                    if (topicID != -1)
                        return VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?t=" + topicID;
                }
            }

            return "";
        }

        public bool CheckPermissions(Guid shortcutID, string currentUrl)
        {
            if (shortcutID.Equals(new Guid("A04A7DBF-6B73-4579-BECE-3F6E346133DB")))
                return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessForumEditor, null);

            else if (shortcutID.Equals(new Guid("87A6B7FC-E872-49db-A327-CEA9CBA59CCC")))
                return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.GetAccessTagEditor, null);

            else if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34"))
                || shortcutID.Equals(new Guid("84DF7BE7-315B-4ba3-9BE1-1E348F6697A5")))
            {
                if (ForumManager.Instance.CurrentPage.Page == ForumPage.TopicList
                    || ForumManager.Instance.CurrentPage.Page == ForumPage.NewPost)
                {
                    var threadID = GetThreadID();
                    if (threadID != 0)
                    {
                        if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")))
                            return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.TopicCreate, new Thread() { ID = threadID });
                        else
                            return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.PollCreate, new Thread() { ID = threadID });
                    }

                    if (ForumManager.Instance.CurrentPage.Page == ForumPage.NewPost)
                    {

                        var topicID = GetTopicID();
                        if (topicID != 0)
                        {
                            if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")))
                                return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.TopicCreate, new Topic() { ID = topicID });
                            else
                                return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.PollCreate, new Topic() { ID = topicID });
                        }
                    }

                }


                else if (ForumManager.Instance.CurrentPage.Page == ForumPage.PostList ||
                         ForumManager.Instance.CurrentPage.Page == ForumPage.EditTopic)
                {
                    var topicID = GetTopicID();
                    if (topicID != 0)
                    {
                        if (shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")))
                            return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.TopicCreate, new Topic() { ID = topicID });
                        else
                            return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.PollCreate, new Topic() { ID = topicID });
                    }
                }

                return ValidateCreateTopicOrPoll(!shortcutID.Equals(new Guid("24CD48B2-C40F-43ec-B3A6-3212C51B8D34")));
            }

            else if (shortcutID.Equals(new Guid("FA5C4BD5-25E7-41c8-A0DC-64DC2A977391")))
            {
                if (ForumManager.Instance.CurrentPage.Page == ForumPage.PostList ||
                    ForumManager.Instance.CurrentPage.Page == ForumPage.NewPost ||
                    ForumManager.Instance.CurrentPage.Page == ForumPage.EditTopic)
                {
                    var topicID = GetTopicID();
                    if (topicID != 0)
                        return ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.PostCreate, new Topic() { ID = topicID });
                }
            }

            return false;
        }

        #endregion

        private Topic GetTopicByID()
        {
            int topicID = GetTopicID();
            return ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, topicID);
        }

        private int GetThreadID()
        {
            string currentUrl = "";
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
                currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;

            Regex regExp = new Regex(@"[\?&]f=(?<threadID>[0-9]*)&?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = regExp.Match(currentUrl);

            if (m.Success)
            {
                int res = -1;
                if (int.TryParse(m.Groups["threadID"].Value, out res))
                    return res;
            }

            return -1;

        }

        private int GetTopicID()
        {
            string currentUrl = "";
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
                currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;

            Regex regExp = new Regex(@"[\?&]t=(?<topicID>[0-9]*)&?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = regExp.Match(currentUrl);
            if (m.Success)
            {
                int res = -1;
                if (int.TryParse(m.Groups["topicID"].Value, out res))
                    return res;
            }

            return -1;
        }

        private static bool ValidateCreateTopicOrPoll(bool isPool)
        {
            return ForumManager.Instance.ValidateAccessSecurityAction((isPool ? ForumAction.PollCreate : ForumAction.TopicCreate), null);

        }



    }
}
