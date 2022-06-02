using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Files.Core;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Masters;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Files
{
    public partial class DocViewer : BasePage
    {
        private string FrameParams = "?key={0}&vkey={1}&url={2}&title={3}&filetype={4}&buttons={5}&lang={6}";
        protected string SrcIframe = "";
        protected int FileId;
        protected int FileVersion;
        protected string FolderUrl;

        protected override void PageLoad()
        {
            (this.Master as IStudioMaster).DisabledSidePanel = true;

            if (Global.EnableShare && SecurityContext.IsAuthenticated)
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

            var fileVersion = Request[UrlConstant.Version];
            var lastVersion = true;
            File file = null;
            Uri fileUri;
            string filePath = "";
            var cult = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            var buttons = "download;";
            var store = Global.GetStore();

            using (var dao = Global.DaoFactory.GetFileDao())
            {
                if (String.IsNullOrEmpty(fileVersion))
                {
                    file = dao.GetFile(Convert.ToInt32(fileId));
                }
                else
                {
                    file = dao.GetFile(Convert.ToInt32(fileId), Convert.ToInt32(fileVersion));
                    if (file != null)
                        lastVersion = file.Version == dao.GetFile(Convert.ToInt32(fileId)).Version;
                }

                if (ErrorIf(file == null, FilesCommonResource.ErrorMassage_FileNotFound)) return;
                if (ErrorIf(file.RootFolderType == FolderType.TRASH, FilesCommonResource.ErrorMassage_ViewTrashItem)) return;
                if (ErrorIf(!Global.GetFilesSecurity().CanRead(file), FilesCommonResource.ErrorMassage_SecurityException_ReadFile)) return;

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
            if (!FileFormats.PreviewedDocFormats.Contains(ext, StringComparer.CurrentCultureIgnoreCase))
            {
                RedirectToHandler(file.ID, file.Version);
                return;
            }

            var UrlDocumentViewer = string.Empty;
            if (FileFormats.SpreadsheetFormats.Contains(ext, StringComparer.CurrentCultureIgnoreCase))
                UrlDocumentViewer = "/xlsviewer.aspx";
            else
                UrlDocumentViewer = "/docviewer.aspx";

            UrlDocumentViewer = string.Concat(UrlDocumentService, UrlDocumentViewer, FrameParams);

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
            if (lastVersion && FileFormats.EditedDocFormats.Contains(ext, StringComparer.CurrentCultureIgnoreCase)
                && Global.GetFilesSecurity().CanEdit(file))
                buttons += "edit;";

            var docKey = Global.GetDocKey(file.ID, file.Version);

            var docVKey = Global.GetValidateKey(docKey);

            SrcIframe = string.Format(UrlDocumentViewer,
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
            FolderUrl = Global.GetFolderUrl(file.FolderID, file.RootFolderType == FolderType.BUNCH, file.RootFolderId);

            Global.DaoFactory.GetTagDao().RemoveTags(Tag.New(SecurityContext.CurrentAccount.ID, file));
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
                                ASC.Files.Constants.URL_HANDLER_DOWNLOAD = '{0}';
                                ASC.Files.Constants.URL_DOCUMENT_EDIT = '{1}';
                                ",
                                 String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsDownload),
                                 FileFormats.EditedDocFormats.Count != 0 ?
                                     PathProvider.BaseAbsolutePath + "doceditor.aspx?" + UrlConstant.FileId + "={0}" :
                                     String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsDownload)
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
            Response.Redirect(string.Format(String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsView), args));
        }
    }
}