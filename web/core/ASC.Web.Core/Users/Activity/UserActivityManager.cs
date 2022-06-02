﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core.Users.Activity
{
    public static class UserActivityManager
    {
        private static string dbid;
        private static readonly List<IUserActivityFilter> filters = new List<IUserActivityFilter>();
        private const int MAX_FETCH_FORWARD = 10;

        private static readonly Cache cache = HttpRuntime.Cache;
        private static readonly TimeSpan expiration = TimeSpan.FromMinutes(10);


        public static void Configure(string databaseId, ConnectionStringSettings connectionString)
        {
            dbid = databaseId;
            if (!DbRegistry.IsDatabaseRegistered(databaseId))
            {
                DbRegistry.RegisterDatabase(databaseId, connectionString);
            }
        }

        public static void AddFilter(IUserActivityFilter f)
        {
            lock (filters)
            {
                if (!filters.Contains(f)) filters.Add(f);
            }
        }

        public static void RemoveFilter(IUserActivityFilter f)
        {
            lock (filters)
            {
                filters.Remove(f);
            }
        }


        public static List<UserActivity> GetUserActivities(int tenant, Guid? userOrDept, Guid product, IEnumerable<Guid> modules, int action, IEnumerable<string> containers, int offset, int count)
        {
            return GetUserActivities(tenant, userOrDept, product, modules, action, containers, DateTime.MinValue, DateTime.MaxValue, offset, count);
        }

        public static List<UserActivity> GetUserActivities(int tenant, Guid? userOrDept, Guid product, IEnumerable<Guid> modules, int action, IEnumerable<string> containers, DateTime from, DateTime to)
        {
            return GetUserActivities(tenant, userOrDept, product, modules, action, containers, from, to, 0, 0);
        }

        private static List<UserActivity> GetUserActivities(int tenant, Guid? userOrDept, Guid product, IEnumerable<Guid> modules, int action, IEnumerable<string> containers, DateTime from, DateTime to, int offset, int count)
        {
            var q = new SqlQuery("webstudio_useractivity")
                .Select("id", "tenantid", "productid", "moduleid", "userid", "contentid", "containerid")
                .Select("actiontype", "actiontext", "businessvalue", "additionaldata", "activitydate")
                .Select("url", "title", "partid", "imagefilename", "htmlpreview", "securityid")
                .Where("tenantid", tenant);

            //userid
            if (userOrDept.HasValue && userOrDept.Value != default(Guid))
            {
                if (CoreContext.UserManager.UserExists(userOrDept.Value))
                {
                    q.Where("userid", userOrDept.Value.ToString());
                }
                else
                {
                    q.Where(Exp.In("userid", CoreContext.UserManager.GetUsersByGroup(userOrDept.Value).Select(u => u.ID.ToString()).ToArray()));
                }
            }

            //productId
            if (product != default(Guid))
            {
                q.Where("productid", product.ToString());
            }

            //moduleIds
            if (modules != null && modules.Any())
            {
                q.Where(Exp.In("moduleid", modules.Select(m => m.ToString()).ToArray()));
            }

            //actionType
            if (action != UserActivityConstants.AllActionType)
            {
                q.Where("actiontype", action);
            }

            //containerIds
            if (containers != null && containers.Any())
            {
                q.Where(Exp.In("containerid", containers.ToArray()));
            }

            //dates
            if (from != DateTime.MinValue)
            {
                q.Where(Exp.Ge("activitydate", TenantUtil.DateTimeToUtc(from).Date));
            }
            if (to != DateTime.MaxValue)
            {
                q.Where(Exp.Le("activitydate", TenantUtil.DateTimeToUtc(to).Date.AddTicks(TimeSpan.TicksPerDay - 1)));
            }

            //limits
            if (0 < offset) q.SetFirstResult(offset);
            if (0 < count) q.SetMaxResults(count);

            q.OrderBy("activitydate", false);

            var key = BuildKey(q);
            var result = cache.Get(key) as List<UserActivity>;
            if (result == null)
            {
                using (var db = GetDbManager())
                {
                    result = GetActivities(db, q, count);

                    lock (cache)
                    {
                        var depkey = BuildDependencyKey(tenant, product);
                        if (cache.Get(depkey) == null)
                        {
                            cache.Insert(depkey, depkey);
                        }
                        cache.Insert(key, result, new CacheDependency(null, new[] { depkey }), DateTime.Now.Add(expiration), Cache.NoSlidingExpiration);
                    }
                }
            }
            return result;
        }


        private static List<UserActivity> GetActivities(DbManager db, SqlQuery q, int max)
        {
            int queried;
            var list = QueryUserActivities(q, db, out queried);

            if (0 < max && list.Count < max && queried == max)
            {
                q.SetMaxResults(max * MAX_FETCH_FORWARD);
                // some items filtered out, query additional
                var from = queried;
                do
                {
                    q.SetFirstResult(from);
                    // query again with cursor moved to last result
                    var additionalList = QueryUserActivities(q, db, out queried);
                    list.AddRange(additionalList);
                    from += queried;// page to next set

                } while (list.Count < max && queried == max * MAX_FETCH_FORWARD && 0 < queried);
            }

            if (0 < max)
            {
                list = list.Take(max).ToList();// force limit
            }
            return list;
        }

        private static List<UserActivity> QueryUserActivities(SqlQuery q, DbManager db, out int queried)
        {
            var result = db.ExecuteList(q).ConvertAll(r => ToUserActivity(r));
            queried = result.Count;
            return result
                .Where(a => filters.All(f => f.FilterActivity(a)))
                .ToList();
        }


        public static IEnumerable<int> GetChangedTenants(DateTime from, DateTime to)
        {
            using (var db = GetDbManager())
            {
                var q = new SqlQuery("webstudio_useractivity").Select("tenantid").Where(Exp.Between("activitydate", from, to)).GroupBy(1);
                return db.ExecuteList(q)
                    .ConvertAll(r => Convert.ToInt32(r[0]));
            }
        }

        public static int SaveUserActivity(UserActivity activity)
        {
            using (var db = GetDbManager())
            {
                var id = db.ExecuteScalar<int>(
                        new SqlInsert("webstudio_useractivity")
                        .InColumnValue("ID", 0)
                        .InColumnValue("TenantID", activity.TenantID)
                        .InColumnValue("ProductID", activity.ProductID.ToString())
                        .InColumnValue("ModuleID", activity.ModuleID.ToString())
                        .InColumnValue("UserID", activity.UserID.ToString())
                        .InColumnValue("ContentID", activity.ContentID)
                        .InColumnValue("ContainerID", activity.ContainerID)
                        .InColumnValue("ActionType", activity.ActionType)
                        .InColumnValue("ActionText", activity.ActionText)
                        .InColumnValue("BusinessValue", activity.BusinessValue)
                        .InColumnValue("AdditionalData", activity.AdditionalData)
                        .InColumnValue("ActivityDate", TenantUtil.DateTimeToUtc(activity.Date))
                        .InColumnValue("URL", activity.URL)
                        .InColumnValue("Title", activity.Title)
                        .InColumnValue("PartID", (activity.ImageOptions != null) ? activity.ImageOptions.PartID : Guid.Empty)
                        .InColumnValue("ImageFileName", (activity.ImageOptions != null) ? activity.ImageOptions.ImageFileName : string.Empty)
                        .InColumnValue("HtmlPreview", activity.HtmlPreview)
                        .InColumnValue("SecurityId", activity.SecurityId)
                        .Identity(0, 0, true)
                    );

                lock (cache)
                {
                    cache.Remove(BuildDependencyKey(activity.TenantID, activity.ProductID));
                    cache.Remove(BuildDependencyKey(activity.TenantID, default(Guid)));
                }
                return id;
            }
        }

        public static void DeleteUserActivity(int id)
        {
            using (var db = GetDbManager())
            {
                db.ExecuteNonQuery(new SqlDelete("webstudio_useractivity").Where("id", id));
            }
        }


        private static DbManager GetDbManager()
        {
            return DbManager.FromHttpContext(dbid);
        }

        private static UserActivity ToUserActivity(object[] r)
        {
            return new UserActivity
            {
                ID = Convert.ToInt64(r[0]),
                TenantID = Convert.ToInt32(r[1]),
                ProductID = new Guid(Convert.ToString(r[2])),
                ModuleID = new Guid(Convert.ToString(r[3])),
                UserID = new Guid(Convert.ToString(r[4])),
                ContentID = Convert.ToString(r[5]),
                ContainerID = Convert.ToString(r[6]),
                ActionType = Convert.ToInt32(r[7]),
                ActionText = Convert.ToString(r[8]),
                BusinessValue = Convert.ToInt32(r[9]),
                AdditionalData = Convert.ToString(r[10]),
                Date = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[11])),
                URL = Convert.ToString(r[12]),
                Title = Convert.ToString(r[13]),
                ImageOptions = new ImageOptions()
                {
                    PartID = String.IsNullOrEmpty(Convert.ToString(r[14])) ? Guid.Empty : new Guid(Convert.ToString(r[14])),
                    ImageFileName = Convert.ToString(r[15])
                },
                HtmlPreview = Convert.ToString(r[16]),
                SecurityId = Convert.ToString(r[17])
            };
        }

        private static string BuildKey(SqlQuery q)
        {
            var s = new StringBuilder(q.ToString());
            foreach (var p in q.GetParameters())
            {
                if (p is IEnumerable)
                {
                    foreach (var v in (IEnumerable)p)
                    {
                        s.Append(v);
                    }
                }
                else
                {
                    s.Append(p);
                }
            }
            return Hasher.Base64Hash(s.ToString());
        }

        private static string BuildDependencyKey(int tenant, Guid product)
        {
            return string.Format("uadepkey{0}{1}", tenant, product);
        }
    }
}