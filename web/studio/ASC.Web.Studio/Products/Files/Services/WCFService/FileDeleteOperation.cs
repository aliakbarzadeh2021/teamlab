using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Services.WCFService
{
    class FileDeleteOperation : FileOperation
    {
        private int trashId;


        protected override FileOperationType OperationType
        {
            get { return FileOperationType.Delete; }
        }


        public FileDeleteOperation(Tenant tenant, List<int> folders, List<int> files)
            : base(tenant, folders, files)
        {
        }


        protected override void Do()
        {
            trashId = FolderDao.GetFolderIDTrash(true);
            Folder root = null;
            if (0 < Folders.Count)
            {
                root = FolderDao.GetRootFolder(Folders[0]);
            }
            else if (0 < Files.Count)
            {
                root = FolderDao.GetRootFolderByFile(Files[0]);
            }
            if (root != null)
            {
                Status += string.Format("folder_{0} ", root.ID);
            }

            DeleteFiles(Files);
            DeleteFolders(Folders);
        }

        private void DeleteFolders(List<int> folders)
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
                else if (!FilesSecurity.CanDelete(folder))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFolder;
                }
                else
                {
                    if (folder.RootFolderType == FolderType.TRASH || folder.RootFolderType == FolderType.BUNCH)
                    {
                        DeleteFiles(FolderDao.GetFiles(id, false));
                        DeleteFolders(FolderDao.GetFolders(id, 0, 0, null).Select(f => f.ID).ToList());
                        if (FolderDao.GetItemsCount(id, FilterType.None, null, true) == 0)
                        {
                            FolderDao.DeleteFolder(id);
                            ProcessedFolder(id);
                        }
                        else
                        {
                            Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteEditingFolder;
                        }
                    }
                    else
                    {
                        FolderDao.MoveFolder(id, trashId);
                        FolderDao.GetFiles(id, true).ForEach(fid => NoticeDelete(FileDao.GetFile(fid)));
                        ProcessedFolder(id);
                    }
                }
                ProgressStep();
            }
        }

        private void DeleteFiles(List<int> files)
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
                else if ((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile;
                }
                else if (!FilesSecurity.CanDelete(file))
                {
                    Error = FilesCommonResource.ErrorMassage_SecurityException_DeleteFile;
                }
                else
                {
                    if (file.RootFolderType == FolderType.TRASH || file.RootFolderType == FolderType.BUNCH)
                    {
                        var path = FileDao.GetUniqFileDirectory(id);

                        FileDao.DeleteFile(id);
                        if (Store.IsDirectory(path))
                        {
                            Store.DeleteDirectory(path);
                        }
                    }
                    else
                    {
                        FileDao.MoveFile(id, trashId);
                        NoticeDelete(file);
                    }
                    ProcessedFile(id);
                }
                ProgressStep();
            }
        }

        private void NoticeDelete(File file)
        {
            //NotifyClient.SendDeleteNoticeAsync(file);
            NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_DocumentInformer, file.UniqID);
        }
    }
}