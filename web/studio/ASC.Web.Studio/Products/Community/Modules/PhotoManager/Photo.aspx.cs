using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify.Recipients;
using ASC.PhotoManager.Data;
using ASC.PhotoManager.Helpers;
using ASC.PhotoManager.Model;
using ASC.PhotoManager.Resources;
using ASC.Web.Community.PhotoManager.Common;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.PhotoManager
{
	public partial class Photo : BasePage
	{
		#region Members

		private Album album;
		private int _selectedPage;
		private int countAlbumItems = 0;

		private IImageStorage service = null;

		#endregion

		#region Properties

		IDirectRecipient IAmAsRecipient
		{
			get
			{
				return (IDirectRecipient)service.NotifySource.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());
			}
		}

		#endregion

		#region Methods

		public string Mode
		{
			get
			{
				return Request.QueryString[ASC.PhotoManager.PhotoConst.ALBUM_MODE] != null ? Request.QueryString[ASC.PhotoManager.PhotoConst.ALBUM_MODE] : ASC.PhotoManager.PhotoConst.ALBUM_MODE_VIEW;
			}
		}

		protected override void PageLoad()
		{
			lbtnEdit.CssClass = "linkAction" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "");

			confirmContainer.Options.IsPopup = true;
            
            ASC.Data.Storage.IDataStore store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
        
			var storage = StorageFactory.GetStorage();
			service = storage;

			InitPageParams();

			if (!IsPostBack)
			{
				countAlbumItems = 0;

				if (!String.IsNullOrEmpty(AlbumID))
				{
					mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT });
                    pnlCurrentAlbum.Visible = true;
                    pnlUserAlbums.Visible = false;
 
					album = storage.GetAlbum(Convert.ToInt64(AlbumID));

                    if (album != null)
                    {
                        bool editRemovePermissions = ASC.Core.SecurityContext.CheckPermissions(album, ASC.PhotoManager.PhotoConst.Action_EditRemovePhoto);

                    if (album == null)
                        Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_DEFAULT);

                        if (editRemovePermissions)
                        {
                            pnlEditPhoto.Visible = true;
                        }

                        ltrLinkAllPhoto.Text = "<a href='" + ASC.PhotoManager.PhotoConst.PAGE_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_USER + "=" + album.UserID + "'>" + PhotoManagerResource.AllAuthorAlbumsTitle + "</a>";

                        string caption = (string.IsNullOrEmpty(album.Caption) ? DisplayUserSettings.GetFullUserName(new Guid(album.UserID)) : album.Caption);

                        mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = HttpUtility.HtmlEncode(album.Event.Name), NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT + "?" + ASC.PhotoManager.PhotoConst.PARAM_EVENT + "=" + album.Event.Id });
                        mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = /*Resources.PhotoManagerResource.ByTitle + " " +*/ caption });

                        string cssStyle = ImageHTMLHelper.GetImagePreviewSizeLimit(album.FaceItem, 260);
                        if (album.FaceItem != null)
                            ltrAlbumFace.Text = "<a href='" + ASC.PhotoManager.PhotoConst.PAGE_PHOTO_DETAILS + "?" + ASC.PhotoManager.PhotoConst.PARAM_PHOTO + "=" + album.FaceItem.Id + "'><img " + cssStyle + " style='border:0px solid #000;margin:8px 0px;' src=\"" + ImageHTMLHelper.GetImageUrl(album.FaceItem.ExpandedStorePreview, store) + "\" /></a>";

                        int countComments = album.CommentsCount;
                        int countViews = album.ViewsCount;
                        LoadAlbumsLinks(album.Event);
                        ltrAlbumInfo.Text = "<div class=\"textMediumDescribe\" style=\"padding: 15px 10px 0px;\"><span class='textBaseSmall'>" + Grammatical.PhotosCount(album.ImagesCount) + "</span><span class='splitter'>|</span>" + Grammatical.ViewsCount(countViews) + "<span class='splitter'>|</span>" + Grammatical.CommentsCount(countComments) + "</div>" +
                            "<div class=\"textMediumDescribe\" style=\"padding: 5px 10px;\">" + PhotoManagerResource.PostedByTitle + ": " + CoreContext.UserManager.GetUsers(new Guid(album.UserID)).RenderProfileLink(ASC.Web.Community.Product.CommunityProduct.ID) + "</div><div class=\"textMediumDescribe\" style=\"padding: 0px 10px;\">" + PhotoManagerResource.LastUpdateTitle + ": " + album.LastUpdate.ToShortDateString() + "</div>";

                        foreach (var albumItem in storage.GetAlbumItems(album))
                        {
                            ltrPhoto.Text += ImageHTMLHelper.GetSmallHTMLImage(albumItem, countAlbumItems, false, 75, store);
                            countAlbumItems++;
                        }
                    }
                    else
                    {
                        pnlCurrentAlbum.Controls.Clear();
                        pnlCurrentAlbum.Controls.Add(new Literal() { Text = string.Format("<div class=\"noContentBlock\">{0}</div>", PhotoManagerResource.NoFoundMessage) });
                        albumsContainer.Visible = false;
                       
                    }
                }
				else
				{
					pnlCurrentAlbum.Visible = false;
					pnlUserAlbums.Visible = true;
					albumsContainer.Visible = false;
					if (!String.IsNullOrEmpty(UserID))
					{
						LoadUserAllPhoto(UserID, store);
					}
					else
					{
						LoadUserAllPhoto(currentUserID.ToString(), store);
					}
				}
			}

			this.Title = HeaderStringHelper.GetPageTitle(PhotoManagerResource.AddonName, mainContainer.BreadCrumbs);

			sideRecentActivity.TenantId = TenantProvider.CurrentTenantID;
			sideRecentActivity.ProductId = Product.CommunityProduct.ID;
			sideRecentActivity.ModuleId = ASC.PhotoManager.PhotoConst.ModuleID;
		}

		private void LoadAlbumsLinks(Event Event)
		{
			StringBuilder sb = new StringBuilder();

			albumsContainer.Title = PhotoManagerResource.OtherAlbums;
			albumsContainer.HeaderCSSClass = "studioSideBoxTagCloudHeader";
			albumsContainer.ImageURL = WebImageSupplier.GetAbsoluteWebPath("photo_albums.png", ASC.PhotoManager.PhotoConst.ModuleID);

			foreach (Album album in service.GetAlbums(Event.Id, null))
			{
				string caption = (string.IsNullOrEmpty(album.Caption) ? /*Resources.PhotoManagerResource.ByTitle + " " +*/ DisplayUserSettings.GetFullUserName(new Guid(album.UserID)) : album.Caption);

				sb.Append("<div style=\"margin-top: 10px;padding-left:20px;\">");
				sb.Append("<a class=\"linkAction\" href=\"" + ASC.PhotoManager.PhotoConst.PAGE_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + album.Id + "\">" + caption + "</a>");
				sb.Append("</div>");
			}

			ltrAlbums.Text = sb.ToString();

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

		private void InitPageParams()
		{
			if (Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_PAGE] == null || Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_PAGE] == string.Empty)
				_selectedPage = 1;
			else
			{
				try
				{
					_selectedPage = Convert.ToInt32(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_PAGE]);
				}
				catch { _selectedPage = 1; }
			}

			if (_selectedPage <= 0)
				_selectedPage = 1;
		}

		private void LoadUserAllPhoto(string userID, ASC.Data.Storage.IDataStore store)
		{
			var storage = StorageFactory.GetStorage();

			string eventID = Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_EVENT];

			IList<Album> list = storage.GetAlbums(0, userID);

			mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT });
			mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = DisplayUserSettings.GetFullUserName(new Guid(userID)) });

			if (list == null || list.Count == 0)
				_contentHolder.Controls.Add(new PhotoNotFoundControl(userID));
			else
				_contentHolder.Controls.Add(new Literal() { Text = ImageHTMLHelper.DrawAlbumsAlone(list, store ) });
		}

		#endregion

		#region Events

		protected void lbtnEdit_Click(object sender, EventArgs e)
		{
			var storage = StorageFactory.GetStorage();

			if (!String.IsNullOrEmpty(AlbumID))
			{
				album = storage.GetAlbum(Convert.ToInt64(AlbumID));

				IList<string> selectedItems = new List<string>();

				foreach (AlbumItem item in storage.GetAlbumItems(album))
				{
					selectedItems.Add(item.Id.ToString());
				}

				Session[ASC.PhotoManager.PhotoConst.PARAM_SELECTED_ITEMS] = selectedItems;

				if (selectedItems.Count > 0)
				{
					Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_EDIT_PHOTO + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_ALBUM]);
				}
			}
		}

		protected void lbtnRemove_Click(object sender, EventArgs e)
		{
			var storage = StorageFactory.GetStorage();
            ASC.Data.Storage.IDataStore store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
            
            if (!String.IsNullOrEmpty(AlbumID))
			{
				album = storage.GetAlbum(Convert.ToInt64(AlbumID));


                store.DeleteFiles(ASC.PhotoManager.PhotoConst.ImagesPath + album.UserID + "/" + album.Id + "/", "*", false);
                storage.RemoveAlbum(album.Id);


				Response.Redirect(ASC.PhotoManager.PhotoConst.PAGE_PHOTO);
			}

		}


		#endregion
	}
}
