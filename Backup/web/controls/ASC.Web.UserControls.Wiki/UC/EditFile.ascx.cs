using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;
using System.IO;
using System.Drawing;
using ASC.Data.Storage;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class EditFile : BaseUserControl
    {

        public string FileName
        {
            get
            {
                if (ViewState["FileName"] == null)
                    return string.Empty;
                return ViewState["FileName"].ToString();
            }
            set
            {
                ViewState["FileName"] = value;
            }
        }

        private Files _fileInfo;
        protected Files CurrentFile
        {
            get
            {
                if (_fileInfo == null)
                {
                    if (string.IsNullOrEmpty(FileName))
                        return null;

                    _fileInfo = wikiDAO.FilesGetByName(FileName);
                }
                return _fileInfo;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (CurrentFile != null && !string.IsNullOrEmpty(CurrentFile.FileName))
                {
                    RisePublishVersionInfo(CurrentFile);
                }
            }
        }


        protected string GetUploadFileName()
        {
            if (CurrentFile == null)
                return string.Empty;

            return CurrentFile.UploadFileName;

        }
        protected string GetFileLink()
        {
            Files file = wikiDAO.FilesGetByName(/*PageNameUtil.Decode*/(FileName));
            if (file == null)
            {
                RisePageEmptyEvent();
                return string.Empty;// "nonefile.png";
            }

            string ext = file.FileLocation.Split('.')[file.FileLocation.Split('.').Length - 1];
            if (!string.IsNullOrEmpty(ext) && !WikiFileHandler.ImageExtentions.Contains(ext.ToLower()))
            {
                return string.Format(@"<a class=""wikiEditButton"" href=""{0}"" title=""{1}"">{2}</a>",
                    ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)),
                    file.FileName,
                    Resources.WikiUCResource.wikiFileDownloadCaption);
            }

            return string.Format(@"<img src=""{0}"" style=""max-width:300px; max-height:200px"" />",
                ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)));
        }


        private static void GetFileLocation(Files file, WikiSection section, string rootFile, out string location)
        {
            string firstFolder = "0", secondFolder = "00";
            byte letter = (byte)file.FileName[0];
            secondFolder = letter.ToString("x");

            firstFolder = secondFolder.Substring(0, 1);

            location = Path.Combine(firstFolder, secondFolder);
            location = Path.Combine(location, EncodeSafeName(file.FileName));//TODO: encode nameprep here
        }

        private static string EncodeSafeName(string fileName)
        {
            return fileName;
        }

        public static void DeleteTempContent(string fileName, string configLocation, WikiSection section, int tenantId, HttpContext context)
        {
            IDataStore storage = StorageFactory.GetStorage(configLocation, tenantId.ToString(), section.DataStorage.ModuleName, context);
            storage.Delete(section.DataStorage.TempDomain, fileName);
        }

        public static void DeleteContent(string fileName, string configLocation, WikiSection section, int tenantId, HttpContext context)
        {
            IDataStore storage = StorageFactory.GetStorage(configLocation, tenantId.ToString(), section.DataStorage.ModuleName, context);
            storage.Delete(section.DataStorage.DefaultDomain, fileName);
        }

        public static SaveResult MoveContentFromTemp(Guid UserId, string fromFileName, string toFileName, string configLocation, WikiSection section, int tenantId, HttpContext context, string rootFile, out string _fileName)
        {
            IDataStore storage = StorageFactory.GetStorage(configLocation, tenantId.ToString(), section.DataStorage.ModuleName, context);

            Files file;

            _fileName = /*PageNameUtil.Decode*/(toFileName);

            file = new Files();
            file.FileName = _fileName;
            file.UploadFileName = _fileName;
            file = PagesProvider.FilesSave(file, tenantId);


            string location;

            GetFileLocation(file, section, rootFile, out location);

            file.UserID = UserId;
            file.FileLocation = location;
            file.FileSize = (int)storage.GetFileSize(section.DataStorage.TempDomain, fromFileName);
            file = PagesProvider.FilesSave(file, tenantId);

            storage.Move(section.DataStorage.TempDomain, fromFileName, section.DataStorage.DefaultDomain, location);
            _fileName = file.FileName;
            return SaveResult.Ok;
        }


        public static SaveResult DirectFileSave(Guid UserId, string _fileName, byte[] _fileContent, string rootFile, WikiSection section, string configLocation, int tenantId, HttpContext context, out string outFileName)
        {
            string fileName = string.Empty;
            Files file;

            //_fileName = PageNameUtil.Decode(_fileName);

            file = new Files();
            file.FileName = _fileName;
            file.UploadFileName = _fileName;
            file = PagesProvider.FilesSave(file, tenantId);


            string location;

            GetFileLocation(file, section, rootFile, out location);

            file.UserID = UserId;
            file.FileLocation = location;
            file.FileSize = _fileContent.Length;
            file = PagesProvider.FilesSave(file, tenantId);

            FileContentSave(location, _fileContent, section, configLocation, tenantId, context);
            outFileName = file.FileName;

            return SaveResult.Ok;
        }

        public static SaveResult DirectFileSave(Guid UserId, FileUpload fuFile, string rootFile, WikiSection section, string configLocation, int tenantId, HttpContext context)
        {
            string fileName = string.Empty;
            if (!fuFile.HasFile)
                return SaveResult.FileEmpty;
            Files file;

            file = new Files();
            file.FileName = fuFile.FileName;//BUG: removed decode
            file.UploadFileName = fuFile.FileName;
            file = PagesProvider.FilesSave(file, tenantId);


            string location;

            GetFileLocation(file, section, rootFile, out location);

            file.UserID = UserId;
            file.FileLocation = location;
            file.FileSize = fuFile.FileBytes.Length;
            PagesProvider.FilesSave(file, tenantId);

            FileContentSave(location, fuFile.FileBytes, section, configLocation, tenantId, context);
            return SaveResult.Ok;
        }

        private static void FileContentSave(string location, byte[] fileContent, WikiSection section, string configLocation, int tenantId, HttpContext context)
        {
            IDataStore storage = StorageFactory.GetStorage(configLocation, tenantId.ToString(), section.DataStorage.ModuleName, context);
            FileContentSave(storage, location, fileContent, section);
        }

        
        private static void FileContentSave(string location, byte[] fileContent, WikiSection section, int tenantId)
        {
            IDataStore storage = StorageFactory.GetStorage(tenantId.ToString(), section.DataStorage.ModuleName);
            FileContentSave(storage, location, fileContent, section);
        }

        private static void FileContentSave(IDataStore storage, string location, byte[] fileContent, WikiSection section)
        {
            using (MemoryStream ms = new MemoryStream(fileContent))
            {
                storage.Save(section.DataStorage.DefaultDomain, location, ms);
            }
        }



        public SaveResult Save(Guid userId)
        {
            string fileName;
            return Save(userId, out fileName);
        }

        public SaveResult Save(Guid userId, out string fileName)
        {
            fileName = string.Empty;
            if (!fuFile.HasFile)
                return SaveResult.FileEmpty;
            Files file;


            if (CurrentFile == null)
            {
                file = new Files();
                file.FileName = FileName.Trim();
                file.UploadFileName = fuFile.FileName;
                file = wikiDAO.FilesSave(file);
            }
            else
            {
                file = CurrentFile;
            }


            string location;

            GetFileLocation(file, WikiSection.Section, Page.MapPath("~"), out location);

            file.UserID = userId;
            file.FileLocation = location;
            file.FileSize = fuFile.FileBytes.Length;
            wikiDAO.FilesSave(file);

            FileContentSave(location, fuFile.FileBytes, WikiSection.Section, TenantId);

            _fileInfo = file;
            RisePublishVersionInfo(file);
            fileName = file.FileName;
            return SaveResult.Ok;
        }


    }
}