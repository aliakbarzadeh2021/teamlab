using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Files.Core;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Masters;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Files
{
    public partial class DocEditor : BasePage
    {
        private string FrameParams = "?key={0}&vkey={1}&url={2}&title={3}&filetype={4}&buttons={5}&lang={6}";
        protected string SrcIframe = "";
        protected int FileId;
        protected int FileVersion;
        protected string DocKey;
        protected string FileNew;
        protected string FolderUrl;

        protected override void PageLoad()
        {
            (this.Master as IStudioMaster).DisabledSidePanel = true;

            if (Global.EnableShare)
            {
                CommonContainerHolder.Controls.Add(LoadControl(AccessRights.Location));
            }

            var _securityEnable = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).SecurityEnable;
            if (!_securityEnable)
            {
                var stub = LoadControl(PremiumStub.Location) as PremiumStub;
                stub.Type = PremiumFeatureType.ManageAccessRights;
                CommonContainerHolder.Controls.Add(stub);
            }

            InitScriptConstants();

            InitIframe();
        }

        private void InitIframe()
        {
            var fileId = Request[UrlConstant.FileId];
            if (ErrorIf(string.IsNullOrEmpty(fileId), FilesCommonResource.ErrorMassage_FileNotFound)) return;

            File file = null;
            Uri fileUri;
            string filePath = "";
            var cult = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            var buttons = "save;";
            var store = Global.GetStore();

            using (var dao = Global.DaoFactory.GetFileDao())
            {
                file = dao.GetFile(Convert.ToInt32(fileId));

                if (ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound)) return;
                if (ErrorIf(file.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem)) return;
                if (ErrorIf(!Global.GetFilesSecurity().CanEdit(file), FilesCommonResource.ErrorMassage_SecurityException_EditFile)) return;
                if (ErrorIf((file.FileStatus & FileStatus.IsEditing) == FileStatus.IsEditing, FilesCommonResource.ErrorMassage_UpdateEditingFile)) return;

                filePath = dao.GetUniqFilePath(file);
                if (ErrorIf(!store.IsFile(filePath), FilesCommonResource.ErrorMassage_FileNotFound)) return;

                fileUri = store.GetUri(filePath);
            }

            var UrlDocumentService = WebConfigurationManager.AppSettings["UrlDocumentService"];
            if (ErrorIf(string.IsNullOrEmpty(UrlDocumentService), string.Empty))
            {
                RedirectToHandler(file.ID, file.Version);
                return;
            }

            var ext = FileFormats.GetExtension(file.Title);
            if (!FileFormats.EditedDocFormats.Contains(ext, StringComparer.CurrentCultureIgnoreCase))
            {
                RedirectToHandler(file.ID, file.Version);
                return;
            }

            var UrlDocumentEditor = string.Empty;
            if (FileFormats.PresentationFormats.Contains(ext, StringComparer.CurrentCultureIgnoreCase))
                UrlDocumentEditor = "/presentation.aspx";
            else
                if (FileFormats.SpreadsheetFormats.Contains(ext, StringComparer.CurrentCultureIgnoreCase))
                    UrlDocumentEditor = "/spreadsheet.aspx";
                else
                    if (FileFormats.DocumentsFormats.Contains(ext, StringComparer.CurrentCultureIgnoreCase))
                        UrlDocumentEditor = "/doceditor.aspx";
                    else
                    {
                        RedirectToHandler(file.ID, file.Version);
                        return;
                    }

            UrlDocumentEditor = string.Concat(UrlDocumentService, UrlDocumentEditor, FrameParams);

            if (Global.EnableShare)
            {
                if (file.RootFolderType == FolderType.COMMON
                    && CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID))
                {
                    buttons += "share;";
                }
                else
                {
                    if (file.RootFolderType == FolderType.USER && file.RootFolderId == Global.FolderMy)
                    {
                        buttons += "share;";
                    }
                }
            }

            var versionForKey = file.Version + 1;
            FileNew = Request[UrlConstant.New];
            //CreateNewDoc
            if (file.Version == 1 && file.ConvertedType != null
                && !string.IsNullOrEmpty(FileNew) && FileNew == "true")
            {
                versionForKey = 1;
            }
            var docKey = Global.GetDocKey(file.ID, versionForKey);

            var docVKey = Global.GetValidateKey(docKey);

            SrcIframe = string.Format(UrlDocumentEditor,
                                        docKey,
                                        docVKey,
                                        HttpUtility.UrlEncode(fileUri.ToString()),
                                        HttpUtility.UrlEncode(file.Title),
                                        (file.ConvertedType ?? FileFormats.GetExtension(file.Title)).Substring(1),
                                        buttons,
                                        cult);

            Title = file.Title;
            FileId = file.ID;
            FileVersion = file.Version;
            DocKey = Global.GetDocKey(file.ID, -1);
            FolderUrl = Global.GetFolderUrl(file.FolderID, file.RootFolderType == FolderType.BUNCH, file.RootFolderId);

            Global.DaoFactory.GetTagDao().RemoveTags(Tag.New(SecurityContext.CurrentAccount.ID, file));

            lock (File.NowEditing) File.NowEditing[file.ID] = DateTime.UtcNow;
        }

        public void InitScriptConstants()
        {
            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.USER_ID = '{0}';
                                ASC.Files.Constants.USER_ADMIN = (true === {1});
                                ",
                                 SecurityContext.CurrentAccount.ID,
                                 CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID).ToString().ToLower());


            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.URL_HANDLER_SAVE = '{0}';
                                ",
                                 String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsSave)
                                 );


            inlineScript.AppendFormat("ASC.Files.TemplateManager.init('{0}', '{1}');",
                                VirtualPathUtility.ToAbsolute("~/template.ashx"),
                                "/products/files/templates/");


            inlineScript.AppendFormat("serviceManager.init('{0}', '{1}');",
                                PathProvider.GetFileServicePath(),
                                PathProvider.GetDocServicePath());


            Page.ClientScript.RegisterStartupScript(this.GetType(), "{EE04A63F-886B-4b63-8F5A-929D77EDBDB8}", inlineScript.ToString(), true);
        }

        private bool ErrorIf(bool condition, string errorMessage)
        {
            if (!condition) return false;

            string url = PathProvider.StartURL();
            if (!string.IsNullOrEmpty(errorMessage))
                url += "#error/" + HttpUtility.UrlEncode(errorMessage);

            Response.Redirect(url);
            return true;
        }

        private void RedirectToHandler(params object[] args)
        {
            Response.Redirect(string.Format(String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsDownload), args));
        }
    }
}