using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Pojo;
using ASC.Web.Controls;
using ASC.Web.UserControls.Bookmarking.Common;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using ASC.Web.UserControls.Bookmarking.Resources;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.UserControls.Bookmarking
{
	public partial class TagsUserControl : System.Web.UI.UserControl
	{
		BookmarkingServiceHelper ServiceHelper = BookmarkingServiceHelper.GetCurrentInstanse();

		private ViewSwitcher SortControl;

		private BookmarkingSortUtil sortUtil = new BookmarkingSortUtil();

		protected void Page_Load(object sender, EventArgs e)
		{
			SortControl = new ViewSwitcher();
			SortControl.SortItemsHeader = BookmarkingUCResource.ShowLabel;
			InitSortControl();
			TagsSortPanel.Controls.Add(SortControl);
			InitTags();
		}

		#region Most Popular Tags
		private void InitTags()
		{
			ServiceHelper.SetPagination();

			IList<Tag> tags = null;
			switch (sortUtil.SortBy)
			{
				case SortByEnum.Name:
					tags = ServiceHelper.GetTagsSortedByName();
					break;
				case SortByEnum.Popularity:
					tags = ServiceHelper.GetMostPopularTags();
					break;
				default:
					tags = ServiceHelper.GetMostRecentTags();
					break;
			}

			if (tags == null || tags.Count == 0)
			{
				TagsContainer.Controls.Add(new NotFoundControl());
				TagsSortingPanel.Visible = false;
				return;
			}
			int i = 0;

			var bookmarks = ServiceHelper.GetMostPopularBookmarksByTag(tags);

			foreach (var tag in tags)
			{
				using (var c = LoadControl(BookmarkUserControlPath.TagInfoUserControlPath) as TagInfoUserControl)
				{
					c.Tag = tag;
					c.BookmarkList = GetBookmarksByTag(bookmarks, tag);
					c.IsOdd = i++ % 2 == 0;
					TagsContainer.Controls.Add(c);
				}
			}
			SetBookmarkingPagination();
		}

		private static IList<Bookmark> GetBookmarksByTag(IList<Bookmark> bookmarks, Tag tag)
		{

			var r = (from b in bookmarks
					 where b.Tags.Contains(tag)
					 orderby b.UserBookmarks.Count descending
					 select b)
					 .Take(BookmarkingBusinessConstants.MostPopularBookmarksByTagLimit)
					 .ToList<Bookmark>();
			return r;
		}
		#endregion


		#region Init Sort Control
		private void InitSortControl()
		{
			var sortBy = Request.QueryString[BookmarkingRequestConstants.SortByParam];
			SortControl.SortItems = sortUtil.GetTagsSortItems(sortBy);
		}
		#endregion

		#region Pagination
		private void SetBookmarkingPagination()
		{
			PageNavigator pageNavigator = new PageNavigator();
			ServiceHelper.InitPageNavigator(pageNavigator, ServiceHelper.GetTagsCount());
			BookmarkingPaginationContainer.Controls.Add(pageNavigator);
		}
		#endregion
	}
}