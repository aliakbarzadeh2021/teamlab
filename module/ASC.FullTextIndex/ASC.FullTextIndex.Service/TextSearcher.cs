using System;
using System.Collections.Generic;
using System.Text;
using ASC.Core.Tenants;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.RU;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace ASC.FullTextIndex.Service
{
    class TextSearcher : IDisposable
    {
        private readonly string module;

        private readonly IndexSearcher searcher;


        public TextSearcher(string module, string path)
        {
            if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");

            this.module = module;
            searcher = new IndexSearcher(path);
        }

        public TextSearchResult Search(string query, Tenant tenant)
        {
            var result = new TextSearchResult(module);

            if (string.IsNullOrEmpty(query)) return result;
            if (30 < query.Length) query = query.Substring(0, 30);

            var parser = new QueryParser("Text", GetAnalizer(tenant));
            parser.SetDefaultOperator(QueryParser.Operator.AND);
            Query q = null;
            try
            {
                q = parser.Parse(query);
            }
            catch (Lucene.Net.QueryParsers.ParseException) { }
            catch (Lucene.Net.Analysis.Standard.ParseException) { }
            if (q == null)
            {
                q = parser.Parse(QueryParser.Escape(query));
            }

            var hits = searcher.Search(q);
            for (int i = 0; i < hits.Length(); i++)
            {
                var doc = hits.Doc(i);
                result.AddIdentifier(doc.Get("Id"));
            }

            return result;
        }

        public void Dispose()
        {
            searcher.Close();
        }

        private Analyzer GetAnalizer(Tenant tenant)
        {
            if (tenant.TenantId == 0) return new RussianAnalyzer();
            return tenant.TenantId == 0 || tenant.GetCulture().TwoLetterISOLanguageName == "ru" ?
                (Analyzer)new RussianAnalyzer() :
                (Analyzer)new StandardAnalyzer();
        }
    }
}
