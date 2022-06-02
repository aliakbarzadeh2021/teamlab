using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Configuration;
using System.IO;

using MySql.Data.MySqlClient;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

using TMResourceData.Model;

namespace TMResourceData
{
    public class ResourceData
    {
        static ResourceData()
        {
            if (!DbRegistry.IsDatabaseRegistered("tmresource"))
            {
                DbRegistry.RegisterDatabase("tmresource", ConfigurationManager.ConnectionStrings["tmresource"]);
            }
        }

        public static void AddCulture(string cultureTitle, string name)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlInsert _sqlInsert = new SqlInsert("res_cultures");
                var _queryInsert = _sqlInsert.InColumnValue("title", cultureTitle).InColumnValue("value", name);
                _dbManager.ExecuteNonQuery(_queryInsert);
            }
        }

        public static void AddResource(string cultureTitle, string resType, DateTime date, ModuleWord word, bool isConsole, string authorLogin)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_reserve");
                var _query = _sql.SelectCount().Where("fileID", word.FileID).Where("cultureTitle", cultureTitle).Where("title", word.Title);

                //нет ключа
                if (_dbManager.ExecuteScalar<int>(_query) == 0)
                {
                    //добавляем в основную таблицу
                    SqlInsert _insert = new SqlInsert("res_data", true);
                    var _queryInsert = _insert.InColumns(new string[] { "title", "textvalue", "cultureTitle", "fileID", "resourceType", "timechanges", "flag", "authorLogin" });
                    _queryInsert.Values(new object[] { word.Title, word.ValueFrom, cultureTitle, word.FileID, resType, date, 2, authorLogin });
                    _dbManager.ExecuteNonQuery(_queryInsert);

                    //добавляем в резервную таблицу
                    if (isConsole)
                    {
                        _insert = new SqlInsert("res_reserve");
                        _queryInsert = _insert.InColumns(new string[] { "title", "textvalue", "cultureTitle", "fileID" });
                        _queryInsert.Values(new object[] { word.Title, word.ValueFrom, cultureTitle, word.FileID });
                        _dbManager.ExecuteNonQuery(_queryInsert);
                    }
                }
                else
                {
                    bool isChange = _dbManager.ExecuteScalar<int>(_query.Where("textvalue", word.ValueFrom)) == 0;

                    SqlUpdate update = new SqlUpdate("res_data");
                    //изменилось или не изменилось + сайт
                    if (isChange || (!isChange && !isConsole))
                    {                            
                        //значение изменилось для английского
                        if (cultureTitle == "Neutral")
                        {
                            update.Set("flag", 3).Where("fileID", word.FileID).Where("title", word.Title);
                            _dbManager.ExecuteNonQuery(update);
                        }

                        update = new SqlUpdate("res_data");
                        update.Set("flag", 1).Set("textvalue", word.ValueFrom).Set("timechanges", date).Set("authorLogin", authorLogin)
                                             .Where("fileID", word.FileID).Where("title", word.Title).Where("cultureTitle", cultureTitle);

                        _dbManager.ExecuteNonQuery(update);
                    }
                    else if (!isChange && isConsole)
                    {
                        //значение не изменилось при работе с консолью
                        update = new SqlUpdate("res_data");
                        update.Set("timechanges", date).Where("fileID", word.FileID).Where("title", word.Title).Where("cultureTitle", cultureTitle);

                        _dbManager.ExecuteNonQuery(update);
                    }
                }
            }
        }

        public static void EditEnglish(ModuleWord word)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlUpdate update = new SqlUpdate("res_data");

                update.Set("textvalue", word.ValueFrom).Where("fileID", word.FileID).Where("title", word.Title).Where("cultureTitle", "Neutral");

                _dbManager.ExecuteNonQuery(update);

            }
        }

        public static void AddComment(ModuleWord word)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlUpdate _sqlUpdate = new SqlUpdate("res_data");
                var _queryUpdate = _sqlUpdate.Set("description", word.TextComment).Where("title", word.Title).Where("fileID", word.FileID).Where("cultureTitle", "Neutral");
                _dbManager.ExecuteNonQuery(_sqlUpdate);
            }
        }

        public static int AddFile(string fileName, string projectName, string moduleName)
        {
            string _fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (_fileNameWithoutExtension.Split('.').Length > 1)
            {
                fileName = _fileNameWithoutExtension.Split('.')[0] + ".resx";
            }

            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_files");
                var _query = _sql.SelectCount().Where("resName", fileName).Where("projectName", projectName).Where("moduleName", moduleName);

                if (Convert.ToInt32(_dbManager.ExecuteScalar(_query)) == 0)
                {
                    SqlInsert _insert = new SqlInsert("res_files", true);
                    var _queryInsert = _insert.InColumns(new string[] { "resName", "projectName", "moduleName" });
                    _queryInsert.Values(new string[] { fileName, projectName, moduleName });

                    _dbManager.ExecuteNonQuery(_queryInsert);
                }

                _sql = new SqlQuery("res_files");
                _query = _sql.Select(new string[] { "id" }).Where("resName", fileName).Where("projectName", projectName).Where("moduleName", moduleName); 
                return _dbManager.ExecuteScalar<int>(_query);
            }
        }

        public static void UpdateDeletedData(int fileID, DateTime date)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlUpdate _sqlUpdate = new SqlUpdate("res_data");
                var _queryUpdate = _sqlUpdate.Set("flag", 4).Where("flag", 1).Where(Exp.Lt("TimeChanges", date)).Where("fileID", fileID);
                _dbManager.ExecuteNonQuery(_sqlUpdate);
            }
        }
    }
}