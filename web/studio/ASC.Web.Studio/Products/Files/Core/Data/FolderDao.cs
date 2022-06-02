using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FullTextIndex;

namespace ASC.Files.Core.Data
{
    class FolderDao : AbstractDao, IFolderDao
    {
        private const string my = "my";
        private const string common = "common";
        private const string share = "share";
        private const string trash = "trash";


        public FolderDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }


        public List<Folder> GetFolders(int parentId, int from, int count, OrderBy orderBy)
        {
            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            var q = GetFolderQuery(Exp.Eq("parent_id", parentId));
            switch (orderBy.SortedBy)
            {
                case SortedByType.AZ:
                    q.OrderBy("title", orderBy.IsAsc);
                    break;
                case SortedByType.DateAndTime:
                    q.OrderBy("create_on", orderBy.IsAsc);
                    break;
                case SortedByType.Author:
                    q.OrderBy("create_by", orderBy.IsAsc);
                    break;
                default:
                    q.OrderBy("title", true);
                    break;
            }

            if (0 < from && from < int.MaxValue) q.SetFirstResult(from);
            if (0 < count && count < int.MaxValue) q.SetMaxResults(count);

            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => ToFolder(r));
        }

        public List<Folder> GetFolders(int[] ids)
        {
            var q = GetFolderQuery(Exp.In("id", ids));
            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => ToFolder(r));
        }

        public Folder GetFolder(int id)
        {
            return DbManager
                .ExecuteList(GetFolderQuery(Exp.Eq("id", id)))
                .ConvertAll(r => ToFolder(r))
                .SingleOrDefault();
        }

        public bool IsExist(int id)
        {
            return DbManager.ExecuteScalar<int>(Query("files_folder").SelectCount().Where(Exp.Eq("id", id))) > 0;
        }

        public Folder GetFolder(String title, int parentId)
        {
            return DbManager
                .ExecuteList(GetFolderQuery(Exp.Eq("title", title) & Exp.Eq("parent_id", parentId)).OrderBy("create_on", true))
                .ConvertAll(r => ToFolder(r))
                .FirstOrDefault();
        }

        public bool IsExist(String title, int parentId)
        {
            return DbManager.ExecuteScalar<int>(
                Query("files_folder")
                .SelectCount()
                .Where("title", title)
                .Where("parent_id", parentId)) > 0;
        }

        public Folder GetRootFolder(int folderID)
        {
            var q = new SqlQuery("files_folder_tree")
                .Select("parent_id")
                .Where("folder_id", folderID)
                .SetMaxResults(1)
                .OrderBy("level", false);

            return DbManager
                .ExecuteList(GetFolderQuery(Exp.EqColumns("id", q)))
                .ConvertAll(r => ToFolder(r))
                .SingleOrDefault();
        }

        public Folder GetRootFolderByFile(int fileID)
        {
            var subq = Query("files_file")
                .Select("folder_id")
                .Where("id", fileID)
                .Distinct();

            var q = new SqlQuery("files_folder_tree")
                .Select("parent_id")
                .Where(Exp.EqColumns("folder_id", subq))
                .SetMaxResults(1)
                .OrderBy("level", false);

            return DbManager
                .ExecuteList(GetFolderQuery(Exp.EqColumns("id", q)))
                .ConvertAll(r => ToFolder(r))
                .SingleOrDefault();
        }


        public IEnumerable<Folder> Search(string text, FolderType folderType)
        {
            if (string.IsNullOrEmpty(text)) return new List<Folder>();

            if (FullTextSearch.SupportModule(FullTextSearch.FileModule))
            {
                var ids = FullTextSearch.Search(text, FullTextSearch.FileModule)
                    .GetIdentifiers()
                    .Where(id => !string.IsNullOrEmpty(id) && id[0] == 'd')
                    .Select(id => int.Parse(id.Substring(1)))
                    .ToList();

                return DbManager
                    .ExecuteList(GetFolderQuery(Exp.In("id", ids)))
                    .ConvertAll(r => ToFolder(r))
                    .Where(f => folderType == FolderType.BUNCH ? f.RootFolderType == FolderType.BUNCH : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON);
            }
            else
            {
                var keywords = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(k => 3 <= k.Trim().Length)
                    .ToList();
                if (keywords.Count == 0) return new List<Folder>();

                var where = Exp.Empty;
                keywords.ForEach(k => where &= Exp.Like("title", k));

                return DbManager
                    .ExecuteList(GetFolderQuery(where))
                    .ConvertAll(r => ToFolder(r))
                    .Where(f => folderType == FolderType.BUNCH ? f.RootFolderType == FolderType.BUNCH : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON);
            }
        }


        public int SaveFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");

            folder.ModifiedOn = TenantUtil.DateTimeNow();
            folder.ModifiedBy = SecurityContext.CurrentAccount.ID;

            if (folder.CreateOn == default(DateTime)) folder.CreateOn = TenantUtil.DateTimeNow();
            if (folder.CreateBy == default(Guid)) folder.CreateBy = SecurityContext.CurrentAccount.ID;

            var tx = DbManager.InTransaction ? null : DbManager.BeginTransaction();
            try
            {
                if (folder.ID != default(int) && IsExist(folder.ID))
                {
                    DbManager.ExecuteNonQuery(
                        Update("files_folder")
                        .Set("title", folder.Title)
                        .Set("modified_on", TenantUtil.DateTimeToUtc(folder.ModifiedOn))
                        .Set("modified_by", folder.ModifiedBy.ToString())
                        .Where("id", folder.ID));
                }
                else
                {
                    folder.ID = DbManager.ExecuteScalar<int>(
                        Insert("files_folder")
                        .InColumnValue("id", 0)
                        .InColumnValue("parent_id", folder.ParentFolderID)
                        .InColumnValue("title", folder.Title)
                        .InColumnValue("create_on", TenantUtil.DateTimeToUtc(folder.CreateOn))
                        .InColumnValue("create_by", folder.CreateBy.ToString())
                        .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(folder.ModifiedOn))
                        .InColumnValue("modified_by", folder.ModifiedBy.ToString())
                        .InColumnValue("folder_type", (int)folder.FolderType)
                        .Identity(1, 0, true));

                    //itself link
                    DbManager.ExecuteNonQuery(
                        new SqlInsert("files_folder_tree")
                        .InColumns("folder_id", "parent_id", "level")
                        .Values(folder.ID, folder.ID, 0));

                    //full path to root
                    DbManager.ExecuteNonQuery(
                        new SqlInsert("files_folder_tree")
                        .InColumns("folder_id", "parent_id", "level")
                        .Values(
                            new SqlQuery("files_folder_tree t")
                            .Select(folder.ID.ToString(), "t.parent_id", "t.level + 1")
                            .Where("t.folder_id", folder.ParentFolderID)));
                }

                if (tx != null) tx.Commit();
            }
            catch
            {
                if (tx != null) tx.Rollback();
                throw;
            }

            if (!DbManager.InTransaction)
            {
                RecalculateFoldersCount(folder.ID);
            }

            return folder.ID;
        }

        public void RenameFolder(int id, string title)
        {
            DbManager.ExecuteNonQuery(
                Update("files_folder")
                .Set("title", title)
                .Set("modified_on", DateTime.UtcNow)
                .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                .Where("id", id));
        }

        public void DeleteFolder(int id)
        {
            if (id == 0) return;

            using (var tx = DbManager.BeginTransaction())
            {
                var subfolders = DbManager
                    .ExecuteList(new SqlQuery("files_folder_tree").Select("folder_id").Where("parent_id", id))
                    .ConvertAll(r => Convert.ToInt32(r[0]));
                if (!subfolders.Contains(id)) subfolders.Add(id); // chashed folder_tree

                var parent = DbManager.ExecuteScalar<int>(Query("files_folder").Select("parent_id").Where("id", id));

                DbManager.ExecuteNonQuery(Delete("files_folder").Where(Exp.In("id", subfolders)));
                DbManager.ExecuteNonQuery(new SqlDelete("files_folder_tree").Where(Exp.In("folder_id", subfolders)));
                DbManager.ExecuteNonQuery(Delete("files_tag_link").Where(Exp.In("entry_id", subfolders)).Where("entry_type", FileEntryType.Folder));
                DbManager.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));

                tx.Commit();

                RecalculateFoldersCount(parent);
            }
        }

        public void MoveFolder(int id, int to)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                var recalcFolders = new List<int> { to };
                var parent = DbManager.ExecuteScalar<int>(Query("files_folder").Select("parent_id").Where("id", id));
                if (parent != 0 && !recalcFolders.Contains(parent)) recalcFolders.Add(parent);

                DbManager.ExecuteNonQuery(
                    Update("files_folder")
                    .Set("parent_id", to)
                    .Set("modified_on", DateTime.UtcNow)
                    .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                    .Where("id", id));

                var subfolders = DbManager
                    .ExecuteList(new SqlQuery("files_folder_tree").Select("folder_id", "level").Where("parent_id", id))
                    .ToDictionary(r => Convert.ToInt32(r[0]), r => Convert.ToInt32(r[1]));

                DbManager.ExecuteNonQuery(new SqlDelete("files_folder_tree").Where(Exp.In("folder_id", subfolders.Keys) & !Exp.In("parent_id", subfolders.Keys)));

                foreach (var subfolder in subfolders)
                {
                    DbManager.ExecuteNonQuery(
                        new SqlInsert("files_folder_tree")
                        .InColumns("folder_id", "parent_id", "level")
                        .Values(
                            new SqlQuery("files_folder_tree")
                            .Select(subfolder.Key.ToString(), "parent_id", "level + 1 + " + subfolder.Value.ToString())
                            .Where("folder_id", to)));
                }

                tx.Commit();

                recalcFolders.ForEach(fid => RecalculateFoldersCount(fid));
                recalcFolders.ForEach(fid => DbManager.ExecuteNonQuery(GetRecalculateFilesCountUpdate(fid)));
            }
        }

        public int CopyFolder(int id, int to)
        {
            var folder = GetFolder(id);
            var copy = new Folder
            {
                ParentFolderID = to,
                Title = folder.Title,
                FolderType = folder.FolderType,
            };

            return SaveFolder(copy);
        }

        public IDictionary<int, string> CanMoveOrCopy(int[] folders, int to)
        {
            var result = new Dictionary<int, string>();

            foreach (var id in folders)
            {
                var count = DbManager.ExecuteScalar<int>(new SqlQuery("files_folder_tree").SelectCount().Where("parent_id", id).Where("folder_id", to));
                if (0 < count)
                {
                    throw new InvalidOperationException(ASC.Web.Files.Resources.FilesJSResource.InfoFolderCopyError);
                }

                var title = DbManager.ExecuteScalar<string>(Query("files_folder").Select("lower(title)").Where("id", id));
                var conflict = DbManager.ExecuteScalar<int>(Query("files_folder").Select("id").Where("lower(title)", title).Where("parent_id", to));
                if (conflict != 0)
                {
                    DbManager.ExecuteList(
                        new SqlQuery("files_file f1")
                        .InnerJoin("files_file f2", Exp.EqColumns("lower(f1.title)", "lower(f2.title)"))
                        .Select("f1.id", "f1.title")
                        .Where(Exp.Eq("f1.tenant_id", TenantID) & Exp.Eq("f1.current_version", true) & Exp.Eq("f1.folder_id", id))
                        .Where(Exp.Eq("f2.tenant_id", TenantID) & Exp.Eq("f2.current_version", true) & Exp.Eq("f2.folder_id", conflict)))
                        .ForEach(r => result[Convert.ToInt32(r[0])] = (string)r[1]);

                    var childs = DbManager.ExecuteList(Query("files_folder").Select("id").Where("parent_id", id)).ConvertAll(r => Convert.ToInt32(r[0]));
                    foreach (var pair in CanMoveOrCopy(childs.ToArray(), conflict))
                    {
                        result.Add(pair.Key, pair.Value);
                    }
                }
            }

            return result;
        }

        public int GetItemsCount(int folderID, FilterType filter, Guid? subjectID, bool withSubfolder)
        {
            switch (filter)
            {
                case FilterType.SpreadsheetsOnly:
                case FilterType.PresentationsOnly:
                case FilterType.PicturesOnly:
                case FilterType.FilesOnly:
                case FilterType.DocumentsOnly:
                case FilterType.ByUser:
                case FilterType.ByDepartment:
                    return GetFilesCount(folderID, filter, subjectID, withSubfolder);

                case FilterType.FoldersOnly:
                    return GetFoldersCount(folderID, withSubfolder);

                default:
                    return GetFoldersCount(folderID, withSubfolder) + GetFilesCount(folderID, filter, subjectID, withSubfolder);
            }
        }

        private int GetFoldersCount(int parentId, bool withSubfoldes)
        {
            var q = new SqlQuery("files_folder_tree").SelectCount().Where("parent_id", parentId);
            if (withSubfoldes)
            {
                q.Where(Exp.Gt("level", 0));
            }
            else
            {
                q.Where("level", 1);
            }
            return DbManager.ExecuteScalar<int>(q);
        }

        private int GetFilesCount(int folderID, FilterType filterType, Guid? subjectID, bool includeSubFolder)
        {
            var q = Query("files_file").SelectCount("distinct id");

            switch (filterType)
            {
                case FilterType.DocumentsOnly:
                case FilterType.PicturesOnly:
                case FilterType.PresentationsOnly:
                case FilterType.SpreadsheetsOnly:
                    q.Where("category", (int)filterType);
                    break;
                case FilterType.ByUser:
                    q.Where("create_by", SecurityContext.CurrentAccount.ID.ToString());
                    break;
                case FilterType.ByDepartment:
                    var users = CoreContext.UserManager.GetUsersByGroup(subjectID ?? Guid.Empty).Select(u => u.ID.ToString()).ToArray();
                    q.Where(Exp.In("create_by", users));
                    break;
            }

            if (includeSubFolder)
            {
                q.Where(Exp.In("folder_id", new SqlQuery("files_folder_tree").Select("folder_id").Where("parent_id", folderID)));
            }
            else
            {
                q.Where("folder_id", folderID);
            }

            return DbManager.ExecuteScalar<int>(q);
        }

        public List<File> GetFiles(int folderID, int from, int count, OrderBy orderBy, FilterType filterType, Guid? subjectID)
        {
            if (orderBy == null) orderBy = new OrderBy(SortedByType.DateAndTime, false);

            var q = GetFileQuery(Exp.Eq("current_version", true) & Exp.Eq("folder_id", folderID));

            switch (orderBy.SortedBy)
            {
                case SortedByType.AZ:
                    q.OrderBy("title", orderBy.IsAsc);
                    break;
                case SortedByType.DateAndTime:
                    q.OrderBy("create_on", orderBy.IsAsc);
                    break;
                case SortedByType.Author:
                    q.OrderBy("create_by", orderBy.IsAsc);
                    break;
                case SortedByType.Size:
                    q.OrderBy("content_length", orderBy.IsAsc);
                    break;
                default:
                    q.OrderBy("title", true);
                    break;
            }

            switch (filterType)
            {
                case FilterType.DocumentsOnly:
                case FilterType.PicturesOnly:
                case FilterType.PresentationsOnly:
                case FilterType.SpreadsheetsOnly:
                    q.Where("category", (int)filterType);
                    break;
                case FilterType.ByUser:
                    q.Where("create_by", (subjectID ?? Guid.Empty).ToString());
                    break;
                case FilterType.ByDepartment:
                    var users = CoreContext.UserManager.GetUsersByGroup(subjectID ?? Guid.Empty).Select(u => u.ID.ToString()).ToArray();
                    q.Where(Exp.In("create_by", users));
                    break;
            }

            if (0 < from && from < int.MaxValue) q.SetFirstResult(from);
            if (0 < count && count < int.MaxValue) q.SetMaxResults(count);

            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => ToFile(r));
        }

        public List<int> GetFiles(int folderId, bool withSubfolders)
        {
            return DbManager.ExecuteList(
                Query("files_file")
                .Select("id")
                .Where("folder_id", folderId))
                .ConvertAll(r => Convert.ToInt32(r[0]));
        }

        public List<Folder> GetParentFolders(int folderId)
        {
            var q = GetFolderQuery(Exp.Empty)
                .InnerJoin("files_folder_tree t", Exp.EqColumns("id", "t.parent_id"))
                .Where("t.folder_id", folderId)
                .OrderBy("t.level", false);

            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => ToFolder(r));
        }


        public int GetFolderID(string module, string bunch, string data, bool createIfNotExists)
        {
            if (string.IsNullOrEmpty(module)) throw new ArgumentNullException("module");
            if (string.IsNullOrEmpty(bunch)) throw new ArgumentNullException("bunch");

            using (var tx = DbManager.BeginTransaction())
            {
                var key = string.Format("{0}/{1}/{2}", module, bunch, data);
                var folderIdStr = DbManager.ExecuteScalar<string>(Query("files_bunch_objects").Select("left_node").Where("right_node", key));
                var folderId = 0;
                int.TryParse(folderIdStr, out folderId);
                if (createIfNotExists && folderId == 0)
                {
                    var folder = new Folder();
                    if (bunch == my)
                    {
                        folder.FolderType = FolderType.USER;
                        folder.Title = my;
                    }
                    else if (bunch == common)
                    {
                        folder.FolderType = FolderType.COMMON;
                        folder.Title = common;
                    }
                    else if (bunch == trash)
                    {
                        folder.FolderType = FolderType.TRASH;
                        folder.Title = trash;
                    }
                    else if (bunch == share)
                    {
                        folder.FolderType = FolderType.SHARE;
                        folder.Title = share;
                    }
                    else
                    {
                        folder.FolderType = FolderType.BUNCH;
                        folder.Title = key;
                    }

                    folderId = SaveFolder(folder);

                    DbManager.ExecuteNonQuery(
                        Insert("files_bunch_objects")
                        .InColumnValue("left_node", folderId)
                        .InColumnValue("right_node", key));
                }
                tx.Commit();

                return folderId;
            }
        }

        public int GetFolderIDTrash(bool createIfNotExists)
        {
            return GetFolderID(FileConst.StorageModule, trash, SecurityContext.CurrentAccount.ID.ToString(), createIfNotExists);
        }

        public int GetFolderIDCommon(bool createIfNotExists)
        {
            return GetFolderID(FileConst.StorageModule, common, null, createIfNotExists);
        }

        public int GetFolderIDUser(bool createIfNotExists)
        {
            return GetFolderID(FileConst.StorageModule, my, SecurityContext.CurrentAccount.ID.ToString(), createIfNotExists);
        }

        public int GetFolderIDShare(bool createIfNotExists)
        {
            return GetFolderID(FileConst.StorageModule, share, null, createIfNotExists);
        }


        private void RecalculateFoldersCount(int id)
        {
            DbManager.ExecuteNonQuery(
                Update("files_folder")
                .Set("foldersCount = (select count(*) - 1 from files_folder_tree where parent_id = id)")
                .Where(Exp.In("id", new SqlQuery("files_folder_tree").Select("parent_id").Where("folder_id", id))));
        }
    }
}