using System;
using System.Collections.Generic;
using System.Web;
using ASC.PhotoManager;
using ASC.PhotoManager.Data;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Users;

namespace ASC.Web.Community.PhotoManager
{
    public class PhotoManagerSearchHandler: ISearchHandler
    {            
        #region ISearchHandler Members

        public SearchResultItem[] Search(string text)
        {   
            List<SearchResultItem> result = new List<SearchResultItem>();

            var storage = StorageFactory.GetStorage();
            IList<AlbumItem> images = storage.SearchAlbumItems(text);

            foreach (AlbumItem image in images)
            {
                SearchResultItem resultItem = new SearchResultItem();
                resultItem.Description = image.Album.Event.Name + ", " + DisplayUserSettings.GetFullUserName(new Guid(image.UserID));
                resultItem.Name = image.Name;
                resultItem.URL = VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath) + ASC.PhotoManager.PhotoConst.PAGE_PHOTO_DETAILS + "?" + ASC.PhotoManager.PhotoConst.PARAM_PHOTO + "=" + image.Id;

                result.Add(resultItem);
            }

            return result.ToArray();
        }

        #endregion
    }
}
