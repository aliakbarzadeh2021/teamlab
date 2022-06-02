using System;
using System.Collections.Generic;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;

namespace ASC.Web.Files.Services.WCFService
{
    internal class FileImportOperation : FileOperation
    {
        private readonly IDocumentProvider docProvider;
        private readonly List<DataToImport> files;
        private readonly int parentId;
        private readonly bool overwrite;
        private readonly string folderName;


        protected override FileOperationType OperationType
        {
            get { return FileOperationType.Import; }
        }


        public FileImportOperation(Tenant tenant, IDocumentProvider docProvider, List<DataToImport> files, int parentId,
                                   bool overwrite, string folderName)
            : base(tenant, null, null)
        {
            Id = Owner.ToString() + OperationType.ToString();
            Source = docProvider.Name;
            this.docProvider = docProvider;
            this.files = files ?? new List<DataToImport>();
            this.parentId = parentId;
            this.overwrite = overwrite;
            this.folderName = folderName;
        }


        protected override double InitProgressStep()
        {
            return files.Count == 0 ? 100d : 100d/files.Count;
        }

        protected override void Do()
        {
            if (files.Count == 0) return;

            var parent = FolderDao.GetFolder(parentId);
            if (parent == null) throw new Exception(FilesCommonResource.ErrorMassage_FolderNotFound);
            if (!FilesSecurity.CanCreate(parent))
                throw new System.Security.SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);
            if (parent.RootFolderType == FolderType.TRASH)
                throw new Exception(FilesCommonResource.ErrorMassage_CreateNewFolderInTrash);

            var to = FolderDao.GetFolder(folderName, parentId);
            if (to == null)
            {
                to = new Folder
                         {
                             FolderType = FolderType.DEFAULT,
                             ParentFolderID = parentId,
                             Title = folderName
                         };
                to.ID = FolderDao.SaveFolder(to);
            }

            foreach (var f in files)
            {
                if (Canceled) return;
                try
                {
                    var size = 0L;
                    using (var stream = docProvider.GetDocumentStream(f.ContentLink, out size))
                    {
                        if (stream == null)
                            throw new Exception("Can not import document " + f.ContentLink + ". Empty stream.");

                        if (SetupInfo.MaxUploadSize < size)
                        {
                            throw new Exception(FilesCommonResource.ErrorMassage_ExceededMaximumFileSize);
                        }

                        var folderId = to.ID;
                        var pos = f.Title.LastIndexOf('/');
                        if (0 < pos)
                        {
                            folderId = GetOrCreateHierarchy(f.Title.Substring(0, pos), to);
                            f.Title = f.Title.Substring(pos + 1);
                        }

                        f.Title = Global.ReplaceInvalidCharsAndTruncate(f.Title);
                        var file = new File
                                       {
                                           Title = f.Title,
                                           FolderID = folderId,
                                           ContentLength = size,
                                           ContentType = "application/octet-stream",
                                       };

                        var conflict = FileDao.GetFile(file.FolderID, file.Title);
                        if (conflict != null)
                        {
                            if (overwrite)
                            {
                                file.ID = conflict.ID;
                                file.Version = conflict.Version + 1;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (size <= 0L)
                        {
                            using (var buffered = stream.GetBuffered())
                            {
                                size = buffered.Length;
                                file.ContentLength = size;
                                try
                                {
                                    file = FileDao.SaveFile(file);
                                    Store.Save(FileDao.GetUniqFilePath(file), buffered, file.Title);
                                }
                                catch (Exception error)
                                {
                                    FileDao.DeleteFile(file.ID);
                                    throw error;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                file = FileDao.SaveFile(file);
                                Store.Save(FileDao.GetUniqFilePath(file), stream, file.Title);
                            }
                            catch (Exception error)
                            {
                                FileDao.DeleteFile(file.ID);
                                throw error;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    Error = error;
                }
                finally
                {
                    ProgressStep();
                }
            }
        }

        private int GetOrCreateHierarchy(string path, Folder root)
        {
            path = path != null ? path.Trim('/') : null;
            if (string.IsNullOrEmpty(path)) return root.ID;

            var pos = path.IndexOf("/");
            var title = 0 < pos ? path.Substring(0, pos) : path;
            path = 0 < pos ? path.Substring(pos + 1) : null;

            title = Global.ReplaceInvalidCharsAndTruncate(title);

            var folder = FolderDao.GetFolder(title, root.ID);
            if (folder == null)
            {
                folder = new Folder
                             {
                                 ParentFolderID = root.ID,
                                 Title = title,
                                 FolderType = FolderType.DEFAULT
                             };
                folder.ID = FolderDao.SaveFolder(folder);
            }
            return GetOrCreateHierarchy(path, folder);
        }
    }
}