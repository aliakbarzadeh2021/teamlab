using ASC.Bookmarking.Common;
using ASC.Bookmarking.Pojo;
using ASC.Core;

namespace ASC.Bookmarking.Business.Permissions
{
    public static class BookmarkingPermissionsCheck
    {
        public static bool PermissionCheckCreateBookmark()
        {
            return SecurityContext.CheckPermissions(BookmarkingBusinessConstants.BookmarkCreateAction);
        }

        public static bool PermissionCheckAddToFavourite()
        {
            return SecurityContext.CheckPermissions(BookmarkingBusinessConstants.BookmarkAddToFavouriteAction);
        }

        public static bool PermissionCheckRemoveFromFavourite(UserBookmark b)
        {
            return SecurityContext.CheckPermissions(new BookmarkPermissionSecurityObject(b.UserID), BookmarkingBusinessConstants.BookmarkRemoveFromFavouriteAction);
        }

        public static bool PermissionCheckCreateComment()
        {
            return SecurityContext.CheckPermissions(BookmarkingBusinessConstants.BookmarkCreateCommentAction);
        }

        public static bool PermissionCheckEditComment()
        {
            return SecurityContext.CheckPermissions(BookmarkingBusinessConstants.BookmarkEditCommentAction);
        }

        public static bool PermissionCheckEditComment(Comment c)
        {
            return SecurityContext.CheckPermissions(new BookmarkPermissionSecurityObject(c.UserID, c.ID), BookmarkingBusinessConstants.BookmarkEditCommentAction);
        }
    }
}
