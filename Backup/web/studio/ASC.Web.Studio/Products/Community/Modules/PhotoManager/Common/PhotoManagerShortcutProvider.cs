using System;
using System.Web;
using ASC.PhotoManager;
using ASC.Web.Core.ModuleManagement.Common;

namespace ASC.Web.Community.PhotoManager
{
    public class PhotoManagerShortcutProvider : IShortcutProvider
    {
        public static string GetCreateContentPageUrl()
        {
            if (ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddPhoto))
                return VirtualPathUtility.ToAbsolute(PhotoConst.AddonPath + PhotoConst.PAGE_ADD_PHOTO);

            return null;
        }
    
        #region IShortcutProvider Members

        public string GetAbsoluteWebPathForShortcut(Guid shortcutID, string currentUrl)
        {                        
            return "";
        }

        public bool CheckPermissions(Guid shortcutID, string currentUrl)
        {
            if (shortcutID.Equals(new Guid("CDE7CBAF-98A6-4228-902F-A690DA89B763")))
            {
                return ASC.Core.SecurityContext.CheckPermissions(ASC.PhotoManager.PhotoConst.Action_AddPhoto);
            }

            return false;
        }

        #endregion        
    }
}
