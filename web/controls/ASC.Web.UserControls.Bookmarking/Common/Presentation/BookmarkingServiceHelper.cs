using System;
using System.Collections.Generic;
using System.Net;
using ASC.Bookmarking.Business;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Pojo;
using ASC.Core;
using ASC.Core.Users;
using System.Web;
using System.Text;
using System.Collections;
using ASC.Bookmarking.Dao;
using System.Text.RegularExpressions;
using ASC.Web.Controls;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking.Util;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using ASC.Web.Core.Users;
using ASC.Notify.Model;
using ASC.Web.Core.ModuleManagement.Common;


namespace ASC.Web.UserControls.Bookmarking.Common.Presentation
{
	/// <summary>
	/// This class is used for the interaction between presentation and business layers.
	/// </summary>
	public class BookmarkingServiceHelper : BookmarkingSessionObject<BookmarkingServiceHelper>
	{
		private IBookmarkingService service = BookmarkingService.GetCurrentInstanse();
		private BookmarkingHibernateDao dao = BookmarkingHibernateDao.GetCurrentInstanse();
		private ViewSwitcher SortControl;

		public CommentsList Comments { get; set; }

		public BookmarkDisplayMode DisplayMode { get; set; }

		public int SelectedTab
		{
			get
			{
				try
				{
					var tab = CurrentRequest.QueryString[BookmarkingRequestConstants.SelectedTab];
					switch (tab)
					{
						case BookmarkingRequestConstants.SelectedTabBookmarkAddedBy:
							return 1;
						default:
							return 0;
					}
				}
				catch {
					return 0;
				}
			}
		}

		private HttpRequest CurrentRequest
		{
			get
			{
				return HttpContext.Current.Request;
			}
		}

		public Bookmark BookmarkToAdd { get; private set; }

		private int _maxResults = BookmarkingSettings.BookmarksCountOnPage;

		public int MaxResults
		{
			get
			{
				return _maxResults;
			}
			set
			{
				_maxResults = value;
			}
		}

		public int FirstResult { get; set; }

		public int CurrentPageNumber { get; set; }

		public long ItemsCount
		{
			get
			{
				return dao.ItemsCount;
			}
		}

		#region Subscription Fields

		public string SubscriptionRecentBookmarkID
		{
			get
			{
				return BookmarkingBusinessConstants.SubscriptionRecentBookmarkID;
			}
		}

		public string SubscriptionBookmarkCommentsID { get; set; }


		#endregion

		#region Init
		public void InitServiceHelper(ViewSwitcher SortControl)
		{
			this.SortControl = SortControl;
		}
		#endregion

		#region Add Bookmark
		public Bookmark AddBookmark(string bookmarkUrl, string bookmarkName, string bookmarkDescription, string bookmarkTags)
		{
			bookmarkUrl = GetLimitedTextForDescription(HttpUtility.HtmlEncode(bookmarkUrl));
			bookmarkName = GetLimitedTextForName(EncodeUserData(bookmarkName, false));
			bookmarkDescription = GetLimitedTextForDescription(EncodeUserData(bookmarkDescription, false));
			
			BookmarkToAdd = GetBookmarkByUrl(bookmarkUrl);


			IList<Tag> tags = ConvertStringToTags(bookmarkTags);
			var bookmark = UserBookmarkInit(bookmarkUrl, bookmarkName, bookmarkDescription, tags);
			if (bookmark != null)
			{
				if (BookmarkToAdd != null && BookmarkToAdd.UserCreatorID != null && BookmarkToAdd.UserCreatorID.Equals(bookmark.UserID))
				{
					var b = BookmarkInit(BookmarkToAdd, bookmarkName, bookmarkDescription, tags);
					BookmarkToAdd = service.UpdateBookmark(b, tags);
					return BookmarkToAdd;
				}

				BookmarkToAdd = service.UpdateBookmark(bookmark, tags);
				return BookmarkToAdd;
			}

			var newBookmark = BookmarkInit(bookmarkUrl, bookmarkName, bookmarkDescription, tags);
			BookmarkToAdd = service.AddBookmark(newBookmark, tags);
			return BookmarkToAdd;
		}

		private static string GetLimitedText(string text, int maxLength)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			return text.Length > maxLength ? text.Substring(0, maxLength) : text;
		}

		private static string GetLimitedTextForName(string text)
		{
			var maxLenght = BookmarkingSettings.NameMaxLength;
			return GetLimitedText(text, maxLenght);
		}

		private static string GetLimitedTextForDescription(string text)
		{
			var maxLenght = BookmarkingSettings.DescriptionMaxLength;
			return GetLimitedText(text, maxLenght);
		}

		public static string EncodeUserData(string text, bool removeSlashes)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			text = text.Replace("\r", " ");			
			text = text.Replace("\t", " ");
			text = text.Trim();

			if (removeSlashes)
			{
				text = text.Replace("\\", string.Empty);
				text = text.Replace("/", string.Empty);
			}
			text = HttpUtility.HtmlDecode(text);
			text = HttpUtility.HtmlDecode(text);			
			text = text.Replace("\"", "'");
			text = text.ReplaceSingleQuote();
			
			text = HttpUtility.HtmlEncode(text);

			text = text.Replace("\n", "<br/>");
			return text;
		}

		public static string EncodeUserData(string text)
		{
			return EncodeUserData(text, true);
		}

		private UserBookmark UserBookmarkInit(string bookmarkUrl, string bookmarkName, string bookmarkDescription, IList<Tag> tags)
		{
			if (BookmarkToAdd == null || !BookmarkToAdd.URL.Equals(bookmarkUrl))
			{
				return null;
			}

			var userBookmark = GetCurrentUserBookmark(BookmarkToAdd);
			if (userBookmark == null)
			{
				userBookmark = new UserBookmark();
				userBookmark.BookmarkID = BookmarkToAdd.ID;
				userBookmark.UserID = GetCurrentUserID();
				userBookmark.Raiting = 1;
			}

			userBookmark.DateAdded = ASC.Core.Tenants.TenantUtil.DateTimeNow();
			userBookmark.Name = bookmarkName;
			userBookmark.Description = bookmarkDescription;

			return userBookmark;
		}

		private Bookmark BookmarkInit(string bookmarkUrl, string bookmarkName, string bookmarkDescription, IList<Tag> tags)
		{
			var currentUserID = GetCurrentUserID();

			var date = ASC.Core.Tenants.TenantUtil.DateTimeNow();

			Bookmark bookmark = new Bookmark(bookmarkUrl, date, bookmarkName, bookmarkDescription);

			UserBookmark userBookmark = new UserBookmark();
			userBookmark.BookmarkID = bookmark.ID;
			userBookmark.Name = bookmark.Name;
			userBookmark.Description = bookmark.Description;
			userBookmark.UserID = currentUserID;
			userBookmark.Raiting = 1;
			userBookmark.DateAdded = date;
			bookmark.UserCreatorID = currentUserID;

			return bookmark;
		}

		private Bookmark BookmarkInit(Bookmark b, string bookmarkName, string bookmarkDescription, IList<Tag> tags)
		{
			b.Name = bookmarkName;
			b.Description = bookmarkDescription;
			b.Tags = tags;

			return b;
		}

		public UserBookmark GetCurrentUserBookmark(Bookmark b)
		{
			return service.GetCurrentUserBookmark(b);
		}

		public UserBookmark GetCurrentUserBookmark(IList<UserBookmark> userBookmarks)
		{
			if (userBookmarks == null || userBookmarks.Count == 0)
			{
				return null;
			}
			var currentUserID = GetCurrentUserID();
			foreach (var userBookmark in userBookmarks)
			{
				if (currentUserID.Equals(userBookmark.UserID))
				{
					return userBookmark;
				}
			}
			return null;
		}

		public Bookmark GetBookmarkWithUserBookmarks(string url)
		{
			return GetBookmarkWithUserBookmarks(url, true);
		}

		public Bookmark GetBookmarkWithUserBookmarks(string url, bool decodeUrlFlag)
		{
			if (string.IsNullOrEmpty(url))
			{
				return null;
			}
			if (decodeUrlFlag)
			{
				url = HttpUtility.HtmlDecode(url);
				url = HttpUtility.HtmlEncode(url);
			}
			BookmarkToAdd = service.GetBookmarkWithUserBookmarks(url);
			if (BookmarkToAdd != null)
			{
				SubscriptionBookmarkCommentsID = BookmarkToAdd.ID.ToString();
			}
			return BookmarkToAdd;
		}

		public Bookmark GetBookmarkWithUserBookmarks()
		{
			var url = GetBookmarkInfoUrl();
			return GetBookmarkWithUserBookmarks(url);
		}

		public Guid GetCurrentUserID()
		{
			return service.GetCurrentUser().ID;
		}
		#endregion

		#region Get Bookmarks

		public IList<Bookmark> GetBookmarks()
		{
			var bookmarks = GetSortedBookmarks();
			bookmarks = GetFullBookmarksInfo(bookmarks);
			return bookmarks;
		}

		private IList<Bookmark> GetSortedBookmarks()
		{
			SetPagination();
			switch (DisplayMode)
			{
				case BookmarkDisplayMode.Favourites:
					var result = GetFavouriteBookmarksByRequest();
					return result;
				case BookmarkDisplayMode.SelectedBookmark:
					var bookmarksList = new List<Bookmark>();
					var url = GetBookmarkInfoUrl();
					if (url != null)
					{
						bookmarksList.Add(GetBookmarkByUrl(url));
						return bookmarksList;
					}
					if (BookmarkToAdd != null)
					{
						bookmarksList.Add(BookmarkToAdd);
						return bookmarksList;
					}
					break;
				case BookmarkDisplayMode.SearchBookmarks:
					return SearchBookmarks();
				case BookmarkDisplayMode.SearchByTag:
					return SearchBookmarksByTag();
				case BookmarkDisplayMode.BookmarksCreatedByUser:
					return GetBookmarksCreatedByUser();
			}
			var bookmarks = GetBookmarksByRequest();
			return bookmarks;
		}

		public IList<Bookmark> GetFullBookmarksInfo(IList<Bookmark> bookmarks)
		{
			return service.GetFullBookmarksInfo(bookmarks);
		}

		private IList<Bookmark> GetBookmarksCreatedByUser()
		{
			var uid = GetUserIDFromRequest();
			service.GetBookmarksCountCreatedByUser(uid);

			var sortUtil = new BookmarkingSortUtil();

			try
			{
				var sortBy = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
				SortControl.SortItems = sortUtil.GetBookmarksCreatedByUserSortItems(sortBy);
			}
			catch
			{
				sortUtil.SortBy = SortByEnum.MostRecent;
			}

			switch (sortUtil.SortBy)
			{
				case SortByEnum.Popularity:
					return service.GetMostPopularBookmarksCreatedByUser(uid, FirstResult, MaxResults);
				default:
					return service.GetBookmarksCreatedByUser(uid, FirstResult, MaxResults);
			}



		}

		private long GetBookmarkIDFromDbByUrl(string url)
		{
			url = HttpUtility.UrlDecode(url);
			var b = GetBookmarkByUrl(url);
			return b == null ? 0 : b.ID;
		}

		private IList<Bookmark> GetAllBookmarks()
		{
			return service.GetAllBookmarks(FirstResult, MaxResults);
		}

		private string GetBookmarkInfoUrl()
		{
			var url = CurrentRequest.QueryString[BookmarkingRequestConstants.UrlGetRequest];
			if (String.IsNullOrEmpty(url))
			{
				return null;
			}
			url = UpdateBookmarkInfoUrl(url);
			return HttpUtility.UrlDecode(url);
		}

		public static string UpdateBookmarkInfoUrl(string url)
		{
			url = UpdateBookmarkInfoUrl(url, "http:/");
			url = UpdateBookmarkInfoUrl(url, "https:/");			
			return url;
		}

		private static string UpdateBookmarkInfoUrl(string url, string urlPrefix)
		{
			if (!url.StartsWith(urlPrefix + "/") && url.StartsWith(urlPrefix))
			{
				url = url.Insert(urlPrefix.Length, "/");
			}
			return url;
		}

		private IList<Bookmark> GetFavouriteBookmarksByRequest()
		{
			var sortByParam = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
			var sortBy = SortByEnum.MostRecent;
			if (string.IsNullOrEmpty(sortByParam))
			{
				InitSortUtil(sortBy);
				return service.GetFavouriteBookmarksSortedByDate(FirstResult, MaxResults);
			}
			switch (sortByParam)
			{
				case BookmarkingRequestConstants.PopularityParam:
					sortBy = SortByEnum.Popularity;
					InitSortUtil(sortBy);
					return service.GetFavouriteBookmarksSortedByRaiting(FirstResult, MaxResults);
				default:
					sortBy = SortByEnum.MostRecent;
					InitSortUtil(sortBy);
					return service.GetFavouriteBookmarksSortedByDate(FirstResult, MaxResults);
			}
		}

		private IList<Bookmark> GetBookmarksByRequest()
		{
			#region Sanity Request check
			if (CurrentRequest == null)
			{
				return GetAllBookmarks();
			}

			var queryString = CurrentRequest.QueryString;
			if (queryString == null || queryString.Count == 0)
			{
				return GetBookmarksBySortEnum(null);
			}
			#endregion

			var sortByParam = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
			return GetBookmarksBySortEnum(sortByParam);
		}

		public IList<Bookmark> GetMostRecentBookmarks(int bookmarksCount)
		{
			return service.GetMostRecentBookmarks(0, bookmarksCount);
		}

		public IList<Bookmark> GetMostRecentBookmarksForWidget(int bookmarksCount)
		{
			return service.GetMostRecentBookmarksWithRaiting(0, bookmarksCount);
		}

		private IList<Bookmark> GetBookmarksBySortEnum(string sortByParam)
		{
			var sortBy = SortByEnum.MostRecent;
			if (string.IsNullOrEmpty(sortByParam))
			{
				InitSortUtil(sortBy);
				return service.GetMostRecentBookmarks(FirstResult, MaxResults);
			}

			switch (sortByParam)
			{
				case BookmarkingRequestConstants.TopOfTheDayParam:
					sortBy = SortByEnum.TopOfTheDay;
					InitSortUtil(sortBy);
					return service.GetTopOfTheDay(FirstResult, MaxResults);
				case BookmarkingRequestConstants.WeekParam:
					sortBy = SortByEnum.Week;
					InitSortUtil(sortBy);
					return service.GetTopOfTheWeek(FirstResult, MaxResults);
				case BookmarkingRequestConstants.MonthParam:
					sortBy = SortByEnum.Month;
					InitSortUtil(sortBy);
					return service.GetTopOfTheMonth(FirstResult, MaxResults);
				case BookmarkingRequestConstants.YearParam:
					sortBy = SortByEnum.Year;
					InitSortUtil(sortBy);
					return service.GetTopOfTheYear(FirstResult, MaxResults);
				default:
					sortBy = SortByEnum.MostRecent;
					InitSortUtil(sortBy);
					return service.GetMostRecentBookmarks(FirstResult, MaxResults);
			}
		}

		private void InitSortUtil(SortByEnum sortBy)
		{
			var sortUtil = new BookmarkingSortUtil();
			switch (DisplayMode)
			{
				case BookmarkDisplayMode.Favourites:
					SortControl.SortItems = sortUtil.GetFavouriteBookmarksSortItems(sortBy);
					break;
				default:
					SortControl.SortItems = sortUtil.GetMainPageSortItems(sortBy);
					break;
			}
		}

		public string GetSearchText()
		{
			string searchString = CurrentRequest.QueryString[BookmarkingRequestConstants.Search];
			if (!String.IsNullOrEmpty(searchString))
			{
				return searchString;
			}
			return null;
		}

		public string GetSearchTag()
		{
			string searchString = CurrentRequest.QueryString[BookmarkingRequestConstants.Tag];
			if (!String.IsNullOrEmpty(searchString))
			{
				return searchString;
			}
			return null;
		}

		public string GetCurrentURL()
		{
			return CurrentRequest.Url.ToString();
		}

		private string GetCurrentURLForPagination()
		{
			string currentUrl = CurrentRequest.Url.ToString();
			StringBuilder sb = new StringBuilder();
			sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
			sb.Append(BookmarkingRequestConstants.Pagination);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append("[\\d]*");

			var a = Regex.Split(currentUrl, sb.ToString());

			sb = new StringBuilder();
			foreach (var item in a)
			{
				sb.Append(item);
			}

			currentUrl = sb.ToString();
			if (currentUrl.IndexOf(BookmarkingRequestConstants.Question_Sybmol) == -1)
			{
				currentUrl += BookmarkingRequestConstants.Question_Sybmol;
			}

			return currentUrl;
		}

		private IList<Tag> ConvertStringToTags(string tagsString)
		{
			return ConvertStringToTags(tagsString, ',');
		}

		private static IList<string> ConvertStringToArray(string searchString)
		{
			char separator = ' ';
			searchString = RemoveExtraSpaces(searchString);
			var a = searchString.Split(separator);
			var tagsList = new List<string>();
			foreach (var s in a)
			{
				if (String.IsNullOrEmpty(s))
				{
					continue;
				}
				tagsList.Add(s.Trim());
			}
			return tagsList;
		}

		private IList<Tag> ConvertStringToTags(string searchString, char separator)
		{
			searchString = RemoveExtraSpaces(searchString);
			var a = searchString.Split(separator);
			var tagsList = new List<Tag>();
			foreach (var s in a)
			{
				if (String.IsNullOrEmpty(s))
				{
					continue;
				}
				var t = new Tag() { Name = GetLimitedTextForName(s.Trim()) };
				if (!tagsList.Contains(t))
				{
					var tag = CreateOrGetTag(t);
					tagsList.Add(tag);
				}
			}
			return tagsList;
		}

		private Tag CreateOrGetTag(Tag t)
		{
			var tags = service.GetAllTags();

			var tagName = t.Name.ToLower();

			foreach (var tag in tags)
			{
				if (tag.Name.ToLower().Equals(tagName))
				{
					return tag;
				}
			}
			return t;
		}

		public IList<Tag> GetBookmarkTags(Bookmark b)
		{
			return service.GetBookmarkTags(b);
		}

		public static string ConvertTagsToString(IList<Tag> tags)
		{
			StringBuilder tagsString = new StringBuilder();
			int i = 0;
			var count = tags.Count;
			foreach (var tag in tags)
			{
				tagsString.Append(tag.Name);
				i++;
				if (i < count)
				{
					tagsString.Append(',');
				}
			}
			return tagsString.ToString();
		}

		public static string ConvertBookmarkToTagsString(Bookmark b)
		{
			return ConvertTagsToString(b.Tags);
		}

		private static string RemoveExtraSpaces(string searchString)
		{
			return System.Text.RegularExpressions.Regex.Replace(searchString, @"\s+", " ");
		}

		#endregion

		#region Current User Info
		public static UserInfo GetUserInfo(Guid userID)
		{
			UserInfo userInfo = CoreContext.UserManager.GetUsers(userID);
			return userInfo;
		}

		public string GetUserNameByRequestParam()
		{
			var uid = GetUserIDFromRequest();
			return GetUserInfo(uid).DisplayUserName();
		}

		private Guid GetUserIDFromRequest()
		{
			var uid = CurrentRequest.QueryString[BookmarkingRequestConstants.UidParam];
			if (!string.IsNullOrEmpty(uid))
			{
				var userID = new Guid(uid);
				if (userID != null && !Guid.Empty.Equals(userID))
				{
					return userID;
				}
			}
			return Guid.Empty;
		}
		#endregion

		#region Sort Params

		private SortParam GetSortParam()
		{
			string sortBy = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
			if (sortBy == null || sortBy.Length == 0)
			{
				return SortParam.NoSorting;
			}

			if (BookmarkingRequestConstants.SortByDateParam.Equals(sortBy))
			{
				return SortParam.SortByDate;
			}
			if (BookmarkingRequestConstants.SortByRaitingParam.Equals(sortBy))
			{
				return SortParam.SortByRaiting;
			}
			return SortParam.NoSorting;
		}

		public static string GenerateBookmarkInfoUrl(string url)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(BookmarkingRequestConstants.BookmarkInfoPageName);
			sb.Append(BookmarkingRequestConstants.Question_Sybmol);

			sb.Append(BookmarkingRequestConstants.UrlGetRequest);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);

			sb.Append(HttpUtility.UrlEncode(url));

			return sb.ToString();
		}

		public string GenerateSortUrl(string sortBy)
		{
			StringBuilder sb = new StringBuilder();
			switch (DisplayMode)
			{
				case BookmarkDisplayMode.Favourites:
					return GenerateSortUrlWithPageName(BookmarkingRequestConstants.FavouriteBookmarksPageName, sortBy);
				default:
					return GenerateSortUrlWithPageName(BookmarkingRequestConstants.BookmarkingPageName, sortBy);
			}
		}

		public string GenerateSortUrlWithPageName(string pageName, string sortBy)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(pageName);

			sb.Append(BookmarkingRequestConstants.Question_Sybmol);

			sb.Append(BookmarkingRequestConstants.SortByParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);

			sb.Append(sortBy);
			return sb.ToString();
		}

		public string GetSearchByTagUrl(Tag tag)
		{
			if (tag != null && !string.IsNullOrEmpty(tag.Name))
			{
				return GetSearchByTagUrl(tag.Name);

			}
			return string.Empty;
		}

		public string GetSearchByTagUrl(string tagName)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(BookmarkingRequestConstants.BookmarkingPageName);
			if (string.IsNullOrEmpty(tagName))
			{
				return sb.ToString();
			}
			sb.Append(BookmarkingRequestConstants.Question_Sybmol);
			sb.Append(BookmarkingRequestConstants.Tag);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(HttpUtility.UrlEncode(tagName));
			return sb.ToString();
		}

		public string GetSearchMostRecentBookmarksByTagUrl()
		{
			var tagName = GetSearchTag();
			StringBuilder sb = new StringBuilder(GetSearchByTagUrl(tagName));
			sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
			sb.Append(BookmarkingRequestConstants.SortByParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(BookmarkingRequestConstants.SortByDateParam);
			return sb.ToString();
		}

		public string GetSearchMostPopularBookmarksByTagUrl()
		{
			var tagName = GetSearchTag();
			StringBuilder sb = new StringBuilder(GetSearchByTagUrl(tagName));
			sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
			sb.Append(BookmarkingRequestConstants.SortByParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(BookmarkingRequestConstants.PopularityParam);
			return sb.ToString();
		}

		private string GetSearchBookmarksUrl()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(BookmarkingRequestConstants.BookmarkingPageName);
			var searchText = GetSearchText();
			if (string.IsNullOrEmpty(searchText))
			{
				return sb.ToString();
			}
			sb.Append(BookmarkingRequestConstants.Question_Sybmol);
			sb.Append(BookmarkingRequestConstants.Search);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(searchText);
			return sb.ToString();
		}

		public string GetSearchMostRecentBookmarksUrl()
		{
			StringBuilder sb = new StringBuilder(GetSearchBookmarksUrl());
			sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
			sb.Append(BookmarkingRequestConstants.SortByParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(BookmarkingRequestConstants.MostRecentParam);
			return sb.ToString();
		}

		public string GetSearchMostPopularBookmarksUrl()
		{
			StringBuilder sb = new StringBuilder(GetSearchBookmarksUrl());
			sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
			sb.Append(BookmarkingRequestConstants.SortByParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(BookmarkingRequestConstants.PopularityParam);
			return sb.ToString();
		}

		private string GetBookmarksCreateByUserUrl()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(BookmarkingRequestConstants.BookmarksCreatedByUserPageName);
			sb.Append(BookmarkingRequestConstants.Question_Sybmol);
			sb.Append(BookmarkingRequestConstants.UidParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			try
			{
				var uid = GetUserIDFromRequest();

				sb.Append(uid);

			}
			catch
			{
			}
			return sb.ToString();
		}

		public string GetMostRecentBookmarksCreateByUserUrl()
		{
			StringBuilder sb = new StringBuilder(GetBookmarksCreateByUserUrl());
			sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
			sb.Append(BookmarkingRequestConstants.SortByParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(BookmarkingRequestConstants.MostRecentParam);
			return sb.ToString();
		}

		public string GetMostPopularBookmarksCreateByUserUrl()
		{
			StringBuilder sb = new StringBuilder(GetBookmarksCreateByUserUrl());
			sb.Append(BookmarkingRequestConstants.AMPERSAND_SYMBOL);
			sb.Append(BookmarkingRequestConstants.SortByParam);
			sb.Append(BookmarkingRequestConstants.EqualsSymbol);
			sb.Append(BookmarkingRequestConstants.PopularityParam);
			return sb.ToString();
		}

		public static string GetCreateBookmarkPageUrl()
		{
			return BookmarkingRequestConstants.CreateBookmarkPageName;
		}
		#endregion

		#region Tags
		public IList<Tag> GetAllTags(string startSymbols, int limit)
		{
			return service.GetAllTags(startSymbols, limit);
		}

		public IList<Tag> GetTagsCloud()
		{
			return service.GetTagsCloud();
		}

		public IList<Tag> GetRelatedTagsCloud(long BookmarkID)
		{
			return service.GetRelatedTagsCloud(BookmarkID);
		}

		public IList<Tag> GetFavouriteTagsCloud()
		{
			return service.GetFavouriteTagsCloud();
		}

		public IList<Tag> GetTagsSortedByName()
		{
			return service.GetTagsSortedByName(FirstResult, MaxResults);
		}

		public IList<Tag> GetMostPopularTags()
		{
			return service.GetMostPopularTags(FirstResult, MaxResults);			
		}

		public int GetTagsCount()
		{
			return service.GetTagsCount();
		}
		#endregion

		#region Get Thumbnail Url
		public static string GetThumbnailUrl(string Url)
		{
			return GetThumbnailUrl(Url, BookmarkingSettings.ThumbSmallSize);
		}

		public static string GetMediumThumbnailUrl(string Url)
		{
			return GetThumbnailUrl(Url, BookmarkingSettings.ThumbMediumSize);
		}

		public static string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size)
		{
			var imageUrl = ThumbnailHelper.Instance.GetThumbnailUrl(Url, size);
			if (!String.IsNullOrEmpty(imageUrl))
			{
				return imageUrl;
			}
			return WebImageSupplier.GetAbsoluteWebPath(BookmarkingRequestConstants.NoImageAvailable, BookmarkingSettings.ModuleId);
		}

        public static string GetThumbnailServiceUrlForUpdate(string Url,string serviceUrl)
        {
            var size = string.Format("{0}x{1}",BookmarkingSettings.ThumbSmallSize.Width,BookmarkingSettings.ThumbSmallSize.Height);
            var imageUrl = string.Format("{0}?url={1}&f=png&s={2}&p={3}", serviceUrl, HttpUtility.UrlEncode(HttpUtility.HtmlDecode(Url)), size, Url.GetHashCode());
            if (!String.IsNullOrEmpty(imageUrl))
            {
                //Make request
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create(imageUrl);
                    using (var responce = (HttpWebResponse)req.GetResponse())
                    {
                        if (responce.StatusCode == HttpStatusCode.OK)
                        {
                            //Means that thumb ready
                            return imageUrl;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

		public static string GetThumbnailUrlForUpdate(string Url)
		{
            var imageUrl = ThumbnailHelper.Instance.GetThumbnailUrlForUpdate(Url, BookmarkingSettings.ThumbSmallSize);
			if (!String.IsNullOrEmpty(imageUrl))
			{
				return imageUrl;
			}
			return null;
		}

		public static void UpdateBookmarkThumbnail(int bookmarkID)
		{
			try
			{
				var b = BookmarkingServiceHelper.GetCurrentInstanse().service.GetBookmarkByID(bookmarkID);
				var bookmarks = new List<Bookmark>();
				bookmarks.Add(b);
				GenerateAllThumbnails(bookmarks, true);
			}
			catch { }
		}

		public static void GenerateAllThumbnails(bool overrideFlag)
		{
			try
			{
				var bookmarks = BookmarkingServiceHelper.GetCurrentInstanse().service.GetAllBookmarks();
				GenerateAllThumbnails(bookmarks, overrideFlag);
			}
			catch { }
		}

		private static void GenerateAllThumbnails(IList<Bookmark> bookmarks,bool overrideFlag)
		{
			try
			{
				
				List<object> p = new List<object>();
				p.Add(bookmarks);
				p.Add(HttpContext.Current);
				p.Add(TenantProvider.CurrentTenantID);
				p.Add(overrideFlag);
				var thread = new System.Threading.Thread(MakeThumbnailCallback);
				thread.SetApartmentState(System.Threading.ApartmentState.STA);
				thread.Start(p);
			}
			catch { }
		}
		private static void MakeThumbnailCallback(object p)
		{
			try
			{
				var obj = p as List<Object>;
				var bookmarks = obj[0] as List<Bookmark>;
				var context = obj[1] as HttpContext;
				var tenantID = (int)obj[2];
				var notOverride = !(bool)obj[3];
				foreach (var b in bookmarks)
				{
					ThumbnailHelper.Instance.MakeThumbnail(b.URL, false, notOverride, context, tenantID);
				}
			}
			catch { }
		}
		#endregion

		#region Pagination
		public void SetPagination()
		{
			CurrentPageNumber = 0;
			var pageNumber = CurrentRequest.QueryString[BookmarkingRequestConstants.Pagination];
			try
			{
				CurrentPageNumber = Convert.ToInt32(pageNumber);
			}
			catch
			{
				CurrentPageNumber = 0;
			}
			if (CurrentPageNumber <= 0)
			{
				CurrentPageNumber = 1;
			}
			FirstResult = (CurrentPageNumber - 1) * MaxResults;
		}

		public void InitPageNavigator(PageNavigator pagination)
		{
			InitPageNavigator(pagination, ItemsCount);
		}

		public void InitPageNavigator(PageNavigator pagination, long itemsCount)
		{
			int visiblePageCount = (int)itemsCount / MaxResults + 1;
			visiblePageCount = visiblePageCount > BookmarkingSettings.VisiblePageCount ? BookmarkingSettings.VisiblePageCount : visiblePageCount;

			pagination.CurrentPageNumber = CurrentPageNumber;
			pagination.EntryCountOnPage = MaxResults;
			pagination.VisiblePageCount = visiblePageCount;
			pagination.EntryCount = (int)itemsCount;
			pagination.PageUrl = GetCurrentURLForPagination();
			pagination.ParamName = "p";
			pagination.VisibleOnePage = false;
		}

		#endregion

		public Bookmark GetBookmarkByUrl(string url)
		{
			BookmarkToAdd = service.GetBookmarkByUrl(url);
			return BookmarkToAdd;
		}

		public Bookmark GetBookmarkByID(long bookmakrID)
		{
			return service.GetBookmarkByID(bookmakrID);
		}

		public IList<UserBookmark> GetUserBookmarks(Bookmark b)
		{
			return service.GetUserBookmarks(b);
		}

		public long GetUserBookmarksCount(Bookmark b)
		{
			return b.UserBookmarks.Count;
		}

		public bool IsCurrentUserBookmark(Bookmark b)
		{
			var currentUserID = GetCurrentUserID();
			var userBookmarks = b.UserBookmarks;
			if (userBookmarks == null || userBookmarks.Count == 0)
			{
				return false;
			}
			foreach (var user in userBookmarks)
			{
				if (currentUserID.Equals(user.UserID))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCurrentUserBookmark()
		{
			if (BookmarkToAdd != null)
			{
				return IsCurrentUserBookmark(BookmarkToAdd);
			}
			return false;
		}

		public Bookmark RemoveBookmarkFromFavourite(long bookmarkID)
		{
			var b = service.RemoveBookmarkFromFavourite(bookmarkID);
			if (b == null)
			{
				ThumbnailHelper.Instance.DeleteThumbnail(BookmarkingService.DeletedBookmarkUrl);
				return null;
			}
			return GetBookmarkWithUserBookmarks(b.URL);
		}

		public enum BookmarkDisplayMode
		{
			AllBookmarks,
			Favourites,
			SelectedBookmark,
			SearchBookmarks,
			SearchByTag,
			CreateBookmark,
			BookmarksCreatedByUser
		}

		public bool IsSelectedBookmarkDisplayMode()
		{
			return BookmarkDisplayMode.SelectedBookmark.Equals(DisplayMode);
		}

		public IList<Tag> GetMostRecentTags()
		{
			return service.GetMostRecentTags(FirstResult, MaxResults);
		}

		public IList<Bookmark> GetMostPopularBookmarksByTag(Tag t)
		{
			return service.GetMostPopularBookmarksByTag(t);
		}

		public IList<Bookmark> GetMostPopularBookmarksByTag(IList<Tag> tags)
		{
			return service.GetMostPopularBookmarksByTag(tags);
		}

		#region Comments

		public Comment GetCommentById(string commentID)
		{
			return service.GetCommentById(new Guid(commentID));
		}

		public void RemoveComment(string CommentID)
		{
			service.RemoveComment(new Guid(CommentID));
		}

		public void UpdateComment(String commentID, string text)
		{
			service.UpdateComment(new Guid(commentID), text);
		}

		public void AddComment(Comment comment)
		{
			service.AddComment(comment);
		}

		public long GetCommentsCount(Bookmark b)
		{
			return b.Comments.Count;
		}

		public IList<Comment> GetBookmarkComments(Bookmark b)
		{
			if (b.Comments != null)
			{
				return b.Comments;
			}
			return service.GetBookmarkComments(b);
		}
		#endregion


		public static string GetHTMLUserAvatar(Guid userID)
		{
			string imgPath = UserPhotoManager.GetSmallPhotoURL(userID);
			if (imgPath != null)
				return "<img class=\"userMiniPhoto\" alt='' src=\"" + imgPath + "\"/>";

			return string.Empty;
		}
		
		public static string GetHTMLUserAvatar()
		{			
			return GetHTMLUserAvatar(BookmarkingServiceHelper.GetCurrentInstanse().GetCurrentUserID());
		}

		public static string GetUserPageLink(Guid userID)
		{
			var userInfo = CoreContext.UserManager.GetUsers(userID);
			var userPageName = VirtualPathUtility.ToAbsolute("~/userprofile.aspx") + "?pid=" + BookmarkingBusinessConstants.CommunityProductID + "&uid=" + userInfo.ID;
			return string.Format("<a class='linkHeader' href='{0}'>{1}</a>", userPageName, userInfo.DisplayUserName());
		}

		public static string GetUserPageLink()
		{
			var userInfo = CoreContext.UserManager.GetUsers(BookmarkingServiceHelper.GetCurrentInstanse().GetCurrentUserID());
			var userPageName = VirtualPathUtility.ToAbsolute("~/userprofile.aspx") + "?uid=" + userInfo.ID;
			return string.Format("<a class='linkHeader' href='{0}'>{1}</a>", userPageName, userInfo.DisplayUserName());
		}

		public static string GetUserBookmarkDescriptionIfChanged(UserBookmark ub)
		{
			try
			{
				var userDescription = ub.Description;
				var b = BookmarkingServiceHelper.GetCurrentInstanse().GetBookmarkByID(ub.BookmarkID);

				if (b != null && b.UserCreatorID != null && ub != null && ub.UserID != null && b.UserCreatorID.Equals(ub.UserID))
				{
					return ub.Description;
				}

				var description = b.Description;
				if (!string.IsNullOrEmpty(userDescription) && !userDescription.ToLower().Equals(description.ToLower()))
				{
					return userDescription;
				}
			}
			catch { }
			return string.Empty;
		}

		public static string GetUserBookmarkDescriptionIfChanged(Bookmark b, UserBookmark ub)
		{
			try
			{

				if (b != null && b.UserCreatorID != null && ub != null && ub.UserID != null && b.UserCreatorID.Equals(ub.UserID))
				{
					return ub.Description;
				}

				var userDescription = ub.Description;
				var description = b.Description;
				if (!string.IsNullOrEmpty(userDescription) && !userDescription.ToLower().Equals(description.ToLower()))
				{
					return userDescription;
				}
			}
			catch { }
			return string.Empty;
		}
		#region Subcriptions

		public void Subscribe(string objectID, INotifyAction notifyAction)
		{
			service.Subscribe(objectID, notifyAction);
		}

		public bool IsSubscribed(string objectID, INotifyAction notifyAction)
		{
			return service.IsSubscribed(objectID, notifyAction);
		}

		public void UnSubscribe(string objectID, INotifyAction notifyAction)
		{
			service.UnSubscribe(objectID, notifyAction);
		}
		#endregion



		#region Common Search

		public SearchResultItem[] SearchBookmarksBySearchString(string searchString)
		{
			var searchStringList = ConvertStringToArray(searchString);			
			var bookmarks = service.SearchAllBookmarks(searchStringList);
			
			var searchResultItems = new List<SearchResultItem>();
			if (bookmarks == null)
			{
				return searchResultItems.ToArray();
			}
			foreach (var b in bookmarks)
			{
				var url = VirtualPathUtility.ToAbsolute(BookmarkingRequestConstants.BookmarkingBasePath) + "/" + GenerateBookmarkInfoUrl(b.URL);
				searchResultItems.Add(new SearchResultItem()
				{
					Name = b.Name,
					Description = b.Description,
					URL = url
				});
			}
			return searchResultItems.ToArray();
		}
		#endregion

		#region Search
		public IList<Bookmark> SearchBookmarks()
		{
			var text = GetSearchText();
			return SearchBookmarks(text);
		}

		private IList<Bookmark> SearchBookmarks(string searchString)
		{
			var searchStringList = ConvertStringToArray(searchString);
			var sortUtil = new BookmarkingSortUtil();

			try
			{
				var sortBy = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
				SortControl.SortItems = sortUtil.GetSearchBookmarksSortItems(sortBy);
			}
			catch
			{
				sortUtil.SortBy = SortByEnum.MostRecent;
			}

			switch (sortUtil.SortBy)
			{
				case SortByEnum.Popularity:
					return service.SearchBookmarksSortedByRaiting(searchStringList, FirstResult, MaxResults);
				default:
					return service.SearchBookmarks(searchStringList, FirstResult, MaxResults);
			}
		}

		private IList<Bookmark> SearchBookmarksByTag()
		{
			var text = GetSearchTag();
			try
			{
				var sortUtil = new BookmarkingSortUtil();
				var sortBy = CurrentRequest.QueryString[BookmarkingRequestConstants.SortByParam];
				SortControl.SortItems = sortUtil.GetBookmarksByTagSortItems(sortBy);

				switch (sortUtil.SortBy)
				{
					case SortByEnum.Popularity:
						return service.SearchMostPopularBookmarksByTag(text, FirstResult, MaxResults);
					default:
						return service.SearchBookmarksByTag(text, FirstResult, MaxResults);
				}
			}
			catch
			{
				return service.SearchBookmarksByTag(text, FirstResult, MaxResults);
			}
		}
		#endregion


		public static string RenderUserProfile(Guid userID)
		{
			return CoreContext.UserManager.GetUsers(userID).RenderProfileLink(BookmarkingBusinessConstants.CommunityProductID);
		}

		public void SetBookmarkTags(Bookmark b)
		{
			service.SetBookmarkTags(b);
		}
	}
}
