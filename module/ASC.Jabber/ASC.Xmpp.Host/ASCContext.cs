using ASC.Core;
using ASC.Core.Configuration;
using ASC.Core.Tenants;

namespace ASC.Xmpp.Host
{
    static class ASCContext
    {
        public static IUserManagerClient UserManager
        {
            get
            {
                return CoreContext.UserManager;
            }
        }

        public static IAuthManagerClient Authentication
        {
            get
            {
                return CoreContext.Authentication;
            }
        }

        public static IGroupManagerClient GroupManager
        {
            get
            {
                return CoreContext.GroupManager;
            }
        }

        public static Tenant GetCurrentTenant()
        {
            return CoreContext.TenantManager.GetCurrentTenant(false);
        }

        public static void SetCurrentTenant(string domain)
        {
            SecurityContext.AuthenticateMe(Constants.CoreSystem);
            if (domain.Contains("."))
            {
                var parts = domain.Split('.');
                if (3 < parts.Length) domain = string.Join(".", new[] { parts[parts.Length - 3], parts[parts.Length - 2], parts[parts.Length - 1] });
            }

            var current = CoreContext.TenantManager.GetCurrentTenant(false);
            if (current == null || string.Compare(current.TenantDomain, domain, true) != 0)
            {
                CoreContext.TenantManager.SetCurrentTenant(domain);
            }
        }
    }
}