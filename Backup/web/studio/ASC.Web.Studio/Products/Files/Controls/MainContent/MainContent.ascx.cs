using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Core;

namespace ASC.Web.Files.Controls
{
    public partial class MainContent : UserControl
    {
        #region Property

        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("MainContent/MainContent.ascx"); } }

        public int FolderIDUserRoot { get; set; }
        public int FolderIDCommonRoot { get; set; }
        public int FolderIDShare { get; set; }
        public int FolderIDTrash { get; set; }
        public int FolderIDCurrentRoot { get; set; }

        public String TitlePage { get; set; }

        public bool CurrentUserAdmin { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            //group filrer
            var groups = new List<GroupInfo>(CoreContext.GroupManager.GetGroups());
            groups.Sort((ug1, ug2) => String.Compare(ug1.Name, ug2.Name));
            list_departments.DataSource = groups;
            list_departments.DataBind();

            //user filter
            list_users.DataSource = CoreContext.UserManager.GetUsers().SortByUserName();
            list_users.DataBind();

            confirmRemoveDialog.Options.IsPopup = true;
            confirmOverwriteDialog.Options.IsPopup = true;
            files_getLinkDialog.Options.IsPopup = true;

            if (!CurrentUserAdmin) CurrentUserAdmin = CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID);

            InitScriptConstants();

            if (Global.EnablePlugin && SecurityContext.IsAuthenticated)
            {
                CommonContainer.Controls.Add(LoadControl(PluginBox.Location));
            }

            CommonContainer.Controls.Add(LoadControl(FileViewer.Location));
        }

        private void InitScriptConstants()
        {

            String ListTypesForUploader = String.Join(" ", FileFormats.SupportedFormats.Select(f => "*" + f + ";").ToArray()).Trim();

            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.FOLDER_ID_MY_FILES = '{0}';
                                ASC.Files.Constants.FOLDER_ID_COMMON_FILES = '{1}';
                                ASC.Files.Constants.FOLDER_ID_SHARE = '{2}';
								ASC.Files.Constants.FOLDER_ID_TRASH = '{3}';
                                ASC.Files.Constants.FOLDER_ID_CURRENT_ROOT = '{4}';
                                ",
                                 Global.FolderMy,
                                 Global.FolderCommon,
                                 Global.FolderShare,
                                 Global.FolderTrash,
                                 FolderIDCurrentRoot
                                 );


            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.USER_ID = '{0}';
                                ASC.Files.Constants.USER_ADMIN = (true === {1});
                                ASC.Files.Constants.TITLE_PAGE = '{2}';
                                ASC.Files.Constants.ENABLE_VERSIONS = (true === {3});
                                ASC.Files.Constants.MAX_NAME_LENGTH = '{4}';
                                ASC.Files.Constants.MAX_UPLOAD_KB = '{5}';
                                ASC.Files.Constants.PLUGIN_ENABLE = (true === {6});
                                ",
                                 SecurityContext.CurrentAccount.ID,
                                 CurrentUserAdmin.ToString().ToLower(),
                                 TitlePage,
                                 Global.EnableVersions.ToString().ToLower(),
                                 Global.MAX_TITLE,
                                 SetupInfo.MaxUploadSizeInKB,
                                 (Global.EnablePlugin && SecurityContext.IsAuthenticated).ToString().ToLower()
                                 );


            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.URL_HANDLER_UPLOAD = '{0}';
                                ASC.Files.Constants.URL_HANDLER_DOWNLOAD = '{1}';
                                ASC.Files.Constants.URL_HANDLER_VIEW = '{2}';
                                ",
                                 String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsUpload),
                                 String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsDownload),
                                 String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsView)
                                 );


            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.URL_DOCUMENT_PREVIEW = '{0}';
                                ASC.Files.Constants.URL_DOCUMENT_EDIT = '{1}';
                                ",
                                 FileFormats.PreviewedDocFormats.Count != 0 ?
                                     PathProvider.BaseAbsolutePath + "docviewer.aspx?" + UrlConstant.FileId + "={0}&" + UrlConstant.Version + "={1}" :
                                     String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsView),
                                 FileFormats.EditedDocFormats.Count != 0 ?
                                     PathProvider.BaseAbsolutePath + "doceditor.aspx?" + UrlConstant.FileId + "={0}" :
                                     String.Concat(FileHandler.FileHandlerPath, FileConst.ParamsDownload)
                                 );


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            inlineScript.AppendFormat(@"
                                ASC.Files.Utils.FolderImagesFiles = '{0}';
                                ASC.Files.Utils.SupportFileTypes = '{1}'; 
                                ASC.Files.Utils.UploadableExtensions = {2};
                                ASC.Files.Utils.EditableExtensions = {3};
                                ASC.Files.Utils.PreviewedImageExtensions = {4};
                                ASC.Files.Utils.PreviewedDocExtensions = {5};
                                ASC.Files.Utils.EditedDocExtensions = {6};
							    ",
                                 PathProvider.GetImagePath("filetype/"),
                                 ListTypesForUploader,
                                 serializer.Serialize(FileFormats.SupportedFormats),
                                 serializer.Serialize(FileFormats.EditableFormats),
                                 serializer.Serialize(FileFormats.PreviewedImageFormats),
                                 serializer.Serialize(FileFormats.PreviewedDocFormats),
                                 serializer.Serialize(FileFormats.EditedDocFormats)
                                 );


            inlineScript.AppendFormat("ASC.Files.TemplateManager.init('{0}', '{1}');",
                                VirtualPathUtility.ToAbsolute("~/template.ashx"),
                                "/products/files/templates/");


            inlineScript.AppendFormat("serviceManager.init('{0}', '{1}');",
                                PathProvider.GetFileServicePath(),
                                PathProvider.GetDocServicePath());


            if (!Page.ClientScript.IsStartupScriptRegistered(this.GetType(), "{19F9A54F-EAC2-4ca1-93BC-FB0E1B94D5BD}"))
                Page.ClientScript.RegisterStartupScript(this.GetType(), "{19F9A54F-EAC2-4ca1-93BC-FB0E1B94D5BD}", inlineScript.ToString(), true);
        }
    }
}