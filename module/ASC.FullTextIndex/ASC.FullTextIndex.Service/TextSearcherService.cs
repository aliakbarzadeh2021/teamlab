﻿using System;
using System.IO;
using System.Linq;
using ASC.Core;
using ASC.FullTextIndex.Service.Cfg;
using log4net;

namespace ASC.FullTextIndex.Service
{
    public class TextSearcherService : ITextIndexService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TextIndexerService));

        private readonly TextIndexCfg configuration;


        public TextSearcherService()
        {
            configuration = new TextIndexCfg();
        }


        public bool SupportModule(string module)
        {
            return configuration.Modules.Any(m => m.Name == module);
        }

        public TextSearchResult Search(int tenantId, string query, string module)
        {
            try
            {
                if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");
                
                var result = new TextSearchResult(module);
                if (string.IsNullOrEmpty(query)) return result;

                var tenant = CoreContext.TenantManager.GetTenant(tenantId);
                var path = configuration.GetIndexPath(tenantId, module);
                if (tenant == null || !Directory.Exists(path)) return result;

                using (var searcher = new TextSearcher(module, path))
                {
                    return searcher.Search(query, tenant);
                }
            }
            catch (Lucene.Net.QueryParsers.ParseException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch (Lucene.Net.Analysis.Standard.ParseException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
