using System;
using System.Collections.Generic;

namespace ASC.Core.Billing
{
    public interface ITariffService
    {
        Tariff GetTariff(int tenantId);

        IEnumerable<PaymentInfo> GetPayments(int tenantId);

        Uri GetShoppingUri(int tenant, TariffPlan plan);
    }
}
