using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Files.Api;
using ASC.Web.Studio.Helpers;
using ASC.Web.Studio.Utility;
using log4net;
using File = ASC.Files.Core.File;

namespace ASC.Projects.Engine
{
    public class FileEngine
    {
        private readonly ASC.Projects.Core.DataInterfaces.IDaoFactory factory;
        private readonly IDataStore _projectsStore;

        public FileEngine(ASC.Projects.Core.DataInterfaces.IDaoFactory factory, IDataStore projectsStore)
        {
            this.factory = factory;
            _projectsStore = projectsStore;
        }

        public int GetRoot(int projectId)
        {
            return FilesIntegration.RegisterBunch("projects", "project", projectId.ToString());
        }

        public File GetFile(int id, int version)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                var file = 0 < version ? dao.GetFile(id, version) : dao.GetFile(id);
                return file;
            }
        }

        public File SaveFile(File file, System.IO.Stream stream)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                file = dao.SaveFile(file);
                FilesIntegration.GetStore().Save(dao.GetUniqFilePath(file), stream);
                return file;
            }
        }

        public void RemoveFile(int id)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                var path = dao.GetUniqFileDirectory(id);
                var store = FilesIntegration.GetStore();
                if (store.IsDirectory(path)) store.DeleteDirectory(path);
                dao.DeleteFile(id);
                try
                {
                    _projectsStore.Delete(GetThumbPath(id));
                }
                catch { }
            }
        }

        public Folder SaveFolder(Folder folder)
        {
            using (var dao = FilesIntegration.GetFolderDao())
            {
                folder.ID = dao.SaveFolder(folder);
                return folder;
            }
        }

        public void AttachFileToMessage(int messageId, int fileId)
        {
            using (var dao = FilesIntegration.GetTagDao())
            {
                dao.SaveTags(new Tag("Message" + messageId, TagType.System, Guid.Empty) { EntryType = FileEntryType.File, EntryId = fileId });
                GenerateImageThumb(GetFile(fileId, 0));
            }
        }

        public void AttachFileToTask(int taskId, int fileId)
        {
            using (var dao = FilesIntegration.GetTagDao())
            {
                dao.SaveTags(new Tag("Task" + taskId, TagType.System, Guid.Empty) { EntryType = FileEntryType.File, EntryId = fileId });
                GenerateImageThumb(GetFile(fileId, 0));
            }
        }

        public List<File> GetTaskFiles(Task task)
        {
            if (task == null) return new List<File>();

            using (var tagdao = FilesIntegration.GetTagDao())
            using (var filedao = FilesIntegration.GetFileDao())
            {
                var ids = tagdao.GetTags("Task" + task.ID, TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();
                var files = 0 < ids.Length ? filedao.GetFiles(ids) : new List<File>();
                SetThumbUrls(files);
                return files;
            }
        }

        public List<File> GetMessageFiles(Message message)
        {
            if (message == null) return new List<File>();
            using (var tagdao = FilesIntegration.GetTagDao())
            using (var filedao = FilesIntegration.GetFileDao())
            {
                var ids = tagdao.GetTags("Message" + message.ID, TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();
                var files = 0 < ids.Length ? filedao.GetFiles(ids) : new List<File>();
                SetThumbUrls(files);
                return files;
            }
        }

        private void SetThumbUrls(List<File> files)
        {
            files.ForEach(f =>
                              {
                                  if (f != null && FileUtility.GetFileTypeByFileName(f.Title) == FileType.Image)
                                  {
                                      f.ThumbnailURL = _projectsStore.GetUri(GetThumbPath(f.ID)).ToString();
                                  }
                              });
        }

        private void GenerateImageThumb(File file)
        {
            if (file == null || FileUtility.GetFileTypeByFileName(file.Title) != FileType.Image) return;

            try
            {
                using (var filedao = FilesIntegration.GetFileDao())
                {
                    var filepath = filedao.GetUniqFilePath(file);
                    using (var stream = FilesIntegration.GetStore().GetReadStream(filepath))
                    {
                        var ii = new ImageInfo();
                        ImageHelper.GenerateThumbnail(stream, GetThumbPath(file.ID), ref ii, 128, 96, _projectsStore);
                    }
                }
            }
            catch(Exception ex)
            {
                LogManager.GetLogger("ASC.Web.Projects").Error(ex);
            }
        }

        private static string GetThumbPath(int fileId)
        {
            var s = fileId.ToString("000000");
            return "thumbs/" + s.Substring(0, 2) + "/" + s.Substring(2, 2) + "/" + s.Substring(4) + "/" + fileId.ToString() + ".jpg";
        }

        internal static Hashtable GetFileListInfoHashtable(IEnumerable<Files.Core.File> uploadedFiles)
        {
            var fileListInfoHashtable = new Hashtable();

            if (uploadedFiles != null)
                foreach (var file in uploadedFiles)
                {
                    var fileInfo = String.Format("{0} ({1})", file.Title, Path.GetExtension(file.Title).ToUpper());
                    fileListInfoHashtable.Add(fileInfo, file.ViewUrl);
                }

            return fileListInfoHashtable;
        }
    }
}
