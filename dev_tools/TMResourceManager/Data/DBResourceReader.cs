using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Resources;
using System.Globalization;
using System.Collections;
using System.Configuration;
using System.IO;

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Dialects;
using ASC.Common.Data.Sql.Expressions;

namespace TMResourceData
{
    public class DBResourceReader : IResourceReader
    {
        string FileName = "";
        string Language = "";

        public DBResourceReader(string _fileName, CultureInfo _culture)
        {
            this.FileName = _fileName;
            Language = _culture.Name;

            if (Language == "")
                Language = "Neutral"; 
            
            if (!DbRegistry.IsDatabaseRegistered("tmresource"))
            {
                DbRegistry.RegisterDatabase("tmresource", ConfigurationManager.ConnectionStrings["tmresource"]);
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            Hashtable _dict = new Hashtable();

            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.Select(new string[] { "res_data.title", "textValue" });
                _query.LeftOuterJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"));
                _query.LeftOuterJoin("res_cultures", Exp.EqColumns("res_cultures.id", "res_data.cultureID"));
                _query.Where("ResName", FileName).Where("res_cultures.Title", Language);

                List<object[]> _list = _dbManager.ExecuteList(_query);

                for (int i = 0; i < _list.Count; i++)
                {
                    _dict.Add(_list[i][0], _list[i][1]);
                }
            }

            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
        }

        void IResourceReader.Close()
        { }
    }
}
