using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using ASC.Common.Data;
using ASC.Common.Web;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Data;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;
using ASC.Files.Core.Security;
using ASC.Security.Cryptography;
using ASC.Core;
using Microsoft.ServiceModel.Web;
using System.Security.Cryptography;
using ASC.Common.Utils;

namespace ASC.Web.Files.Classes
{
    public class Global
    {
        public const int MAX_TITLE = 170;

        public static readonly Regex invalidTitleChars = new Regex("[\\\\/:*?\"<>|';\\+]");

        public static IDaoFactory DaoFactory
        {
            get { return new DaoFactory(TenantProvider.CurrentTenantID, FileConst.DatabaseId); }
        }

        public static int FolderMy
        {
            get { return SecurityContext.IsAuthenticated ? GetFolderIdAndProccessFirstVisit(true) : 0; }
        }

        public static int FolderCommon
        {
            get { return GetFolderIdAndProccessFirstVisit(false); }
        }

        public static int FolderShare
        {
            get
            {
                if (!CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).SecurityEnable)
                    return 0;

                using (var dao = DaoFactory.GetFolderDao())
                    return EnableShare && SecurityContext.IsAuthenticated ? dao.GetFolderIDShare(true) : 0;
            }
        }

        public static int FolderTrash
        {
            get
            {
                using (var dao = DaoFactory.GetFolderDao())
                    return SecurityContext.IsAuthenticated ? dao.GetFolderIDTrash(true) : 0;
            }
        }

        public static bool EnableVersions
        {
            get;
            private set;
        }

        public static bool EnablePlugin
        {
            get;
            private set;
        }

        public static bool EnableShare
        {
            get;
            private set;
        }

        static Global()
        {
            var cmp = StringComparison.InvariantCultureIgnoreCase;
            EnableShare = bool.TrueString.Equals(WebConfigurationManager.AppSettings["EnableShare"] ?? "true", cmp);
            EnableVersions = bool.TrueString.Equals(WebConfigurationManager.AppSettings["EnableVersions"] ?? "true", cmp);
            EnablePlugin = bool.TrueString.Equals(WebConfigurationManager.AppSettings["EnablePlugin"] ?? "true", cmp);
        }

        public static IDataStore GetStore()
        {
            return StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), FileConst.StorageModule);
        }

        public static IDataStore GetStoreTmp()
        {
            return StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), FileConst.StorageModuleTmp);
        }

        public static FileSecurity GetFilesSecurity()
        {
            return new FileSecurity(DaoFactory);
        }

        public static List<String> SearchFiles(String targetDir, String searchTemplate)
        {

            var result = new List<String>();

            Directory.GetDirectories(targetDir).ToList().ForEach(dir => result.AddRange(SearchFiles(dir, searchTemplate)));

            result.AddRange(Directory.GetFiles(targetDir, searchTemplate).ToList());

            return result;
        }


        public static string ReplaceInvalidCharsAndTruncate(string title)
        {
            if (string.IsNullOrEmpty(title)) return title;
            if (MAX_TITLE < title.Length)
            {
                var pos = title.LastIndexOf('.');
                if (MAX_TITLE - 20 < pos)
                {
                    title = title.Substring(0, MAX_TITLE - (title.Length - pos)) + title.Substring(pos);
                }
                else
                {
                    title = title.Substring(0, MAX_TITLE);
                }
            }
            return invalidTitleChars.Replace(title, "_");
        }

        public static string GetFolderUrl(Folder folder)
        {
            return GetFolderUrl(folder.ID, folder.RootFolderType == FolderType.BUNCH, folder.RootFolderId);
        }

        public static string GetFolderUrl(int folderId, bool bunch, int? rootFolderId)
        {
            if (!bunch)
            {
                return VirtualPathUtility.ToAbsolute(string.Concat(PathProvider.BaseVirtualPath, "#", folderId));
            }

            using (var dao = DaoFactory.GetFolderDao())
            {
                int prId;
                var root = rootFolderId ?? dao.GetFolder(folderId).RootFolderId;

                if (!int.TryParse(dao.GetFolder(root).Title.Replace("projects/project/", ""), out prId))
                    return string.Empty;

                return string.Format("{0}?{1}={2}#{3}", VirtualPathUtility.ToAbsolute(PathProvider.ProjectVirtualPath), UrlConstant.ProjectId, prId, folderId);
            }
        }
        
        private static int GetFolderIdAndProccessFirstVisit(bool my)
        {
            using (var dao = DaoFactory.GetFolderDao())
            using (var filedao = DaoFactory.GetFileDao())
            {
                var id = my ? dao.GetFolderIDUser(false) : dao.GetFolderIDCommon(false);
                if (id == 0)
                {
                    id = my ? dao.GetFolderIDUser(true) : dao.GetFolderIDCommon(true);
                    if (my)
                    {
                        SaveFile(filedao, id, "About My Documents.txt");
                        SaveFile(filedao, id, "Edit Me.doc");
                    }
                    else
                    {
                        SaveFile(filedao, id, "About Shared Documents.txt");
                        SaveFile(filedao, id, "Welcome to Docs.doc");

                        var images = new Folder
                        {
                            Title = "Images",
                            ParentFolderID = id,
                        };
                        var imagefolderId = dao.SaveFolder(images);
                        foreach (var imagefile in Directory.GetFiles(HostingEnvironment.MapPath("~/products/files/app_data/template/"), "*.jpg"))
                        {
                            SaveFile(filedao, imagefolderId, Path.GetFileName(imagefile));
                        }
                    }
                }

                return id;
            }
        }

        private static void SaveFile(IFileDao filedao, int folder, string fileName)
        {
            using (var stream = new System.IO.FileStream(HostingEnvironment.MapPath("~/products/files/app_data/template/" + fileName), System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var file = new File
                {
                    Title = fileName,
                    ContentLength = stream.Length,
                    ContentType = MimeMapping.GetMimeMapping(System.IO.Path.GetExtension(fileName)),
                    FolderID = folder,
                };
                file = filedao.SaveFile(file);
                stream.Position = 0;
                GetStore().Save(filedao.GetUniqFilePath(file), stream);
            }
        }

        public static string GetValidateKey(string docKey)
        {
            var userIp = WebConfigurationManager.AppSettings["ServerAddress"];
                
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    userIp = HttpContext.Current.Request.UserHostAddress;
                }
            }
            catch { }


            var secretKey = WebConfigurationManager.AppSettings["SecretValidateKey"] ?? "TeamLab";
            
            return Signature.Create(new { expire = DateTime.UtcNow, key = docKey, ip = userIp },
                                    secretKey);
        }

        public static string GetDocKey(int fileId, int fileVersion)
        {
            var keyDoc = Encoding.UTF8.GetBytes("teamlab_" + fileId + "_" + fileVersion)
                            .ToList().Concat(MachinePseudoKeys.GetMachineConstant()).ToArray();

            return Global.invalidTitleChars.Replace(Hasher.Base64Hash(keyDoc, HashAlg.SHA256), "_");
        }

        public static Stream GetConvertedFile(File file)
        {
            if (file == null) return null;

            var UrlDocumentService = WebConfigurationManager.AppSettings["UrlDocumentService"];
            if (string.IsNullOrEmpty(UrlDocumentService)) return null;

            var filePath = DaoFactory.GetFileDao().GetUniqFilePath(file);
            var fileUri = GetStore().GetUri(filePath);

            var docKey = Global.GetDocKey(file.ID, file.Version);

            var docVKey = Global.GetValidateKey(docKey);

            var url = string.Format(UrlDocumentService + "/ConvertService.ashx?key={0}&vkey={1}&url={2}&title={3}&filetype={4}&outputtype={5}",
                                    docKey,
                                    docVKey,
                                    HttpUtility.UrlEncode(fileUri.ToString()),
                                    HttpUtility.UrlEncode(file.Title),
                                    (file.ConvertedType ?? FileFormats.GetExtension(file.Title)).Substring(1),
                                    FileFormats.GetExtension(file.Title).Substring(1)
                                    );

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = Convert.ToInt32(WebConfigurationManager.AppSettings["DocServiceTimeout"] ?? "120000");

            Stream stream = null;
            var countTry = 0;
            var maxTry = 3;
            while (countTry < maxTry)
            {
                try
                {
                    countTry++;
                    stream = req.GetResponse().GetResponseStream();
                    break;
                }
                catch (WebException ex)
                {
                    if (ex.Status != WebExceptionStatus.Timeout)
                        throw new WebProtocolException(HttpStatusCode.BadRequest, Resources.FilesCommonResource.ErrorMassage_BadRequest, ex);
                }
            }

            if (countTry == maxTry)
                throw new WebException("Timeout", WebExceptionStatus.Timeout);

            return stream;
        }
    }
}