using System;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.PhotoManager;
using ASC.PhotoManager.Data;
using ASC.PhotoManager.Helpers;
using ASC.PhotoManager.Model;
using ASC.PhotoManager.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.PhotoManager
{
	internal class PhotoNotFoundControl : NotFoundControl
	{

		public PhotoNotFoundControl(bool searchMode)
		{
			if (!searchMode)
			{
				this.Text = PhotoManagerResource.NoPhotosWidgetMessage;

				this.HasLink = ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddPhoto);
                this.LinkFormattedText = string.Format(PhotoManagerResource.CreatePhotosWidgetMessage, "<a href=\"{0}\">", "</a>");
                this.LinkURL = VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath + PhotoConst.PAGE_ADD_PHOTO);
			}
		}

		public PhotoNotFoundControl(string userID)
		{
			if (userID == SecurityContext.CurrentAccount.ID.ToString())
			{
				this.Text = PhotoManagerResource.YouHaveNoPhotosTitle;
				this.HasLink = ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddPhoto);
                this.LinkFormattedText = string.Format(PhotoManagerResource.CreatePhotosWidgetMessage, "<a href=\"{0}\">", "</a>");
                this.LinkURL = VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath + PhotoConst.PAGE_ADD_PHOTO);
			}
			else
			{
				this.Text = PhotoManagerResource.UserHaveNoPhotosTitle;
			}
		}
	}

	[AjaxNamespace("Default")]
	public partial class Default : BasePage
	{
		#region Members

		private int _selectedPage;
		private int _CountEventPerPage = 3;
		private int _CountPhotoPerPage;
		private int _CountPhotoPerAlbum;
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

		protected override void PageLoad()
		{
			//this.Title = Resources.PhotoManagerResource.PageTitleDefault;
            LoadData();
			formContainer.Options.IsPopup = true;
			AjaxPro.Utility.RegisterTypeForAjax(typeof(Default), this.Page);
			this.Title = HeaderStringHelper.GetPageTitle(PhotoManagerResource.AddonName, mainContainer.BreadCrumbs);
			
			sideRecentActivity.TenantId = TenantProvider.CurrentTenantID;
			sideRecentActivity.ProductId = Product.CommunityProduct.ID;
			sideRecentActivity.ModuleId = ASC.PhotoManager.PhotoConst.ModuleID;
		}

		private void LoadData()
		{
            var storage = StorageFactory.GetStorage();
			service = storage;

			_CountPhotoPerPage = CountMediumPhoto;
			_CountPhotoPerAlbum = CountSmallPhoto;

			InitPageParams();
            ASC.Data.Storage.IDataStore store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
            
			if (!IsPostBack)
			{
				StringBuilder sb = new StringBuilder();


				#region all events
				if (String.IsNullOrEmpty(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_EVENT]))
				{
					var events = storage.GetEvents((_selectedPage - 1) * _CountEventPerPage, _CountEventPerPage);

					mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT });


					var count = storage.GetEventsCount();

					ASC.Web.Controls.PageNavigator pageNavigator = new ASC.Web.Controls.PageNavigator()
					{
						PageUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT + "?t=",
						CurrentPageNumber = _selectedPage,
						EntryCountOnPage = _CountEventPerPage,
						VisiblePageCount = 5,
						ParamName = "page",
						EntryCount = (int)count
					};

					pageNavigatorHolder.Controls.Add(pageNavigator);

					sb.Append(ImageHTMLHelper.DrawEvents(events, store));
				}
				#endregion

				#region selected event
				else
				{
					var Event = storage.GetEvent(Convert.ToInt64(Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_EVENT]));

                    mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = PhotoManagerResource.PhotoTitle, NavigationUrl = ASC.PhotoManager.PhotoConst.PAGE_DEFAULT });
                                        
                    if (Event != null)
                    {
                        mainContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = Event != null ? Event.Name : Request.QueryString[ASC.PhotoManager.PhotoConst.PARAM_EVENT] });

                        if (storage.GetAlbumsCount(Event.Id, null) == 0)
                        {
                            sb.Append("<center><div style='margin: 40px 0px 80px 0px;' class=\"headerPanel\">" + PhotoManagerResource.EventHaveNoAlbumsMsg.Replace(":UPLOAD_LINK", "<a class=\"linkHeaderLight\" style=\"text-decoration: underline;\" href=\"" + ASC.PhotoManager.PhotoConst.PAGE_ADD_PHOTO + "?" + "event=" + Event.Id + "\">" + PhotoManagerResource.UploadPhotosLink + "</a>").Replace(":REMOVE_LINK", "<a class=\"linkHeaderLight\" style=\"text-decoration: underline;\" href=\"javascript:EventsManager.RemoveEvent(" + Event.Id + ");\">" + PhotoManagerResource.RemoveButton + "</a>") + "</div><center>");
                        }
                        sb.Append(ImageHTMLHelper.DrawEvent(Event, store));
                    }
                    else
                        sb.AppendFormat("<div class=\"noContentBlock\">{0}</div>", PhotoManagerResource.NoFoundMessage);

				}
				#endregion


				if (String.IsNullOrEmpty(sb.ToString()))
					_contentHolder.Controls.Add(new PhotoNotFoundControl(false));
				else
					_contentHolder.Controls.Add(new Literal() { Text = sb.ToString() });
			}
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

		#endregion

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public string SaveEvent(long id, string name, string description, string dateTime)
		{
			var storage = StorageFactory.GetStorage();
			Event Event = storage.GetEvent(id);

			ASC.Core.SecurityContext.DemandPermissions(Event, ASC.PhotoManager.PhotoConst.Action_EditRemoveEvent);


			DateTime date;
			DateTime.TryParse(dateTime, out date);

			Event.Name = GetLimitedText(name);
			Event.Description = GetLimitedText(description);
			Event.Timestamp = date;

			storage.SaveEvent(Event);

			return id.ToString();
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public string RemoveEvent(long id)
		{
			var storage = StorageFactory.GetStorage();
			Event Event = storage.GetEvent(id);

			ASC.Core.SecurityContext.DemandPermissions(Event, ASC.PhotoManager.PhotoConst.Action_EditRemoveEvent);
            var store = ASC.Data.Storage.StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "photo");
        

			foreach (Album album in storage.GetAlbums(id, null))
			{
                string pathAlbum = ASC.PhotoManager.PhotoConst.ImagesPath + album.UserID + "/" + album.Id + "/";


                store.DeleteFiles(pathAlbum, "*", false);

			}

            IImageStorage _storage = StorageFactory.GetStorage();
            ISubscriptionProvider subscriptionProvider = _storage.NotifySource.GetSubscriptionProvider();

            subscriptionProvider.UnSubscribe(
                             ASC.PhotoManager.PhotoConst.NewEventComment,  
                             id.ToString()                                 
                        );
            
			storage.RemoveEvent(Event.Id);

			return id.ToString();//RenderEvents();
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse GetInfoEvent(long id)
		{
			AjaxResponse rs = new AjaxResponse();

			var storage = StorageFactory.GetStorage();
			Event Event = storage.GetEvent(id);

			rs.rs1 = Event.Name;
			rs.rs2 = Event.Description;
			rs.rs3 = Event.Timestamp.ToShortDateString();

			return rs;//RenderEvents();
		}
	}
}
