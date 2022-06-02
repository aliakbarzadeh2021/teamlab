using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Web.Studio.Controls.Dashboard.Dao
{
    static class WidgetManager
    {
        private static string databaseID;

        private static DbManager dbManager
        {
            get { return DbManager.FromHttpContext(databaseID); }
        }


        public static void Configure(string dbId, ConnectionStringSettings connectionStringSettings)
        {
            databaseID = dbId;
            if (!DbRegistry.IsDatabaseRegistered(databaseID))
            {
                DbRegistry.RegisterDatabase(databaseID, connectionStringSettings);
            }
        }


        public static WidgetContainer GetWidgetContainer(Guid containerID, int tenantID, Guid UserID)
        {
            lock (ContainersCache)
            {
                return ContainersCache.Values
                    .ToList()
                    .Find(c => c.ContainerID == containerID && c.TenantID == tenantID && c.UserID == UserID);
            }
        }

        public static WidgetContainer GetWidgetContainer(Guid id)
        {
            lock (ContainersCache)
            {
                return ContainersCache.ContainsKey(id) ? ContainersCache[id] : null;
            }
        }



        

        public static bool SaveWidgetContainer(WidgetContainer container)
        {
            var columnsCount = GetColumnSchemaPercents(container.ColumnSchemaType).Length;
            foreach (var s in container.States)
            {
                if (columnsCount <= s.X) s.X = 0;
            }
            try
            {
                using (var tr = dbManager.Connection.BeginTransaction())
                {
                    dbManager.ExecuteNonQuery(new SqlDelete("webstudio_widgetstate").Where("WidgetContainerID", container.ID.ToString()));
                    dbManager.ExecuteNonQuery(
                        new SqlInsert("webstudio_widgetcontainer", true)
                        .InColumnValue("ID", container.ID.ToString())
                        .InColumnValue("ContainerID", container.ContainerID.ToString())
                        .InColumnValue("UserID", container.UserID.ToString())
                        .InColumnValue("TenantID", container.TenantID)
                        .InColumnValue("SchemaID", (int)container.ColumnSchemaType));
                    foreach (var s in container.States)
                    {
                        dbManager.ExecuteNonQuery(
                            new SqlInsert("webstudio_widgetstate")
                            .InColumnValue("WidgetID", s.ID.ToString())
                            .InColumnValue("WidgetContainerID", s.ContainerID.ToString())
                            .InColumnValue("ColumnID", s.X)
                            .InColumnValue("SortOrderInColumn", s.Y));
                    }
                    tr.Commit();
                }
                lock (ContainersCache)
                {
                    ContainersCache[container.ID] = container;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int[] GetColumnSchemaPercents(ColumnSchemaType columnSchemaType)
        {
            switch (columnSchemaType)
            {
                case ColumnSchemaType.Schema_25_50_25: return new[] { 25, 50, 25 };
                case ColumnSchemaType.Schema_33_33_33: return new[] { 33, 34, 33 };
                default: throw new ArgumentOutOfRangeException("columnSchemaType");
            }
        }


        private static IDictionary<Guid, WidgetContainer> containersCache;

        private static IDictionary<Guid, WidgetContainer> ContainersCache
        {
            get
            {
                if (containersCache == null)
                {
                    lock (typeof(WidgetContainer))
                    {
                        if (containersCache == null) LoadContainersCache();
                    }
                }
                return containersCache;
            }
        }

        private static void LoadContainersCache()
        {
            containersCache = new Dictionary<Guid, WidgetContainer>();
            var q = new SqlQuery()
                .From("webstudio_widgetcontainer c")
                .LeftOuterJoin("webstudio_widgetstate s", Exp.EqColumns("c.ID", "s.WidgetContainerID"))
                .Select("c.ID", "c.ContainerID", "c.UserID", "c.TenantID", "c.SchemaID", "s.WidgetID", "s.ColumnID", "s.SortOrderInColumn")
                .OrderBy("c.ID", true);

            foreach (var r in dbManager.ExecuteList(q))
            {
                var id = new Guid((string)r[0]);
                if (!containersCache.ContainsKey(id))
                {
                    containersCache[id] = new WidgetContainer(id, new Guid((string)r[1]))
                    {
                        UserID = new Guid((string)r[2]),
                        TenantID = Convert.ToInt32(r[3]),
                        ColumnSchemaType = (ColumnSchemaType)Convert.ToInt32(r[4]),
                    };
                }
                if (r[5] != null)
                {
                    var s = new WidgetState(new Guid((string)r[5]), id) { X = Convert.ToInt32(r[6]), Y = Convert.ToInt32(r[7]) };
                    containersCache[id].States.Add(s);
                }
            }
        }
    }
}
