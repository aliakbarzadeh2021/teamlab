using System;
using System.Collections.Generic;
using System.Web;
using ASC.Core;
using ASC.Web.Core.Users.Activity;
using System.Globalization;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.Studio.Utility;
using ASC.Core.Tenants;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiActivityPublisher : BaseUserActivityPublisher
    {

        internal static void PublishInternal(UserActivity activity)
        {
            UserActivityPublisher.Publish<WikiActivityPublisher>(activity);
        }

        internal static string GetContentID(object page)
        {
            string result = string.Empty;
            if (page is Pages)
            {
                result = string.Format(CultureInfo.CurrentCulture, "wikiPage#{0}", (page as Pages).PageName);
            }
            else if(page is Files)
            {
                result = string.Format(CultureInfo.CurrentCulture, "wikiFile#{0}", (page as Files).FileName);
            }

            return result;
        }

        internal static string GetTitle(object page)
        {
            string result = string.Empty;
            if (page is Pages)
            {
                result = (page as Pages).PageName;
                if(string.IsNullOrEmpty(result))
                {
                    result = WikiResource.MainWikiCaption;
                }
            }
            else if (page is Files)
            {
                result = (page as Files).FileName;
            }

            return result;
        }

        internal static string GetUrl(object page)
        {
            string result = string.Empty;
            if (page is Pages)
            {

                result = ActionHelper.GetViewPagePath(WikiManager.ViewVirtualPath, (page as Pages).PageName); 
            }
            else if (page is Files)
            {
                result = ActionHelper.GetViewFilePath(WikiManager.ViewVirtualPath, (page as Files).FileName);
            }

            return result;
        }


        internal static UserActivity ComposeActivityByPage(object page)
        {
            UserActivity ua = new UserActivity();
            ua.TenantID = TenantProvider.CurrentTenantID;
            ua.ContentID = GetContentID(page);
            ua.Date = TenantUtil.DateTimeNow();
            ua.ModuleID = WikiManager.ModuleId;
            ua.ProductID = Product.CommunityProduct.ID;
            ua.Title = GetTitle(page);
            ua.URL = GetUrl(page);

            return ua;
        }

        internal static UserActivity ApplyCustomeActivityParams(UserActivity ua, string actionText, Guid userID, int actionType, int businessValue)
        {
            ua.ImageOptions = new ImageOptions();
            ua.ImageOptions.PartID = WikiManager.ModuleId;
            ua.ImageOptions.ImageFileName = string.Empty;
            ua.ActionText = actionText;
            ua.UserID = userID;
            ua.ActionType = actionType;
            ua.BusinessValue = businessValue;
            return ua;
        }

        



        public static void AddPage(Pages page)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_PageAdded,
                            page.UserID,
                            UserActivityConstants.ContentActionType,
                            UserActivityConstants.NormalContent
                        );

                PublishInternal(ua);
        }

        public static bool EditPage(Pages page)
        {
            if(page.Version == 1)//New Page Saved!!!
            {
                AddPage(page);
                return false;
            }
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_PageEdited,
                            page.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.ImportantActivity
                        );

            PublishInternal(ua);
            return true;
        }

        public static void RevertPage(Pages page)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_VersionRevert,
                            page.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }

        public static void AddFile(Files file)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(file),
                            WikiResource.wikiAction_FileAdded,
                            file.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.ImportantActivity
                        );

            PublishInternal(ua);
        }

        public static void DeleteFile(Files file)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(file),
                            WikiResource.wikiAction_FileDeleted,
                            file.UserID,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }

       

        public static void AddPageComment(Pages page, WikiComments newComment)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_CommentAdded,
                            newComment.UserId,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.NormalActivity
                        );

            PublishInternal(ua);
        }

        public static void EditPageComment(Pages page, WikiComments newComment)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_CommentEdited,
                            newComment.UserId,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }

        public static void DeletePageComment(Pages page, WikiComments newComment)
        {
            UserActivity ua =
                        ApplyCustomeActivityParams(
                            ComposeActivityByPage(page),
                            WikiResource.wikiAction_CommentDeleted,
                            newComment.UserId,
                            UserActivityConstants.ActivityActionType,
                            UserActivityConstants.SmallActivity
                        );

            PublishInternal(ua);
        }

    }
}
