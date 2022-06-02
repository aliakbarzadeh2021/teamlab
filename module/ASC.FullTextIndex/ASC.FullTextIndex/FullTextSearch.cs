using System;
using System.Configuration;
using System.ServiceModel;
using ASC.Core;
using log4net;

namespace ASC.FullTextIndex
{
    public static class FullTextSearch
    {
        public static readonly string BlogsModule = "Blogs";
        public static readonly string NewsModule = "News";
        public static readonly string PhotosModule = "Photos";
        public static readonly string BookmarksModule = "Bookmarks";
        public static readonly string WikiModule = "Wiki";
        public static readonly string ForumModule = "Forum";
        public static readonly string ProjectsModule = "Projects";
        public static readonly string FileModule = "Files";
        public static readonly string[] AllModules = new[] 
		{ 
			BlogsModule, 
			NewsModule, 
			PhotosModule, 
			BookmarksModule, 
			ForumModule, 
			ProjectsModule,
			WikiModule,
            FileModule
		};


        private static readonly ILog log = LogManager.GetLogger(typeof(FullTextSearch));

        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(1);

        private static DateTime lastErrorTime = default(DateTime);

        private static bool IsServiceProbablyNotAvailable()
        {
            bool enabled;
            if (!bool.TryParse(ConfigurationManager.AppSettings["fullTextSearch"], out enabled))
            {
                enabled = false;
            }
            return !enabled || (lastErrorTime != default(DateTime) && lastErrorTime + timeout > DateTime.Now);
        }


        public static bool SupportModule(string module)
        {
            var result = false;
            if (IsServiceProbablyNotAvailable()) return result;

            using (var service = new TextIndexServiceClient())
            {
                try
                {
                    result = service.SupportModule(module);
                }
                catch (FaultException fe)
                {
                    LogError(fe);
                }
                catch (CommunicationException ce)
                {
                    LogError(ce);
                    lastErrorTime = DateTime.Now;
                }
                catch (TimeoutException te)
                {
                    LogError(te);
                    lastErrorTime = DateTime.Now;
                }
            }
            return result;
        }

        public static TextSearchResult Search(string query, string module)
        {
            return Search(query, module, CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        public static TextSearchResult Search(string query, string module, int tenantId)
        {
            var result = new TextSearchResult(module);
            if (IsServiceProbablyNotAvailable()) return result;

            using (var service = new TextIndexServiceClient())
            {
                try
                {
                    result = service.Search(tenantId, query, module);
                }
                catch (FaultException fe)
                {
                    LogError(fe);
                }
                catch (CommunicationException ce)
                {
                    LogError(ce);
                    lastErrorTime = DateTime.Now;
                }
                catch (TimeoutException te)
                {
                    LogError(te);
                    lastErrorTime = DateTime.Now;
                }
            }
            return result;
        }

        private static void LogError(Exception error)
        {
            log.Error(error);
        }


        private class TextIndexServiceClient : ClientBase<ITextIndexService>, ITextIndexService, IDisposable
        {
            public bool SupportModule(string module)
            {
                return Channel.SupportModule(module);
            }

            public TextSearchResult Search(int tenant, string query, string module)
            {
                return Channel.Search(tenant, query, module);
            }

            void IDisposable.Dispose()
            {
                try
                {
                    Close();
                }
                catch (CommunicationException)
                {
                    Abort();
                }
                catch (TimeoutException)
                {
                    Abort();
                }
                catch (Exception)
                {
                    Abort();
                    throw;
                }
            }
        }
    }
}
