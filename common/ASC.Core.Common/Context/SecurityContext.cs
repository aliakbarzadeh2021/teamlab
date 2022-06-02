using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Web;
using ASC.Common.Security;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
using ASC.Core.Security.Authentication;
using ASC.Core.Security.Authorizing;
using ASC.Security.Cryptography;
using AuthConst = ASC.Core.Configuration.Constants;
using UserConst = ASC.Core.Users.Constants;

namespace ASC.Core
{
    public static class SecurityContext
    {
        private const string AUTH_PRINCIPAL = "__Auth.Principal";
        private const string AUTH_COOKIE = "__Auth.Cookie";


        public static IAccount CurrentAccount
        {
            get { return Principal.Identity is IAccount ? (IAccount)Principal.Identity : AuthConst.Guest; }
        }

        public static bool IsAuthenticated
        {
            get { return CurrentAccount.IsAuthenticated; }
        }

        public static bool DemoMode
        {
            get { return IsAuthenticated && CurrentAccount.ID == Constants.Demo.ID; }
        }

        public static IPermissionResolver PermissionResolver
        {
            get;
            private set;
        }


        static SecurityContext()
        {
            var permissionProvider = new PermissionProvider();
            var azManager = new AzManager(new RoleProvider(), permissionProvider);
            PermissionResolver = new PermissionResolver(azManager, permissionProvider);
        }


        public static string AuthenticateMe(string login, string password)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (password == null) throw new ArgumentNullException("password");

            var credential = CreateCredential(CoreContext.TenantManager.GetCurrentTenant().TenantId, login, password);
            AuthenticateMe(new UserAccount(credential));

            var cookie = CookieStorage.Save(credential);
            WebCookie = cookie;
            return cookie;
        }

        public static bool AuthenticateMe(string cookie)
        {
            if (cookie == null) throw new ArgumentNullException("cookie");

            var credential = CookieStorage.Get(cookie);
            if (credential == null || credential.Tenant != CoreContext.TenantManager.GetCurrentTenant().TenantId)
            {
                return false;
            }

            try
            {
                AuthenticateMe(new UserAccount(credential));
                WebCookie = cookie;
                return true;
            }
            catch
            {
                CookieStorage.Remove(cookie);
                WebCookie = null;
                return false;
            }
        }

        public static void AuthenticateMe(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            var roles = new List<string>() { Role.Everyone };


            if (account is ISystemAccount && account.ID == AuthConst.CoreSystem.ID)
            {
                roles.Add(Role.System);
            }

            if (account is IUserAccount)
            {
                var credential = ((UserAccount)account).GetCredential();
                if (credential == null) throw new SecurityException("Invalid username or password.");

                var u = CoreContext.UserManager.GetUsers(credential.Tenant, credential.Login, credential.PasswordHash);
                if (u.ID == UserConst.LostUser.ID)
                {
                    throw new SecurityException("Invalid username or password.");
                }
                if (CoreContext.UserManager.IsUserInGroup(u.ID, UserConst.GroupAdmin.ID))
                {
                    roles.Add(Role.Administrators);
                }
                roles.Add(CoreContext.UserManager.IsUserInGroup(u.ID, UserConst.GroupVisitor.ID) ? Role.Visitors : Role.Users);

                account = new UserAccount(u, credential.Tenant);
            }

            Principal = new GenericPrincipal(account, roles.ToArray());
        }

        public static void Logout()
        {
            if (WebCookie != null)
            {
                CookieStorage.Remove(WebCookie);
            }
            WebCookie = null;
            Principal = null;
        }

        public static string SetUserPassword(Guid userID, string password)
        {
            CoreContext.Authentication.SetUserPassword(userID, password);
            if (CurrentAccount.ID == userID)
            {
                var credential = CreateCredential(CoreContext.TenantManager.GetCurrentTenant().TenantId, userID.ToString(), password);
                return CookieStorage.Update(credential, WebCookie);
            }
            return null;
        }


        public static bool CheckPermissions(params IAction[] actions)
        {
            return PermissionResolver.Check(CurrentAccount, actions);
        }

        public static bool CheckPermissions(ISecurityObject securityObject, params IAction[] actions)
        {
            return CheckPermissions(securityObject, null, actions);
        }

        public static bool CheckPermissions(ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider, params IAction[] actions)
        {
            return PermissionResolver.Check(CurrentAccount, objectId, securityObjProvider, actions);
        }

        public static void DemandPermissions(params IAction[] actions)
        {
            PermissionResolver.Demand(CurrentAccount, actions);
        }

        public static void DemandPermissions(ISecurityObject securityObject, params IAction[] actions)
        {
            DemandPermissions(securityObject, null, actions);
        }

        public static void DemandPermissions(ISecurityObjectId objectId, ISecurityObjectProvider securityObjProvider, params IAction[] actions)
        {
            PermissionResolver.Demand(CurrentAccount, objectId, securityObjProvider, actions);
        }


        internal static Credential CreateCredential(int tenantID, string login, string password)
        {
            return new Credential(login, Hasher.Base64Hash(password, HashAlg.SHA256), tenantID);
        }


        private static IPrincipal Principal
        {
            get
            {
                var principal = GetFromHttpSession<IPrincipal>(AUTH_PRINCIPAL);
                if (principal != null)
                {
                    Thread.CurrentPrincipal = principal;
                    if (HttpContext.Current != null) HttpContext.Current.User = principal;
                }
                return Thread.CurrentPrincipal;
            }
            set
            {
                SetToHttpSession(AUTH_PRINCIPAL, value);
                Thread.CurrentPrincipal = value;
                if (HttpContext.Current != null) HttpContext.Current.User = value;
            }
        }

        private static string WebCookie
        {
            get { return GetFromHttpSession<string>(AUTH_COOKIE); }
            set { SetToHttpSession(AUTH_COOKIE, value); }
        }

        private static T GetFromHttpSession<T>(string name)
        {
            return HttpContext.Current != null && HttpContext.Current.Session != null
                ? (T)HttpContext.Current.Session[name]
                : default(T);
        }

        private static void SetToHttpSession(string name, object obj)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[name] = obj;
            }
        }
    }
}