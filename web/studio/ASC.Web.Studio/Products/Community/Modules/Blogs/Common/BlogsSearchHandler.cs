using System;
using System.Collections.Generic;
using System.Web;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;
using ASC.Blogs.Core.Resources;
using ASC.Web.Community.Product;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.Users;

namespace ASC.Web.Community.Blogs
{
    public class BlogsSearchHandler : BaseSearchHandlerEx
    {
        
        public override SearchResultItem[] Search(string text)
        {

            var posts = BasePage.GetEngine().SearchPosts(text, new ASC.Blogs.Core.PagingQuery());
            var result = new List<SearchResultItem>(posts.Count);
            foreach (Post post in posts)
            {
                SearchResultItem resultItem = new SearchResultItem();
                resultItem.Description = BlogsResource.Blogs + ", " + DisplayUserSettings.GetFullUserName(post.UserID) + ", " + post.Datetime.ToLongDateString();
                resultItem.Name = post.Title;
                resultItem.URL = VirtualPathUtility.ToAbsolute(Constants.BaseVirtualPath) + "viewblog.aspx?blogid=" + post.ID.ToString();

                result.Add(resultItem);
            }

            return result.ToArray();
        }

        
        public override string AbsoluteSearchURL
        {
            get { return VirtualPathUtility.ToAbsolute(Constants.BaseVirtualPath + "/"); }
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions() { ImageFileName = "blog_add.png", PartID = Constants.ModuleID }; }
        }

        public override string PlaceVirtualPath
        {
            get { return Constants.BaseVirtualPath; }
        }

        public override string SearchName
        {
            get { return BlogsResource.SearchDefaultString; }
        }

        public override Guid ProductID
        {
            get { return CommunityProduct.ID; }
        }

        public override Guid ModuleID
        {
            get
            {
                return Constants.ModuleID;
            }
        }
    }
}
