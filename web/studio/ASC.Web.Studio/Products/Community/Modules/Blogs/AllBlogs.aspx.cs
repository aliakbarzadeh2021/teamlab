using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Blogs.Core.Domain;
using ASC.Blogs.Core.Resources;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Users;
using ASC.Blogs.Core;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Community.Blogs
{
    [AjaxNamespace("AllBlogs")]
    public partial class AllBlogs : BasePage
    {
        private int sortedFiledID = 0;
        private SortDirection direction = SortDirection.Ascending;

        protected override void PageLoad()
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(AllBlogs), this.Page);

            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = BlogsResource.AddonName, NavigationUrl = VirtualPathUtility.ToAbsolute(Constants.BaseVirtualPath) });
            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = BlogsResource.AllBlogsMenuTitle });

            this.Title = HeaderStringHelper.GetPageTitle(ASC.Blogs.Core.Resources.BlogsResource.AddonName, mainContainer.BreadCrumbs);

            var engine = BasePage.GetEngine();

            var listBlogs = GetBlogsStatistic(engine, 0, true);
            RenderRatingTable(listBlogs);

			InitSidePanel(engine, TagCloud);
			sideRecentActivity.TenantId = TenantProvider.CurrentTenantID;
			sideRecentActivity.ProductId = Product.CommunityProduct.ID;
			sideRecentActivity.ModuleId = ASC.Blogs.Core.Constants.ModuleID;

            base.InitSubscribers(actions);

        }

       
        private List<BlogInfo> GetBlogsStatistic(BlogsEngine engine, int filedID, bool sortDirection)
        {

            CMPSort sort = new CMPSort(filedID, sortDirection ? SortDirection.Ascending : SortDirection.Descending);
            
            this.sortedFiledID = filedID;
            this.direction = sortDirection ? SortDirection.Ascending : SortDirection.Descending;

            var stat = engine.GetAuthorsStatistic();

            List<BlogInfo> listBlogs = new List<BlogInfo>(stat.Count);

            foreach (var blog in stat)
	        {
                listBlogs.Add(new BlogInfo()
                {
                    ID = blog.Value1,
                    BlogsCount = blog.Value2,
                    CommentsCount = blog.Value3,
                    ReviewCount = blog.Value4,

                    Name = DisplayUserSettings.GetFullUserName(blog.Value1)
                });
	        }

            listBlogs.Sort(sort);

            return listBlogs;
            
        }


        private void FillUserNames(ref List<Rating> ratings)
        {
            for (int i = 0; i < ratings.Count; i++)
            {
                ratings[i].Name = DisplayUserSettings.GetFullUserName(ratings[i].ID);
            }
        }

        private void RenderRatingTable(List<BlogInfo> ratings)
        {
            StringBuilder sb = new StringBuilder();
            mainContainer.Body = new PlaceHolder();

            if (ratings == null || ratings.Count == 0)
            {
                mainContainer.Body.Controls.Add(new BlogNotFoundControl(false));
                return;
            }

            sb = new StringBuilder();

            sb.Append("<div id=\"blg_rating_list\">");
            sb.Append(FillData(ratings));
            sb.Append("</div>");

            mainContainer.Body.Controls.Add(new LiteralControl(sb.ToString()));

        }

        private string FillData(List<BlogInfo> ratings)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;

            sb.Append("<table class=\"tableContainer\" cellspacing=\"0\" cellpadding=\"0\">");

            sb.Append("<tr>");

            string sortImg = "<img style='margin:0 0 -4px 2px;' src='" + WebImageSupplier.GetAbsoluteWebPath(this.direction.Equals(SortDirection.Descending) ? "sort_up.png" : "sort_down.png") + "'/>";
            string blankImg = "<img style='margin:0 0 -4px 2px;' width='12' src='" + WebImageSupplier.GetAbsoluteWebPath("blank.gif") + "' >";


            sb.AppendFormat("<th class=\"columnFirst\" style=\"width: 40%;\"><a class=\"header\" href=\"javascript:BlogsManager.BlogTblSort(0);\">{0}</a>{1}</th>", ASC.Blogs.Core.Resources.BlogsResource.AddonName, (sortedFiledID == 0 ? sortImg : blankImg));
            sb.AppendFormat("<th style=\"width: 20%;text-align:center;padding:0 0 16px;\"><a class=\"header\" href=\"javascript:BlogsManager.BlogTblSort(1);\">{0}</a>{1}</th>", ASC.Blogs.Core.Resources.BlogsResource.PostsTitle, (sortedFiledID == 1 ? sortImg : blankImg));
            sb.AppendFormat("<th style=\"width: 20%;text-align:center;padding:0 0 16px;\"><a class=\"header\" href=\"javascript:BlogsManager.BlogTblSort(2);\">{0}</a>{1}</th>", ASC.Blogs.Core.Resources.BlogsResource.ViewsTitle, (sortedFiledID == 2 ? sortImg : blankImg));
            sb.AppendFormat("<th style=\"width: 20%;text-align:center;padding:0 0 16px;\"><a class=\"header\" href=\"javascript:BlogsManager.BlogTblSort(3);\">{0}</a>{1}</th></tr>", ASC.Blogs.Core.Resources.BlogsResource.CommentsTitle, (sortedFiledID == 3 ? sortImg : blankImg));


            foreach (BlogInfo info in ratings)
            {
                sb.Append("<tr " + (i % 2 == 0 ? " class=\"even\" " : " class=\"odd\" ") + " >");
                sb.Append("<td class=\"columnFirst\" style=\"width: 40%;\" ><div class=\"clearFix\"><div style=\"float: left;\">" + ImageHTMLHelper.GetHTMLUserAvatar(info.ID) + "</div>");
                sb.Append("<div style=\"padding-left:45px;padding-top: 4px;\"><a class=\"linkHeaderLight\" href=\"./?userid=" + info.ID + "\">" + DisplayUserSettings.GetFullUserName(info.ID) + "</a>");

                sb.Append("<td class=\"headerBase\" style=\"width: 20%;\">" + info.BlogsCount + "</td><td class=\"headerBase\" style=\"width: 20%;\">" + info.ReviewCount + "</td><td class=\"headerBase\" style=\"width: 20%;\">" + info.CommentsCount + "</td></tr>");
                i++;
            }
            if (ratings.Count % 2 == 1)
                sb.Append("<tr><td colspan='4' style='font-size:1px;padding:0px;'>&nbsp;</td></tr>");

            sb.Append("</table>");

            return sb.ToString();
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string Sort(int filedID, bool sortDirection)
        {

            var engine = BasePage.GetEngine();

            var listBlogs = GetBlogsStatistic(engine, filedID, sortDirection);

            return FillData(listBlogs);
        }

       
    }

    public class CMPSort : IComparer<BlogInfo>
    {
        private int sortExpression;
        private SortDirection sortDirection;

        public CMPSort(int sortExpression, SortDirection sortDirection)
        {
            this.sortExpression = sortExpression;
            this.sortDirection = sortDirection;
        }
        public int Compare(BlogInfo r1, BlogInfo r2)
        {
            switch (sortExpression)
            {
                case 0:
                    if (sortDirection == SortDirection.Ascending)
                        return r1.Name.CompareTo(r2.Name);
                    else
                        return -r1.Name.CompareTo(r2.Name);

                case 1:
                    if (sortDirection == SortDirection.Ascending)
                        return r1.BlogsCount.CompareTo(r2.BlogsCount);
                    else
                        return -r1.BlogsCount.CompareTo(r2.BlogsCount);

                case 2:
                    if (sortDirection == SortDirection.Ascending)
                        return r1.ReviewCount.CompareTo(r2.ReviewCount);
                    else
                        return -r1.ReviewCount.CompareTo(r2.ReviewCount);

                case 3:
                    if (sortDirection == SortDirection.Ascending)
                        return r1.CommentsCount.CompareTo(r2.CommentsCount);
                    else
                        return -r1.CommentsCount.CompareTo(r2.CommentsCount);
            }

            return 0;
        }

    }
}
