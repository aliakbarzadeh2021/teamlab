using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Security.Authentication;
using ASC.Notify;
using ASC.Notify.Recipients;
using ASC.PhotoManager;
using ASC.PhotoManager.Data;
using ASC.PhotoManager.Helpers;
using ASC.PhotoManager.Model;
using ASC.PhotoManager.Resources;
using ASC.Web.Community.PhotoManager.Common;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Helpers;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Community.PhotoManager
{
    [AjaxNamespace("AddPhoto")]
    public partial class AddPhoto : BasePage
    {
        private bool editable = true;
        protected Album selectedAlbum = null;
        protected long requestedEvent = -1;

        #region Methods

        protected override void PageLoad()
        {
            if (SetupInfo.WorkMode == WorkMode.Promo)
                Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_DEFAULT, true);

            if (!ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddPhoto))
                Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_DEFAULT);

            if (ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context))
                Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_DEFAULT);

            formContainer.Options.IsPopup = true;

            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT });
            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.UploadPhotoTitle });

            this.Title = HeaderStringHelper.GetPageTitle(PhotoManagerResource.AddonName, mainContainer.BreadCrumbs);

            AjaxPro.Utility.RegisterTypeForAjax(typeof(AddPhoto), this.Page);
            btnUpload.Text = PhotoManagerResource.FinishUploadLink;
            btnSave.Text = PhotoManagerResource.SavePhotosButton;

            if (!IsPostBack)
            {
                Session["imagesInfo"] = new List<ImageInfo>();
            }
            if (!string.IsNullOrEmpty(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_EVENT]))
            {
                try
                {
                    requestedEvent = Convert.ToInt64(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_EVENT]);
                }
                catch { requestedEvent = -1; }
            }

            GetRequestParams();

            sideRecentActivity.TenantId = TenantProvider.CurrentTenantID;
            sideRecentActivity.ProductId = Product.CommunityProduct.ID;
            sideRecentActivity.ModuleId = ASC.PhotoManager.PhotoConst.ModuleID;

            _uploadSwitchHolder.Controls.Add(new FileUploaderModeSwitcher());
        }

        private string GetFileName(string fileName)
        {
            string tmp = "";
            int startIndex = fileName.LastIndexOf("\\");
            if (startIndex > 0)
            {
                tmp = fileName.Substring(startIndex + 1, fileName.Length - startIndex - 1);
            }
            else
            {
                tmp = fileName;
            }

            return tmp.Substring(0, tmp.LastIndexOf('.'));
        }

        private string GetFileExtension(string fileName)
        {
            if (fileName == null)
            {
                return fileName;
            }

            return fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.'));

        }

        private string GetFileNameThumbnail(string fileName)
        {
            return fileName.Insert(fileName.LastIndexOf('.'), ASC.PhotoManager.PhotoConst.THUMB_SUFFIX);
        }

        private string GetNextFileName(string fileName, int nextValue)
        {
            int position = fileName.LastIndexOf('(');
            if (position < 0)
            {
                position = fileName.Length;
            }
            return fileName.Substring(0, position) + "(" + nextValue + ")";
        }

        private void GetRequestParams()
        {
            string selectedID = Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_ALBUM];

            long id = 0;
            if (!string.IsNullOrEmpty(selectedID) && long.TryParse(selectedID, out id))
            {
                var storage = StorageFactory.GetStorage();
                selectedAlbum = storage.GetAlbum(id);
                editable = false;
            }

            if (selectedAlbum == null)
                if (!Int64.TryParse(Request[ASC.PhotoManager.PhotoConst.PARAM_EVENT], out requestedEvent))
                    requestedEvent = -1;
        }

        public string RenderEventsSelector()
        {
            StringBuilder sb = new StringBuilder();
            var storage = StorageFactory.GetStorage();
            var events = storage.GetEvents(0, int.MaxValue);

            long selectedEvent = -1;

            if (requestedEvent != -1) selectedEvent = requestedEvent;

            try
            {
                if (selectedAlbum != null)
                    selectedEvent = selectedAlbum.Event.Id;

            }
            catch { }

            sb.Append("<select " + (editable ? string.Empty : "disabled=\"true\" ") + " id=\"events_selector\" name=\"events_selector\" onchange=\"javascript:PhotoManager.EventsSelectorHandle();\" class=\"comboBox\" style=\"width: 100%;\">");
            sb.Append("<option class=\"textMediumDescribe\" value=\"-1\"  " + (selectedEvent == (long)(-1) ? "selected" : string.Empty) + ">" + PhotoManagerResource.ChooseEventTitle + "</option>");

            foreach (Event item in events)
            {
                sb.Append("<option " + (selectedEvent == item.Id ? "selected" : string.Empty) + " value=\"" + item.Id.ToString() + "\" >" + HttpUtility.HtmlEncode(item.Name) + "</option>");
            }

            sb.Append("</select>");

            return sb.ToString();
        }

        private string AddPreviewImage(string fileName, string imageName, long imageID, bool isAlbumFace, int imageNumber, int width, int height)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<span><input name=\"image_id_" + imageNumber + "\" name=\"image_id_" + imageNumber + "\" type=\"hidden\" value=\"" + imageID + "\" />");
            sb.Append("<div style=\"width: 200px;height:200px;background-color: #EBF0F4;padding:5px;\">" + ImageHTMLHelper.GetHTMLThumb(fileName, 200, width, height) + "</div><div></div>");
            sb.Append("<div class='textBigDescribe clearFix' style=\"padding-left:5px;padding-top:5px;width: 200px;\"><div style='padding-top: 5px;float:left'>" + PhotoManagerResource.EditPhotoNameTitle + "</div><div style='float:right'><label class='textMediumDescribe' for=\"face_" + imageNumber + "\">" + PhotoManagerResource.AlbumCoverTitle + "</label><input maxlength=\"255\" type=\"radio\" id=\"face_" + imageNumber + "\" name=\"album_face\" value=\"" + imageID + "\" " + (isAlbumFace ? "checked" : "") + " /></div></div><div><input  class=\"textEdit\" style=\"width:200px;margin-top:5px;\" name=\"" + ASC.PhotoManager.PhotoConst.PARAM_EDIT_NAME + imageNumber + "\" id=\"" + ASC.PhotoManager.PhotoConst.PARAM_EDIT_NAME + imageNumber + "\" value='" + imageName + "' type=\"text\"/></div>");
            sb.Append("</span>");

            return sb.ToString();
        }

        List<string> CreateImagesInfo(string encodingString)
        {
            List<string> info = new List<string>();

            foreach (string response in encodingString.Split('|'))
            {
                ImageInfo currentImageInfo = new ImageInfo();
                byte[] byteArray = Convert.FromBase64String(response);
                string convertedResponse = System.Text.Encoding.UTF8.GetString(byteArray);

                if (info != null)
                    info.Add(convertedResponse);
            }

            return info;
        }

        List<string> CreateImagesInfoBySimple()
        {
            List<string> info = new List<string>();
            ASC.Data.Storage.IDataStore store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
            var storage = StorageFactory.GetStorage();

            string uid = SecurityContext.CurrentAccount.ID.ToString();
            string eventID = Request["events_selector"];

            var albums = storage.GetAlbums(Convert.ToInt64(eventID), uid);

            Album currentAlbum = null;
            currentAlbum = 0 < albums.Count ? albums[0] : null;

            if (currentAlbum == null)
            {
                Event Event = storage.GetEvent(Convert.ToInt64(eventID));

                currentAlbum = new Album();
                currentAlbum.Event = Event;
                currentAlbum.UserID = uid;

                storage.SaveAlbum(currentAlbum);
            }
            string fileNamePath = PhotoConst.ImagesPath + uid + "/" + currentAlbum.Id + "/";

            string[] listFiles = store.ListFilesRelative("", fileNamePath, "*.*", false);

            for (int j = 0; j < Request.Files.Count; j++)
            {
                HttpPostedFile file = Request.Files[j];

                if (file.ContentLength > SetupInfo.MaxUploadSize)
                    continue;

                if (string.IsNullOrEmpty(file.FileName))
                    continue;

                ImageInfo currentImageInfo = new ImageInfo();

                string fileExtension = GetFileExtension(file.FileName);
                string fileNameWithOutExtension = GetFileName(file.FileName);
                string addSuffix = string.Empty;

                int i = 1;

                while (CheckFile(listFiles, fileNameWithOutExtension + addSuffix + ASC.PhotoManager.PhotoConst.THUMB_SUFFIX + fileExtension))
                {
                    addSuffix = "(" + i.ToString() + ")";
                    i++;
                }

                string fileNameThumb = fileNamePath + fileNameWithOutExtension + addSuffix + ASC.PhotoManager.PhotoConst.THUMB_SUFFIX + "." + ASC.PhotoManager.PhotoConst.jpeg_extension;
                string fileNamePreview = fileNamePath + fileNameWithOutExtension + addSuffix + ASC.PhotoManager.PhotoConst.PREVIEW_SUFFIX + "." + ASC.PhotoManager.PhotoConst.jpeg_extension;


                currentImageInfo.Name = fileNameWithOutExtension;
                currentImageInfo.PreviewPath = fileNamePreview;
                currentImageInfo.ThumbnailPath = fileNameThumb;
                Stream fs = file.InputStream;

                try
                {
                    EXIFReader reader = new EXIFReader(fs);
                    currentImageInfo.ActionDate = (string)reader[PropertyTagId.DateTime];
                }
                catch { }

                ImageHelper.GenerateThumbnail(fs, fileNameThumb, ref currentImageInfo, store);
                ImageHelper.GeneratePreview(fs, fileNamePreview, ref currentImageInfo, store);

                fs.Dispose();

                AlbumItem image = new AlbumItem(currentAlbum);
                image.Name = currentImageInfo.Name;
                image.Timestamp = ASC.Core.Tenants.TenantUtil.DateTimeNow();
                image.UserID = uid;

                image.Location = currentImageInfo.Name;


                image.PreviewSize = new Size(currentImageInfo.PreviewWidth, currentImageInfo.PreviewHeight);
                image.ThumbnailSize = new Size(currentImageInfo.ThumbnailWidth, currentImageInfo.ThumbnailHeight);

                storage.SaveAlbumItem(image);

                currentAlbum.FaceItem = image;

                storage.SaveAlbum(currentAlbum);

                if (info != null)
                    info.Add(image.Id.ToString());
            }

            return info;
        }
        private bool CheckFile(string[] listFiles, string fileName)
        {
            foreach (var file in listFiles)
            {
                if (fileName == file.ToString())
                    return true;
            }
            return false;
        }
        private void SavePhotoItem(int i)
        {
            var storage = StorageFactory.GetStorage();
            AlbumItem image = storage.GetAlbumItem(Convert.ToInt64(Request.Form["image_id_" + i]));

            if (selectedAlbum == null)
                selectedAlbum = image.Album;

            image.Name = GetLimitedText(Request.Form[ASC.PhotoManager.PhotoConst.PARAM_EDIT_NAME + i]);

            storage.SaveAlbumItem(image);
        }

        #endregion

        #region AJAX events

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string SafeSession()
        {
            return "";

        }
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string CreateEvent(string name, string description, string dateTime)
        {
            SecurityContext.DemandPermissions(ASC.PhotoManager.PhotoConst.Action_AddEvent);

            var storage = StorageFactory.GetStorage();

            DateTime date;
            DateTime.TryParse(dateTime, out date);

            Event item = new Event()
            {
                Name = GetLimitedText(name),
                Description = GetLimitedText(description),
                Timestamp = date,
                UserID = SecurityContext.CurrentAccount.ID.ToString()
            };

            storage.SaveEvent(item);

            return "<option value=\"" + item.Id.ToString() + "\" onclick=\"javascript:PhotoManager.LoadEvent(" + item.Id.ToString() + ");\">" + HttpUtility.HtmlEncode(item.Name) + "</option>";
        }

        #endregion

        #region Events

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            int i = 0, photoCount = 0;
            bool simpleUploader = false;

            List<string> imagesInfo = new List<string>();

            StringBuilder sb = new StringBuilder();
            IList<AlbumItem> images = new List<AlbumItem>();
            var storage = StorageFactory.GetStorage();

            try
            {
                long eventID = Convert.ToInt64(Request.Form["events_selector"]);
                string authorID = SecurityContext.CurrentAccount.ID.ToString();

                Album currentAlbum = null;

                if (selectedAlbum != null)
                {
                    currentAlbum = selectedAlbum;
                }
                else if (string.IsNullOrEmpty(authorID))
                    return;
                else if (authorID != "0")
                {
                    var albums = storage.GetAlbums(eventID, authorID);
                    currentAlbum = 0 < albums.Count ? albums[0] : null;
                }

                if (currentAlbum == null)
                {
                    Event Event = storage.GetEvent(eventID);

                    currentAlbum = new Album();
                    currentAlbum.Event = Event;
                    currentAlbum.UserID = SecurityContext.CurrentAccount.ID.ToString();

                    storage.SaveAlbum(currentAlbum);
                }

                if (!simpleUploader)
                {
                    imagesInfo = CreateImagesInfo(Request.Form["phtm_imagesInfo"]);
                }
                else
                {
                    imagesInfo = CreateImagesInfoBySimple();
                }

                if (imagesInfo != null)
                {
                    i = 0;

                    ASC.Data.Storage.IDataStore store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");

                    foreach (string info in imagesInfo)
                    {
                        AlbumItem item = storage.GetAlbumItem(Convert.ToInt64(info));
                        images.Add(item);

                        if (photoCount != 0 && photoCount % 3 == 0)
                            sb.Append("</div>");

                        if (photoCount % 3 == 0)
                            sb.Append("<div class=\"borderLight tintMediumLight clearFix\" style=\"padding:20px; border-left:none;border-right:none;margin-bottom:8px;\">");


                        sb.Append("<div style='float:left;margin-bottom:5px;" + (photoCount % 3 == 0 ? "" : "margin-left:22px; ") + "'>");

                        sb.Append(AddPreviewImage(ImageHTMLHelper.GetImageUrl(item.ExpandedStorePreview, store), item.Name, item.Id,
                            i == 0, i, item.PreviewSize.Width, item.PreviewSize.Height));

                        sb.Append("</div>");

                        i++;
                        photoCount++;
                    }
                }

                sb.Append("</div>");

                ltrUploadedImages.Text = sb.ToString();
                pnlImageForm.Visible = false;
                pnlSave.Visible = true;

                storage.SaveAlbum(currentAlbum, images);

                string path = AjaxPro.Utility.HandlerPath;

            }
            catch (Exception)
            {
                return;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int i = 0;

            while (!string.IsNullOrEmpty(Request.Form["image_id_" + i]))
            {
                SavePhotoItem(i);
                i++;
            }

            var storage = StorageFactory.GetStorage();

            long albumItemId = 0;
            if (!string.IsNullOrEmpty(Request.Form["album_face"]) && long.TryParse(Request.Form["album_face"], out albumItemId))
            {
                var item = storage.GetAlbumItem(albumItemId);
                selectedAlbum.FaceItem = item;
                storage.SaveAlbum(selectedAlbum);
            }

            Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + selectedAlbum.Id, true);
        }

        #endregion
    }
}