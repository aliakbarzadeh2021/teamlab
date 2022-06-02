using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Recipients;
using ASC.PhotoManager;
using ASC.PhotoManager.Data;
using ASC.PhotoManager.Helpers;
using ASC.PhotoManager.Model;
using ASC.PhotoManager.Resources;
using ASC.Web.Community.PhotoManager.Common;
using ASC.Web.Controls;
using ASC.Web.Controls.CommentInfoHelper;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Helpers;

namespace ASC.Web.Community.PhotoManager
{
    [AjaxNamespace("PhotoDetails")]
    public partial class PhotoDetails : BasePage
    {
        #region Members

        public AlbumItem image;
        // private int max_height = 500;
        // private int max_width = 600;

        #endregion

        #region Methods

        protected override void PageLoad()
        {
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "photodetails_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ASC.Data.Storage.WebPath.GetPath("products/community/modules/photomanager/css/photomanagerstyle.css") + "\">", false);
            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "jqhistory_script", ASC.Data.Storage.WebPath.GetPath("products/community/modules/photomanager/js/jquery.history.js"));
            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "history_script", ASC.Data.Storage.WebPath.GetPath("products/community/modules/photomanager/js/history.js"));
            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "photodetails_script", ASC.Data.Storage.WebPath.GetPath("products/community/modules/photomanager/js/photodetails.js"));

            StringBuilder jsResources = new StringBuilder();
            jsResources.Append("if(typeof window.ASC==='undefined')window.ASC={};");
            jsResources.Append("if(typeof window.ASC.Photos==='undefined')window.ASC.Photos={};");
            jsResources.Append("if(typeof window.ASC.Photos.Resources==='undefined')window.ASC.Photos.Resources={};");
            jsResources.Append("window.ASC.Photos.Resources.confirmRemoveMessage='" + PhotoManagerResource.ConfirmRemoveMessage + "';");
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "talk_resources", jsResources.ToString(), true);

            if (string.IsNullOrEmpty(PhotoID))
                Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_PHOTO);

            if (!IsPostBack)
                LoadData();

            this.Title = HeaderStringHelper.GetPageTitle(PhotoManagerResource.AddonName, mainContainer.BreadCrumbs);
            AjaxPro.Utility.RegisterTypeForAjax(typeof(PhotoDetails), this.Page);

            sideRecentActivity.TenantId = TenantProvider.CurrentTenantID;
            sideRecentActivity.ProductId = Product.CommunityProduct.ID;
            sideRecentActivity.ModuleId = ASC.PhotoManager.PhotoConst.ModuleID;
        }

        private void LoadData()
        {
            if (!string.IsNullOrEmpty(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_PHOTO]))
            {
                var storage = StorageFactory.GetStorage();

                image = storage.GetAlbumItem(Convert.ToInt64(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_PHOTO]));

                if (image != null)
                {
                    if (storage.GetAlbumItems(image.Album).Count == 0)
                        Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_DEFAULT);

                    SlideShowButton.Visible = storage.GetAlbumItems(image.Album).Count > 1 ? true : false;

                    CommentsList CommentsList = new CommentsList();
                    ConfigureCommentsList(CommentsList, image);
                    String scripts = CommentsList.GetClientScripts(Page.Request.Url.Scheme + Uri.SchemeDelimiter + Page.Request.Url.Host + ":" + Page.Request.Url.Port, Page);
                    Page.Header.Controls.Add(new Literal() { Text = scripts });

                    LoadHeaderForImage(image);

                    LoadAlbumsLinks(image.Album.Event);

                    LoadThumbnails(image.Album);

                    // LoadComments(image);

                    hdnPhotoName.Value = HttpUtility.HtmlEncode(image.Name);
                    hdnImageID.Value = image.Id.ToString();
                    hdnAlbumID.Value = image.Album.Id.ToString();
                    hdnDefaultImageID.Value = image.Id.ToString();

                }
                else
                {
                    mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT });
                    pnlContent.Visible = false;
                    albumsContainer.Visible = false;
                    ltrMessage.Text = string.Format("<div class=\"noContentBlock\">{0}</div>", PhotoManagerResource.NoFoundMessage);
                }
            }
        }

        private void LoadThumbnails(Album AlbumItem)
        {
            var storage = StorageFactory.GetStorage();
            var store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
            var items = storage.GetAlbumItems(AlbumItem);

            var thumbnails = new List<Dictionary<String, String>>();
            for (int i = 0, n = items.Count; i < n; i++)
            {
                var thumbnail = new Dictionary<String, String>();
                thumbnail.Add("Id", items[i].Id.ToString());
                thumbnail.Add("Name", (items[i].Name ?? string.Empty).HtmlEncode());
                thumbnail.Add("Src", ImageHTMLHelper.GetImageUrl(items[i].ExpandedStoreThumb, store));
                thumbnails.Add(thumbnail);
            }

            PhotoThumbnails.DataSource = thumbnails;
            PhotoThumbnails.DataBind();
        }

        private void LoadHeaderForImage(AlbumItem image)
        {
            var caption = (string.IsNullOrEmpty(image.Album.Caption) ? DisplayUserSettings.GetFullUserName(new Guid(image.Album.UserID)) : image.Album.Caption).HtmlEncode();

            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT });
            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = HttpUtility.HtmlEncode(image.Album.Event.Name), NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT + "?" + ASC.PhotoManager.PhotoConst.PARAM_EVENT + "=" + image.Album.Event.Id });
            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = caption, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + image.Album.Id });
            mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle });
        }

        private void LoadComments(AlbumItem image)
        {
            var imageComments = StorageFactory.GetStorage().GetComments(image.Id);
            var comments = new List<ASC.Web.Controls.CommentInfoHelper.CommentInfo>();

            AppendChildsComments(ref comments, imageComments);

        }

        private void LoadAlbumsLinks(Event Event)
        {
            StringBuilder sb = new StringBuilder();

            albumsContainer.Title = PhotoManagerResource.OtherAlbums;
            albumsContainer.HeaderCSSClass = "studioSideBoxTagCloudHeader";
            albumsContainer.ImageURL = WebImageSupplier.GetAbsoluteWebPath("photo_albums.png", ASC.PhotoManager.PhotoConst.ModuleID);

            foreach (Album album in StorageFactory.GetStorage().GetAlbums(Event.Id, null))
            {
                string caption = (string.IsNullOrEmpty(album.Caption) ? DisplayUserSettings.GetFullUserName(new Guid(album.UserID)) : album.Caption).HtmlEncode();

                sb.Append("<div style=\"margin-top: 10px;padding-left:20px;\">");
                sb.Append("<a class=\"linkAction\" href=\"" + ASC.PhotoManager.PhotoConst.PAGE_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + album.Id + "\">" + caption + "</a>");
                sb.Append("</div>");
            }

            ltrAlbums.Text = sb.ToString();

        }

        private void AppendChildsComments(ref List<CommentInfo> commentsInfo, IList<Comment> comments)
        {
            foreach (Comment comment in comments)
            {
                var info = new CommentInfo();

                info.CommentID = comment.Id.ToString();
                info.UserID = new Guid(comment.UserID);
                info.TimeStampStr = comment.Timestamp.Ago();

                if (SecurityContext.DemoMode)
                    info.IsRead = true;
                else
                    info.IsRead = comment.IsRead;

                info.Inactive = comment.Inactive;
                info.CommentBody = comment.Text;// postParser.Parse(comment.Text);
                info.UserFullName = DisplayUserSettings.GetFullUserName(new Guid(comment.UserID));
                info.UserAvatar = ImageHTMLHelper.GetHTMLImgUserAvatar(new Guid(comment.UserID));
                info.UserPost = CoreContext.UserManager.GetUsers(new Guid(comment.UserID)).Title ?? "";

                info.IsEditPermissions = SecurityContext.CheckPermissions(image, PhotoConst.Action_EditRemoveComment);

                info.IsResponsePermissions = SecurityContext.CheckPermissions(PhotoConst.Action_AddComment);

                var tempComments = new List<CommentInfo>();

                AppendChildsComments(ref tempComments, comment.Comments);

                info.CommentList = tempComments;

                commentsInfo.Add(info);
            }
        }

        private static void ConfigureCommentsList(CommentsList commentList, AlbumItem image)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.IsShowAddCommentBtn = ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddComment);
            commentList.Simple = false;

            var count = image != null ? image.CommentsCount : 0;
            commentList.CommentsCountTitle = count.ToString();

            commentList.ObjectID = image != null ? image.Id.ToString() : "";
            commentList.BehaviorID = "commentsObj";
            commentList.TotalCount = (int)count;

            commentList.JavaScriptAddCommentFunctionName = "PhotoDetails.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "PhotoDetails.LoadCommentText";
            commentList.JavaScriptPreviewCommentFunctionName = "PhotoDetails.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "PhotoDetails.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "PhotoDetails.UpdateComment";
            commentList.FckDomainName = "photomanager_comments";


        }

        private void DeletePhoto(AlbumItem image)
        {
            SecurityContext.DemandPermissions(image, PhotoConst.Action_EditRemovePhoto);

            var storage = StorageFactory.GetStorage();
            var store = Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
            var tempAlbum = image.Album;

            if (image.Album.FaceItem.Equals(image))
            {
                var items = storage.GetAlbumItems(tempAlbum);
                tempAlbum.FaceItem = items[items[0].Equals(image) && 1 < tempAlbum.ImagesCount ? 1 : 0];
                storage.SaveAlbum(tempAlbum);
            }

            RemoveCommentsFCKUploads(storage.GetComments(image.Id));
            storage.RemoveAlbumItem(image.Id);

            store.Delete(ASC.PhotoManager.PhotoConst.ImagesPath + image.ExpandedStoreThumb);
            store.Delete(ASC.PhotoManager.PhotoConst.ImagesPath + image.ExpandedStorePreview);
        }

        private void RemoveCommentsFCKUploads(List<Comment> comments)
        {
            foreach (var comment in comments)
            {
                RemoveCommentsFCKUploads(comment.Comments.ToList());
                CommonControlsConfigurer.FCKUploadsRemoveForItem("photomanager_comments", comment.Id.ToString());
            }
        }

        private void EditPhoto()
        {
            var storage = StorageFactory.GetStorage();
            image = storage.GetAlbumItem(Convert.ToInt64(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_PHOTO]));

            SecurityContext.DemandPermissions(image, PhotoConst.Action_EditRemovePhoto);

            IList<string> images = new List<string>();
            images.Add(image.Id.ToString());

            Session.Add(ASC.PhotoManager.PhotoConst.PARAM_SELECTED_ITEMS, images);
            Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_EDIT_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_PHOTO + "=" + Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_PHOTO]);
        }

        private string GetNextFileName(string fileName, int nextValue)
        {
            int position = fileName.LastIndexOf('(');
            if (position < 0)
                position = fileName.Length;
            return fileName.Substring(0, position) + "(" + nextValue + ")";
        }

        private string GetFileNameIndex(string fileName)
        {
            int positionStart = fileName.LastIndexOf('(');
            int positionEnd = fileName.LastIndexOf(')');
            if (positionStart < 0) return "1";
            return fileName.Substring(positionStart + 1, positionEnd - positionStart - 1);
        }

        private void Rotate(string inPath, string outPath, bool isBackRotate, ASC.Data.Storage.IDataStore store)
        {
            if (store.IsFile(inPath))
                ImageHelper.RotateImage(inPath, outPath, isBackRotate, store);
        }

        private string GetHTMLComment(Comment comment, bool isPreview)
        {
            ASC.Web.Controls.CommentInfoHelper.CommentInfo info = new ASC.Web.Controls.CommentInfoHelper.CommentInfo();

            info.CommentID = comment.Id.ToString();
            info.UserID = new Guid(comment.UserID);
            info.TimeStamp = comment.Timestamp;
            info.TimeStampStr = comment.Timestamp.Ago();

            info.IsRead = true;
            info.Inactive = comment.Inactive;
            info.CommentBody = comment.Text;
            info.UserFullName = DisplayUserSettings.GetFullUserName(new Guid(comment.UserID));
            info.UserAvatar = ImageHTMLHelper.GetHTMLImgUserAvatar(new Guid(comment.UserID));
            info.UserPost = CoreContext.UserManager.GetUsers(new Guid(comment.UserID)).Title ?? "";

            if (!isPreview)
            {
                info.IsEditPermissions = SecurityContext.CheckPermissions(image, PhotoConst.Action_EditRemoveComment);

                info.IsResponsePermissions = SecurityContext.CheckPermissions(PhotoConst.Action_AddComment);
            }

            var defComment = new CommentsList();
            ConfigureCommentsList(defComment, null);

            return ASC.Web.Controls.CommentInfoHelper.CommentsHelper.GetOneCommentHtmlWithContainer(defComment, info, comment.ParentId <= 0, false);
        }

        private string GetHTMLComment(string text, string commentID)
        {
            Comment comment = new Comment(0)
            {
                Text = text,
                Timestamp = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                UserID = SecurityContext.CurrentAccount.ID.ToString()
            };

            if (!String.IsNullOrEmpty(commentID))
            {
                var storage = StorageFactory.GetStorage();
                comment = storage.GetComment(Convert.ToInt64(commentID));
                comment.Text = text;
            }

            return GetHTMLComment(comment, true);
        }

        protected string RenderEditPhotoLink()
        {

            return ASC.PhotoManager.PhotoConst.PAGE_EDIT_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_PHOTO + "=" + image.Id.ToString();
        }

        protected string RenderEditPhotoLink(AlbumItem img)
        {
            return ASC.PhotoManager.PhotoConst.PAGE_EDIT_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_PHOTO + "=" + img.Id;
        }

        protected string RenderSlideShowLink()
        {
            return ASC.PhotoManager.PhotoConst.PAGE_SLIDER + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + image.Album.Id + "&" + ASC.PhotoManager.PhotoConst.PARAM_PHOTO + "=" + image.Id.ToString();
        }

        protected string RenderSlideShowLink(AlbumItem img)
        {
            return ASC.PhotoManager.PhotoConst.PAGE_SLIDER + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + img.Album.Id + "&" + ASC.PhotoManager.PhotoConst.PARAM_PHOTO + "=" + img.Id.ToString();
        }

        #endregion

        #region AJAX methods

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetPreview(string text, string commentID)
        {
            return GetHTMLComment(text, commentID);
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string RemoveComment(string commentID, string pid)
        {
            ASC.Core.SecurityContext.DemandPermissions(ASC.PhotoManager.PhotoConst.Action_EditRemoveComment);

            long commentId = 0;
            if (!string.IsNullOrEmpty(commentID) && long.TryParse(commentID, out commentId))
            {
                var storage = StorageFactory.GetStorage();
                Comment comment = storage.GetComment(commentId);
                var item = storage.GetAlbumItem(comment.ItemID);

                storage.RemoveComment(commentId);

                PhotoUserActivityPublisher.RemoveComment(item, comment, SecurityContext.CurrentAccount.ID);
            }
            return commentID;
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse AddComment(string parrentCommentID, string photoID, string text, string pid)
        {
            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = parrentCommentID;

            ASC.Core.SecurityContext.DemandPermissions(ASC.PhotoManager.PhotoConst.Action_AddComment);

            var storage = StorageFactory.GetStorage();
            image = storage.GetAlbumItem(Convert.ToInt64(photoID));

            Comment newComment = new Comment(image.Id);
            newComment.Text = text;
            newComment.Timestamp = ASC.Core.Tenants.TenantUtil.DateTimeNow();
            newComment.UserID = SecurityContext.CurrentAccount.ID.ToString();

            if (!string.IsNullOrEmpty(parrentCommentID))
            {
                newComment.ParentId = Convert.ToInt64(parrentCommentID);
            }

            var count = storage.SaveComment(image, newComment);
            storage.ReadAlbumItem(newComment.ItemID, SecurityContext.CurrentAccount.ID.ToString());

            bool odd = count % 2 == 1;


            var comment = newComment;

            ASC.Web.Controls.CommentInfoHelper.CommentInfo info = new ASC.Web.Controls.CommentInfoHelper.CommentInfo();

            info.CommentID = comment.Id.ToString();
            info.UserID = new Guid(comment.UserID);
            info.TimeStampStr = comment.Timestamp.Ago();
            info.IsRead = true;
            info.Inactive = comment.Inactive;
            info.CommentBody = comment.Text; //postParser.Parse(comment.Text);
            info.UserFullName = DisplayUserSettings.GetFullUserName(new Guid(comment.UserID));
            info.UserAvatar = ImageHTMLHelper.GetHTMLImgUserAvatar(new Guid(comment.UserID));
            info.UserPost = CoreContext.UserManager.GetUsers(new Guid(comment.UserID)).Title ?? "";

            info.IsEditPermissions = ASC.Core.SecurityContext.CheckPermissions(image, ASC.PhotoManager.PhotoConst.Action_EditRemoveComment);

            info.IsResponsePermissions = ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddComment);

            var defComment = new CommentsList();
            ConfigureCommentsList(defComment, image);

            resp.rs2 = CommentsHelper.GetOneCommentHtmlWithContainer(defComment, info, newComment.ParentId == 0, odd);


            return resp;
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse UpdateComment(string commentID, string text, string pid)
        {
            ASC.Core.SecurityContext.DemandPermissions(ASC.PhotoManager.PhotoConst.Action_EditRemoveComment);

            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = commentID;

            var storage = StorageFactory.GetStorage();
            var comment = storage.GetComment(Convert.ToInt64(commentID));
            var image = storage.GetAlbumItem(comment.ItemID);

            comment.Text = text;
            storage.SaveComment(image, comment);

            resp.rs2 = text;

            PhotoUserActivityPublisher.UpdateComment(image, comment, SecurityContext.CurrentAccount.ID);

            return resp;
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string LoadCommentText(string commentID, string pid)
        {
            ASC.Core.SecurityContext.DemandPermissions(ASC.PhotoManager.PhotoConst.Action_EditRemoveComment);

            var storage = StorageFactory.GetStorage();
            Comment comment = storage.GetComment(Convert.ToInt64(commentID));

            return comment.Text;
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse GetImage(string imageID)
        {
            var storage = StorageFactory.GetStorage();
            var store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
            var currentItem = storage.GetAlbumItem(Convert.ToInt64(imageID));
            if (currentItem == null)
                return new AjaxResponse()
                {
                    status = "error",
                    message = "have no photo",
                    rs2 = "",
                    rs3 = WebImageSupplier.GetAbsoluteWebPath("blank.gif")
                };
            var items = storage.GetAlbumItems(currentItem.Album);
            var pos = items.IndexOf(currentItem);

            var htw = new HtmlTextWriter((TextWriter)new StringWriter());
            var imageComments = StorageFactory.GetStorage().GetComments(currentItem.Id);
            var comments = new List<ASC.Web.Controls.CommentInfoHelper.CommentInfo>();
            AppendChildsComments(ref comments, imageComments);

            var CommentsList = new CommentsList();
            ConfigureCommentsList(CommentsList, currentItem);
            CommentsList.Items = comments;
            CommentsList.RenderControl(htw);

            if (!SecurityContext.DemoMode)
                storage.ReadAlbumItem(currentItem.Id, currentUserID.ToString());

            return new AjaxResponse()
            {
                rs1 = currentItem.Id.ToString(),
                rs2 = (currentItem.Name ?? string.Empty).HtmlEncode(),
                rs3 = ImageHTMLHelper.GetImageUrl(currentItem.ExpandedStorePreview, store),
                rs4 = (currentItem.Description ?? string.Empty).HtmlEncode(),
                rs5 = currentItem.PreviewSize.Width.ToString(),
                rs6 = currentItem.PreviewSize.Height.ToString(),
                rs7 = Grammatical.ViewsCount(currentItem.ViewsCount),
                rs8 = Grammatical.CommentsCount(currentItem.CommentsCount),
                rs9 = RenderEditPhotoLink(currentItem),
                rs10 = RenderSlideShowLink(currentItem),
                rs11 = pos > 0 ? items[pos - 1].Id.ToString() : "",
                rs12 = pos < items.Count - 1 ? items[pos + 1].Id.ToString() : "",
                rs13 = htw.InnerWriter.ToString()
            };
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DeleteImage(string imageID)
        {
            IImageStorage storage = StorageFactory.GetStorage();
            AlbumItem CurrentItem = storage.GetAlbumItem(Convert.ToInt64(imageID));
            List<AlbumItem> Items = storage.GetAlbumItems(CurrentItem.Album);
            Int32 pos = Items.IndexOf(CurrentItem);
            Int32 NewPos = pos == Items.Count - 1 ? pos - 1 : pos + 1;
            NewPos = NewPos == 0 ? -1 : NewPos;

            AlbumItem NewCurrentItem = NewPos == -1 ? null : Items[NewPos];
            DeletePhoto(CurrentItem);
            return new AjaxResponse()
            {
                rs1 = imageID,
                rs2 = NewPos.ToString(),
                rs3 = NewCurrentItem != null ? NewCurrentItem.Id.ToString() : ASC.PhotoManager.PhotoConst.PAGE_PHOTO
            };
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse ImageRotate(string imageID, bool isBackRotate)
        {
            try
            {
                var storage = StorageFactory.GetStorage();
                var store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");

                var albumItem = storage.GetAlbumItem(Convert.ToInt64(imageID));

                var pathPreview = ASC.PhotoManager.PhotoConst.ImagesPath + albumItem.ExpandedStorePreview;
                var pathThumb = ASC.PhotoManager.PhotoConst.ImagesPath + albumItem.ExpandedStoreThumb;

                var j = 1;
                try
                {
                    j = Convert.ToInt32(GetFileNameIndex(albumItem.Location));
                }
                catch
                {
                    j = 1;
                }

                while (store.IsFile((ASC.PhotoManager.PhotoConst.ImagesPath + albumItem.ExpandedStoreThumb)))
                {
                    albumItem.Location = GetNextFileName(albumItem.Location, j++);
                }
                var newPathPreview = ASC.PhotoManager.PhotoConst.ImagesPath + albumItem.ExpandedStorePreview;
                var newPathThumb = ASC.PhotoManager.PhotoConst.ImagesPath + albumItem.ExpandedStoreThumb;

                Rotate(pathPreview, newPathPreview, isBackRotate, store);
                Rotate(pathThumb, newPathThumb, isBackRotate, store);

                albumItem.PreviewSize = new Size(albumItem.PreviewSize.Height, albumItem.PreviewSize.Width);
                albumItem.ThumbnailSize = new Size(albumItem.ThumbnailSize.Height, albumItem.ThumbnailSize.Width);

                storage.SaveAlbumItem(albumItem);

                return new AjaxResponse()
                {
                    rs1 = albumItem.Id.ToString(),
                    rs2 = albumItem.Name.HtmlEncode(),
                    rs3 = ImageHTMLHelper.GetImageUrl(albumItem.ExpandedStorePreview, store),
                    rs4 = ImageHTMLHelper.GetImageUrl(albumItem.ExpandedStoreThumb, store),
                    rs5 = albumItem.PreviewSize.Width.ToString(),
                    rs6 = albumItem.PreviewSize.Height.ToString()
                };
            }
            catch
            {
                return new AjaxResponse()
                {
                    rs1 = "0",
                    rs2 = string.Empty,
                    rs3 = WebImageSupplier.GetAbsoluteWebPath("no_photo.png", PhotoConst.ModuleID),
                    rs4 = WebImageSupplier.GetAbsoluteWebPath("photo_icon.png", PhotoConst.ModuleID),
                    rs5 = "100",
                    rs6 = "100"
                };
            }
        }

        #endregion
    }
}
