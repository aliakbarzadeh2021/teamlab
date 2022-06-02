﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASC.Core.Tenants;

namespace ASC.Core.Caching
{
    public class CachedQuotaService : IQuotaService
    {
        private const string KEY_QUOTA_ROWS = "quotarows";
        private readonly IQuotaService service;
        private readonly ICache cache;
        private readonly TrustInterval interval;
        private int syncQuotaRows;


        public TimeSpan CacheExpiration
        {
            get;
            set;
        }


        public CachedQuotaService(IQuotaService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this.service = service;
            this.cache = new AspCache();
            this.interval = new TrustInterval();
            this.syncQuotaRows = 0;

            CacheExpiration = TimeSpan.FromMinutes(10);
        }


        public TenantQuota GetTenantQuota(int tenant)
        {
            var key = "quota/" + tenant;
            var q = cache.Get(key) as TenantQuota;
            if (q == null)
            {
                q = service.GetTenantQuota(tenant) ?? (tenant != Tenant.DEFAULT_TENANT ? GetTenantQuota(Tenant.DEFAULT_TENANT) : TenantQuota.Default);
                cache.Insert(key, q, DateTime.UtcNow.Add(CacheExpiration));
            }
            return q;
        }

        public TenantQuota SaveTenantQuota(TenantQuota quota)
        {
            quota = service.SaveTenantQuota(quota);
            var key = "quota/" + quota.Tenant;
            cache.Insert(key, quota, DateTime.UtcNow.Add(CacheExpiration)); 
            return quota;
        }

        public void RemoveTenantQuota(int tenant)
        {
            service.RemoveTenantQuota(tenant);
            cache.Remove("quota/" + tenant);
        }


        public void SetTenantQuotaRow(TenantQuotaRow row, bool exchange)
        {
            service.SetTenantQuotaRow(row, exchange);
            interval.Expire();
        }

        public IEnumerable<TenantQuotaRow> FindTenantQuotaRows(TenantQuotaRowQuery query)
        {
            if (query == null) throw new ArgumentNullException("query");

            if (Interlocked.CompareExchange(ref syncQuotaRows, 1, 0) == 0)
            {
                try
                {
                    var rows = cache.Get(KEY_QUOTA_ROWS) as Dictionary<string, List<TenantQuotaRow>>;
                    if (rows == null || interval.Expired)
                    {
                        var date = rows != null ? interval.StartTime : DateTime.MinValue;
                        interval.Start(CacheExpiration);

                        var changes = service.FindTenantQuotaRows(new TenantQuotaRowQuery(Tenant.DEFAULT_TENANT).WithLastModified(date))
                            .GroupBy(r => r.Tenant.ToString())
                            .ToDictionary(g => g.Key, g => g.ToList());

                        // merge changes from db to cache
                        if (rows == null)
                        {
                            rows = changes;
                        }
                        else
                        {
                            foreach (var p in changes)
                            {
                                if (rows.ContainsKey(p.Key))
                                {
                                    var cachedRows = rows[p.Key];
                                    foreach (var r in p.Value)
                                    {
                                        cachedRows.RemoveAll(c => c.Path == r.Path);
                                        cachedRows.Add(r);
                                    }
                                }
                                else
                                {
                                    rows[p.Key] = p.Value;
                                }
                            }
                        }

                        cache.Insert(KEY_QUOTA_ROWS, rows, CacheExpiration);
                    }
                }
                finally
                {
                    syncQuotaRows = 0;
                }
            }
            var quotaRows = cache.Get(KEY_QUOTA_ROWS) as IDictionary<string, List<TenantQuotaRow>>;
            if (quotaRows == null) return new TenantQuotaRow[0];

            lock (quotaRows)
            {
                var list = quotaRows.ContainsKey(query.Tenant.ToString()) ?
                    quotaRows[query.Tenant.ToString()] :
                    new List<TenantQuotaRow>();

                if (query != null && !string.IsNullOrEmpty(query.Path))
                {
                    return list.Where(r => query.Path == r.Path);
                }
                return list.ToList();
            }
        }
    }
}
