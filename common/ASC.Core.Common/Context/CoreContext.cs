using System.Configuration;
using ASC.Core.Billing;
using ASC.Core.Caching;
using ASC.Core.Configuration;
using ASC.Core.Data;
using ASC.Core.Notify;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace ASC.Core
{
    public static class CoreContext
    {
        static CoreContext()
        {
            var unityConfigurationSection = ConfigurationManager.GetSection("unity");
            if (unityConfigurationSection == null)
            {
                ConfigureCoreContextByDefault();
            }
            else
            {
                //used lazy compilation if no unity configuration section.
                ConfigureCoreContextByUnity(unityConfigurationSection);
            }
        }


        public static IConfigurationClient Configuration
        {
            get;
            private set;
        }

        public static ITenantManagerClient TenantManager
        {
            get;
            private set;
        }

        public static IUserManagerClient UserManager
        {
            get;
            private set;
        }

        public static IGroupManagerClient GroupManager
        {
            get;
            private set;
        }

        public static IAuthManagerClient Authentication
        {
            get;
            private set;
        }

        public static IAzManagerClient AuthorizationManager
        {
            get;
            private set;
        }

        public static INotify Notify
        {
            get;
            private set;
        }

        internal static ISubscriptionManagerClient SubscriptionManager
        {
            get;
            private set;
        }


        private static void ConfigureCoreContextByUnity(object section)
        {
            if (((UnityConfigurationSection)section).Containers["Core"] == null)
            {
                ConfigureCoreContextByDefault();
            }
            else
            {
                var unity = new UnityContainer().LoadConfiguration("Core");
                Configuration = unity.Resolve<IConfigurationClient>();
                TenantManager = unity.Resolve<ITenantManagerClient>();
                UserManager = unity.Resolve<IUserManagerClient>();
                GroupManager = unity.Resolve<IGroupManagerClient>();
                Authentication = unity.Resolve<IAuthManagerClient>();
                AuthorizationManager = unity.Resolve<IAzManagerClient>();
                SubscriptionManager = unity.Resolve<ISubscriptionManagerClient>();
                Notify = unity.Resolve<INotify>();
            }
        }

        private static void ConfigureCoreContextByDefault()
        {
            var cs = ConfigurationManager.ConnectionStrings["core"];
            if (cs == null)
            {
                throw new ConfigurationErrorsException("Can not configure CoreContext: connection string with name core not found.");
            }

            var tenantService = new CachedTenantService(new DbTenantService(cs));
            var userService = new CachedUserService(new DbUserService(cs));
            var azService = new CachedAzService(new DbAzService(cs));
            var quotaService = new CachedQuotaService(new DbQuotaService(cs));
            var subService = new CachedSubscriptionService(new DbSubscriptionService(cs));
            var tariffService = new TariffService(cs, quotaService);

            Configuration = new ClientConfiguration(tenantService);
            TenantManager = new ClientTenantManager(tenantService, quotaService, Configuration.Standalone ? null : tariffService);
            UserManager = new ClientUserManager(userService);
            GroupManager = new ClientUserManager(userService);
            Authentication = new ClientAuthManager(userService);
            AuthorizationManager = new ClientAzManager(azService);
            SubscriptionManager = new ClientSubscriptionManager(subService);
            Notify = new NotifyImpl();
        }
    }
}