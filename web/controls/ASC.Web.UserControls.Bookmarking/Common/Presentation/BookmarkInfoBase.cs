﻿using System;
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Pojo;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using ASC.Web.UserControls.Bookmarking.Resources;

namespace ASC.Web.UserControls.Bookmarking.Common.Presentation
{
	public abstract class BookmarkInfoBase : System.Web.UI.UserControl
	{
		#region Fields
		protected void Page_Load(object sender, EventArgs e)
		{
			InitUserControl();
		}

		public abstract void InitUserControl();

		protected BookmarkingServiceHelper ServiceHelper = BookmarkingServiceHelper.GetCurrentInstanse();
		#endregion

		#region Bookmark Fields
		public Bookmark Bookmark { get; set; }

		public UserBookmark UserBookmark { get; set; }

		public string Description
		{
			get
			{
				return Bookmark.Description.ReplaceSingleQuote();
			}
		}

		public string URL
		{
			get
			{
				return Bookmark.URL;
			}
		}

		public string Name
		{
			get
			{
				return Bookmark.Name.ReplaceSingleQuote();
			}
		}

		public long Raiting
		{
			get
			{
				return ServiceHelper.GetUserBookmarksCount(Bookmark);
			}
		}

		public string TagsString
		{
			get
			{
				return BookmarkingServiceHelper.ConvertBookmarkToTagsString(Bookmark).ReplaceSingleQuote();
			}
		}

		public DateTime Date
		{
			get
			{
				return Bookmark.Date;
			}
		}

		public long GetBookmarkID()
		{
			if (Bookmark == null)
			{
				return 0;
			}
			return Bookmark.ID;
		}

		public string UserBookmarkDescription
		{
			get
			{
				if (UserBookmark == null)
				{
					return Description;
				}
				return UserBookmark.Description.ReplaceSingleQuote();
			}
		}

		public bool HasDescription()
		{
			return !string.IsNullOrEmpty(UserBookmarkDescription);
		}



		public string UserBookmarkName
		{
			get
			{
				if (UserBookmark == null)
				{
					return Name;
				}
				return UserBookmark.Name.ReplaceSingleQuote();
			}
		}

		public string UserTagsString { get; set; }

		public DateTime UserDate
		{
			get
			{
				if (UserBookmark == null)
				{
					return Date;
				}
				return UserBookmark.DateAdded;
			}
		}

		public bool IsTagsIncluded()
		{
			if (Bookmark != null)
			{
				var tags = Bookmark.Tags;
				if (tags != null && tags.Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCurrentUserBookmark()
		{
			return ServiceHelper.IsCurrentUserBookmark(Bookmark);
		}

		public string GetThumbnailUrl()
		{
			return BookmarkingServiceHelper.GetThumbnailUrl(URL);
		}

		public string GetMediumThumbnailUrl()
		{
			return BookmarkingServiceHelper.GetMediumThumbnailUrl(URL);
		}

		private string GetThumbnailUrl(BookmarkingThumbnailSize size)
		{
			return BookmarkingServiceHelper.GetThumbnailUrl(URL, size);
		}

		public string GetTagsWebPath()
		{
			return WebImageSupplier.GetAbsoluteWebPath(BookmarkingRequestConstants.TagsImageName, BookmarkingSettings.ModuleId);
		}

		public string GetSearchByTagUrl(object bookmarkTagName)
		{
			var name = bookmarkTagName as string;
			return ServiceHelper.GetSearchByTagUrl(name);
		}

		public string GetBookmarkInfoUrlAddedByTab()
		{
			var url = string.Format("{0}&{1}={2}",
									BookmarkingServiceHelper.GenerateBookmarkInfoUrl(URL),
									BookmarkingRequestConstants.SelectedTab,
									BookmarkingRequestConstants.SelectedTabBookmarkAddedBy
									);
			return url;
		}

		public string GetBookmarkInfoUrl()
		{
			return BookmarkingServiceHelper.GenerateBookmarkInfoUrl(URL);
		}

		public string GetBookmarkRaiting()
		{
			return new BookmarkRaitingUserControl().GetBookmarkRaiting(Bookmark, GetUniqueId().ToString(), GetSingleBookmarkID());
		}

		public string GetUserBookmarkRaiting()
		{
			return new BookmarkRaitingUserControl().GetBookmarkRaiting(Bookmark, UserBookmark, GetUniqueId().ToString(), GetSingleBookmarkID());
		}

		public string GetSingleBookmarkID()
		{
			return GetUniqueId() + "SingleBookmark";
		}

		#endregion

		#region User Info
		public bool ShowAddedByUserInfo
		{
			get
			{
				switch (ServiceHelper.DisplayMode)
				{
					case BookmarkingServiceHelper.BookmarkDisplayMode.AllBookmarks:
						return true;
					case BookmarkingServiceHelper.BookmarkDisplayMode.SelectedBookmark:
						return true;
				}
				return false;
			}
		}

		public string RenderProfileLink()
		{
			return RenderProfileLink(GetBookmarkCreator());
		}

		public string RenderProfileLink(UserInfo userInfo)
		{
			try
			{
				return userInfo.RenderProfileLink(BookmarkingBusinessConstants.CommunityProductID);
			}
			catch
			{
				return string.Empty;
			}
		}

		public string RenderProfileLink(Object userID)
		{
			var user = GetUserInfoByUserID(userID);
			return RenderProfileLink(user);
		}

		public string GetBookmarkCreatorImageUrl()
		{
			return GetImageUrl(GetBookmarkCreator());
		}

		private static string GetImageUrl(UserInfo userInfo)
		{
			try
			{
                return userInfo.GetMediumPhotoURL();
			}
			catch
			{
				return string.Empty;
			}
		}

		protected UserInfo GetBookmarkCreator()
		{
			if (Bookmark == null)
			{
				return null;
			}
			return CoreContext.UserManager.GetUsers(Bookmark.UserCreatorID);
		}

		public string GetUserImageUrl(object userID)
		{
			var user = GetUserInfoByUserID(userID);
			return GetImageUrl(user);
		}

		public string GetUserImage(object userID)
		{
			var user = GetUserInfoByUserID(userID);
			return BookmarkingServiceHelper.GetHTMLUserAvatar(user.ID);
		}

		private static UserInfo GetUserInfoByUserID(object userID)
		{
			try
			{
				Guid id = (Guid)userID;
				return CoreContext.UserManager.GetUsers(id);
			}
			catch
			{
				return null;
			}
		}
		#endregion

		#region Uuid

		private Guid _uniqueId;

		public Guid UniqueId
		{
			get
			{
				if (_uniqueId == null || Guid.Empty.Equals(_uniqueId))
				{
					_uniqueId = Guid.NewGuid();
				}
				return _uniqueId;
			}
			set
			{
				_uniqueId = value;
			}
		}

		/// <summary>
		/// Return unique id, which will be upted for every instance.
		/// Calling this method for the same instance will return the same result.
		/// </summary>
		/// <returns></returns>
		public Guid GetUniqueId()
		{
			return UniqueId;
		}
		#endregion

		#region Added By Info

		public string GetDateAddedAsString(object date)
		{
			try
			{
				var dateAdded = (DateTime)date;
				return BookmarkingConverter.GetDateAsString(dateAdded);
			}
			catch
			{
				return string.Empty;
			}
		}

		public string GetDateAddedAsString()
		{
			try
			{
				return Date.ToShortTimeString() + " " + Date.ToShortDateString();
			}
			catch
			{
				return string.Empty;
			}
		}

		public long CommentsCount
		{
			get
			{
				if (Bookmark != null)
				{
					return ServiceHelper.GetCommentsCount(Bookmark);
				}
				return 0;
			}
		}

		public string CommentString
		{
			get
			{
				var commentsCount = CommentsCount;
				if (commentsCount == 0)
				{
					return string.Empty;
				}
				var comments = GrammaticalHelper.ChooseNumeralCase((int)commentsCount,
																	BookmarkingUCResource.CommentsNominative,
																	BookmarkingUCResource.CommentsGenitiveSingular,
																	BookmarkingUCResource.CommentsGenitivePlural
																	);
				return String.Format("{0} {1}", commentsCount, comments);
			}
		}

		public string GetUserPageLink(object userID)
		{
			return BookmarkingServiceHelper.GetUserPageLink(new Guid(userID.ToString()));
		}


		#endregion

		#region Display Mode


		public bool IsAllBookmarksMode()
		{
			return BookmarkingServiceHelper.BookmarkDisplayMode.AllBookmarks == ServiceHelper.DisplayMode;
		}

		public bool IsBookmarkInfoMode
		{
			get
			{
				return BookmarkingServiceHelper.BookmarkDisplayMode.SelectedBookmark == ServiceHelper.DisplayMode;
			}
		}

		public string FavouriteBookmarksMode
		{
			get
			{
				return (BookmarkingServiceHelper.BookmarkDisplayMode.Favourites == ServiceHelper.DisplayMode).ToString().ToLower();
			}
		}
		#endregion

		#region Permissions Check

		public static bool PermissionCheckAddToFavourite()
		{
			return BookmarkingPermissionsCheck.PermissionCheckAddToFavourite();
		}

		public bool PermissionCheckRemoveFromFavourite()
		{
			if (UserBookmark != null)
			{
				return BookmarkingPermissionsCheck.PermissionCheckRemoveFromFavourite(UserBookmark);
			}
			return false;
		}

		#endregion


		public string IsPromo
		{
			get
			{
				return (SetupInfo.WorkMode == WorkMode.Promo).ToString().ToLower();
			}
		}

		public string GetUniqueIDFromSingleBookmark(string SingleBookmarkDivID)
		{
			if (!string.IsNullOrEmpty(SingleBookmarkDivID))
			{
				var a = SingleBookmarkDivID.Split(new string[] { "SingleBookmark" }, StringSplitOptions.None);
				if (a.Length == 2)
				{
					return a[0];
				}
			}
			return Guid.NewGuid().ToString();
		}

		public string GetRandomGuid()
		{
			return Guid.NewGuid().ToString();
		}



		public string GetUserBookmarkDescriptionIfChanged(object o)
		{
			try
			{
				if (o is UserBookmark)
				{
					var ub = o as UserBookmark;
					if (Bookmark != null)
					{
						return BookmarkingServiceHelper.GetUserBookmarkDescriptionIfChanged(Bookmark, ub);
					}
					return BookmarkingServiceHelper.GetUserBookmarkDescriptionIfChanged(ub);
				}
			}
			catch { }
			return string.Empty;
		}
	}
}
