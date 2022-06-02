using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Blogs.Core;
using ASC.Blogs.Core.Domain;


using ASC.Core;
using ASC.Core.Users;
using ASC.Notify.Recipients;
using ASC.Web.Controls;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Community.Blogs
{
    [AjaxNamespace("Default")]
    public partial class Default : BasePage
    {
        #region Properties

        private int SelectedPage
        {
            get
            {
                int result = 1;
                Int32.TryParse(Request.QueryString["page"], out result);
                if (result <= 0)
                    result = 1;
                return result;

            }
        }
        private int BlogsPerPage
        {
            get { return 5; }
        }

        public string GroupID
        {
            get
            {
                return Request.QueryString["groupID"];
            }
        }
        public string UserID
        {
            get
            {
                return Request.QueryString["userID"];
            }
        }
        public string TagName
        {
            get
            {
                return Request.QueryString["tagName"];
            }
        }
        public string Search
        {
            get
            {
                return Request.QueryString["search"];
            }
        }
        
        #endregion

        #region Methods

        protected override void PageLoad()
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(Default), this.Page);


            Guid? userId = null;
            if (!String.IsNullOrEmpty(UserID))
            {
                userId = Guid.NewGuid();
                try
                {
                    userId = new Guid(UserID);
                }
                catch { }
            }

            var postsQuery = new PostsQuery();
            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = ASC.Blogs.Core.Resources.BlogsResource.AddonName, NavigationUrl = VirtualPathUtility.ToAbsolute(ASC.Blogs.Core.Constants.BaseVirtualPath) });
            if (userId.HasValue)
            {
                mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = DisplayUserSettings.GetFullUserName(userId.Value).HtmlEncode() });
                postsQuery.SetUser(userId.Value);
            }
            else if (!String.IsNullOrEmpty(TagName))
            {
                mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = HeaderStringHelper.GetHTMLSearchByTagHeader(HttpUtility.HtmlEncode(TagName)) });
                postsQuery.SetTag(TagName);
            }
            else if (!String.IsNullOrEmpty(Search))
            {
                mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = HeaderStringHelper.GetHTMLSearchHeader(HttpUtility.HtmlEncode(Search)) });
                postsQuery.SetSearch(Search);
            }
            
            if (!IsPostBack)
            {
                var engine = BasePage.GetEngine();
                FillPosts(postsQuery, engine);

                InitSidePanel(engine,  TagCloud);
				sideRecentActivity.TenantId = TenantProvider.CurrentTenantID;
				sideRecentActivity.ProductId = Product.CommunityProduct.ID;
				sideRecentActivity.ModuleId = ASC.Blogs.Core.Constants.ModuleID;

                base.InitSubscribers(actions);
            }

            this.Title = HeaderStringHelper.GetPageTitle(ASC.Blogs.Core.Resources.BlogsResource.AddonName, mainContainer.BreadCrumbs);
        }




       protected string QueryString(string excludeParamList)
        {
            string queryString = "&" + Request.QueryString.ToString();

            foreach (string excludeParamName in excludeParamList.Split(','))
            {
                int startPos = queryString.IndexOf("&" + excludeParamName + "=");
                int endPos;
                if (startPos != -1)
                {
                    endPos = queryString.IndexOf("&", startPos + 1);

                    if (endPos == -1)
                    {
                        queryString = queryString.Remove(startPos, queryString.Length - startPos);
                    }
                    else
                    {
                        queryString = queryString.Remove(startPos, endPos - startPos);
                    }
                }
            }
            return queryString.Trim('&');
        }


       void FillPosts(PostsQuery query, BlogsEngine engine)
        {
            query
                .SetOffset((SelectedPage - 1) * BlogsPerPage)
                .SetCount(BlogsPerPage);

            SetTotalPostsCount(engine.GetPostsCount(query));
            var posts = engine.SelectPosts(query);
            FillSelectedPage(posts, engine);
        }

        private void FillSelectedPage(List<Post> posts, BlogsEngine engine)
        {
            StringBuilder sb = new StringBuilder();
            placeContent.Controls.Add(new Literal() { Text = "<div>" });

            var post_with_comments = engine.GetPostsCommentsCount(posts);

            for (int i = 0; i < post_with_comments.Count; i++)
            {

                Post post = post_with_comments[i].Value1;
                int commentCount = post_with_comments[i].Value2;

                sb = new StringBuilder();

                sb.Append("<div class=\"" + (i % 2 == 1 ? "tintLight" : "tintMedium") + " borderBase\" style=\"border-width: " + (i == 0 ? "1px" : "0") + " 0 1px 0;padding: 10px 14px;\">");

                sb.Append("<table cellspacing='0' cellpadding='0' border='0'><tr><td valign='top'>");
                sb.Append("<div style='padding-top:4px;'>" + ImageHTMLHelper.GetLinkUserAvatar(post.UserID) + "</div>");
				sb.Append("</td><td><div style='margin-left:15px; width:590px;' class='longWordsBreak'>");

                sb.Append("<a href=\"viewblog.aspx?blogid=" + post.ID.ToString() + "\" class=\"linkHeaderLight\">" + HttpUtility.HtmlEncode(post.Title) + "</a>");

                sb.Append("<div style='padding-bottom: 2px;padding-top: 15px;'>");

                sb.Append("<a class='linkHeaderSmall' href='" + VirtualPathUtility.ToAbsolute(ASC.Blogs.Core.Constants.BaseVirtualPath) + "?userid=" + post.UserID + "'><span style='font-weight:normal;'>" + ASC.Blogs.Core.Resources.BlogsResource.BlogOfTitle + ":&nbsp;&nbsp;</span>" + DisplayUserSettings.GetFullUserName(post.UserID).HtmlEncode() + "</a>");

                sb.Append("</div>");
                sb.Append("<div >");
                sb.Append("<span class='textMediumDescribe' style='margin-right:5px;'>" + ASC.Blogs.Core.Resources.BlogsResource.PostedTitle + ":</span> " + CoreContext.UserManager.GetUsers(post.UserID).RenderProfileLink(ASC.Web.Community.Product.CommunityProduct.ID));
                sb.Append("<span class='textMediumDescribe'>&nbsp;&nbsp;" + post.Datetime.Ago() + "</span>");
                sb.Append("</div>");

                sb.Append("</div>");
                placeContent.Controls.Add(new Literal() { Text = sb.ToString() });


                
                sb = new StringBuilder();

				sb.Append("<div style='margin-top: 22px; margin-left:15px; width:590px; overflow-y:hidden;' class='longWordsBreak'>");

                sb.Append(HtmlUtility.GetPreview(post.Content, "<div style='margin-top:15px;'><a style='text-decoration:none;' href=\"viewblog.aspx?blogid=" + post.ID.ToString() + "\"><font style='text-decoration:underline;'>" + ASC.Blogs.Core.Resources.BlogsResource.ReadMoreLink + "</font><font style='font-size:14px;text-decoration:none;'>&nbsp;&#8594</font></a></div>", ASC.Web.Community.Product.CommunityProduct.ID));

                sb.Append("</div>");


                sb.Append("<div class='clearFix' style='margin-top: 17px;  margin-left:15px;'>");
                if (post.TagList.Count > 0)
                {
                    sb.Append("<div style=\" float:left; width:70%;\" class=\"textMediumDescribe\">");
                    sb.Append("<img style=\"margin-bottom:-2px;margin-right:2px;\" src=\"" + WebImageSupplier.GetAbsoluteWebPath("tags.png", BlogsSettings.ModuleID) + "\">");

                    int j = 0;
                    foreach (Tag tag in post.TagList)
                    {
                        if (j != 0)
                            sb.Append(", ");
                        j++;
                        sb.Append("<a style='margin-left:5px;' class=\"linkDescribe\" href=\"./?tagname=" + HttpUtility.UrlEncode(tag.Content) + "\">" + HttpUtility.HtmlEncode(tag.Content) + "</a>");
                    }
                    
                    sb.Append("</div>");
                }

                sb.Append("<div style='float:right; width:29%; text-align:right;'>");
                sb.Append("<a href='viewblog.aspx?blogid=" + post.ID + "#comments'>" + ASC.Blogs.Core.Resources.BlogsResource.CommentsTitle + ": " + commentCount.ToString() + "</a>");
                sb.Append("</div>");

                sb.Append("</div></td></tr></table>");

                sb.Append("</div>");


                placeContent.Controls.Add(new Literal() { Text = sb.ToString() });

            }

            if (posts == null || posts.Count == 0)
            {
                placeContent.Controls.Add(new BlogNotFoundControl(!string.IsNullOrEmpty(UserID) || !string.IsNullOrEmpty(Search)));
            }

            placeContent.Controls.Add(new Literal() { Text = "</div>" });

        }

        #endregion


        void SetTotalPostsCount(int count)
        {
            ASC.Web.Controls.PageNavigator pageNavigator = new ASC.Web.Controls.PageNavigator()
            {
                PageUrl = "./" + "?" + QueryString("page"),
                CurrentPageNumber = SelectedPage,
                EntryCountOnPage = BlogsPerPage,
                VisiblePageCount = 5,
                ParamName = "page",
                EntryCount = count
            };

            pageNavigatorHolder.Controls.Add(pageNavigator);
        }

    }
}