using System;
using ASC.Web.Core.ModuleManagement.Common;
using System.Web;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiShortcutProvider : IShortcutProvider
    {

        public static string GetCreateContentPageUrl()
        {
            return VirtualPathUtility.ToAbsolute(WikiManager.BaseVirtualPath + "/default.aspx") + "?action=new";
        }

        public string GetAbsoluteWebPathForShortcut(Guid shortcutID, string currentUrl)
        {
            return "";
        }

        public bool CheckPermissions(Guid shortcutID, string currentUrl)
        {
            return true;
        }
    }
}
