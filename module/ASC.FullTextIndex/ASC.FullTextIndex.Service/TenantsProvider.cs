using System;
using System.Collections.Generic;
using System.Configuration;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.FullTextIndex.Service
{
    class TenantsProvider
    {
        private readonly string dbid;
        private DateTime last;


        public TenantsProvider(ConnectionStringSettings connectionString)
        {
            dbid = connectionString.Name;
            if (!DbRegistry.IsDatabaseRegistered(dbid)) DbRegistry.RegisterDatabase(dbid, connectionString);
        }

        public List<Tenant> GetTenants()
        {
            var result = new List<Tenant>();
            var tenants = CoreContext.TenantManager.GetTenants();

            if (last != DateTime.MinValue)
            {
                using (var db = new DbManager(dbid))
                {
                    var ids = db
                        .ExecuteList(new SqlQuery("webstudio_useractivity").Select("tenantid").Where(Exp.Ge("ActivityDate", last.AddMinutes(-1))).GroupBy(1))
                        .ConvertAll(r => (int)r[0]);
                    result.AddRange(tenants.FindAll(t => ids.Contains(t.TenantId)));

                    ids = db
                        .ExecuteList(new SqlQuery("files_file").Select("tenant_id").Where(Exp.Ge("modified_on", last.AddMinutes(-1))).GroupBy(1))
                        .ConvertAll(r => (int)r[0]);
                    result.AddRange(tenants.FindAll(t => ids.Contains(t.TenantId)));
                }
            }
            else
            {
                result.AddRange(tenants);
            }
            last = DateTime.UtcNow;

            result.RemoveAll(t => t.Status != TenantStatus.Active);
            return result;
        }
    }
}
