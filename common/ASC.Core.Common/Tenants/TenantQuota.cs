using System;

namespace ASC.Core.Tenants
{
    [Serializable]
    public class TenantQuota
    {
        public static readonly TenantQuota Default = new TenantQuota(Tenants.Tenant.DEFAULT_TENANT)
        {
            MaxFileSize = 25 * 1024 * 1024, // 25Mb
            MaxTotalSize = long.MaxValue,
            HttpsEnable = true,
            SecurityEnable = true,
        };


        public int Tenant
        {
            get;
            private set;
        }

        public long MaxFileSize
        {
            get;
            set;
        }

        public long MaxTotalSize
        {
            get;
            set;
        }

        public bool HttpsEnable
        {
            get;
            set;
        }

        public bool SecurityEnable
        {
            get;
            set;
        }


        public TenantQuota(int tenant)
        {
            Tenant = tenant;
        }


        public override int GetHashCode()
        {
            return Tenant.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var q = obj as TenantQuota;
            return q != null && q.Tenant == Tenant;
        }
    }
}
