using System;
using System.Text;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;


namespace ASC.Core.Users
{
    public static class StudioUserInfoExtension
    {
        public static bool IsOwner(this UserInfo userInfo)
        {
            if (userInfo == null)
                return false;

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if (tenant == null)
                return false;
            return tenant.OwnerId.Equals(userInfo.ID);
        }

        public static bool IsAdmin(this UserInfo userInfo)
        {
            if (userInfo == null)
                return false;

            return CoreContext.UserManager.IsUserInGroup(userInfo.ID, ASC.Core.Users.Constants.GroupAdmin.ID);
        }       
  
        public static bool IsOnline(this UserInfo userInfo)
        {
            if (userInfo == null)
                return false;

            return (UserOnlineManager.Instance.OnlineUsers.Find(uvi => uvi.UserInfo.ID.Equals(userInfo.ID)) != null);
        }

        public static string GetCurrentURL(this UserInfo userInfo)
        {
            if (userInfo == null)
                return "";

            var userVisitInfo = UserOnlineManager.Instance.OnlineUsers.Find(uvi => uvi.UserInfo.ID.Equals(userInfo.ID));
            if (userVisitInfo != null)
                return userVisitInfo.CurrentURL;

            return "";
        }

        public static string GetUserProfilePageURL(this UserInfo userInfo)
        {
            return CommonLinkUtility.GetUserProfile(userInfo.ID, CommonLinkUtility.GetProductID() );
        }

        public static string GetUserProfilePageURL(this UserInfo userInfo, Guid productID)
        {   
            if (userInfo == null)
                return "";

            return CommonLinkUtility.GetUserProfile(userInfo.ID, productID);
        }


        public static string GetUserProfilePageURL(this UserInfo userInfo, Guid productID, UserProfileType profileType)
        {
            if (userInfo == null)
                return "";

            return CommonLinkUtility.GetUserProfile(userInfo.ID, productID, profileType);
        }

        public static string GetCurrentModuleName(this UserInfo userInfo)
        {
            if (userInfo == null)
                return "";

            var userVisitInfo = UserOnlineManager.Instance.OnlineUsers.Find(uvi => uvi.UserInfo.ID.Equals(userInfo.ID));
            if (userVisitInfo != null)
            {
                var module = UserOnlineManager.Instance.GetCurrentModule(userVisitInfo.CurrentURL);
                if (module != null)
                    return module.ModuleName;
            }

            return "";
        }

        public static Guid GetCurrentModuleID(this UserInfo userInfo)
        {
            if (userInfo == null)
                return Guid.Empty;

            var userVisitInfo = UserOnlineManager.Instance.OnlineUsers.Find(uvi => uvi.UserInfo.ID.Equals(userInfo.ID));
            if (userVisitInfo != null)
            {
                var module = UserOnlineManager.Instance.GetCurrentModule(userVisitInfo.CurrentURL);
                if (module != null)
                    return module.ModuleID;
            }

            return Guid.Empty;
        }

        public static string RenderUserStatus(this UserInfo userInfo)
        {
            StringBuilder sb = new StringBuilder();
            if (userInfo.IsOnline())
            {
              sb.AppendFormat("<img align=\"absmiddle\" alt=\"\" style=\"margin-right:3px;\" src=\"{0}\"/>", WebImageSupplier.GetAbsoluteWebPath("status_online.png"));
              sb.Append(Resources.Resource.Online);
            }
            else
            {
              sb.AppendFormat("<img align=\"absmiddle\" alt=\"\" style=\"margin-right:3px;\" src=\"{0}\"/>", WebImageSupplier.GetAbsoluteWebPath("status_offline.png"));
              sb.Append(Resources.Resource.Offline);
            }
            return sb.ToString();
        }

        public static string RenderUserCommunication(this UserInfo userInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(RenderUserStatus(userInfo));


            return sb.ToString();
        }

        public static string RenderProfileLink(this UserInfo userInfo, Guid productID)
        {
            StringBuilder sb = new StringBuilder();

            if (userInfo == null || !CoreContext.UserManager.UserExists(userInfo.ID))
            {
                sb.Append("<span class='userLink textMediumDescribe' style='white-space:nowrap;'>");
                sb.Append(Resources.Resource.ProfileRemoved);
                sb.Append("</span>");
            }
			else if (Array.Exists(Configuration.Constants.SystemAccounts, a => a.ID == userInfo.ID))
			{
				sb.Append("<span class='userLink textMediumDescribe' style='white-space:nowrap;'>");
				sb.Append(userInfo.LastName);
				sb.Append("</span>");
			}
			else
            {
                Guid popupID = Guid.NewGuid();
                sb.AppendFormat("<span class=\"userLink\" style=\"white-space:nowrap;\" id=\"{0}\" data-uid=\"{1}\" data-pid=\"{2}\">", popupID, userInfo.ID, productID);                
                sb.AppendFormat("<a class='linkDescribe' href=\"{0}\">{1}</a>", userInfo.GetUserProfilePageURL(productID), userInfo.DisplayUserName(true));
                sb.Append("</span>");
            }
            return sb.ToString();
        }

        public static string RenderPopupInfoScript(this UserInfo userInfo, Guid productID, string elementID)
        {
            if (userInfo == ASC.Core.Users.Constants.LostUser) return "";
            StringBuilder sb = new StringBuilder();
            sb.Append("<script language='javascript'> StudioUserProfileInfo.RegistryElement('" + elementID + "','\"" + userInfo.ID + "\",\"" + productID + "\"'); </script>");
            return sb.ToString();
        }
    }
}
