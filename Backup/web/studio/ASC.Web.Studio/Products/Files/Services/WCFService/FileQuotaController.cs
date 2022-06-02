using System;
using System.Collections.Generic;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Services.WCFService
{
    static class FileQuotaController
    {
        public static long QuotaGet()
        {
            var s = Global.GetStore();
            var used = s != null && s.QuotaController != null ? s.QuotaController.QuotaUsedGet(null, null) : 0;
            var max = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).MaxTotalSize;
            return GetUsedPercent(max, used);
        }

        private static int GetUsedPercent(long max, long used)
        {
            if (max != 0)
            {
                if (used >= max || Math.Abs(used - max) < 1024 * 1024) return 100;//less 1MB

                var percent = (int)(100d * used / (double)max);
                return percent == 0 && 0 < used ? 1 : percent;
            }
            return 0;
        }
    }
}