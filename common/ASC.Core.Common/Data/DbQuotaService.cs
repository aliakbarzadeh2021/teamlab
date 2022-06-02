using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;

namespace ASC.Core.Data
{
    public class DbQuotaService : DbBaseService, IQuotaService
    {
        private const string tenants_quota = "tenants_quota";
        internal const string tenants_quotarow = "tenants_quotarow";


        public DbQuotaService(ConnectionStringSettings connectionString)
            : base(connectionString, "tenant")
        {
        }

        public TenantQuota GetTenantQuota(int tenant)
        {
            var q = Query(tenants_quota, tenant)
                .Select("tenant", "max_file_size", "max_total_size", "https_enable", "security_enable");
            
            return ExecList(q)
                .ConvertAll(r => new TenantQuota(Convert.ToInt32(r[0]))
                {
                    MaxFileSize = Convert.ToInt64(r[1]),
                    MaxTotalSize = Convert.ToInt64(r[2]),
                    HttpsEnable = Convert.ToBoolean(r[3]),
                    SecurityEnable = Convert.ToBoolean(r[4]),
                })
                .SingleOrDefault();
        }

        public TenantQuota SaveTenantQuota(TenantQuota quota)
        {
            if (quota == null) throw new ArgumentNullException("quota");

            var i = Insert(tenants_quota, quota.Tenant)
                .InColumnValue("max_file_size", quota.MaxFileSize)
                .InColumnValue("max_total_size", quota.MaxTotalSize)
                .InColumnValue("https_enable", quota.HttpsEnable)
                .InColumnValue("security_enable", quota.SecurityEnable);

            ExecNonQuery(i);
            return quota;
        }

        public void RemoveTenantQuota(int tenant)
        {
            var d = Delete(tenants_quota, tenant);
            ExecNonQuery(d);
        }


        public void SetTenantQuotaRow(TenantQuotaRow row, bool exchange)
        {
            if (row == null) throw new ArgumentNullException("row");

            ExecAction(db =>
            {
                var counter = db.ExecScalar<long>(Query(tenants_quotarow, row.Tenant)
                    .Select("counter")
                    .Where("path", row.Path));

                db.ExecNonQuery(Insert(tenants_quotarow, row.Tenant)
                    .InColumnValue("path", row.Path)
                    .InColumnValue("counter", exchange ? counter + row.Counter : row.Counter)
                    .InColumnValue("tag", row.Tag)
                    .InColumnValue("last_modified", DateTime.UtcNow));
            });
        }

        public IEnumerable<TenantQuotaRow> FindTenantQuotaRows(TenantQuotaRowQuery query)
        {
            if (query == null) throw new ArgumentNullException("query");

            var q = new SqlQuery(tenants_quotarow).Select("tenant", "path", "counter", "tag");
            if (query.Tenant != Tenant.DEFAULT_TENANT)
            {
                q.Where("tenant", query.Tenant);
            }
            if (!string.IsNullOrEmpty(query.Path))
            {
                q.Where("path", query.Path);
            }
            if (query.LastModified != default(DateTime))
            {
                q.Where(Exp.Ge("last_modified", query.LastModified));
            }

            return ExecList(q)
                .ConvertAll(r => new TenantQuotaRow
                {
                    Tenant = Convert.ToInt32(r[0]),
                    Path = (string)r[1],
                    Counter = Convert.ToInt64(r[2]),
                    Tag = (string)r[3],
                });
        }
    }
}
