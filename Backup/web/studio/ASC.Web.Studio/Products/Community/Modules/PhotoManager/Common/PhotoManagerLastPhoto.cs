using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.PhotoManager;
using ASC.PhotoManager.Data;
using ASC.PhotoManager.Helpers;
using ASC.PhotoManager.Resources;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Controls.Dashboard;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.PhotoManager
{
    [Serializable]
    public class PhotoManagerWidgetSettings : ASC.Web.Core.Utility.Settings.ISettings
    {
        public int MaxCountLastAlbums { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{CAB3155F-F112-4e21-A27E-EA3A91AEAEC8}"); }
        }

        public ASC.Web.Core.Utility.Settings.ISettings GetDefault()
        {
            return new PhotoManagerWidgetSettings() { MaxCountLastAlbums = 5 };
        }

        #endregion
    }

    [AjaxNamespace("PhotoManagerLastPhoto")]
    [WidgetPosition(2, 2)]
    public class PhotoManagerLastPhoto : WebControl
    {
        #region Members

        private System.Collections.Generic.IList<AlbumItem> images = new System.Collections.Generic.List<AlbumItem>();
        
        #endregion

        #region Events

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);            
            Utility.RegisterTypeForAjax(typeof(PhotoManagerLastPhoto));
        }
        
        #endregion
        
        #region Methods
        
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string UpdateContent()
        {
            return RenderLastUpdateContent();
        }

        public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            base.RenderBeginTag(writer);
            writer.Write("<div id=\"PhotoManager_LastPhotoContent\" >");
        }

        protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
        {
            base.RenderContents(writer);
            writer.Write(RenderLastUpdateContent());
        }

        public override void RenderEndTag(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("</div>");
            base.RenderEndTag(writer);
        }

        private string RenderLastUpdateContent()
        {
            PhotoManagerWidgetSettings widgetSettings = SettingsManager.Instance.LoadSettingsFor<PhotoManagerWidgetSettings>(SecurityContext.CurrentAccount.ID);
            StringBuilder sb = new StringBuilder();

            var storage = StorageFactory.GetStorage();
            IList<Album> list = storage.GetLastAlbums(widgetSettings.MaxCountLastAlbums);

            sb.Append("<div class='clearFix'>");

            if (list.Count > 0)
            {

                foreach (Album item in list)
                {
                    sb.Append(GetAlbumInfo(item));
                }

                sb.Append("<div style=\"padding-top:10px;\">");
                sb.Append("<a href=\"" + VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath + PhotoConst.PAGE_DEFAULT) + "\">" + PhotoManagerResource.SeeAllAlbumsLink + "</a>");
                sb.Append("</div>");
            }
            else
            {
                if(ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddPhoto))
                {
                    sb.Append("<div class=\"empty-widget\" style=\"padding:40px; text-align: center;\">" + 
                        String.Format(PhotoManagerResource.EmptyWidgetMessage,
                                        string.Format("<div style=\"padding-top:3px;\"><a class=\"promoAction\" href=\"{0}\">", VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath + ASC.PhotoManager.PhotoConst.PAGE_ADD_PHOTO)),
                                        "</a></div>") + "</div>");
                }
                else
                    sb.Append("<div class=\"empty-widget\" style=\"padding:40px; text-align: center;\">" + PhotoManagerResource.YouHaveNoPhotosTitle + "</div>");
            }


            sb.Append("</div>");


            return sb.ToString();
        }
        private string GetAlbumInfo(Album item)
        {
            StringBuilder sb = new StringBuilder();

            AlbumItem face = item.FaceItem;

			DateTime date = item.LastUpdate;
            var store = ASC.Data.Storage.StorageFactory.GetStorage("~/Products/Community/Modules/PhotoManager/web.config", TenantProvider.CurrentTenantID.ToString(), "photo", HttpContext.Current);

            string albumURL = VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath + PhotoConst.PAGE_PHOTO) + "?" + ASC.PhotoManager.PhotoConst.PARAM_ALBUM + "=" + item.Id;
            
            sb.Append("<div style=\"margin-bottom:10px;\">");
            sb.Append("<table cellpadding='0' cellspacing='0' border='0'><tr valign=\"top\"><td style=\"width:60px;\">");
            sb.Append(GetHTMLSmallThumb(face, 54, albumURL, store));

            sb.Append("</td><td>");
            sb.Append("<div style='padding-left:10px;'>");
            
            sb.Append("<div style=\"margin-top:2px;\">");
            sb.Append("<a class='linkHeaderLightMedium' href='" + VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath + PhotoConst.PAGE_DEFAULT) + "?" + ASC.PhotoManager.PhotoConst.PARAM_EVENT + "=" + item.Event.Id + "'>" + HttpUtility.HtmlEncode(item.Event.Name) + "</a>");
            sb.Append("</div>");            
            
            sb.Append("<div style=\"margin-top: 6px;\">");
            sb.Append("<a class='linkHeaderSmall' href='" + albumURL + "'>" + /*Resources.PhotoManagerResource.ByTitle + "&nbsp;" +*/ (face != null ? DisplayUserSettings.GetFullUserName(new Guid(face.UserID)) : "") + "</a>");
            sb.Append("</div>");
            
            sb.Append("<div style=\"margin-top: 5px;\">");
            sb.Append("<a href='" + albumURL +"'>" + Grammatical.PhotosCount("{0}&nbsp;{1}",item.ImagesCount) +"</a>");

            sb.Append("<span class='textMediumDescribe' style='margin-left:10px;'>"  + date.ToShortDateString() + "</span>");
            sb.Append("</div>");
            
            sb.Append("</div>");
            sb.Append("</td></tr></table>");
            
            sb.Append("</div>");

            return sb.ToString();
        }
        private string GetHTMLSmallThumb(AlbumItem image, int maxSize, string link, ASC.Data.Storage.IDataStore store)
        {
            StringBuilder sb = new StringBuilder();
            
            string limit = ImageHTMLHelper.GetImageSizeLimit(image, maxSize);            

            sb.Append("<span style=\"padding: 3px;	display: inline-block;	position: relative;	text-align: center;	vertical-align: top;\" >");
            sb.Append("<table border=0 cellpadding=\"0\" cellspacing=\"0\"><tr><td style=\"text-align:center;vertical-align:middle;border:solid 0px #cfcfcf;height:" + maxSize + "px;width:" + maxSize + "px;table-layout:fixed;\">");
            sb.Append("<a href=\"" + link + "\">");
            sb.Append("<img " + limit + " title=\"" + (image != null ? image.Name : "") + "\" src=\"" + (image != null ? ImageHTMLHelper.GetImageUrl(image.ExpandedStoreThumb, store) : "") + "\" class=\"borderBase\">");
            
            sb.Append("</a></td></tr></table></span>");

            return sb.ToString();
        }
        
        
        #endregion
    }
}
