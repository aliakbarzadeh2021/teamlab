using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FullTextIndex;

namespace ASC.Files.Core.Data
{
    class FileDao : AbstractDao, IFileDao
    {
        private object syncRoot = new object();


        public FileDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {

        }


        private Exp BuildLike(string[] columns, string[] keywords)
        {
            var like = Exp.Empty;
            foreach (var keyword in keywords)
            {
                var keywordLike = Exp.Empty;
                foreach (var column in columns)
                {
                    keywordLike = keywordLike | Exp.Like(column, keyword, SqlLike.StartWith) | Exp.Like(column, ' ' + keyword);
                }
                like = like & keywordLike;
            }
            return like;
        }

        public Dictionary<File, String> Search(String searchText, FolderType folderType)
        {
            var result = new Dictionary<File, String>();

            if (FullTextSearch.SupportModule(FullTextSearch.FileModule))
            {
                var indexResult = FullTextSearch.Search(searchText, FullTextSearch.FileModule);
                var ids = indexResult.GetIdentifiers()
                    .Where(id => !string.IsNullOrEmpty(id) && id[0] != 'd')
                    .ToDictionary(id => Convert.ToInt32(id), id => indexResult.GetText(id));

                return DbManager
                    .ExecuteList(GetFileQuery(Exp.In("id", ids.Keys) & Exp.Eq("current_version", true)))
                    .ConvertAll(r => ToFile(r))
                    .Where(f => folderType == FolderType.BUNCH ? f.RootFolderType == FolderType.BUNCH : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON)
                    .ToDictionary(r => r, r => ids.ContainsKey(r.ID) ? ids[r.ID] : string.Empty);
            }
            else
            {
                var keywords = searchText
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(k => 3 <= k.Trim().Length)
                    .ToArray();

                if (keywords.Length == 0) return result;

                var q = GetFileQuery(Exp.Eq("f.current_version", true) & BuildLike(new[] { "f.title" }, keywords));

                return DbManager
                    .ExecuteList(q)
                    .ConvertAll(r => ToFile(r))
                    .Where(f => folderType == FolderType.BUNCH ? f.RootFolderType == FolderType.BUNCH : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON)
                    .ToDictionary(f => f, _ => string.Empty);
            }
        }

        public File GetFile(int fileID, int fileVersion)
        {
            return DbManager
                .ExecuteList(GetFileQuery(Exp.Eq("id", fileID) & Exp.Eq("version", fileVersion)))
                .ConvertAll(r => ToFile(r))
                .SingleOrDefault();
        }

        public List<File> GetFiles(int[] fileID)
        {
            if (fileID == null || fileID.Length == 0) return new List<File>();

            return DbManager
                .ExecuteList(GetFileQuery(Exp.In("id", fileID) & Exp.Eq("current_version", true)))
                .ConvertAll(r => ToFile(r));
        }

        public void MoveFile(int id, int to)
        {
            if (id == 0) return;

            using (var tx = DbManager.BeginTransaction())
            {
                var fromFolders = DbManager
                    .ExecuteList(Query("files_file").Select("folder_id").Where("id", id).GroupBy("id"))
                    .ConvertAll(r => Convert.ToInt32(r[0]));

                DbManager.ExecuteNonQuery(
                    Update("files_file")
                    .Set("folder_id", to)
                    .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                    .Set("modified_on", DateTime.UtcNow)
                    .Where("id", id));

                tx.Commit();

                fromFolders.ForEach(fid => RecalculateFilesCount(fid));
                RecalculateFilesCount(to);
            }
        }

        public void FileRename(int fileID, String newTitle)
        {
            DbManager.ExecuteNonQuery(
                Update("files_file")
                .Set("title", newTitle)
                .Set("modified_on", DateTime.UtcNow)
                .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                .Where("id", fileID));
        }

        public File GetFile(int folderID, String title)
        {
            if (String.IsNullOrEmpty(title)) throw new ArgumentNullException(title);

            return DbManager
                .ExecuteList(GetFileQuery(Exp.Eq("title", title) & Exp.Eq("current_version", true) & Exp.Eq("folder_id", folderID)))
                .ConvertAll(r => ToFile(r))
                .SingleOrDefault();
        }

        public bool IsExist(String title, int folderID)
        {
            int fileCount = DbManager.ExecuteScalar<int>(
                Query("files_file")
                .SelectCount()
                .Where("title", title)
                .Where("folder_id", folderID));

            return fileCount != 0;
        }


        public int CopyFile(int id, int to)
        {
            var result = 0;
            // copy only current version
            var file = GetFiles(new[] { id }).SingleOrDefault();
            if (file != null)
            {
                var copy = new File
                {
                    ContentLength = file.ContentLength,
                    ContentType = file.ContentType,
                    FileStatus = file.FileStatus,
                    FolderID = to,
                    Title = file.Title,
                    Version = file.Version,
                    ConvertedType = file.ConvertedType,
                };

                copy = SaveFile(copy);

                result = copy.ID;
            }

            return result;
        }

        public File SaveFile(File file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            lock (syncRoot)
            {
                using (var tx = DbManager.BeginTransaction())
                {
                    if (file.ID == 0)
                    {
                        file.ID = DbManager.ExecuteScalar<int>(new SqlQuery("files_file").SelectMax("id")) + 1;
                        file.Version = 1;
                    }

                    if (SecurityContext.CurrentAccount.IsAuthenticated) file.ModifiedBy = SecurityContext.CurrentAccount.ID;
                    file.ModifiedOn = TenantUtil.DateTimeNow();
                    if (file.CreateBy == default(Guid)) file.CreateBy = SecurityContext.CurrentAccount.ID;
                    if (file.CreateOn == default(DateTime)) file.CreateOn = TenantUtil.DateTimeNow();

                    DbManager.ExecuteNonQuery(
                        Update("files_file")
                        .Set("current_version", false)
                        .Where("id", file.ID)
                        .Where("current_version", true));

                    DbManager.ExecuteNonQuery(
                        Insert("files_file")
                        .InColumnValue("id", file.ID)
                        .InColumnValue("version", file.Version)
                        .InColumnValue("title", file.Title)
                        .InColumnValue("folder_id", file.FolderID)
                        .InColumnValue("create_on", TenantUtil.DateTimeToUtc(file.CreateOn))
                        .InColumnValue("create_by", file.CreateBy.ToString())
                        .InColumnValue("content_type", file.ContentType)
                        .InColumnValue("content_length", file.ContentLength)
                        .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                        .InColumnValue("modified_by", file.ModifiedBy.ToString())
                        .InColumnValue("category", (int)FileFormats.GetFileType(file.Title))
                        .InColumnValue("current_version", true)
                        .InColumnValue("file_status", (int)FileStatus.None)
                        .InColumnValue("converted_type", file.ConvertedType));

                    tx.Commit();
                }

                RecalculateFilesCount(file.FolderID);
            }
            return file;
        }

        public File GetFile(int fileID)
        {
            return DbManager
                .ExecuteList(GetFileQuery(Exp.Eq("id", fileID) & Exp.Eq("current_version", true)))
                .ConvertAll(r => ToFile(r))
                .SingleOrDefault();
        }

        public List<File> GetFileHistory(int fileID)
        {
            var files = DbManager
                .ExecuteList(GetFileQuery(Exp.Eq("id", fileID)).OrderBy("version", false))
                .ConvertAll(r => ToFile(r));

            if (files.Count > 0) files.RemoveAt(0);

            return files;
        }

        public void DeleteFile(int id)
        {
            if (id == 0) return;

            using (var tx = DbManager.BeginTransaction())
            {
                var fromFolders = DbManager
                    .ExecuteList(Query("files_file").Select("folder_id").Where("id", id).GroupBy("id"))
                    .ConvertAll(r => Convert.ToInt32(r[0]));

                DbManager.ExecuteNonQuery(Delete("files_file").Where("id", id));
                DbManager.ExecuteNonQuery(Delete("files_tag_link").Where("entry_id", id).Where("entry_type", FileEntryType.File));
                DbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));

                tx.Commit();

                fromFolders.ForEach(fid => RecalculateFilesCount(fid));
            }
        }

        public bool IsExist(int fileID, int fileVersion)
        {
            var count = DbManager.ExecuteScalar<int>(Query("files_file").SelectCount().Where("id", fileID).Where("version", fileVersion));
            return count != 0;
        }

        public bool IsExist(int fileID)
        {
            int count = DbManager.ExecuteScalar<int>(Query("files_file").SelectCount().Where("id", fileID));
            return count != 0;
        }

        public String GetUniqFileDirectory(int fileID)
        {
            return string.Format("folder_{0}/file_{1}", (fileID / 1000 + 1) * 1000, fileID);
        }

        public String GetUniqFilePath(File file)
        {
            return file != null ? string.Format("{0}/v{1}/content{2}", GetUniqFileDirectory(file.ID), file.Version, FileFormats.GetExtension(file.Title)) : null;
        }


        private void RecalculateFilesCount(int folderId)
        {
            DbManager.ExecuteNonQuery(GetRecalculateFilesCountUpdate(folderId));
        }
    }
}
