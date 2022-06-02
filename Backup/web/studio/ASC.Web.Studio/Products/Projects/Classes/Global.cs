using System;
using System.Collections.Generic;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Classes
{
    public class Global
    {

        #region Constants

        public static readonly string DB_ID = "projects";

        public static readonly String FileKeyFormat = "{0}/{1}/{2}/{3}"; // ProjectID/FileID/FileVersion/FileTitle

        public static readonly int EntryCountOnPage = 15;
        public static readonly int VisiblePageCount = 3;

        public static readonly String FileStorageModule = "projects";
        public static readonly String FileStorageModuleTemp = "projects_temp";

        public static readonly List<String> ImgExts = new List<string>
        {
            ".bmp", ".cod", ".gif",".ief", ".jpe",".jpeg",
            ".jpg",".jfif",".tiff",".cmx",".ico",".pnm",
            ".pbm",".ppm",".rgb",".xbm",".xpm",".xwd", ".png"
        };

        public static readonly List<String> ArchiveExts = new List<string>
        {
            ".zip", ".rar", ".ace", ".arc", ".arj", ".bh", ".cab", ".enc", ".gz", 
            ".ha", ".jar", ".lha", ".lzh", ".pak", ".pk3", ".tar", ".tgz", ".uu",
            ".uue", ".xxe", ".z", ".zoo"                                                               
        };

        public static readonly List<String> TextExts = new List<string> { ".txt" };

        public static readonly List<String> PdfExts = new List<string> { ".pdf" };

        public static readonly List<String> ExcelExts = new List<string>
        {
            ".xlsx", ".xls"
        };

        public static readonly List<String> WordExts = new List<string>
        {
            ".doc", ".docx", ".odt"
        };

        #endregion

        #region Property

        public static EngineFactory EngineFactory
        {
            get { return new EngineFactory(DB_ID, TenantProvider.CurrentTenantID, GetProjectStore()); }
        }

        public static bool IsAdmin
        {
            get { return CoreContext.UserManager.IsUserInGroup(ASC.Core.SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID); }
        }

        static ModuleManager _moduleManager = null;
        public static ModuleManager ModuleManager
        {
            get
            {
                if (_moduleManager == null) _moduleManager = new ModuleManager();
                return _moduleManager;
            }
        }
        #endregion

        #region Methods

        public static string GetFileExtension(string fileName)
        {
            string ext = string.Empty;
            Boolean hita = false;
            int i = fileName.Length - 1;
            char[] arr = fileName.ToCharArray();
            while (i > 0 & !hita)
            {
                if (arr[i] == '.') hita = true;
                else ext = arr[i] + ext;
                i = i - 1;
            }

            return ext;
        }

        public static String ContentLengthToString(long contentLength)
        {

            if (contentLength / Math.Pow(10, 6) > 1) return String.Format("{0} {1}", Math.Round((contentLength / Math.Pow(10, 6)), 2), ProjectsFileResource.MB);

            if (contentLength / Math.Pow(10, 3) > 1) return String.Format("{0} {1}", Math.Round((contentLength / Math.Pow(10, 3)), 2), ProjectsFileResource.kb);

            return String.Format("{0} {1}", contentLength, ProjectsFileResource.B);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bigSize"></param>
        /// <returns></returns>
        public static String GetImgFilePath(String fileName, bool bigSize)
        {

            String extention = GetFileExtension(fileName);

            if (String.IsNullOrEmpty(extention)) return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/unknown.gif" : "filetype/unknown_16.gif", ProductEntryPoint.ID);

            extention = extention.ToLower();

            if (!extention.StartsWith(".")) extention = extention.Insert(0, ".");

            if (ImgExts.Contains(extention))
                return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/image.gif" : "filetype/image_16.gif", ProductEntryPoint.ID);

            if (TextExts.Contains(extention))
                return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/txt.gif" : "filetype/txt_16.gif", ProductEntryPoint.ID);

            if (PdfExts.Contains(extention))
                return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/pdf.gif" : "filetype/pdf_16.gif", ProductEntryPoint.ID);

            if (ExcelExts.Contains(extention))
                return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/excel.gif" : "filetype/excel_16.gif", ProductEntryPoint.ID);

            if (WordExts.Contains(extention))
                return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/word_doc.gif" : "filetype/word_doc_16.gif", ProductEntryPoint.ID);

            if (ArchiveExts.Contains(extention))
                return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/archive.gif" : "filetype/archive_16.gif", ProductEntryPoint.ID);

            return WebImageSupplier.GetAbsoluteWebPath(bigSize ? "filetype/unknown.gif" : "filetype/unknown_16.gif", ProductEntryPoint.ID);

        }

        public static String RenderEntityPlate(EntityType entityType, bool isFixed)
        {

            String entityTitle;
            String imgFileName = String.Empty;
            String backgroundColor = String.Empty;

            switch (entityType)
            {

                case EntityType.Team:
                    entityTitle = ProjectResource.Team.ToLower();
                    imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_team.gif", ProductEntryPoint.ID);
                    backgroundColor = "#473388";
                    break;
                case EntityType.Comment:
                    entityTitle = ProjectsCommonResource.Comment.ToLower();
                    imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_comment.gif", ProductEntryPoint.ID);
                    backgroundColor = "#1d5f99";
                    break;
                case EntityType.Task:
                    entityTitle = TaskResource.Task.ToLower();
                    imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_task.gif", ProductEntryPoint.ID);
                    backgroundColor = "#88b601";
                    break;
                case EntityType.Project:
                    entityTitle = ProjectResource.Project.ToLower();
                    imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_project.gif", ProductEntryPoint.ID);
                    //backgroundColor = "#A0D158";
                    backgroundColor = "#f88e14";
                    break;
                case EntityType.Milestone:
                    entityTitle = MilestoneResource.Milestone.ToLower();
                    imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_milestone.gif", ProductEntryPoint.ID);
                    backgroundColor = "#e34603";
                    break;
                case EntityType.Message:
                    entityTitle = MessageResource.Message.ToLower();
                    imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_message.gif", ProductEntryPoint.ID);
                    backgroundColor = "#e4bc00";
                    break;
                case EntityType.File:
                    entityTitle = ProjectsFileResource.File.ToLower();
                    imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_file.gif", ProductEntryPoint.ID);
                    backgroundColor = "#0797ba";
                    break;
                case EntityType.TimeSpend:
                    entityTitle = ProjectsCommonResource.Time.ToLower();
                    //imgFileName = WebImageSupplier.GetAbsoluteWebPath("bg_file.gif", ProductEntryPoint.ID);
                    backgroundColor = "#f67575";
                    break;
                default:
                    entityTitle = "";
                    imgFileName = "";
                    break;
            }

            entityTitle = entityTitle.Substring(0, 1).ToUpper() + entityTitle.Remove(0, 1);

            if (isFixed)
            {
                return String.Format(@"<div style='background-color: {1}' class='pm-entity-plate-fixed'>{0}</div>", entityTitle, backgroundColor);
            }

            return String.Format(@"<div style='background-color: {1}' class='pm-entity-plate'>{0}</div>", entityTitle, backgroundColor);
        }

        public static String RenderCommonContainerHeader(String title, EntityType entityType)
        {


            return
                String.Format(
                    @"
<table cellspacing='0' cellpadding='0'>    
<tr>
<td style='padding:5px 15px 0 0;vertical-align: top;'>
{0}
</td>
<td>
{1}
</td>
</tr>
</table> 
",
                   RenderEntityPlate(entityType, false), title);

        }

        public static String RenderPrivateProjectHeader(String title)
        {


            return
                String.Format(
                    @"
<table cellspacing='0' cellpadding='0'>    
<tr>
<td style='padding:5px 15px 0 0;vertical-align: top;'>
<img src='{2}' title='{3}' alt='{3}' style='float: left; margin-top: 1px;'/>
{0}
</td>
<td>
{1}
</td>
</tr>
</table> 
",
                   RenderEntityPlate(EntityType.Project, false),
                   title,
                   WebImageSupplier.GetAbsoluteWebPath("lock.png", ProductEntryPoint.ID),
                   ProjectResource.HiddenProject);

        }

        public static string GetHTMLUserAvatar(Guid userID)
        {
            string imgPath = UserPhotoManager.GetSmallPhotoURL(userID);
            if (imgPath != null)
                return "<img class=\"userMiniPhoto\" alt='' src=\"" + imgPath + "\"/>";

            return "";
        }
             
      

        public static IDataStore GetProjectStore()
        {
            return StorageFactory.GetStorage(PathProvider.BaseVirtualPath + "web.config", TenantProvider.CurrentTenantID.ToString(), "projects");
        }
        
        #endregion
    }
}