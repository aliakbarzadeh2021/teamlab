using System;
using ASC.Core.Tenants;

namespace ASC.Core.Billing
{
    public class Tariff
    {
        public TariffPlan Plan
        {
            get;
            set;
        }

        public TenantQuota Quota
        {
            get;
            set;
        }

        public DateTime DueDate
        {
            get;
            set;
        }

        public bool Expired
        {
            get { return DueDate.Date < DateTime.UtcNow.Date; }
        }
    }
}
