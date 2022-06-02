using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ASC.Common.Data;
using ASC.Common.Utils;
using ASC.Core.Tenants;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.RU;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace ASC.FullTextIndex.Service
{
    class TextIndexer : IDisposable
    {
        private readonly Tenant tenant;

        private readonly ModuleInfo module;

        private string lastDateTimeFile;

        private readonly Lucene.Net.Store.Directory directory;

        private readonly IndexModifier indexModifier;


        public TextIndexer(string path, Tenant tenant, ModuleInfo module)
        {
            this.tenant = tenant;
            this.module = module;
            lastDateTimeFile = Path.Combine(path, "last.time");

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            directory = Lucene.Net.Store.FSDirectory.GetDirectory(path, false);

            var create = directory.List().Length == 0;
            indexModifier = new IndexModifier(directory, GetAnalizer(tenant), create);
        }

        public int FindChangedAndIndex()
        {
            var lastDateTime = File.Exists(lastDateTimeFile) ?
                DateTime.Parse(File.ReadAllText(lastDateTimeFile)).ToUniversalTime() :
                DateTime.MinValue;

            var indexes = SelectIndexes(lastDateTime, true);
            lastDateTime = DateTime.UtcNow;
            foreach (var index in indexes)
            {
                indexModifier.DeleteDocuments(new Term("Id", index.Id));
                if (string.IsNullOrEmpty(index.Text)) continue;

                var doc = new Document();
                doc.Add(new Field("Id", index.Id, Field.Store.YES, Field.Index.UN_TOKENIZED, Field.TermVector.YES));
                doc.Add(new Field("Text", index.Text, Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.NO));

                indexModifier.AddDocument(doc);
            }

            File.WriteAllText(lastDateTimeFile, lastDateTime.ToString("o"));

            return indexes.Count;
        }

        public int FindRemovedAndIndex()
        {
            var indexIds = SelectIndexes(DateTime.MinValue, false).ConvertAll(i => i.Id);
            var affected = 0;
            var reader = IndexReader.Open(directory);
            try
            {
                var terms = reader.Terms();
                try
                {
                    while (terms.Next())
                    {
                        var term = terms.Term();
                        if (term.Field() == "Id" && !indexIds.Contains(term.Text()))
                        {
                            reader.DeleteDocuments(term);
                            affected++;
                        }
                    }
                }
                finally
                {
                    terms.Close();
                }
            }
            finally
            {
                reader.Close();
            }
            return affected;
        }

        public void Dispose()
        {
            indexModifier.Optimize();
            indexModifier.Flush();
            indexModifier.Close();
        }


        private List<TextIndexInfo> SelectIndexes(DateTime date, bool withContent)
        {
            if (!DbRegistry.IsDatabaseRegistered(module.Name)) DbRegistry.RegisterDatabase(module.Name, module.ConnectionString);

            using (var db = new DbManager(module.Name))
            {
                return db.Connection
                    .CreateCommand(withContent ? module.Select : GetIdSelect(module.Select))
                    .AddParameter("tenant", tenant.TenantId)
                    .AddParameter("lastModified", date)
                    .ExecuteList()
                    .ConvertAll(r => new TextIndexInfo(r[0].ToString(), withContent ? (string)r[1] : null, 2 < r.Length && r[2] != null ? Convert.ToBoolean(r[2]) : false));
            }
        }

        private Analyzer GetAnalizer(Tenant tenant)
        {
            return tenant.TenantId == 0 || tenant.GetCulture().TwoLetterISOLanguageName == "ru" ?
                (Analyzer)new RussianAnalyzer() :
                (Analyzer)new StandardAnalyzer();
        }

        [DebuggerDisplay("{Id} - {Text}")]
        private class TextIndexInfo
        {
            public string Id
            {
                get;
                private set;
            }

            public string Text
            {
                get;
                private set;
            }

            public TextIndexInfo(string id, string text, bool hasHtml)
            {
                if (id == null) throw new ArgumentNullException("id");

                Id = id;
                Text = hasHtml && !string.IsNullOrEmpty(text) ? HtmlUtil.GetText(text) : text;
            }
        }

        private string GetIdSelect(string sql)
        {
            //cut text columns from sql select
            var select = new StringBuilder();
            var inselect = false;
            var start = -1;
            var brackets = 0;
            for (int i = 0; i < sql.Length; i++)
            {
                var c = sql[i];
                select.Append(c);

                if (c == '(') brackets++;
                if (c == ')') brackets--;

                if (brackets == 0 && select.ToString().EndsWith("select", StringComparison.InvariantCultureIgnoreCase))
                {
                    inselect = true;
                }
                if (inselect && start == -1 && c == ',' && brackets == 0)
                {
                    start = i;
                }
                if (brackets == 0 && inselect && start != -1 && select.ToString().EndsWith("from", StringComparison.InvariantCultureIgnoreCase))
                {
                    select.Remove(select.Length - (i - start + 1), i - start + 1);
                    select.Append(" from");

                    start = -1;
                    inselect = false;
                }
            }
            return select.ToString();
        }
    }
}
