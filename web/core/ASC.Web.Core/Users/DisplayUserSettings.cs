using System;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Web.Core.Users
{
    [Serializable]
    public class DisplayUserSettings : ISettings
    {
        public Guid ID
        {
            get { return new Guid("2EF59652-E1A7-4814-BF71-FEB990149428"); }
        }

        public bool IsDisableGettingStarted { get; set; }

        public bool IsChangedDefaultPwd { get; set; }


        public ISettings GetDefault()
        {
            return new DisplayUserSettings()
            {
                IsDisableGettingStarted = false,
                IsChangedDefaultPwd = false
            };
        }

        public static string GetFullUserName(Guid userID)
        {
            return GetFullUserName(CoreContext.UserManager.GetUsers(userID));
        }

        public static string GetFullUserName(UserInfo userInfo)
        {
            return GetFullUserName(userInfo, DisplayUserNameFormat.Default);
        }

        public static string GetFullUserName(UserInfo userInfo, DisplayUserNameFormat format)
        {
            if (userInfo == null || userInfo.ID.Equals(ASC.Common.Security.Authorizing.Constants.Demo.ID))
            {
                return "Demo";
            }
            if (userInfo == null || (userInfo != null && !userInfo.ID.Equals(Guid.Empty) && !CoreContext.UserManager.UserExists(userInfo.ID)))
            {
                return "profile removed";
            }
            return UserFormatter.GetUserName(userInfo, format);
        }
    }
}
