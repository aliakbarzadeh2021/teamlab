using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using ASC.Core;
using System.Configuration;

namespace ASC.Web.Core.Utility
{
    public static class WebLog
    {
        private class WebLogFileInfo : IDisposable
        {
            internal Stream FileStream { get; set; }
            internal DateTime CreationDate { get; set; }

            public void Dispose()
            {
                if (FileStream != null)
                {
                    FileStream.Dispose();
                }
            }
        }

        private const string CacheKeyConst = "web_app_log";

        private static string CacheKey
        {
            get { return string.Format("{0}-{1}", CacheKeyConst, CoreContext.TenantManager.GetCurrentTenant().TenantId); }
        }

        private static Stream LogStream
        {
            get
            {
                var stream = HttpRuntime.Cache.Get(CacheKey) as WebLogFileInfo;
                if (stream == null)
                {
                    //Create new
                    stream = GetStream();
                }
                else
                {
                    if (DateTime.UtcNow.Day != stream.CreationDate.Day)
                    {
                        //Create new 
                        stream = GetStream();
                    }
                }
                return stream.FileStream;
            }
        }

        private static WebLogFileInfo GetStream()
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                      Path.Combine(System.Configuration.ConfigurationManager.AppSettings["weblog.folder"],
                                                   CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString()));
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            var stream = new WebLogFileInfo()
            {
                CreationDate = DateTime.UtcNow,
                FileStream = new FileStream(string.Format("{0}\\weblog_{1}_{2}.txt", logDir, CoreContext.TenantManager.GetCurrentTenant().TenantId, DateTime.UtcNow.ToString("yyyy-MM-dd")), FileMode.Append, FileAccess.Write, FileShare.ReadWrite)
            };
            //Putin
            HttpRuntime.Cache.Insert(CacheKey, stream, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
            return stream;
        }


        public static void WriteLog(string format, params object[] args)
        {
            if (bool.TrueString.Equals(ConfigurationManager.AppSettings["weblog.enable"], StringComparison.OrdinalIgnoreCase))
            {
                string writeString = string.Format("{0};{1};{2}{3}",
                    DateTime.UtcNow.ToString("HH:mm:ss"),
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    string.Format(format, args),
                    Environment.NewLine);
                var writeBuffer = Encoding.UTF8.GetBytes(writeString);
                lock (LogStream)
                {
                    LogStream.Write(writeBuffer, 0, writeBuffer.Length);
                    LogStream.Flush();
                }
            }
        }

    }
}