using System;
using System.Web.UI.WebControls;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.UserControls.Wiki.Handlers;
using System.IO;
using System.Collections.Generic;
using ASC.Web.UserControls.Wiki.UC;
using ASC.Web.Studio.Utility;
using System.Web;
using ASC.Web.UserControls.Wiki.Resources;
using System.Linq;

namespace ASC.Web.Community.Wiki
{
    public partial class ListFiles : WikiBasePage
    {

        protected bool hasFilesToDelete = false;

        protected bool CanDeleteTheFile(IWikiObjectOwner owner)
        {
            return SecurityContext.CheckPermissions(new WikiObjectsSecurityObject(owner), ASC.Web.Community.Wiki.Common.Constants.Action_RemoveFile);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            (Master as WikiMaster).GetNavigateActionsVisible += new WikiMaster.GetNavigateActionsVisibleHandle(ListFiles_GetNavigateActionsVisible);
            PageHeaderText = WikiResource.menu_ListFiles;

            cmdUploadFile.Text = cmdUploadFile_Top.Text = WikiResource.menu_AddNewFile;

            UploadFileContainer.Options.IsPopup = true;
            UploadFileContainer.Options.OnCancelButtonClick = "javascript:HideUploadFileBox();";

            if (!IsPostBack)
            {
                cmdFileUpload.Text = WikiResource.cmdUpload;
                cmdFileUploadCancel.Text = WikiResource.cmdCancel;
                cmdUploadFile.Visible = cmdUploadFile_Top.Visible = SecurityContext.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_UploadFile) && !ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context);
                BindRepeater();
            }
        }


        protected void cmdFileUpload_Click(object sender, EventArgs e)
        {
            if (fuFile.HasFile)
            {
                EditFile.DirectFileSave(SecurityContext.CurrentAccount.ID, fuFile, MapPath("~"), WikiSection.Section, ConfigLocation, TenantId, HttpContext.Current);
                //Do a redirect after post stupid bastard!
                Response.Redirect(Request.Url.ToString(), true);
            }

            WikiMaster.UpdateNavigationItems();
            BindRepeater();
        }


        protected string GetMaxFileUploadString()
        {
            string result = string.Empty;
            long lMaxFileSize = FileUploader.MaxUploadSize;
            if (lMaxFileSize == 0)
                return result;

            result = GetFileLengthToString(lMaxFileSize);
            result = string.Format(WikiUCResource.wikiMaxUploadSizeFormat, result);

            return result;
        }

        private void BindRepeater()
        {
            List<Files> resultToShow = PagesProvider.FilesGetAll(TenantId);
            UpdateHasFilesToDelete(resultToShow);
            rptFilesList.DataSource = resultToShow;
            rptFilesList.DataBind();
        }


        private void UpdateHasFilesToDelete(List<Files> files)
        {
            var lsitToDelete = from file in files
                               where SecurityContext.CheckPermissions(new WikiObjectsSecurityObject(file), ASC.Web.Community.Wiki.Common.Constants.Action_RemoveFile)
                               select file;

            hasFilesToDelete = false;
            if (lsitToDelete != null && lsitToDelete.Count() > 0)
            {
                hasFilesToDelete = true;
            }

        }
        //protected void cmdDelete_Click(object sender, EventArgs e)
        //{
        //    PagesProvider.FilesDelete((sender as LinkButton).CommandName);
        //    BindRepeater();
        //}

        protected string GetFileName(Files file)
        {
            return file.FileName;
        }

        protected string GetFileViewLink(Files file)
        {
            string ext = file.FileLocation.Split('.')[file.FileLocation.Split('.').Length - 1];
            if (!string.IsNullOrEmpty(ext) && !WikiFileHandler.ImageExtentions.Contains(ext.ToLower()))
            {
                return this.ResolveUrlLC(string.Format(WikiSection.Section.ImageHangler.UrlFormat, HttpUtility.UrlEncode(file.FileName)));
            }

            return this.ResolveUrlLC(string.Format(WikiSection.Section.ImageHangler.UrlFormat, HttpUtility.UrlEncode(file.FileName), TenantId));
        }

        protected string GetFileViewLinkPopUp(Files file)
        {
            return string.Format(@"javascript:popitup('{0}'); return false;", GetFileViewLink(file));
        }

        WikiNavigationActionVisible ListFiles_GetNavigateActionsVisible()
        {
            return WikiNavigationActionVisible.UploadFile;
        }

        //protected string GetFileEditLink(Files file)
        //{
        //    return ActionHelper.GetEditFilePath(this.ResolveUrlLC("Default.aspx"), file.FileName);
        //}

        //protected string GetFileInfo(Files file)
        //{
        //    if (file.UserID.Equals(Guid.Empty))
        //    {
        //        return string.Empty;
        //    }

        //    return ProcessVersionInfo(file.FileName, file.UserID, file.Date, file.Version, true);
        //}


        protected string GetAuthor(Files file)
        {
            return CoreContext.UserManager.GetUsers(file.UserID).RenderProfileLink(ASC.Web.Community.Product.CommunityProduct.ID);
        }

        protected string GetDate(Files file)
        {
            return string.Format("<span class=\"wikiDateTime\">{0}</span> {1}", file.Date.ToString("t"), file.Date.ToString("d"));
        }

        protected string GetFileTypeClass(Files file)
        {
            var fileType = FileUtility.GetFileTypeByFileName(file.FileLocation);

            string result = string.Empty;
            switch (fileType)
            {
                case FileType.Archive:
                    result = "Archive_16";
                    break;
                case FileType.Excel:
                    result = "Excel_16";
                    break;
                case FileType.Image:
                    result = "Image_16";
                    break;
                case FileType.Pdf:
                    result = "Pdf_16";
                    break;
                case FileType.Txt:
                    result = "Txt_16";
                    break;
                case FileType.WordDoc:
                    result = "WordDoc_16";
                    break;
                case FileType.Unknown:
                default:
                    result = "Unknown_16";
                    break;
            }

            return string.Format("ft{0}", result);
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            string fileName = (sender as LinkButton).CommandName;
            if (string.IsNullOrEmpty(fileName))
                return;
            Files file = PagesProvider.FilesGetByName(fileName, TenantId);
            if (file != null && !string.IsNullOrEmpty(file.FileLocation))
            {
                EditFile.DeleteContent(file.FileLocation, ConfigLocation, PageWikiSection, TenantId, HttpContext.Current);
            }
            PagesProvider.FilesDelete(fileName, TenantId);
            BindRepeater();
        }
    }

}