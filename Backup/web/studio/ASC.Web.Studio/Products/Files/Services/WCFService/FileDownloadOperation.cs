using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ICSharpCode.SharpZipLib.Zip;

namespace ASC.Web.Files.Services.WCFService
{
    class FileDownloadOperation : FileOperation
    {
        protected override FileOperationType OperationType
        {
            get { return FileOperationType.Download; }
        }


        public FileDownloadOperation(Tenant tenant, List<int> folders, List<int> files)
            : base(tenant, folders, files)
        {
            Id = Owner.ToString() + OperationType.ToString();//one download per user
        }


        protected override void Do()
        {
            var files = GetFiles();
            if (files == null || files.Count == 0)
            {
                throw new Exception(FilesCommonResource.ErrorMassage_FileNotFound);
            }

            ReplaceLongTitles(files);

            using (var stream = CompressToZip(files))
            {
                if (stream != null)
                {
                    stream.Position = 0;
                    var fileName = UrlConstant.DownloadTitle + ".zip";
                    StoreTmp.Save(
                        string.Empty,
                        string.Format(@"{0}\{1}", Owner, fileName),
                        stream,
                        "application/zip",
                        "attachment; filename=\"" + fileName + "\"");
                    Status = string.Format("{0}?{1}=bulk", FileHandler.FileHandlerPath, UrlConstant.Action);
                }
            }
        }


        private NameValueCollection GetFiles()
        {
            var result = new NameValueCollection();
            if (0 < Files.Count)
            {
                var files = FilesSecurity.FilterRead(FileDao.GetFiles(Files.ToArray())).ToList();
                files.ForEach(f =>
                    result.Add(f.Title,
                        f.ConvertedType == null ?
                            FileDao.GetUniqFilePath(f) :
                            f.ID.ToString())
                    );
                TagDao.RemoveTags(files.Select(f => Tag.New(SecurityContext.CurrentAccount.ID, f)).ToArray());
            }
            if (0 < Folders.Count)
            {
                var filesInFolder = GetFilesInFolders(Folders, string.Empty);
                if (filesInFolder == null) return null;
                result.Add(filesInFolder);
            }
            return result;
        }

        private NameValueCollection GetFilesInFolders(IList<int> folders, string path)
        {
            if (Canceled) return null;

            var result = new NameValueCollection();
            for (int i = 0; i < folders.Count; i++)
            {
                var folder = FolderDao.GetFolder(folders[i]);
                if (folder == null || !FilesSecurity.CanRead(folder)) continue;
                var folderPath = path + folder.Title + "/";

                var files = FilesSecurity.FilterRead(FolderDao.GetFiles(folder.ID, 0, 0, null, FilterType.None, null)).ToList();
                files.ForEach(f =>
                    result.Add(folderPath + f.Title,
                        f.ConvertedType == null ?
                            FileDao.GetUniqFilePath(f) :
                            f.ID.ToString())
                    );
                TagDao.RemoveTags(files.Select(f => Tag.New(SecurityContext.CurrentAccount.ID, f)).ToArray());

                var nestedFolders = FilesSecurity.FilterRead(FolderDao.GetFolders(folder.ID, 0, 0, null)).ToList();
                if (files.Count == 0 && nestedFolders.Count == 0)
                {
                    result.Add(folderPath, String.Empty);
                }

                var filesInFolder = GetFilesInFolders(nestedFolders.ConvertAll(f => f.ID), folderPath);
                if (filesInFolder == null) return null;
                result.Add(filesInFolder);
            }
            return result;
        }


        private Stream CompressToZip(NameValueCollection entries)
        {
            var stream = TempStream.Create();
            using (var zip = new ZipOutputStream(stream))
            {
                zip.IsStreamOwner = false;
                zip.SetLevel(3);

                foreach (var title in entries.AllKeys)
                {
                    if (Canceled)
                    {
                        zip.Dispose();
                        stream.Dispose();
                        return null;
                    }

                    var counter = 0;
                    foreach (var path in entries[title].Split(','))
                    {
                        var newtitle = title;
                        if (0 < counter)
                        {
                            var suffix = " (" + counter + ")";
                            newtitle = 0 < newtitle.IndexOf('.') ? newtitle.Insert(newtitle.LastIndexOf('.'), suffix) : newtitle + suffix;
                        }
                        var zipentry = new ZipEntry(newtitle) { DateTime = DateTime.UtcNow };
                        lock (zip)
                        {
                            ZipConstants.DefaultCodePage = Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage;
                            zip.PutNextEntry(zipentry);
                        }
                        if (!string.IsNullOrEmpty(path))
                        {
                            var fileId = 0;
                            if (int.TryParse(path, out fileId))
                            {
                                var file = Global.DaoFactory.GetFileDao().GetFile(fileId);
                                //Take from converter
                                try
                                {
                                    var readStream = Global.GetConvertedFile(file);
                                    if (readStream != null)
                                    {
                                        readStream.StreamCopyTo(zip);
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                using (var readStream = Store.IronReadStream(string.Empty, path, 10))
                                {
                                    readStream.StreamCopyTo(zip);
                                }
                            }
                        }
                        counter++;
                    }
                    lock (zip)
                    {
                        ZipConstants.DefaultCodePage = Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage;
                        zip.CloseEntry();
                    }
                    ProgressStep();
                }
                return stream;
            }
        }

        private void ReplaceLongTitles(NameValueCollection files)
        {
            foreach (var title in new List<string>(files.AllKeys))
            {
                if (200 < title.Length && 0 < title.IndexOf('/'))
                {
                    var path = files[title];
                    files.Remove(title);

                    var newtitle = "LONG_FOLDER_NAME" + title.Substring(title.LastIndexOf('/'));
                    files.Add("LONG_FOLDER_NAME" + title.Substring(title.LastIndexOf('/')), path);
                }
            }
        }
    }
}