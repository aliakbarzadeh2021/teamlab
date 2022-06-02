using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Web.Core.Utility.Settings
{
    public class SettingsManager
    {
        private string dbId;
        private Dictionary<string, ISettings> cache = new Dictionary<string, ISettings>();


        public static SettingsManager Instance
        {
            get;
            private set;
        }


        static SettingsManager()
        {
            Instance = new SettingsManager();
        }

        private SettingsManager()
        {
        }

        public void Configure(string databaseID, ConnectionStringSettings connectionStringSettings)
        {
            dbId = databaseID;
            if (!DbRegistry.IsDatabaseRegistered(databaseID))
            {
                DbRegistry.RegisterDatabase(databaseID, connectionStringSettings);
            }
        }

        public bool SaveSettings<T>(T settings, int tenantID) where T : ISettings
        {
            return SaveSettingsFor<T>(settings, tenantID, Guid.Empty);
        }

        public bool SaveSettingsFor<T>(T settings, Guid userID) where T : ISettings
        {
            return SaveSettingsFor<T>(settings, 0, userID);
        }

        private bool SaveSettingsFor<T>(T settings, int tenantID, Guid userID) where T : ISettings
        {
            if (settings == null) throw new ArgumentNullException("settings");
            try
            {
                using (var db = GetDbManager())
                {
                    var insert = new SqlInsert("webstudio_settings", true)
                        .InColumnValue("id", settings.ID.ToString())
                        .InColumnValue("userid", userID.ToString())
                        .InColumnValue("tenantid", tenantID)
                        .InColumnValue("data", BinarySerializer.Serialize(settings));
                    db.ExecuteNonQuery(insert);
                }
                ToCache(tenantID, userID, settings);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public T LoadSettings<T>(int tenantID) where T : ISettings
        {
            return LoadSettingsFor<T>(tenantID, Guid.Empty);
        }

        public T LoadSettingsFor<T>(Guid userID) where T : ISettings
        {
            return LoadSettingsFor<T>(0, userID);
        }

        private T LoadSettingsFor<T>(int tenantID, Guid userID) where T : ISettings
        {
            var settings = (ISettings)Activator.CreateInstance<T>().GetDefault();
            if (settings == null) throw new InvalidOperationException(string.Format("Default settings of type '{0}' can not be null.", typeof(T)));

            var cacheSettings = FromCache(settings.ID, tenantID, userID);
            if (cacheSettings != null)
            {
                return (T)cacheSettings;
            }

            try
            {
                byte[] data = null;
                var q = new SqlQuery("webstudio_settings")
                    .Select("data")
                    .Where("id", settings.ID.ToString())
                    .Where("tenantid", tenantID)
                    .Where("userid", userID.ToString());
                using (var db = GetDbManager())
                {
                    data = db.ExecuteScalar<byte[]>(q);
                }
                if (data != null)
                {
                    settings = (ISettings)BinarySerializer.Deserialize(data);
                }
                ToCache(tenantID, userID, settings);
            }
            catch
            {
                ToCache(tenantID, userID, settings);
            }
            return (T)settings;
        }


        private void ToCache(int tenantID, Guid userID, ISettings settings)
        {
            lock (cache)
            {
                cache[string.Format("{0}|{1}|{2}", settings.ID, tenantID, userID)] = settings;
            }
        }

        private ISettings FromCache(Guid settingsID, int tenantID, Guid userID)
        {
            var key = string.Format("{0}|{1}|{2}", settingsID, tenantID, userID);
            lock (cache)
            {
                return cache.ContainsKey(key) ? cache[key] : null;
            }
        }

        private DbManager GetDbManager()
        {
            return DbManager.FromHttpContext(dbId);
        }


        class BinarySerializer
        {
            private static readonly BinarySerializer instance = new BinarySerializer();
            private static readonly BinaryFormatter formatter = new BinaryFormatter();


            public static byte[] Serialize(object obj)
            {
                var ms = new MemoryStream();
                Serialize(obj, ms);
                var data = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(data, 0, data.Length);
                return data;
            }

            public static void Serialize(object obj, Stream stream)
            {
                if (obj == null) throw new ArgumentNullException("obj");
                if (!stream.CanWrite) throw new ArgumentException("stream");
                formatter.Serialize(stream, obj);
            }

            public static object Deserialize(Stream stream)
            {
                return formatter.Deserialize(stream);
            }

            public static object Deserialize(byte[] data)
            {
                var ms = new MemoryStream(data);
                ms.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(ms);
            }
        }
    }
}
