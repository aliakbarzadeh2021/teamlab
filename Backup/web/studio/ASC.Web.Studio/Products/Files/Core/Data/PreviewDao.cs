using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Files.Core.Data
{
    class PreviewDao : AbstractDao, IPreviewDao
    {
        //private const string GetFilesForConvertQuery = "SELECT f.id,f.version,f.tenant_id,fp.failed FROM files_file f LEFT JOIN files_filepreview fp ON f.id=fp.id AND f.version=fp.version AND (fp.converted=0 OR fp.processing=0) WHERE (fp.id IS NULL AND fp.version IS NULL) OR (fp.failed < 3  AND fp.failed > 0) AND (UTC_TIMESTAMP()-fp.process_timestamp) > (300 * fp.failed) ORDER BY fp.process_timestamp ASC,f.create_on DESC";
        private const string GetFilesForConvertQuery =
            "SELECT f.id,f.version,f.tenant_id,fp.failed FROM files_file f LEFT JOIN files_filepreview fp ON f.id=fp.id AND f.version=fp.version AND (fp.converted=0 OR fp.processing=0) WHERE (fp.id IS NULL AND fp.version IS NULL AND f.version=(SELECT max(fmax.version) FROM files_file fmax WHERE fmax.id=f.id)) OR (fp.failed < 3  AND fp.failed > 0) AND (UTC_TIMESTAMP()-fp.process_timestamp) > (300 * fp.failed) ORDER BY fp.process_timestamp ASC,f.create_on DESC";//This is new version with only ltest versions.
        private const string GetFileForConvertQuery = GetFilesForConvertQuery + " LIMIT 1";

        private const string StatsQueueQuery =
            @"SELECT count(*) FROM files_file f LEFT JOIN files_filepreview fp ON f.id=fp.id AND f.version=fp.version AND (fp.converted=0 OR fp.processing=0) WHERE (fp.id IS NULL AND fp.version IS NULL AND f.version=(SELECT max(fmax.version) FROM files_file fmax WHERE fmax.id=f.id)) OR (fp.failed < 3  AND fp.failed > 0)";
        private const string StatsConvertingQuery =  @"SELECT count(*) FROM files_filepreview WHERE processing=1";

        public PreviewDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {
        }

        private void SetProcessing(string servicename, PreviewFile file, bool procesing, bool completed, bool failed, string status, long perf)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                SetProcessingInternal(file, servicename, procesing, completed, failed, status,perf);
                tx.Commit();
            }
        }

        public void SetFileCompleted(PreviewFile file, string servicename, string status, long tookTimeMs)
        {
            SetProcessing(servicename, file, false, true, false, status,tookTimeMs);
        }

        public void SetFileError(PreviewFile file, string servicename, string status)
        {
            SetProcessing(servicename, file, false, false, true, status,0);
        }

        public void SetFileFatalError(PreviewFile file, string servicename, string status)
        {
            SetProcessingInternal(file, servicename, status, false, false, 1000,0);//Failed forever
        }

        public void CollectStats(int intervalInMinutes, DateTime lastCollection)
        {
            var queued = DbManager.ExecuteScalar<long>(StatsQueueQuery);
            var converting = DbManager.ExecuteScalar<long>(StatsConvertingQuery);
            var errors = DbManager.ExecuteScalar(new SqlQuery("files_filepreview").SelectCount().Where(Exp.Ge("failed",1)).Where(Exp.Ge("process_timestamp",lastCollection)));
            var avgConversion = DbManager.ExecuteScalar<double>(new SqlQuery("files_filepreview").Select("sum(perf)/count(*)").Where(Exp.Ge("perf", 0)).Where(Exp.Ge("process_timestamp", lastCollection)));
            var converted = DbManager.ExecuteScalar(new SqlQuery("files_filepreview").SelectCount().Where("converted", true).Where(Exp.Ge("process_timestamp", lastCollection)));
            var nowDate = DateTime.UtcNow;
            var nowMinute = ((int)nowDate.Minute/(int)intervalInMinutes)*intervalInMinutes;
            var time = new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, nowDate.Hour, nowMinute, 0,
                                    DateTimeKind.Utc);
            //Insert
            DbManager.ExecuteNonQuery(
                new SqlInsert("files_filepreview_stat", true).InColumnValue("time", time).InColumnValue("queued", queued)
                    .InColumnValue("converting", converting).InColumnValue("errors", errors??0).InColumnValue("perf", avgConversion).InColumnValue("converted",converted??0));
        }

        public int ClearProcessing(string servicename)
        {
            return DbManager.ExecuteNonQuery(
                new SqlUpdate("files_filepreview").Where("processing", true).Where("service", servicename).Set(
                    "processing", false));
        }

        private void SetProcessingInternal(PreviewFile file, string servicename, bool procesing, bool completed, bool failed, string status, long perf)
        {
            if (!failed)
            {
                SetProcessingInternal(file, servicename, status, procesing, completed, 0,perf);
            }
            else
            {

                var failedCount =
                    DbManager.ExecuteScalar<int>(
                        new SqlQuery("files_filepreview").Select("failed").Where("id", file.Id).Where("version",
                                                                                                      file.Version));
                SetProcessingInternal(file, servicename, status, procesing, completed, failedCount + 1,perf);

            }
        }

        private void SetProcessingInternal(PreviewFile file, string servicename, string status, bool procesing, bool completed, int errorcount, long perf)
        {
            DbManager.ExecuteNonQuery(new SqlInsert("files_filepreview", true)
                                          .InColumns("id", "version", "tenant_id", "created_on", "process_timestamp",
                                                     "service", "status", "processing", "converted", "failed","perf")
                                          .Values(file.Id, file.Version, file.TenantId, DateTime.UtcNow,
                                                  DateTime.UtcNow, servicename, status, procesing, completed, errorcount,perf)
                );
        }

        public PreviewFile GetNextFileForProcessing(string servicename)
        {
            //Warning! Huge query ahead
            using (var tx = DbManager.BeginTransaction())
            {
                var file = DbManager.ExecuteList(GetFileForConvertQuery)
                    .ConvertAll(x => ConvertPreviewFile(x)).FirstOrDefault();
                if (file != null)
                {
                    //Set processing. don't touch other fields
                    SetProcessingInternal(file, servicename, "processing", true, false, file.Failed,0);
                }
                tx.Commit();
                return file;
            }
        }



        private Dictionary<string, object> TimeParam()
        {
            return new Dictionary<string, object>() { { "time", DateTime.UtcNow } };
        }

        public PreviewFile GetNextFileWithoutPreview()
        {
            //Warning! Huge query ahead
            return DbManager.ExecuteList(GetFileForConvertQuery).ConvertAll(x => ConvertPreviewFile(x)).FirstOrDefault();
        }

        public List<PreviewFile> GetAllFilesWithoutPreview()
        {
            return DbManager.ExecuteList(GetFilesForConvertQuery).ConvertAll(x => ConvertPreviewFile(x));
        }

        private PreviewFile ConvertPreviewFile(object[] input)
        {
            return new PreviewFile()
                       {
                           Id = Convert.ToInt64(input[0]),
                           Version = Convert.ToInt64(input[1]),
                           TenantId = Convert.ToInt64(input[2]),
                           Failed = Convert.ToInt32(input[3]),
                       };
        }

        public string GetBaseUrl(string pathPrefix, PreviewType type)
        {
            if (pathPrefix == null) throw new ArgumentNullException("pathPrefix");
            return string.Format("{0}/{1}", Path.GetDirectoryName(pathPrefix).Replace("\\", "/"), type);
        }

        public string GeFlashPreviewUrl(string pathPrefix)
        {
            return GetBaseUrl(pathPrefix, PreviewType.Swf) + "/source.xml";
        }

        public string GePreviewHtmlUrl(string pathPrefix)
        {
            return GetBaseUrl(pathPrefix, PreviewType.Html) + "/text.html";
        }

        public string GeImagePreviewUrl(string pathPrefix)
        {
            return GetBaseUrl(pathPrefix, PreviewType.Png) + "/0.png";
        }

        public string GeImageThumbUrl(string pathPrefix, int width, int height)
        {
            return GetBaseUrl(pathPrefix, PreviewType.Png) + string.Format("/0_{0}x{1}.png", width, height);
        }

        public void AddTextForIndex(long fileid, string text)
        {
            DbManager.ExecuteNonQuery(Insert("files_filetext").InColumns("id", "lastupdate", "body").Values(fileid, DateTime.UtcNow, text));
        }

        public IEnumerable<PreviewStatus> GetStatuses(int tenantId, int[] fileIds, int[] versions)
        {
            var statuses = new List<PreviewStatus>();
            string error = null;
            try
            {
                var q = new SqlQuery("files_filepreview")
                    .Select("converted", "processing", "failed", "status", "id", "version")
                    .Where("tenant_id", tenantId)
                    .Where(Exp.In("id", fileIds));

                statuses = DbManager
                    .ExecuteList(q)
                    .ConvertAll(r => new PreviewStatus()
                    {
                        IsConverted = Convert.ToBoolean(r[0]),
                        IsProcessing = Convert.ToBoolean(r[1]),
                        ErrorCount = Convert.ToInt32(r[2]),
                        Status = (string)r[3],
                        FileId = Convert.ToInt32(r[4]),
                        Version = Convert.ToInt32(r[5]),
                    });
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            var result = new List<PreviewStatus>();
            for (int i = 0; i < fileIds.Length; i++)
            {
                var status = statuses.Find(s => s.FileId == fileIds[i] && s.Version == versions[i]);
                if (status == null) status = new PreviewStatus() { FileId = fileIds[i], Version = versions[i], IsInQueue = true, Status = error };
                if (status.IsConverted && status.Status == "not supported") status.IsConverted = false;
                result.Add(status);
            }

            return result;
        }

        public bool IsConverted(int tenantId, long fileId, long versionId)
        {
            try
            {
                return DbManager.ExecuteScalar<bool>(new SqlQuery("files_filepreview").Where("id", fileId).Where("version", versionId).Where("tenant_id",
                                                                                                        tenantId).Select("converted"));
            }
            catch (Exception)
            {
                return false;
            }
        }

    
        public void TrackUserActivity(int activityType)
        {
            var isExistRecord = 0 < DbManager.ExecuteScalar<int>(Query("stat_plugin_info").SelectCount());
            if (!isExistRecord)
            {
                DbManager.ExecuteNonQuery(Insert("stat_plugin_info")
                    .InColumnValue("plugin_install_count", 0)
                    .InColumnValue("run_oo_count", 0));
            }
            if (activityType == 1)
            {
                DbManager.ExecuteNonQuery(Update("stat_plugin_info").Set("plugin_install_count = plugin_install_count + 1"));
            }
            if (activityType == 2)
            {
                DbManager.ExecuteNonQuery(Update("stat_plugin_info").Set("run_oo_count = run_oo_count + 1"));
            }
        }
    }
}
