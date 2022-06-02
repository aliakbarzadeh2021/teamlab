using System;
using System.IO;
using System.Web;
using System.Xml;
using ASC.Core;
using ASC.Data.Storage;

namespace ASC.Web.Studio.Services.Backup
{
    public class Uploader
    {
        public static string UploadBackupFile(Stream backupFile, DateTime validTo, Guid userName, int tennantId)
        {
            IDataStore store = GetStorage();
            //Generate backup name
            var fname = MakeFileName(tennantId);
            return store.SavePrivate(string.Empty, fname, backupFile, validTo);
        }

        private static string MakeFileName(int tennantId)
        {
            return string.Format("{2}.teamlab.com-{0}.{1}", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"), "bak", CoreContext.TenantManager.GetTenant(tennantId).TenantAlias).ToLowerInvariant();
        }

        private static IDataStore GetStorage()
        {
            return StorageFactory.GetStorage(string.Empty, "backupfiles", "backup", HttpContext.Current, null);//Get 1 tennant for all and w/o quota!
        }

        public static void CleanOldFiles(TimeSpan oldThreshold)
        {
            GetStorage().DeleteExpired(string.Empty, string.Empty, oldThreshold);
        }
    }
}