using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.IO;
using System.Configuration;

using MySql.Data.MySqlClient;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

using TMResourceData.Model;

namespace TMResourceData
{
    public class GetResource
    {
        static GetResource()
        {
            if (!DbRegistry.IsDatabaseRegistered("tmresource"))
            {
                DbRegistry.RegisterDatabase("tmresource", ConfigurationManager.ConnectionStrings["tmresource"]);
            }
        }

        public static List<ResCulture> GetCultures()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_cultures");
                var _query = _sql.Select(new string[] { "title", "value" }).OrderBy("title", true);

                return _dbManager.ExecuteList(_query).ConvertAll(r => GetCultureFromDB(r)); 
            }
        }

        static ResCulture GetCultureFromDB(object[] r)
        {
            return new ResCulture() { Title = (string)r[0], Value = (string)r[1] };
        }

        public static List<string> GetProjects()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_files");

                var _query = _sql.Select(new string[] { "projectName" }).Where(Exp.Eq("islock", 0)).GroupBy("projectName").OrderBy("id", true);

                return _dbManager.ExecuteList(_query).ConvertAll(r => (string)r[0]); 

            }
        }

        public static List<string> GetAllProject()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_files");

                var _query = _sql.Select(new string[] { "projectName" }).Distinct();

                return _dbManager.ExecuteList(_query).ConvertAll(r => (string)r[0]);

            }
        }

        #region Module

        public static List<ProjectModule> GetModules(string projectName)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_files");

                var _query = _sql.Select(new string[] { "moduleName", "islock" }).Distinct().Where(Exp.Eq("projectName", projectName));

                return _dbManager.ExecuteList(_query).ConvertAll(r => new ProjectModule() { Name = (string)r[0], IsLock = (bool)r[1] });
            }
        }

        public static List<ProjectModule> GetListModuleData(string projectName, string to, bool isLock)
        {
            List<ProjectModule> listModules = GetModules(projectName);

            listModules = listModules.FindAll(r => r.IsLock == isLock);

            foreach (ProjectModule module in listModules)
            {
                GetModuleData(projectName, module, to, "");
            }

            return listModules;
        }

        public static ProjectModule GetModuleData(string projectName, ProjectModule module, string to, string search)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.Select(new string[] { "title", "fileid", "textValue", "description", "flag", "link" })
                                 .LeftOuterJoin("res_files", Exp.EqColumns("res_files.ID", "res_data.fileID"))
                                 .Where("moduleName", module.Name)
                                 .Where("projectName", projectName)
                                 .Where("cultureTitle", "Neutral")
                                 .Where("flag != 4")
                                 .Where("resourceType", "text")
                                 .OrderBy("textValue", true);

                if (!String.IsNullOrEmpty(search))
                    _query.Where(Exp.Like("textvalue", search));

                List<ModuleWord> wordsFrom = _dbManager.ExecuteList(_query).ConvertAll(r => GetWord(r));

                _sql = new SqlQuery("res_data");
                _query = _sql.Select(new string[] { "title", "fileid", "textValue", "description", "flag", "link" })
                             .LeftOuterJoin("res_files", Exp.EqColumns("res_files.ID", "res_data.fileID"))
                             .Where("moduleName", module.Name)
                             .Where("projectName", projectName)
                             .Where("flag != 4")
                             .Where("cultureTitle", to)
                             .Where("resourceType", "text")
                             .OrderBy("textValue", true);

                List<ModuleWord> wordsTo = _dbManager.ExecuteList(_query).ConvertAll(r => GetWord(r));

                foreach (ModuleWord wordFrom in wordsFrom)
                {
                    ModuleWord wordTo = wordsTo.Find(_p => _p.Title == wordFrom.Title && _p.FileID == wordFrom.FileID);

                    if (wordTo != null)
                    {
                        if (wordTo.Flag == 3)
                            module.Changed.Add(wordFrom);
                        else
                            module.Translated.Add(wordFrom);
                    }
                    else
                    {
                        module.UnTranslated.Add(wordFrom);
                    }
                }

                return module;
            }
        }

        public static ModuleWord GetWord(object[] r)
        {
            return new ModuleWord() { Title = (string)r[0], FileID = (int)r[1], ValueFrom = (string)r[2], TextComment = (string)r[3], Flag = (int)r[4], Link = (string)r[5] };
        }

        public static void LockModules(string projectName, string modules)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlUpdate _sqlUpdate = new SqlUpdate("res_files");
                var _queryUpdate = _sqlUpdate.Set("isLock", 1).Where("projectName", projectName).Where(Exp.In("moduleName", modules.Split(',')));
                _dbManager.ExecuteNonQuery(_sqlUpdate);

                _sqlUpdate = new SqlUpdate("res_files");
                _queryUpdate = _sqlUpdate.Set("isLock", 0).Where("projectName", projectName).Where(!Exp.In("moduleName", modules.Split(',')));
                _dbManager.ExecuteNonQuery(_sqlUpdate);
            }
        }

        public static void UnLockModules()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlUpdate _sqlUpdate = new SqlUpdate("res_files");
                var _queryUpdate = _sqlUpdate.Set("isLock", 0);
                _dbManager.ExecuteNonQuery(_sqlUpdate);
            }
        }

        public static void LockModules()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlUpdate _sqlUpdate = new SqlUpdate("res_files");
                var _queryUpdate = _sqlUpdate.Set("isLock", 1);
                _dbManager.ExecuteNonQuery(_sqlUpdate);
            }
        }

        #endregion

        #region Word

        public static void AddLink(string resource, string fileName, string page)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                var _query = new SqlQuery("res_data");
                _query.Select("res_data.id")
                    .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileid"))
                    .Where("res_data.title", resource).Where("res_files.resName", fileName).Where("res_data.cultureTitle", "Neutral");

                int _key = _dbManager.ExecuteScalar<int>(_query);

                var _update = new SqlUpdate("res_data");
                _update.Set("link", page).Where("id", _key);
                _dbManager.ExecuteNonQuery(_update);
            }
        }

        public static ModuleWord GetValueByKey(ModuleWord word, string to)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.Select(new string[] { "textvalue", "description", "link" })
                                 .Where("fileID", word.FileID)
                                 .Where("cultureTitle", "Neutral")
                                 .Where("title", word.Title);

                _dbManager.ExecuteList(_query).ForEach(r => GetValue(word, to, r));

                _sql = new SqlQuery("res_data");
                _query = _sql.Select(new string[] { "textvalue" })
                             .Where("fileID", word.FileID)
                             .Where("cultureTitle", to)
                             .Where("title", word.Title);

                word.ValueTo = (string)_dbManager.ExecuteScalar(_query) ?? "";

                _sql = new SqlQuery("res_data as res1");
                _query = _sql.Select(new string[] { "res1.textvalue" }).Distinct()
                             .InnerJoin("res_data as res2", Exp.And(Exp.EqColumns("res1.title", "res2.title"), Exp.EqColumns("res1.fileid", "res2.fileid")))
                             .Where("res1.cultureTitle", to)
                             .Where("res2.cultureTitle", "Neutral")
                             .Where("res2.textvalue", word.ValueFrom);

                word.Alternative = new List<string>();
                _dbManager.ExecuteList(_query).ForEach(r => word.Alternative.Add((string)r[0]));
                word.Alternative.Remove(word.ValueTo);

                _sql = new SqlQuery("res_files");
                _query = _sql.Select("resname").Where("id", word.FileID);

                word.FileName = (string)_dbManager.ExecuteScalar(_query);

                return word;
            }
        }

        public static void GetValue(ModuleWord word, string to, object[] r)
        {
            word.ValueFrom = (string)r[0] ?? "";
            word.TextComment = (string)r[1] ?? "";
            string link = (string)r[2] ?? "";

            if (!String.IsNullOrEmpty(link))
                word.Link = String.Format("http://{0}-translator.teamlab.info{1}", to, link);
            else
                word.Link = "";
        }

        public static List<ResCulture> GetListLanguages(int fileID, string title)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_cultures");
                var _query = _sql.Select(new string[] { "res_cultures.title", "res_cultures.value" })
                                 .LeftOuterJoin("res_data", Exp.EqColumns("res_cultures.title", "res_data.cultureTitle"))
                                 .Where("res_data.fileID", fileID)
                                 .Where("res_data.title", title);

                List<ResCulture> language = _dbManager.ExecuteList(_query).ConvertAll(r => GetCultureFromDB(r));
                
                language.Remove(language.Find(p => p.Title == "Neutral"));

                return language;
            }
        }

        #endregion

        #region Author

        public static List<ResCulture> GetAuthorLang(string login)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_cultures");

                var _query = _sql.Select(new string[] { "res_cultures.title", "res_cultures.value" })
                    .InnerJoin("res_authorslang", Exp.EqColumns("res_cultures.title", "res_authorslang.cultureTitle"))
                    .InnerJoin("res_authors", Exp.EqColumns("res_authors.login", "res_authorslang.authorLogin"))
                    .Where("res_authors.login", login);

                return _dbManager.ExecuteList(_query).ConvertAll(r => GetCultureFromDB(r));
            }
        }

        public static List<Author> GetListAuthors()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_authors");

                var _query = _sql.Select(new string[] { "login", "password", "isAdmin" });

                return   _dbManager.ExecuteList<Author>(_query);
                /*
                foreach (Author _author in _authors)
                {
                    _sql = new SqlQuery("res_cultures");

                    _query = _sql.Select(new string[] { "res_cultures.title", "res_cultures.value" })
                        .InnerJoin("res_authorslang", Exp.EqColumns("res_cultures.title", "res_authorslang.cultureTitle"))
                        .Where("res_authorslang.authorLogin", _author.Login);

                    _author.Langs = _dbManager.ExecuteList(_query).ConvertAll(r => GetCultureFromDB(r));
                }
                
                return _authors;*/

            }
        }

        public static Author GetAuthor(string login)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_authors");

                var _query = _sql.Select(new string[] {"login", "password", "isAdmin" }).Where("login", login);

                Author _author = _dbManager.ExecuteList(_query).ConvertAll(r => GetAuthorFromDB(r)).FirstOrDefault();

                _sql = new SqlQuery("res_cultures");

                _query = _sql.Select(new string[] { "res_cultures.title", "res_cultures.value" })
                    .InnerJoin("res_authorslang", Exp.EqColumns("res_cultures.title", "res_authorslang.cultureTitle"))
                    .Where("res_authorslang.authorLogin", _author.Login);

                _author.Langs = _dbManager.ExecuteList(_query).ConvertAll(r => GetCultureFromDB(r));

                return _author;

            }
        }
        
        public static Author GetAuthorFromDB(object[] r)
        {
            return new Author() { Login = (string)r[0], Password = (string)r[1], IsAdmin = (bool)r[2] };
        }

        public static Author CreateAuthor(Author author, string[] languages)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlInsert _sqlInsert = new SqlInsert("res_authors", true);
                var _queryInsert = _sqlInsert.InColumnValue("login", author.Login).InColumnValue("password", author.Password).InColumnValue("isAdmin", author.IsAdmin);
                _dbManager.ExecuteNonQuery(_queryInsert);

                SqlDelete delete = new SqlDelete("res_authorslang");
                delete.Where("authorLogin", author.Login);
                _dbManager.ExecuteNonQuery(delete);

                foreach (string lang in languages)
                {
                    _sqlInsert = new SqlInsert("res_authorslang", true);
                    _queryInsert = _sqlInsert.InColumnValue("authorLogin", author.Login).InColumnValue("cultureTitle", lang);
                    _dbManager.ExecuteNonQuery(_queryInsert);
                }
            }

            return author;
        }

        public static void DeleteAuthor(string login)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlDelete _sql = new SqlDelete("res_authors");

                var _query = _sql.Where("login", login);

                _dbManager.ExecuteNonQuery(_query);

            }
        }

        public static bool IsAuthor(Author author)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_authors");

                var _query = _sql.SelectCount().Where("login", author.Login).Where("password", author.Password);

                int count = _dbManager.ExecuteScalar<int>(_query);

                if (count == 0)
                    return false;
                else
                    return true;
            }
        }

        public static bool IsAdmin(Author author)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_authors");

                var _query = _sql.SelectCount().Where("login", author.Login).Where("isadmin", 1);

                int count = _dbManager.ExecuteScalar<int>(_query);

                if (count == 0)
                    return false;
                else
                    return true;
            }
        }

        public static void SetAuthorOnline(string login)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlUpdate _sql = new SqlUpdate("res_authors");

                _sql.Set("lastVisit", DateTime.UtcNow).Where("login", login);

                _dbManager.ExecuteNonQuery(_sql);

            }
        }

        public static List<string> GetOnlineAuthors()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_authors");

                var _query = _sql.Select("login").Where(Exp.Ge("LastVisit", DateTime.UtcNow.AddMinutes(-20)));

                return _dbManager.ExecuteList(_query).ConvertAll(r=> (string) r[0]);
            }
        }

        #endregion

        #region Statistics

        public static List<StatisticModule> GetStatistic()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.SelectCount().Select(new string[] { "res_cultures.value", "res_files.projectName" })
                    .InnerJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileid"))
                    .InnerJoin("res_cultures", Exp.EqColumns("res_data.cultureTitle", "res_cultures.title"))
                    .Where(!Exp.Eq("flag", 4))
                    .Where(!Exp.Eq("flag", 3))
                    .Where("resourceType", "text")
                    .Where("isLock", 0)
                    .GroupBy(new string[] { "value", "projectName" })
                    .OrderBy("value", true)
                    .OrderBy("projectName", true);

                List<StatisticModule> stat = _dbManager.ExecuteList(_query).ConvertAll(r => GetStatisticFromDB(r));
                List<StatisticModule> allStat = new List<StatisticModule>();

                foreach (string project in stat.Select(stat1 => stat1.ProjectName).Distinct())
                {
                    StatisticModule allWords = new StatisticModule() { Culture = "All Words" };
                    allWords.Count = stat.FindAll(r => r.ProjectName == project && r.Culture != "English").Sum(r => r.Count);
                    allWords.ProjectName = project;
                    allWords.DisplayCount = allWords.Count.ToString();

                    StatisticModule englishModule = stat.Find(r => r.Culture == "English" && r.ProjectName == project);

                    foreach (string culture in stat.FindAll(r=> r.Culture != "English").Select(stat1 => stat1.Culture).Distinct())
                    {
                        StatisticModule cultureStat = stat.Find(r=> r.Culture== culture && r.ProjectName == project);
                        if (cultureStat != null)
                            cultureStat.DisplayCount += "(" + Math.Round((double)cultureStat.Count / englishModule.Count, 2) * 100 + "%)";
                    }

                    allStat.Add(allWords);
                }

                stat.AddRange(allStat);

                return stat;
            }

        }

        public static List<StatisticUser> GetUserStatistic()
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data as r1");

                var _query = _sql.SelectCount().Select(new string[] { "res_cultures.title", "r1.authorLogin", "sum(length(r2.textvalue))" })
                    .InnerJoin("res_data as r2", Exp.And(Exp.EqColumns("r1.fileID", "r2.fileID"), Exp.EqColumns("r1.title", "r2.title")))
                    .InnerJoin("res_cultures", Exp.EqColumns("r1.cultureTitle", "res_cultures.title"))
                    .Where(!Exp.Eq("r1.flag", 4))
                    .Where(!Exp.Eq("r1.flag", 3))
                    .Where("r2.cultureTitle", "Neutral")
                    .GroupBy(new string[] { "title", "authorLogin" })
                    .OrderBy("title", true)
                    .OrderBy("authorLogin", true);

                return _dbManager.ExecuteList(_query).ConvertAll(r => GetUserStatisticFromDB(r));
            }

        }

        public static List<StatisticUser> GetUserStatistic(DateTime from, DateTime till)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data as r1");

                var _query = _sql.SelectCount().Select(new string[] { "res_cultures.title", "r1.authorLogin", "sum(length(r2.textvalue))" })
                    .InnerJoin("res_data as r2", Exp.And(Exp.EqColumns("r1.fileID", "r2.fileID"), Exp.EqColumns("r1.title", "r2.title")))
                    .InnerJoin("res_cultures", Exp.EqColumns("r1.cultureTitle", "res_cultures.title"))
                    .Where(!Exp.Eq("r1.flag", 4))
                    .Where(!Exp.Eq("r1.flag", 3))
                    .Where(!Exp.Eq("r1.authorLogin", "Console"))                    
                    .Where(!Exp.Eq("r1.cultureTitle", "Neutral"))
                    .Where(Exp.Ge("r1.timeChanges", from))
                    .Where(Exp.Le("r1.timeChanges", till))
                    .Where("r2.cultureTitle", "Neutral")
                    .GroupBy(new string[] { "title", "authorLogin" })
                    .OrderBy("title", true)
                    .OrderBy("authorLogin", true);

                return _dbManager.ExecuteList(_query).ConvertAll(r => GetUserStatisticFromDB(r));
            }

        }

        public static StatisticModule GetStatisticFromDB(object[] r)
        {
            return new StatisticModule() { Count = Convert.ToInt32(r[0]), DisplayCount = r[0].ToString(), Culture = (string)r[1], ProjectName = (string)r[2] };
        }

        public static StatisticUser GetUserStatisticFromDB(object[] r)
        {
            return new StatisticUser() { WordsCount = Convert.ToInt32(r[0]), Culture = (string)r[1], Login = (string)r[2], SignCount = Convert.ToInt32(r[3]) };
        }

        #endregion

        #region Search

        public static List<ProjectModule> SearchAll(string languageTo, string search)
        {
            List<ProjectModule> listModules = new List<ProjectModule>();

            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.Select(new string[] { "title", "fileid", "textValue", "description", "flag", "link", "moduleName", "projectName" })
                                 .LeftOuterJoin("res_files", Exp.EqColumns("res_files.ID", "res_data.fileID"))
                                 .Where("cultureTitle", languageTo)
                                 .Where("flag != 4")
                                 .Where(Exp.Like("textvalue", search))
                                 .Where("resourceType", "text")
                                 .OrderBy("textValue", true);

                List<ModuleWord> wordsFrom = _dbManager.ExecuteList(_query).ConvertAll(r => GetWord(r));
            }

            return listModules;
        }

        #endregion

        #region DBResourceManager

        public static void UpdateHashTable(ref Hashtable table, DateTime date)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.Select(new string[] { "res_data.textValue", "res_data.title", "res_files.ResName", "res_data.cultureTitle" });
                _query.LeftOuterJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"));
                _query.Where(Exp.Ge("timechanges", date));

                List<object[]> _list = _dbManager.ExecuteList(_query);

                for (int i = 0; i < _list.Count; i++)
                {
                    string _key = _list[i][1].ToString() + _list[i][2].ToString() + _list[i][3].ToString();

                    if (table[_key] != null)
                        table[_key] = _list[i][0];
                    else
                        table.Add(_key, _list[i][0]);
                }
            }
        }

        public static void UpdateDBRS(DBResourceSet databaseResourceSet, string fileName, string culture, DateTime date)
        {
            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.Select(new string[] { "title", "textValue" })
                .LeftOuterJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"))
                .Where("ResName", fileName)
                .Where("cultureTitle", culture)
                .Where(Exp.Ge("timechanges", date));
                //_query.Where(Exp.Eq("resourcetype", "text"));

                List<object[]> _list = _dbManager.ExecuteList(_query);

                for (int i = 0; i < _list.Count; i++)
                {
                    databaseResourceSet.SetString(_list[i][0], _list[i][1]);
                }
            }
        }

        public static DateTime UpdateDate(string fileName, string culture)
        {
            DateTime _date = new DateTime();

            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.SelectMax("timechanges")
                .LeftOuterJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"))
                .Where("ResName", fileName)
                .Where("cultureTitle", culture)
                .Where(Exp.Eq("resourcetype", "text"));

                try { _date = Convert.ToDateTime(_dbManager.ExecuteScalar(_query)); }
                catch { }
            }

            return _date;
        }

        public static Hashtable GetAllData()
        {
            Hashtable _hashTable = new Hashtable();

            using (DbManager _dbManager = new DbManager("tmresource"))
            {
                SqlQuery _sql = new SqlQuery("res_data");

                var _query = _sql.Select(new string[] { "textValue", "title", "ResName", "cultureTitle" })
                                 .LeftOuterJoin("res_files", Exp.EqColumns("res_files.id", "res_data.fileID"));

                List<object[]> _list = _dbManager.ExecuteList(_query);

                for (int i = 0; i < _list.Count; i++)
                    _hashTable.Add(_list[i][1].ToString() + _list[i][2].ToString() + _list[i][3].ToString(), _list[i][0]);
            }

            return _hashTable;
        }

        #endregion
    }
}
