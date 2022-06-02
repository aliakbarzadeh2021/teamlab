using System;
using System.Text.RegularExpressions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Users
{
    /// <summary>
    /// Web studio user manager helper
    /// </summary>
    public sealed class UserManagerWrapper
    {
        public static Guid AdminID
        {
            get
            {
                return new Guid("00000000-0000-0000-0000-000000000ace");
            }
        }

        private static bool TestUniqueUserName(UserInfo[] users, string uniqueName)
        {
            if (String.IsNullOrEmpty(uniqueName))
                return false;

            foreach (var user in users)
                if (String.Compare(uniqueName, user.UserName, true) == 0)
                    return false;

            return true;
        }

        private static string MakeUniqueName(UserInfo userInfo)
        {
            var users = CoreContext.UserManager.GetUsers(EmployeeStatus.All);
            string uniqueName = userInfo.UserName ?? "";

            if (TestUniqueUserName(users, uniqueName))
                return uniqueName;

            //try from email
            if (!String.IsNullOrEmpty(userInfo.Email))
            {
                uniqueName = userInfo.Email;
                int ind = userInfo.Email.IndexOf("@");
                if (ind != -1)
                    uniqueName = userInfo.Email.Substring(0, ind);

                if (TestUniqueUserName(users, uniqueName))
                    return uniqueName;
            }

            Regex escapeSymbols = new Regex(@"['\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            //try from name
            string[] splitters = new string[] { ".", "_", "-" };
            foreach (var splitter in splitters)
            {
                uniqueName = escapeSymbols.Replace(Common.Utils.Transliter.Translit("ru", "en", userInfo.FirstName ?? ""), "") + splitter +
                             escapeSymbols.Replace(Common.Utils.Transliter.Translit("ru", "en", userInfo.LastName ?? ""), "");

                if (TestUniqueUserName(users, uniqueName))
                    return uniqueName;

                uniqueName = escapeSymbols.Replace(Common.Utils.Transliter.Translit("ru", "en", userInfo.LastName ?? ""), "") + splitter +
                             escapeSymbols.Replace(Common.Utils.Transliter.Translit("ru", "en", userInfo.FirstName ?? ""), "");

                if (TestUniqueUserName(users, uniqueName))
                    return uniqueName;
            }

            string split = "__";
            bool isUnique = false;
            while (!isUnique)
            {
                uniqueName = escapeSymbols.Replace(Common.Utils.Transliter.Translit("ru", "en", userInfo.FirstName ?? ""), "") + split +
                             escapeSymbols.Replace(Common.Utils.Transliter.Translit("ru", "en", userInfo.LastName ?? ""), "");

                isUnique = TestUniqueUserName(users, uniqueName);
                split += "_";
            }

            return uniqueName;
        }

        public static bool CheckUniqueEmail(Guid userID, string email)
        {
            foreach (var user in CoreContext.UserManager.GetUsers(EmployeeStatus.All))
            {
                if (String.Equals(user.Email, email, StringComparison.InvariantCultureIgnoreCase)
                  && !userID.Equals(user.ID) && !String.IsNullOrEmpty(email))
                    return false;
            }

            return true;
        }

        public static UserInfo AddUser(UserInfo userInfo, string password)
        {
            return AddUser(userInfo, password, false);
        }

        public static UserInfo AddUser(UserInfo userInfo, string password, bool afterInvite)
        {
            return AddUser(userInfo, password, afterInvite, true);
        }

        public static UserInfo AddUser(UserInfo userInfo, string password, bool afterInvite, bool notify)
        {
            if (userInfo == null) throw new ArgumentNullException("userInfo");

            CheckPasswordPolicy(password);

            if (!CheckUniqueEmail(userInfo.ID, userInfo.Email))
                throw new Exception(CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists"));

            userInfo.UserName = MakeUniqueName(userInfo);

            var newUserInfo = CoreContext.UserManager.SaveUserInfo(userInfo);
            CoreContext.Authentication.SetUserPassword(newUserInfo.ID, password);

            if ((newUserInfo.Status & EmployeeStatus.Active) == EmployeeStatus.Active && notify)
            {
                //NOTE: Notify user only if it's active
                if (afterInvite)
                    StudioNotifyService.Instance.UserInfoAddedAfterInvite(newUserInfo, password);
                else
                    StudioNotifyService.Instance.UserInfoAdded(newUserInfo, password);
            }

            return newUserInfo;
        }

        public static UserInfo SaveUserInfo(UserInfo userInfo, string password)
        {
            if (userInfo == null) throw new ArgumentNullException("userInfo");

            CheckPasswordPolicy(password);

            if (!CheckUniqueEmail(userInfo.ID, userInfo.Email))
                throw new Exception(CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists"));

            var prevUserInfo = CoreContext.UserManager.GetUsers(userInfo.ID);
            var newUserInfo = CoreContext.UserManager.SaveUserInfo(userInfo);
            CoreContext.Authentication.SetUserPassword(newUserInfo.ID, password);

            StudioNotifyService.Instance.UserInfoUpdated(prevUserInfo, newUserInfo, password);

            UserOnlineManager.Instance.UpdateOnlineUserInfo(userInfo.ID);

            return newUserInfo;
        }

        public static UserInfo SaveUserInfo(UserInfo userInfo)
        {
            if (userInfo == null) throw new ArgumentNullException("userInfo");

            if (!CheckUniqueEmail(userInfo.ID, userInfo.Email))
                throw new Exception(CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists"));

            var prevUserInfo = CoreContext.UserManager.GetUsers(userInfo.ID);
            var newUserInfo = CoreContext.UserManager.SaveUserInfo(userInfo);

            StudioNotifyService.Instance.UserInfoUpdated(prevUserInfo, newUserInfo);

            UserOnlineManager.Instance.UpdateOnlineUserInfo(userInfo.ID);

            return newUserInfo;
        }

        public static void SetUserPassword(Guid userID, string password)
        {
            CheckPasswordPolicy(password);

            var cookie = SecurityContext.SetUserPassword(userID, password);
            StudioNotifyService.Instance.UserPasswordChanged(userID, password);

            if (cookie != null)
                CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
        }

        public static void CheckPasswordPolicy(string password)
        {
            if (String.IsNullOrEmpty(password))
                throw new Exception(Resources.Resource.ErrorPasswordEmpty);

            if (password.Length < 6)
                throw new Exception(Resources.Resource.ErrorPasswordToShort);
        }

        public static void SendUserPassword(string email)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException("email");

            var userInfo = Array.Find(CoreContext.UserManager.GetUsers(), u => String.Compare(u.Email, email, true) == 0);
            if (userInfo == null)
            {
                throw new Exception(String.Format(Resources.Resource.ErrorUserNotFoundByEmail, email));
            }
            StudioNotifyService.Instance.SendUserPassword(userInfo, CoreContext.Authentication.GetUserPasswordHash(userInfo.ID));
        }

        const string Noise = "1234567890mnbasdflkjqwerpoiqweyuvcxnzhdkqpsdk";

        public static string GeneratePassword()
        {
            return GeneratePassword(6, 6, Noise);
        }

        static long _counter = 0;
        internal static string GeneratePassword(int minLength, int maxLength, string noise)
        {
            Random rnd = new Random(DateTime.Now.Millisecond + (int)System.Threading.Interlocked.Increment(ref _counter));
            int length = minLength + rnd.Next(maxLength - maxLength);

            string pwd = "";
            while (length-- > 0)
                pwd += noise.Substring(rnd.Next(noise.Length - 1), 1);

            return pwd;
        }


        public static void TransferUser2Department(Guid userID, Guid depID)
        {
            var user = CoreContext.UserManager.GetUsers(userID);
            var department = CoreContext.GroupManager.GetGroupInfo(depID);
            if (!department.ID.Equals(ASC.Core.Users.Constants.LostGroupInfo.ID) &&
                !user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
            {

                var groups = CoreContext.UserManager.GetUserGroups(userID);
                GroupInfo oldDepartment = null;
                if (groups.Length > 0)
                    oldDepartment = groups[0];

                if (oldDepartment != null && oldDepartment.ID.Equals(department.ID))
                    return;

                if (oldDepartment != null)
                {
                    CoreContext.UserManager.RemoveUserFromGroup(user.ID, oldDepartment.ID);
                }

                CoreContext.UserManager.AddUserIntoGroup(user.ID, department.ID);
                user.Department = department.Name;
                CoreContext.UserManager.SaveUserInfo(user);
            }
        }

        public static void RenameDepartment(Guid depID, string newName)
        {
            var department = Array.Find(CoreContext.UserManager.GetDepartments(), (d) => d.ID == depID);
            if (department != null)
            {
                department.Name = newName;
                CoreContext.GroupManager.SaveGroupInfo(department);
                var users = CoreContext.UserManager.GetUsersByGroup(department.ID);
                foreach (var user in users)
                {
                    user.Department = department.Name;
                    CoreContext.UserManager.SaveUserInfo(user);
                }
            }
        }
    }
}
