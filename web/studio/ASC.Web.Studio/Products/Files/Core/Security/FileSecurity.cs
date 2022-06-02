using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Web.Files.Api;

namespace ASC.Files.Core.Security
{
    public class FileSecurity : IFileSecurity
    {
        private readonly IDaoFactory daoFactory;


        public FileShare DefaultMyShare
        {
            get { return FileShare.Restrict; }
        }

        public FileShare DefaultCommonShare
        {
            get { return FileShare.Read; }
        }


        public FileSecurity(IDaoFactory daoFactory)
        {
            this.daoFactory = daoFactory;
        }


        public bool CanRead(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Read);
        }

        public bool CanCreate(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Create);
        }

        public bool CanEdit(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Edit);
        }

        public bool CanDelete(FileEntry file, Guid userId)
        {
            return Can(file, userId, FilesSecurityActions.Delete);
        }

        public bool CanRead(FileEntry file)
        {
            return CanRead(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanCreate(FileEntry file)
        {
            return CanCreate(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanEdit(FileEntry file)
        {
            return CanEdit(file, SecurityContext.CurrentAccount.ID);
        }

        public bool CanDelete(FileEntry file)
        {
            return CanDelete(file, SecurityContext.CurrentAccount.ID);
        }

        public IEnumerable<FileEntry> FilterRead(IEnumerable<FileEntry> entries)
        {
            return Filter(entries, FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID);
        }

        public IEnumerable<File> FilterRead(IEnumerable<File> entries)
        {
            return Filter(entries.Cast<FileEntry>(), FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID).Cast<File>();
        }

        public IEnumerable<Folder> FilterRead(IEnumerable<Folder> entries)
        {
            return Filter(entries.Cast<FileEntry>(), FilesSecurityActions.Read, SecurityContext.CurrentAccount.ID).Cast<Folder>();
        }


        private bool Can(FileEntry entry, Guid userId, FilesSecurityActions action)
        {
            return Filter(new[] { entry }, action, userId).Any();
        }

        private IEnumerable<FileEntry> Filter(IEnumerable<FileEntry> entries, FilesSecurityActions action, Guid userId)
        {
            if (entries == null || !entries.Any()) return Enumerable.Empty<FileEntry>();

            entries = entries.Where(f => f != null);
            var result = new List<FileEntry>(entries.Count());

            // save entries order
            var order = entries.Select((f, i) => new { Id = f.UniqID, Pos = i }).ToDictionary(e => e.Id, e => e.Pos);

            // common or my files
            Func<FileEntry, bool> filter = f => f.RootFolderType == FolderType.COMMON || f.RootFolderType == FolderType.USER || f.RootFolderType == FolderType.SHARE;
            if (entries.Any(filter))
            {
                var subjects = GetUserSubjects(userId);
                List<FileShareRecord> shares = null;
                foreach (var e in entries.Where(filter))
                {
                    if (!CoreContext.Authentication.GetAccountByID(userId).IsAuthenticated)
                    {
                        // guest can only read
                        if (action == FilesSecurityActions.Read)
                        {
                            result.Add(e);
                        }
                        continue;
                    }

                    if (e.RootFolderType == FolderType.USER && e.RootFolderCreator == userId)
                    {
                        // user has all right in his folder
                        result.Add(e);
                        continue;
                    }

                    if (DefaultCommonShare == FileShare.Read && action == FilesSecurityActions.Read && e is Folder && ((Folder)e).FolderType == FolderType.COMMON)
                    {
                        // all can read Common folder
                        result.Add(e);
                        continue;
                    }

                    if (action == FilesSecurityActions.Read && e is Folder && ((Folder)e).FolderType == FolderType.SHARE)
                    {
                        // all can read Share folder
                        result.Add(e);
                        continue;
                    }

                    if (e.RootFolderType == FolderType.COMMON && CoreContext.UserManager.IsUserInGroup(userId, ASC.Core.Users.Constants.GroupAdmin.ID))
                    {
                        // administrator in Common has all right
                        result.Add(e);
                        continue;
                    }

                    if (shares == null)
                    {
                        shares = GetShares(entries.ToArray()).Join(subjects, r => r.Subject, s => s, (r, s) => r).ToList();// shares ordered by level
                    }

                    FileShareRecord ace;
                    if (e is File)
                    {
                        ace = shares.FirstOrDefault(r => r.EntryId == e.ID && r.EntryType == FileEntryType.File);
                        if (ace == null)
                        {
                            // share on parent folders
                            ace = shares.Where(r => r.EntryId == ((File)e).FolderID && r.EntryType == FileEntryType.Folder)
                                .OrderBy(r => subjects.IndexOf(r.Subject))
                                .ThenByDescending(r => r.Share)
                                .FirstOrDefault();
                        }
                    }
                    else
                    {
                        ace = shares.Where(r => r.EntryId == e.ID && r.EntryType == FileEntryType.Folder)
                            .OrderBy(r => subjects.IndexOf(r.Subject))
                            .ThenByDescending(r => r.Share)
                            .FirstOrDefault();
                    }
                    var defaultShare = e.RootFolderType == FolderType.USER ? DefaultMyShare : DefaultCommonShare;
                    e.Access = ace != null ? ace.Share : defaultShare;

                    if (action == FilesSecurityActions.Read && e.Access <= FileShare.Read) result.Add(e);
                    else if (action == FilesSecurityActions.Edit && e.Access <= FileShare.ReadWrite) result.Add(e);
                    else if (action == FilesSecurityActions.Create && e.Access <= FileShare.ReadWrite) result.Add(e);
                    // can't delete in My other people's files
                    else if (action == FilesSecurityActions.Delete && e.Access <= FileShare.ReadWrite && e.RootFolderType == FolderType.COMMON) result.Add(e);
                    else if (e.Access <= FileShare.Read && e.CreateBy == userId) result.Add(e);

                    if (e.CreateBy == userId) e.Access = FileShare.None; //HACK: for client
                }
            }

            // files in bunch
            filter = f => f.RootFolderType == FolderType.BUNCH;
            if (entries.Any(filter))
            {
                using (var dao = daoFactory.GetFolderDao())
                {
                    var findedAdapters = new Dictionary<int, IFileSecurity>();
                    foreach (var e in entries.Where(filter))
                    {
                        IFileSecurity adapter = null;

                        if (!findedAdapters.ContainsKey(e.RootFolderId))
                        {
                            var root = dao.GetFolder(e.RootFolderId);
                            if (root != null)
                            {
                                adapter = FilesIntegration.GetFileSecurity(root.Title);
                            }
                            findedAdapters[e.RootFolderId] = adapter;
                        }

                        adapter = findedAdapters[e.RootFolderId];
                        if (adapter != null)
                        {
                            if (action == FilesSecurityActions.Create && adapter.CanCreate(e, userId)) result.Add(e);
                            if (action == FilesSecurityActions.Delete && adapter.CanDelete(e, userId)) result.Add(e);
                            if (action == FilesSecurityActions.Read && adapter.CanRead(e, userId)) result.Add(e);
                            if (action == FilesSecurityActions.Edit && adapter.CanEdit(e, userId)) result.Add(e);
                        }
                    }
                }
            }

            // files in trash
            filter = f => f.RootFolderType == FolderType.TRASH;
            if (entries.Any(filter))
            {
                using (var dao = daoFactory.GetFolderDao())
                {
                    var mytrashId = dao.GetFolderID(FileConst.StorageModule, "trash", userId.ToString(), false);
                    foreach (var e in entries.Where(filter))
                    {
                        // only in my trash
                        if (e.RootFolderId == mytrashId) result.Add(e);
                    }
                }
            }

            if (CoreContext.UserManager.IsUserInGroup(userId, ASC.Core.Users.Constants.GroupAdmin.ID))
            {
                // administrator can work with crashed entries (crash in files_folder_tree)
                filter = f => f.RootFolderType == FolderType.DEFAULT;
                foreach (var e in entries.Where(filter))
                {
                    result.Add(e);
                }
            }

            // restore entries order
            result.Sort((x, y) => order[x.UniqID].CompareTo(order[y.UniqID]));
            return result;
        }


        public void Share(int entryId, FileEntryType entryType, Guid @for, FileShare share)
        {
            using (var dao = daoFactory.GetSecurityDao())
            {
                var r = new FileShareRecord
                {
                    Tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    EntryId = entryId,
                    EntryType = entryType,
                    Subject = @for,
                    Owner = SecurityContext.CurrentAccount.ID,
                    Share = share,
                };
                dao.SetShare(r);
            }
        }

        public IEnumerable<FileShareRecord> GetShares(params FileEntry[] entries)
        {
            using (var dao = daoFactory.GetSecurityDao())
            {
                return dao.GetShares(entries);
            }
        }

        public List<FileEntry> GetSharesForMe()
        {
            using (var dao = daoFactory.GetSecurityDao())
            {
                var shares = dao.GetShares(GetUserSubjects(SecurityContext.CurrentAccount.ID));
                return ToFileEntries(shares);
            }
        }

        public void RemoveSubject(Guid subject)
        {
            using (var dao = daoFactory.GetSecurityDao())
            {
                dao.RemoveSubject(subject);
            }
        }

        public List<Guid> GetUserSubjects(Guid userId)
        {
            // priority order
            return new[] { userId }
                .Union(CoreContext.UserManager.GetUserGroups(userId, FileConst.GroupCategoryId).Select(g => g.ID))
                .Union(CoreContext.UserManager.GetUserGroups(userId, ASC.Core.Users.IncludeType.Distinct).Select(g => g.ID))
                .ToList();
        }

        private List<FileEntry> ToFileEntries(IEnumerable<FileShareRecord> records)
        {
            var result = new List<FileEntry>();
            // no restrict
            var norestict = new List<FileShareRecord>();
            foreach (var r in records)
            {
                if (!records.Any(s => r.EntryId == s.EntryId && r.EntryType == s.EntryType && s.Share == FileShare.Restrict))
                {
                    norestict.Add(r);
                }
            }

            using (var ddao = daoFactory.GetFolderDao())
            using (var fdao = daoFactory.GetFileDao())
            {
                var foldersId = norestict.Where(r => r.EntryType == FileEntryType.Folder && r.Share != FileShare.Restrict).Select(r => r.EntryId).ToArray();
                var folders = ddao.GetFolders(foldersId);
                // set access
                folders.ForEach(f => f.Access = norestict.First(r => r.EntryId == f.ID && r.EntryType == FileEntryType.Folder).Share);
                result.AddRange(folders.Cast<FileEntry>());

                var filesId = norestict.Where(r => r.EntryType == FileEntryType.File && r.Share != FileShare.Restrict).Select(r => r.EntryId).ToArray();
                var files = fdao.GetFiles(filesId);
                // set access
                files.ForEach(f => f.Access = norestict.First(r => r.EntryId == f.ID && r.EntryType == FileEntryType.File).Share);
                result.AddRange(files.Cast<FileEntry>());
            }
            return result;
        }


        private enum FilesSecurityActions
        {
            Read,
            Create,
            Edit,
            Delete,
        }
    }
}
