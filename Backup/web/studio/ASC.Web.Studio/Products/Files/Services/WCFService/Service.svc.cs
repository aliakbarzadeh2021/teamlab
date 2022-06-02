using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.NotifyService;
using Microsoft.ServiceModel.Web;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Services.WCFService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceErrorLoggingBehavior]
    public class Service : IFileStorageService
    {
        private readonly ProgressQueue tasks = new ProgressQueue(10, TimeSpan.FromMinutes(5), true);
        private readonly IDictionary<string, IList<string>> tips = new Dictionary<string, IList<string>>();
        private readonly List<int> updates = new List<int>();


        public ItemList<Folder> GetFolders(String parentFolderID, string from, string count, OrderBy orderBy)
        {
            using (var folderDao = GetFolderDao())
            {
                var parentId = int.Parse(parentFolderID);
                int total;
                var folders = GetEntries(folderDao.GetFolder(parentId), FilterType.FoldersOnly, Guid.Empty, orderBy, 0,
                                         0, out total);
                return new ItemList<Folder>(folders.OfType<Folder>());
            }
        }

        public Folder GetFolder(String folderID)
        {
            using (var folderDao = GetFolderDao())
            {
                var f = folderDao.GetFolder(Convert.ToInt32(folderID));
                return FileSecurity.CanRead(f) ? f : null;
            }
        }

        public DataWrapper GetFolderItems(string sparentId, string sfrom, string scount, string sfilter, OrderBy orderBy,
                                          string ssubject, string sthumbnailUrl)
        {
            var parentId = int.Parse(sparentId);
            var from = Convert.ToInt32(sfrom);
            var count = Convert.ToInt32(scount);
            var filter = (FilterType) Convert.ToInt32(sfilter);
            var subjectId = string.IsNullOrEmpty(ssubject) ? Guid.Empty : new Guid(ssubject);

            using (var folderDao = GetFolderDao())
            {
                var parent = folderDao.GetFolder(parentId);
                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanRead(parent), FilesCommonResource.ErrorMassage_SecurityException_ViewFolder);
                ErrorIf(parent.RootFolderType == FolderType.TRASH && parent.ID != Global.FolderTrash,
                        FilesCommonResource.ErrorMassage_ViewTrashItem);

                var total = 0;
                var entries = GetEntries(parent, filter, subjectId, orderBy, from, count, out total);

                var breadCrumbs = FileSecurity.FilterRead(folderDao.GetParentFolders(parentId)).ToList();

                var firstVisible = breadCrumbs.ElementAtOrDefault(0);
                if (firstVisible != null && firstVisible.FolderType == FolderType.DEFAULT) //not first level
                {
                    var root = folderDao.GetFolder(folderDao.GetFolderIDShare(false));
                    if (root != null)
                    {
                        firstVisible.ParentFolderID = root.ID;
                        breadCrumbs.Insert(0, root);
                    }
                }

                var prevVisible = breadCrumbs.ElementAtOrDefault(breadCrumbs.Count() - 2);
                if (prevVisible != null)
                {
                    parent.ParentFolderID = prevVisible.ID;
                }

                parent.Shareable = (parent.RootFolderType == FolderType.USER &&
                                    parent.RootFolderCreator == SecurityContext.CurrentAccount.ID) ||
                                   (parent.RootFolderType == FolderType.COMMON &&
                                    CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID,
                                                                          ASC.Core.Users.Constants.GroupAdmin.ID));

                return new DataWrapper
                           {
                               Folders = entries.OfType<Folder>().ToList(),
                               Files = entries.OfType<File>().ToList(),
                               Total = total,

                               FolderPathParts =
                                   new ItemDictionary<string, string>(breadCrumbs.ToDictionary(f => f.ID.ToString(),
                                                                                               f => f.Title)),
                               FolderInfo = parent,

                               Quota = FileQuotaController.QuotaGet(),
                               Tip = GetTip(),
                           };
            }
        }


        private IEnumerable<FileEntry> GetEntries(Folder parent, FilterType filter, Guid subjectId, OrderBy orderBy,
                                                  int from, int count, out int total)
        {
            using (var folderDao = GetFolderDao())
            {
                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);

                var entries = Enumerable.Empty<FileEntry>();

                if (parent.FolderType == FolderType.SHARE)
                {
                    var shared = (IEnumerable<FileEntry>) FileSecurity.GetSharesForMe();
                    shared = FilterEntries(shared, filter, subjectId)
                        .Where(f => f.CreateBy != SecurityContext.CurrentAccount.ID && // don't show my files
                                    f.RootFolderType == FolderType.USER);
                        // don't show common files (common files can read)
                    entries = entries.Concat(shared);
                }
                else
                {
                    if (filter == FilterType.None || filter == FilterType.FoldersOnly)
                    {
                        var folders =
                            folderDao.GetFolders(parent.ID, 0, 0, new OrderBy(SortedByType.AZ, true)).Cast<FileEntry>();
                        folders = FileSecurity.FilterRead(folders);
                        entries = entries.Concat(folders);
                    }
                    if (filter != FilterType.FoldersOnly)
                    {
                        var files =
                            folderDao.GetFiles(parent.ID, 0, 0, new OrderBy(SortedByType.AZ, true), filter, subjectId).
                                Cast<FileEntry>();
                        files = FileSecurity.FilterRead(files);
                        entries = entries.Concat(files);
                    }
                }

                entries = SortEntries(entries, orderBy);

                total = entries.Count();
                if (0 < from) entries = entries.Skip(from);
                if (0 < count) entries = entries.Take(count);

                return entries.ToList();
            }
        }

        private IEnumerable<FileEntry> FilterEntries(IEnumerable<FileEntry> entries, FilterType filter, Guid subjectId)
        {
            Func<FileEntry, bool> where = _ => true;
            if (filter == FilterType.ByUser)
            {
                where = f => f.CreateBy == subjectId;
            }
            else if (filter == FilterType.ByDepartment)
            {
                var users = CoreContext.UserManager.GetUsersByGroup(subjectId).Select(u => u.ID);
                where = f => users.Contains(f.CreateBy);
            }
            else if (filter == FilterType.FilesOnly || filter == FilterType.DocumentsOnly ||
                     filter == FilterType.PicturesOnly || filter == FilterType.PresentationsOnly ||
                     filter == FilterType.SpreadsheetsOnly)
            {
                where =
                    f => f is File && (FileFormats.GetFileType(f.Title) == filter || filter == FilterType.FilesOnly);
            }
            else if (filter == FilterType.FoldersOnly)
            {
                where = f => f is Folder;
            }
            return entries.Where(where);
        }

        private IEnumerable<FileEntry> SortEntries(IEnumerable<FileEntry> entries, OrderBy orderBy)
        {
            Comparison<FileEntry> sorter;
            var c = orderBy.IsAsc ? 1 : -1;
            switch (orderBy.SortedBy)
            {
                case SortedByType.Author:
                    sorter =
                        (x, y) =>
                        c*
                        string.Compare(CoreContext.UserManager.GetUsers(x.CreateBy).DisplayUserName(),
                                       CoreContext.UserManager.GetUsers(y.CreateBy).DisplayUserName());
                    break;

                case SortedByType.DateAndTime:
                    sorter = (x, y) => c*DateTime.Compare(x.ModifiedOn, y.ModifiedOn);
                    break;
                case SortedByType.Size:
                    sorter = (x, y) => x is File && y is File
                                           ? c*((File) x).ContentLength.CompareTo(((File) y).ContentLength)
                                           : string.Compare(x.Title, y.Title);
                    break;

                default:
                    sorter = (x, y) => c*string.Compare(x.Title, y.Title);
                    break;
            }

            // folders on top
            var folders = entries.OfType<Folder>().Cast<FileEntry>().ToList();
            var files = entries.OfType<File>().Cast<FileEntry>().ToList();
            folders.Sort(sorter);
            files.Sort(sorter);

            return folders.Concat(files);
        }

        #region File Manager

        public ItemDictionary<Int32, String> GetSiblingsFile(String fileID, OrderBy orderBy)
        {
            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var file = fileDao.GetFile(Convert.ToInt32(fileID));
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound, true);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile, true);

                var folder = folderDao.GetFolder(file.FolderID);
                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound, true);
                ErrorIf(folder.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem, true);

                var result = new ItemDictionary<Int32, String>();
                FileSecurity.FilterRead(folderDao.GetFiles(folder.ID, 0, 0, orderBy, FilterType.None, null))
                    .Where(f =>
                           FileFormats.IsPreviewed(f.Title)
                           && FileFormats.GetFileType(f.Title).Equals(FileFormats.GetFileType(file.Title))
                    )
                    .ToList()
                    .ForEach(f => result.Add(f.ID, f.Title));

                return result;
            }
        }

        public File CreateNewFile(String folderID, String fileTitle)
        {
            var parentFolderID = Convert.ToInt32(folderID);
            fileTitle = Global.ReplaceInvalidCharsAndTruncate(fileTitle);

            using (var fileDao = GetFileDao())
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(parentFolderID);
                ErrorIf(folder == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(folder.RootFolderType == FolderType.TRASH,
                        FilesCommonResource.ErrorMassage_CreateNewFolderInTrash);
                ErrorIf(!FileSecurity.CanCreate(folder), FilesCommonResource.ErrorMassage_SecurityException_Create);

                var templatePath =
                    HostingEnvironment.MapPath(
                        "~/Products/Files/App_Data/template/0648530C-71B0-47c2-ADE2-593D6F286783" +
                        FileFormats.GetExtension(fileTitle));
                var length = new FileInfo(templatePath).Length;

                var file = new File
                               {
                                   Title = fileTitle,
                                   FolderID = parentFolderID,
                                   FileStatus = FileStatus.IsEditing,
                                   ContentLength = length
                               };

                switch (FileFormats.GetFileType(file.Title))
                {
                    case FilterType.PresentationsOnly:
                        file.ContentType = "application/vnd.oasis.opendocument.presentation";
                        file.ConvertedType = ".zip";
                        break;
                    case FilterType.SpreadsheetsOnly:
                        file.ContentType = "application/vnd.oasis.opendocument.spreadsheet";
                        file.ConvertedType = ".xlsx";
                        break;
                    case FilterType.DocumentsOnly:
                        file.ContentType = "application/vnd.oasis.opendocument.text";
                        file.ConvertedType = ".zip";
                        break;
                    default:
                        throw new NotSupportedException(FilesCommonResource.ErrorMassage_NotSupportedFormat);
                }

                file = fileDao.SaveFile(file);
                using (var stream = System.IO.File.OpenRead(templatePath))
                {
                    GetStore().Save(fileDao.GetUniqFilePath(file), stream, file.Title);
                }
                return file;
            }
        }

        public void TrackEditFile(String fileID, string docKey, bool isFinish)
        {
            int id;
            if (!int.TryParse(fileID, out id)) return;

            ErrorIf(docKey != Global.GetDocKey(id, -1), FilesCommonResource.ErrorMassage_SecurityException);

            lock (File.NowEditing)
            {
                if (isFinish)
                {
                    File.NowEditing.Remove(id);
                }
                else
                {
                    File.NowEditing[id] = DateTime.UtcNow;
                }
            }
        }

        public List<int> CheckEditing(ItemList<String> filesId)
        {
            var result = new List<int>();
            foreach (var sid in filesId)
            {
                int id;
                if (int.TryParse(sid, out id))
                {
                    if (File.NowEditing.ContainsKey(id))
                        result.Add(id);
                }
            }
            return result;
        }

        public string MakeEditFile(String fileID)
        {
            using (var dao = GetFileDao())
            {
                var file = dao.GetFile(Convert.ToInt32(fileID));
                if (file == null || (file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing)
                    return string.Empty;
                return EmailValidationKeyProvider.GetEmailKey(file.ID.ToString() + file.Title) + "." +
                       SecurityContext.CurrentAccount.ID.ToString("N").ToUpper();
            }
        }

        public Stream GetFileStreamForEditing()
        {
            var queryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            if (String.IsNullOrEmpty(queryParameters["id"]))
                throw new ArgumentException("Can not find id parameter in url.");
            if (String.IsNullOrEmpty(queryParameters["ticket"]))
                throw new ArgumentException("Can not find ticket parameter in url.");

            using (var dao = GetFileDao())
            using (var tagDao = GetTagDao())
            {
                var file = dao.GetFile(Convert.ToInt32(queryParameters["id"]));
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);

                var userId = ValidateTicketAndGetUserId(file, queryParameters["ticket"]);
                ErrorIf(!FileSecurity.CanRead(file, userId), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);
                ErrorIf(!FileFormats.IsEdited(file.Title), FilesCommonResource.ErrorMassage_NotSupportedFormat);

                tagDao.RemoveTags(Tag.New(userId, file));

                WebOperationContext.Current.OutgoingResponse.ContentLength = file.ContentLength;
                WebOperationContext.Current.OutgoingResponse.ContentType = file.ContentType;

                return file.ConvertedType == null
                           ? GetStore().GetReadStream(dao.GetUniqFilePath(file))
                           : Global.GetConvertedFile(file);
            }
        }

        public void SaveEditedFile(Stream stream)
        {
            var queryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            if (string.IsNullOrEmpty(queryParameters["id"]))
                throw new ArgumentException("Can not find id parameter in url.");
            if (string.IsNullOrEmpty(queryParameters["ticket"]))
                throw new ArgumentException("Can not find ticket parameter in url.");

            using (var fileDao = GetFileDao())
            using (var tagDao = GetTagDao())
            {
                var file = fileDao.GetFile(Convert.ToInt32(queryParameters["id"]));
                var userId = ValidateTicketAndGetUserId(file, queryParameters["ticket"]);
                ErrorIf(!FileSecurity.CanEdit(file, userId), FilesCommonResource.ErrorMassage_SecurityException);

                var parser = new MultipartParser(stream);
                if (!parser.Success) stream.Seek(0, SeekOrigin.Begin);

                var recipients = NotifySource.Instance.GetSubscriptionProvider()
                    .GetRecipients(NotifyConstants.Event_DocumentInformer, file.UniqID)
                    .Select(id => new Guid(id.ID))
                    .Where(id => id != userId);

                tagDao.SaveTags(recipients.Select(r => Tag.New(r, file)).ToArray());

                file.Version++;
                file.ContentLength = parser.FileContents.Length;
                file.ConvertedType = null;

                if (file.Title.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase) ||
                    file.Title.EndsWith(".docx", StringComparison.CurrentCultureIgnoreCase) ||
                    file.Title.EndsWith(".pptx", StringComparison.CurrentCultureIgnoreCase))
                {
                    file.Title = file.Title.TrimEnd('x', 'X');
                }

                file = fileDao.SaveFile(file);
                GetStore().Save(fileDao.GetUniqFilePath(file),
                                parser.Success ? new MemoryStream(parser.FileContents) : stream, file.Title);

                if (file.ContentLength != 0) NotifyClient.SendUpdateNoticeAsync(userId, file);
            }
        }

        public String FileRename(String fileID, string newTitle)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(Convert.ToInt32(fileID));
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanEdit(file), FilesCommonResource.ErrorMassage_SecurityException_RenameFile);
                ErrorIf((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing,
                        FilesCommonResource.ErrorMassage_UpdateEditingFile);

                newTitle = Global.ReplaceInvalidCharsAndTruncate(newTitle);
                if (string.Compare(file.Title, newTitle, false) != 0)
                {
                    fileDao.FileRename(file.ID, newTitle);
                }
                return newTitle;
            }
        }

        public ItemList<File> GetFileHistory(String id)
        {
            using (var fileDao = GetFileDao())
            {
                var fileId = Convert.ToInt32(id);
                var file = fileDao.GetFile(fileId);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                return new ItemList<File>(fileDao.GetFileHistory(fileId));
            }
        }

        public File UpdateToVersion(String fileID, String version)
        {
            using (var fileDao = GetFileDao())
            using (var tagDao = GetTagDao())
            {
                var fromFile = fileDao.GetFile(Convert.ToInt32(fileID), Convert.ToInt32(version));
                ErrorIf(!FileSecurity.CanEdit(fromFile), FilesCommonResource.ErrorMassage_SecurityException_EditFile);
                ErrorIf((fromFile.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing,
                        FilesCommonResource.ErrorMassage_SecurityException_UpdateEditingFile);

                lock (updates)
                {
                    ErrorIf(updates.Contains(fromFile.ID), FilesCommonResource.ErrorMassage_UpdateEditingFile);
                    updates.Add(fromFile.ID);
                }

                try
                {
                    var currFile = fileDao.GetFile(Convert.ToInt32(fileID));
                    var newFile = new File
                                      {
                                          ID = fromFile.ID,
                                          Version = currFile.Version + 1,
                                          Title = fromFile.Title,
                                          ContentLength = fromFile.ContentLength,
                                          ContentType = fromFile.ContentType,
                                          FileStatus = fromFile.FileStatus,
                                          FolderID = fromFile.FolderID,
                                          CreateBy = fromFile.CreateBy,
                                          CreateOn = fromFile.CreateOn,
                                          ModifiedBy = fromFile.ModifiedBy,
                                          ModifiedOn = fromFile.ModifiedOn,
                                          ConvertedType = fromFile.ConvertedType
                                      };
                    newFile = fileDao.SaveFile(newFile);

                    var fromFilePath = fileDao.GetUniqFilePath(fromFile);
                    var store = GetStore();
                    using (var stream = store.IronReadStream(string.Empty, fromFilePath, 10))
                    {
                        store.Save(fileDao.GetUniqFilePath(newFile), stream, newFile.Title);
                    }

                    NotifyClient.SendUpdateNoticeAsync(SecurityContext.CurrentAccount.ID, newFile);

                    var recipients = NotifySource.Instance.GetSubscriptionProvider()
                        .GetRecipients(NotifyConstants.Event_DocumentInformer, newFile.UniqID)
                        .Select(id => new Guid(id.ID))
                        .Where(id => id != SecurityContext.CurrentAccount.ID)
                        .ToList();
                    tagDao.SaveTags(recipients.Select(r => Tag.New(r, newFile)).ToArray());

                    return newFile;
                }
                finally
                {
                    lock (updates) updates.Remove(fromFile.ID);
                }
            }
        }

        #endregion


        #region Import

        public bool IsZohoAuthentificated(string login, string pass)
        {
            try
            {
                new ZohoDocumentProvider(login, pass);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public ItemList<DataToImport> GetGoogleDocs(String token)
        {
            using (var google = new GoogleDocumentProvider(token))
            {
                return DocumentsToDataImport(google.GetDocuments());
            }
        }

        public ItemList<DataToImport> GetZohoDocs(string login, string pass)
        {
            return DocumentsToDataImport(new ZohoDocumentProvider(login, pass).GetDocuments());
        }

        public ItemList<DataToImport> GetBoxNetDocs(string token)
        {
            return DocumentsToDataImport(new BoxDocumentProvider(token).GetDocuments());
        }

        public ItemList<FileOperationResult> ExecImportFromGoogleDocs(String token, String tofolder,
                                                                      String ignoreCoincidenceFiles,
                                                                      List<DataToImport> dataToImport)
        {
            using (var google = new GoogleDocumentProvider(token))
            {
                return ImportDocuments(google, dataToImport, tofolder, ignoreCoincidenceFiles,
                                       FilesCommonResource.ImportFromGoogle);
            }
        }

        public ItemList<FileOperationResult> ExecImportFromZoho(String login, String pass, String tofolder,
                                                                String ignoreCoincidenceFiles,
                                                                List<DataToImport> dataToImport)
        {
            return ImportDocuments(new ZohoDocumentProvider(login, pass), dataToImport, tofolder, ignoreCoincidenceFiles,
                                   FilesCommonResource.ImportFromZoho);
        }

        public ItemList<FileOperationResult> ExecImportFromBoxNet(String token, String tofolder,
                                                                  String ignoreCoincidenceFiles,
                                                                  List<DataToImport> dataToImport)
        {
            return ImportDocuments(new BoxDocumentProvider(token), dataToImport, tofolder, ignoreCoincidenceFiles,
                                   FilesCommonResource.ImportFromBoxNet);
        }

        private ItemList<FileOperationResult> ImportDocuments(IDocumentProvider provider, List<DataToImport> documents,
                                                              string parent, string overwriteStr, string folderName)
        {
            bool overwrite;
            bool.TryParse(overwriteStr, out overwrite);

            var import = new FileImportOperation(
                CoreContext.TenantManager.GetCurrentTenant(),
                provider,
                documents,
                Convert.ToInt32(parent),
                overwrite,
                folderName);

            lock (tasks)
            {
                var oldTask = tasks.GetStatus(import.Id.ToString());
                if (oldTask != null && !oldTask.IsCompleted)
                {
                    throw new InvalidOperationException("Too many imports.");
                }
                tasks.Add(import);
            }
            return GetTasksStatuses();
        }

        private ItemList<DataToImport> DocumentsToDataImport(IEnumerable<Document> documents)
        {
            var folders = documents.Where(d => d.IsFolder).ToDictionary(d => d.Id);
            return new ItemList<DataToImport>(
                documents
                    .Where(d => !d.IsFolder)
                    .Select(d => new DataToImport
                                     {
                                         Title =
                                             GetDocumentPath(folders, d.Parent) +
                                             Global.ReplaceInvalidCharsAndTruncate(d.Title),
                                         ContentLink = d.ContentLink,
                                         CreateBy =
                                             d.CreateBy ??
                                             CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).
                                                 DisplayUserName(),
                                         CreateOn = d.CreateOn.ToShortDateString()
                                     })
                    .OrderBy(d => d.Title));
        }

        private string GetDocumentPath(IDictionary<string, Document> folders, string parentId)
        {
            if (string.IsNullOrEmpty(parentId) || !folders.ContainsKey(parentId)) return string.Empty;
            var parent = folders[parentId];
            return GetDocumentPath(folders, parent.Parent) + Global.ReplaceInvalidCharsAndTruncate(parent.Title) + "/";
        }

        #endregion Import


        public ItemList<FileOperationResult> GetTasksStatuses()
        {
            var operations = tasks.GetItems()
                .Where(t => t is FileOperation && ((FileOperation) t).Owner == SecurityContext.CurrentAccount.ID)
                .Select(o => tasks.GetStatus(o.Id.ToString()))
                .Select(o => ((FileOperation) o).GetResult())
                .ToList();

            return new ItemList<FileOperationResult>(operations);
        }

        public ItemList<FileOperationResult> TerminateTasks()
        {
            var statuses = GetTasksStatuses().ToList();
            statuses.ForEach(s => { if (s.OperationType != FileOperationType.Import) s.Progress = 100; });

            foreach (var i in tasks.GetItems())
            {
                try
                {
                    var task = i as FileOperation;
                    if (task != null && task.GetResult().OperationType != FileOperationType.Import)
                    {
                        task.Terminate();
                        tasks.Remove(task);
                    }
                }
                catch (Exception ex)
                {
                    log4net.LogManager.GetLogger("ASC.Web.Files").Error(ex);
                }
            }
            return new ItemList<FileOperationResult>(statuses);
        }

        public ItemList<FileOperationResult> BulkDownload(ItemList<String> items)
        {
            List<int> folders;
            List<int> files;
            ParseArrayItems(items, out folders, out files);

            var task = new FileDownloadOperation(CoreContext.TenantManager.GetCurrentTenant(), folders, files);

            lock (tasks)
            {
                var oldTask = tasks.GetStatus(task.Id.ToString());
                if (oldTask != null)
                {
                    if (oldTask.IsCompleted)
                    {
                        tasks.Remove(oldTask);
                    }
                    else
                    {
                        throw new InvalidOperationException("Too many downloads.");
                    }
                }
                tasks.Add(task);
            }

            return GetTasksStatuses();
        }


        public Folder CreateNewFolder(String title, String parentFolderID)
        {
            if (string.IsNullOrEmpty(title) || String.IsNullOrEmpty(parentFolderID)) throw new ArgumentException();

            title = Global.ReplaceInvalidCharsAndTruncate(title);
            using (var folderDao = GetFolderDao())
            {
                var parent = folderDao.GetFolder(Convert.ToInt32(parentFolderID));
                ErrorIf(parent == null, FilesCommonResource.ErrorMassage_FolderNotFound);
                ErrorIf(!FileSecurity.CanCreate(parent), FilesCommonResource.ErrorMassage_SecurityException_Create);

                return folderDao.GetFolder(folderDao.SaveFolder(new Folder {Title = title, ParentFolderID = parent.ID}));
            }
        }

        public String FolderRename(String folderID, String newTitle)
        {
            using (var folderDao = GetFolderDao())
            {
                var folder = folderDao.GetFolder(Convert.ToInt32(folderID));
                ErrorIf(!FileSecurity.CanEdit(folder), FilesCommonResource.ErrorMassage_SecurityException_RenameFolder);

                newTitle = Global.ReplaceInvalidCharsAndTruncate(newTitle);
                if (string.Compare(folder.Title, newTitle, false) != 0)
                {
                    folderDao.RenameFolder(folder.ID, newTitle);
                }
                return newTitle;
            }
        }

        public File GetLastFileVersion(String fileID, String setThumbnailURLBool)
        {
            using (var fileDao = GetFileDao())
            {
                var file = fileDao.GetFile(Convert.ToInt32(fileID));
                ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound);
                ErrorIf(!FileSecurity.CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile);

                return file;
            }
        }

        public ItemDictionary<int, String> MoveOrCopyFilesCheck(ItemList<String> items, String destFolderID)
        {
            var result = new ItemDictionary<int, String>();
            if (items.Count == 0) return result;

            List<int> folders;
            List<int> files;
            ParseArrayItems(items, out folders, out files);

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var toFolder = folderDao.GetFolder(Convert.ToInt32(destFolderID));
                if (toFolder == null) return result;
                ErrorIf(!FileSecurity.CanCreate(toFolder), FilesCommonResource.ErrorMassage_SecurityException_Create, true);

                foreach (var id in files)
                {
                    var file = fileDao.GetFile(id);
                    if (file != null && fileDao.IsExist(file.Title, toFolder.ID))
                    {
                        result.Add(id, file.Title);
                    }
                }

                foreach (var pair in folderDao.CanMoveOrCopy(folders.ToArray(), toFolder.ID))
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            return result;
        }

        public ItemList<FileOperationResult> MoveOrCopyItems(ItemList<String> items, String destFolderID,
                                                             String overwriteFiles, String isCopyOperation)
        {
            if (items.Count != 0)
            {
                List<int> foldersID;
                List<int> filesID;
                ParseArrayItems(items, out foldersID, out filesID);

                var task = new FileMoveCopyOperation(
                    CoreContext.TenantManager.GetCurrentTenant(),
                    foldersID,
                    filesID,
                    Convert.ToInt32(destFolderID),
                    Convert.ToBoolean(isCopyOperation),
                    Convert.ToBoolean(overwriteFiles) ? FileConflictResolveType.Overwrite : FileConflictResolveType.Skip);

                tasks.Add(task);
            }
            return GetTasksStatuses();
        }

        public ItemList<FileOperationResult> DeleteItems(ItemList<String> items) 
        {
            List<int> foldersID;
            List<int> filesID;
            ParseArrayItems(items, out foldersID, out filesID);

            var task = new FileDeleteOperation(CoreContext.TenantManager.GetCurrentTenant(), foldersID, filesID);
            tasks.Add(task);

            return GetTasksStatuses();
        }

        public ItemList<FileOperationResult> EmptyTrash()
        {
            using (var dao = GetFolderDao())
            {
                var trashId = dao.GetFolderIDTrash(true);
                var folders = dao.GetFolders(trashId, 0, int.MaxValue, null).Select(f => f.ID).ToList();
                var files = dao.GetFiles(trashId, false).ToList();
                var task = new FileDeleteOperation(CoreContext.TenantManager.GetCurrentTenant(), folders, files);
                tasks.Add(task);
            }
            return GetTasksStatuses();
        }

        public ItemList<Favorite> GetFavorites()
        {
            using (var dao = GetTagDao())
            {
                var result = new List<Favorite>();
                foreach (var t in dao.GetTags(SecurityContext.CurrentAccount.ID, TagType.Favorite))
                {
                    result.Add(Favorite.FromTag(t));
                }
                return new ItemList<Favorite>(result);
            }
        }

        public ItemList<Favorite> AddToFavorites(String folderPath, String title)
        {
            using (var dao = GetTagDao())
            {
                var result = new List<Favorite>();
                foreach (var t in dao.SaveTags(Tag.Favorite(title, SecurityContext.CurrentAccount.ID, folderPath)))
                {
                    result.Add(Favorite.FromTag(t));
                }
                return new ItemList<Favorite>(result);
            }
        }

        public void DeleteFavorite(String favoriteID)
        {
            if (string.IsNullOrEmpty(favoriteID)) return;
            using (var dao = GetTagDao())
            {
                dao.RemoveTags(Convert.ToInt32(favoriteID));
            }
        }

        #region Group Manager

        public ItemList<GroupInfoWrapper> GetGroups()
        {
            var groups = CoreContext.GroupManager.GetGroups().Select(g => new GroupInfoWrapper(g, true))
                .Concat(
                    CoreContext.GroupManager.GetGroups(FileConst.GroupCategoryId).Select(
                        g => new GroupInfoWrapper(g, false)))
                .OrderBy(g => g.Name)
                .ToList();
            groups.Insert(0, new GroupInfoWrapper(ASC.Core.Users.Constants.GroupAdmin, true));
            groups.Insert(0, new GroupInfoWrapper(ASC.Core.Users.Constants.GroupEveryone, true));
            return new ItemList<GroupInfoWrapper>(groups);
        }

        public ItemList<GroupInfoWrapper> SaveGroup(string id, string name, ItemList<Guid> usersID)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            var gi = string.IsNullOrEmpty(id) || id == "0"
                         ? new GroupInfo(FileConst.GroupCategoryId) {Name = name}
                         : CoreContext.GroupManager.GetGroupInfo(new Guid(id));

            if (gi.CategoryID == FileConst.GroupCategoryId)
            {
                gi.Name = name;
                gi = CoreContext.GroupManager.SaveGroupInfo(gi);

                foreach (var u in CoreContext.UserManager.GetUsersByGroup(gi.ID))
                {
                    CoreContext.UserManager.RemoveUserFromGroup(u.ID, gi.ID);
                }
                if (usersID != null && 0 < usersID.Count)
                {
                    usersID.ForEach(uId => CoreContext.UserManager.AddUserIntoGroup(uId, gi.ID));
                }
            }
            return new ItemList<GroupInfoWrapper> {new GroupInfoWrapper(gi, false)};
        }

        public void DeleteGroup(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            var g = CoreContext.GroupManager.GetGroupInfo(new Guid(id));
            if (g.CategoryID == FileConst.GroupCategoryId)
            {
                CoreContext.GroupManager.DeleteGroup(g.ID);
                FileSecurity.RemoveSubject(g.ID);
            }
        }

        public ItemList<Guid> GetUsersByGroup(string id)
        {
            if (string.IsNullOrEmpty(id)) return new ItemList<Guid>();

            var users = CoreContext.UserManager.GetUsersByGroup(new Guid(id)).Select(u => u.ID);
            return new ItemList<Guid>(users);
        }

        #endregion Group Manager

        public ItemList<AceWrapper> GetSharedInfo(ItemList<String> items)
        {
            if (items == null || items.Count == 0) return new ItemList<AceWrapper>();

            List<int> folders;
            List<int> files;
            ParseArrayItems(items, out folders, out files);

            var result = new ItemList<AceWrapper>();

            using (var folderDao = GetFolderDao())
            using (var fileDao = GetFileDao())
            {
                var entries = folders.Select(id => (FileEntry) folderDao.GetFolder(id))
                    .Concat(files.Select(id => (FileEntry) fileDao.GetFile(id)));
                foreach (var f in entries)
                {
                    var records = FileSecurity
                        .GetShares(f)
                        .GroupBy(r => r.Subject)
                        .Select(g => g.OrderBy(r => r.Level).ThenByDescending(r => r.Share).FirstOrDefault());

                    foreach (var r in records)
                    {
                        var u = CoreContext.UserManager.GetUsers(r.Subject);
                        var isgroup = false;
                        var title = u.DisplayUserName(true);

                        if (u.ID == ASC.Core.Users.Constants.LostUser.ID)
                        {
                            var g = CoreContext.GroupManager.GetGroupInfo(r.Subject);
                            isgroup = true;
                            title = g.Name;

                            if (g.ID == ASC.Core.Users.Constants.LostGroupInfo.ID)
                            {
                                FileSecurity.RemoveSubject(r.Subject);
                                continue;
                            }
                        }

                        var w = new AceWrapper
                                    {
                                        SubjectId = r.Subject,
                                        SubjectName = title,
                                        SubjectGroup = isgroup,
                                        Share = r.Share,
                                        Owner =
                                            f.RootFolderType == FolderType.USER
                                                ? f.RootFolderCreator == r.Subject
                                                : f.CreateBy == r.Subject,
                                    };
                        result.Add(w);
                    }
                    if (!result.Any(w => w.Owner))
                    {
                        var ownerId = f.RootFolderType == FolderType.USER ? f.RootFolderCreator : f.CreateBy;
                        var w = new AceWrapper
                                    {
                                        SubjectId = ownerId,
                                        SubjectName = FileEntry.GetUserName(ownerId),
                                        SubjectGroup = false,
                                        Share = ASC.Files.Core.Security.FileShare.ReadWrite,
                                        Owner = true,
                                    };
                        result.Add(w);
                    }
                    if (f.RootFolderType == FolderType.COMMON)
                    {
                        if (!result.Any(w => w.SubjectId == ASC.Core.Users.Constants.GroupAdmin.ID))
                        {
                            var w = new AceWrapper
                                        {
                                            SubjectId = ASC.Core.Users.Constants.GroupAdmin.ID,
                                            SubjectGroup = true,
                                            Share = ASC.Files.Core.Security.FileShare.ReadWrite,
                                            Owner = false,
                                            LockedRights = true,
                                        };
                            result.Add(w);
                        }
                        if (!result.Any(w => w.SubjectId == ASC.Core.Users.Constants.GroupEveryone.ID))
                        {
                            var w = new AceWrapper
                                        {
                                            SubjectId = ASC.Core.Users.Constants.GroupEveryone.ID,
                                            SubjectGroup = true,
                                            Share = FileSecurity.DefaultCommonShare,
                                            Owner = false,
                                        };
                            result.Add(w);
                        }
                    }
                }
            }

            result.Sort((x, y) => string.Compare(x.SubjectName, y.SubjectName));
            return result;
        }

        public void SetAceObject(ItemList<AceWrapper> aceWrappers)
        {
            ErrorIf(
                !CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).
                     SecurityEnable, FilesCommonResource.ErrorMassage_SecurityException);

            foreach (var w in aceWrappers)
            {
                if (w.ObjectIDs == null || w.ObjectIDs.Count == 0) continue;
                var entryId = int.Parse(w.ObjectIDs[0].Replace("folder_", string.Empty).Replace("file_", string.Empty));
                var entryType = w.ObjectIDs[0].StartsWith("folder_") ? FileEntryType.Folder : FileEntryType.File;

                using (var ddao = GetFolderDao())
                using (var fdao = GetFileDao())
                {
                    var entry = entryType == FileEntryType.Folder
                                    ? (FileEntry) ddao.GetFolder(entryId)
                                    : (FileEntry) fdao.GetFile(entryId);
                    if (entry == null) return;

                    ErrorIf(
                        entry.RootFolderType == FolderType.COMMON &&
                        !CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID,
                                                               ASC.Core.Users.Constants.GroupAdmin.ID),
                        FilesCommonResource.ErrorMassage_SecurityException);
                    ErrorIf(entry.RootFolderType == FolderType.USER && entry.RootFolderId != Global.FolderMy,
                            FilesCommonResource.ErrorMassage_SecurityException);

                    var subjects = FileSecurity.GetUserSubjects(w.SubjectId);
                    var ace = FileSecurity.GetShares(entry)
                        .Where(r => subjects.Contains(r.Subject))
                        .OrderBy(r => subjects.IndexOf(r.Subject))
                        .ThenByDescending(r => r.Share)
                        .FirstOrDefault();

                    var defaultShare = entry.RootFolderType == FolderType.COMMON
                                           ? FileSecurity.DefaultCommonShare
                                           : FileSecurity.DefaultMyShare;
                    var parentShare = ace != null ? ace.Share : defaultShare;
                    var share = parentShare == w.Share ? ASC.Files.Core.Security.FileShare.None : w.Share;

                    FileSecurity.Share(entryId, entryType, w.SubjectId, share);

                    if (entry.RootFolderType == FolderType.USER)
                    {
                        var recipients = new List<Notify.Recipients.IRecipient>();
                        var listUsersId = new List<Guid>();
                        if (w.SubjectGroup)
                            listUsersId =
                                CoreContext.UserManager.GetUsersByGroup(w.SubjectId).Select(ui => ui.ID).ToList();
                        else
                            listUsersId.Add(w.SubjectId);

                        listUsersId.ForEach(id =>
                                                {
                                                    var recipient =
                                                        NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                                                            id.ToString());
                                                    //NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_DocumentInformer, entry.UniqID, recipient);
                                                    recipients.Add(recipient);
                                                });

                        if (entryType == FileEntryType.Folder)
                        {
                            NotifyClient.SendShareNoticeAsync((Folder) entry, share, recipients.ToArray());
                        }
                        else
                        {
                            NotifyClient.SendShareNoticeAsync((File) entry, share, recipients.ToArray());
                        }
                    }
                }
            }
        }

        public void RemoveAce(string objectID)
        {
            if (string.IsNullOrEmpty(objectID)) return;

            var entryId = int.Parse(objectID.Replace("folder_", string.Empty).Replace("file_", string.Empty));
            var entryType = objectID.StartsWith("folder_") ? FileEntryType.Folder : FileEntryType.File;

            FileSecurity.Share(entryId, entryType, SecurityContext.CurrentAccount.ID,
                               ASC.Files.Core.Security.FileShare.Restrict);
        }

        private FileSecurity FileSecurity
        {
            get { return Global.GetFilesSecurity(); }
        }

        private IFolderDao GetFolderDao()
        {
            return Global.DaoFactory.GetFolderDao();
        }

        private IFileDao GetFileDao()
        {
            return Global.DaoFactory.GetFileDao();
        }

        private ITagDao GetTagDao()
        {
            return Global.DaoFactory.GetTagDao();
        }

        private IDataStore GetStore()
        {
            return Global.GetStore();
        }


        private void ParseArrayItems(ItemList<String> data, out List<int> folderID, out List<int> filesID)
        {
            folderID = new List<int>();
            filesID = new List<int>();
            foreach (var id in data)
            {
                if (id.StartsWith("folder_")) folderID.Add(Convert.ToInt32(id.Substring("folder_".Length)));
                if (id.StartsWith("file_")) filesID.Add(Convert.ToInt32(id.Substring("file_".Length)));
            }
        }

        private String GetTip()
        {
            var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (!tips.ContainsKey(lang))
            {
                var file = HostingEnvironment.MapPath(string.Format("~/Products/Files/App_Data/tips/tips.{0}.xml", lang));
                tips[lang] = System.IO.File.Exists(file)
                                 ? XElement.Load(file).Descendants("tip").Select(e => e.Value).ToList()
                                 : new List<string>();
            }
            var values = tips[lang];
            return 0 < values.Count ? values[new Random().Next(0, values.Count)] : string.Empty;
        }

        private Guid ValidateTicketAndGetUserId(File file, string ticket)
        {
            var id = Guid.Empty;
            ticket = ticket ?? string.Empty;
            var pos = ticket.LastIndexOf('.');
            if (0 < pos && pos < ticket.Length)
            {
                var key = ticket.Substring(0, pos);
                if (EmailValidationKeyProvider.ValidateEmailKey(file.ID.ToString() + file.Title, key,
                                                                TimeSpan.FromHours(12)))
                {
                    id = new Guid(ticket.Substring(pos + 1));
                }
            }
            return id;
        }

        private void ErrorIf(bool condition, string errorMessage)
        {
            ErrorIf(condition, errorMessage, false);
        }

        private void ErrorIf(bool condition, string errorMessage, bool json)
        {
            if (condition) throw GenerateException(new Exception(errorMessage), json);
        }

        private WebProtocolException GenerateException(Exception error, bool json)
        {
            var weberror = new WebProtocolException(HttpStatusCode.BadRequest, FilesCommonResource.ErrorMassage_BadRequest, error);
            if (!json)
            {
                var element = new XElement("error", new XElement("message", FilesCommonResource.ErrorMassage_BadRequest));
                var current = element;
                var inner = error;
                while (inner != null)
                {
                    var el = new XElement("inner",
                                          new XElement("message", inner.Message),
                                          new XElement("type", inner.GetType()),
                                          new XElement("source", inner.Source),
                                          new XElement("stack", inner.StackTrace));
                    current.Add(el);
                    current = el;
                    inner = inner.InnerException;
                }

                weberror = new WebProtocolException(HttpStatusCode.BadRequest, FilesCommonResource.ErrorMassage_BadRequest, element, false, error);
            }

            weberror.Data["message"] = error.Message;

            return weberror;
        }
    }
}