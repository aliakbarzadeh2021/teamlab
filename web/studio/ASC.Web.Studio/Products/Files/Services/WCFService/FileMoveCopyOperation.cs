using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.NotifyService;

namespace ASC.Web.Files.Services.WCFService
{
    class FileMoveCopyOperation : FileOperation
    {
        private int toFolder;
        private bool copy;
        private FileConflictResolveType resolveType;


        protected override FileOperationType OperationType
        {
            get { return copy ? FileOperationType.Copy : FileOperationType.Move; }
        }


        public FileMoveCopyOperation(Tenant tenant, List<int> folders, List<int> files, int toFolder, bool copy, FileConflictResolveType resolveType)
            : base(tenant, folders, files)
        {
            this.toFolder = toFolder;
            this.copy = copy;
            this.resolveType = resolveType;
        }


        protected override void Do()
        {
            Status += string.Format("folder_{0} ", toFolder);

            //TODO: check on each iteration?
            var to = FolderDao.GetFolder(toFolder);
            if (to == null) return;
            if (!FilesSecurity.CanCreate(to)) throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);

            if (copy)
            {
                Folder rootFrom = null;
                if (0 < Folders.Count) rootFrom = FolderDao.GetRootFolder(Folders[0]);
                if (0 < Files.Count) rootFrom = FolderDao.GetRootFolderByFile(Files[0]);
                if (rootFrom != null && rootFrom.FolderType == FolderType.TRASH) throw new InvalidOperationException("Can not copy from Trash.");
                if (to.RootFolderType == FolderType.TRASH) throw new InvalidOperationException("Can not copy to Trash.");
            }

            MoveOrCopyFolders(Folders, toFolder, copy);
            MoveOrCopyFiles(Files, toFolder, copy, resolveType);
        }

        private void MoveOrCopyFolders(List<int> folders, int to, bool copy)
        {
            if (folders.Count == 0) return;

            foreach (var id in folders)
            {
                if (Canceled) return;

                var folder = FolderDao.GetFolder(id);
                if (folder == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FolderNotFound;
                }
                else if (!FilesSecurity.CanRead(folder))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_ReadFolder;
                }
                else if (folder.ParentFolderID != to)
                {
                    //if destination folder contains folder with same name then merge folders
                    var conflictFolder = FolderDao.GetFolder(folder.Title, to);

                    if (copy || conflictFolder != null)
                    {
                        int newFolder;
                        if (conflictFolder != null)
                        {
                            newFolder = conflictFolder.ID;
                        }
                        else
                        {
                            newFolder = FolderDao.CopyFolder(id, to);
                            ProcessedFolder(id);
                        }
                        MoveOrCopyFiles(FolderDao.GetFiles(id, false), newFolder, copy, resolveType);
                        MoveOrCopyFolders(FolderDao.GetFolders(id, 0, 0, null).Select(f => f.ID).ToList(), newFolder, copy);
                        if (!copy && conflictFolder != null)
                        {
                            if (FolderDao.GetItemsCount(id, FilterType.None, null, true) == 0 && FilesSecurity.CanDelete(folder))
                            {
                                FolderDao.DeleteFolder(id);
                                ProcessedFolder(id);
                            }
                        }
                    }
                    else
                    {
                        if (FilesSecurity.CanDelete(folder))
                        {
                            FolderDao.MoveFolder(id, to);
                            ProcessedFolder(id);
                        }
                        else
                        {
                            Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                        }
                    }
                }
                ProgressStep();
            }
        }

        private void MoveOrCopyFiles(List<int> files, int to, bool copy, FileConflictResolveType resolveType)
        {
            if (files.Count == 0) return;

            foreach (var id in files)
            {
                if (Canceled) return;

                var file = FileDao.GetFile(id);
                if (file == null)
                {
                    Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else if (!FilesSecurity.CanRead(file))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_ReadFile;
                }
                else if (file.FolderID != to)
                {
                    var conflict = FileDao.GetFile(to, file.Title);
                    if (conflict != null && !FilesSecurity.CanEdit(conflict))
                    {
                        Error = FilesCommonResource.ErrorMassage_SecurityException;
                    }
                    else if (conflict == null)
                    {
                        var newFile = 0;
                        if (copy)
                        {
                            try
                            {
                                newFile = FileDao.CopyFile(id, to);
                                var path = FileDao.GetUniqFileDirectory(id);
                                if (Store.IsDirectory(path))
                                {
                                    Store.CopyDirectory(String.Empty, path, String.Empty, FileDao.GetUniqFileDirectory(newFile));
                                }
                                ProcessedFile(id);
                            }
                            catch
                            {
                                FileDao.DeleteFile(newFile);
                                throw;
                            }
                        }
                        else
                        {
                            if ((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                            {
                                Error = FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile;
                            }
                            else if (FilesSecurity.CanDelete(file))
                            {
                                FileDao.MoveFile(id, to);
                                ProcessedFile(id);
                            }
                            else
                            {
                                Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                            }
                        }
                    }
                    else
                    {
                        if (resolveType == FileConflictResolveType.Overwrite)
                        {
                            using (var stream = Store.IronReadStream(string.Empty, FileDao.GetUniqFilePath(file), 10))
                            {
                                conflict.Version++;
                                Store.Save(FileDao.GetUniqFilePath(conflict), stream);
                                conflict = FileDao.SaveFile(conflict);
                            }

                            NotifyClient.SendUpdateNoticeAsync(SecurityContext.CurrentAccount.ID, conflict);
                            TagDao.SaveTags(
                                NotifySource.Instance.GetSubscriptionProvider().GetRecipients(
                                    NotifyConstants.Event_DocumentInformer, conflict.UniqID).Select(r => Tag.New(new Guid(r.ID), conflict)).ToArray());

                            if (copy)
                            {
                                ProcessedFile(id);
                            }
                            else
                            {
                                if ((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                                {
                                    Error = FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile;
                                }
                                else if (FilesSecurity.CanDelete(file))
                                {
                                    NotifySource.Instance.GetSubscriptionProvider()
                                        .GetRecipients(NotifyConstants.Event_DocumentInformer, file.UniqID)
                                        .ToList()
                                        .ForEach(
                                            r =>
                                            NotifySource.Instance.GetSubscriptionProvider().Subscribe(
                                                NotifyConstants.Event_DocumentInformer, conflict.UniqID, r));

                                    NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(
                                        NotifyConstants.Event_DocumentInformer, file.UniqID);

                                    FileDao.DeleteFile(file.ID);
                                    Store.DeleteDirectory(FileDao.GetUniqFileDirectory(file.ID));
                                    ProcessedFile(id);
                                }
                                else
                                {
                                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                                }
                            }
                        }
                        else if (resolveType == FileConflictResolveType.Skip)
                        {
                            //nothing
                        }
                    }
                }
                ProgressStep();
            }
        }
    }
}