using System;
using System.Collections.Generic;
using System.Text;
using ASC.Bookmarking.Pojo;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Bookmarking.Common.Util
{
	public static class BookmarkingBusinessUtil
	{
		public static string GenerateBookmarkInfoUrl(Bookmark b)
		{
			return ASC.Bookmarking.Business.BookmarkingService.ModifyBookmarkUrl(b);
		}		

		public static string GenerateBookmarksUrl(Bookmark b)
		{
			return VirtualPathUtility.ToAbsolute(BookmarkingBusinessConstants.BookmarkingBasePath + "/default.aspx");
		}

		public static string RenderProfileLink(Guid userID)
		{
			return CoreContext.UserManager.GetUsers(userID).RenderProfileLink(BookmarkingBusinessConstants.CommunityProductID);
		}

		public static string GetRecipientSubscriptionConfigURL(Guid recipientID)
		{
			return CommonLinkUtility.GetFullAbsolutePath(
						CommonLinkUtility.GetUserProfile(
							recipientID,
							CommonLinkUtility.GetProductID(),
							UserProfileType.Subscriptions));
		}
	}
}
