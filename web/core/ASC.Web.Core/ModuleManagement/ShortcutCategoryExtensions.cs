using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Core;

namespace ASC.Web.Core.ModuleManagement
{   
    public partial class ShortcutCategory
    {
        public Module Module { get; set; }

        public Guid Guid
        {
            get { return new Guid(this.ID); }
        }

        public string Name
        {
            get
            {
                if (String.IsNullOrEmpty(this.ResourceClassTypeName))
                    return ModuleManager.GetModuleResource(Module.ResourceClassTypeName, this.NameResourceKey);
                else
                    return ModuleManager.GetModuleResource(this.ResourceClassTypeName, this.NameResourceKey);
            }
        }

        public string Description
        {
            get
            {
                if (String.IsNullOrEmpty(this.ResourceClassTypeName))
                    return ModuleManager.GetModuleResource(Module.ResourceClassTypeName, this.DescriptionResourceKey);
                else
                    return ModuleManager.GetModuleResource(this.ResourceClassTypeName, this.DescriptionResourceKey);
            }
        }

        private bool CheckAccess(ref Shortcut shortcut, string currentURL)
        {
            return CheckAccess(ref shortcut, currentURL, false);
        }
        private bool CheckAccess(ref Shortcut shortcut, string currentURL, bool isMenu)
        {
            if (shortcut == null)
                return false;

            IShortcutProvider provider = null;
            if (shortcut.IsProtected || shortcut.IsDynamic)            
                provider = this.Module.ShortcutProvider;

            if (shortcut.IsMenuOnly && !isMenu)
                return false;

            if (this.PageConfigurations!= null)
            {
                foreach(var pageConfig in this.PageConfigurations)
                {
                    //check is current page
                    if(!String.IsNullOrEmpty(pageConfig.URL)
                        && currentURL.IndexOf(VirtualPathUtility.ToAbsolute(pageConfig.URL), StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        if (pageConfig.DenyShortcuts != null)
                        {
                            foreach (var denyShortcut in pageConfig.DenyShortcuts)
                            {
                                if (String.Equals(denyShortcut.RefName, shortcut.RefName, StringComparison.InvariantCultureIgnoreCase))
                                    return false;
                            }
                        }

                        break;
                    }
                }
            }   

            if (shortcut.IsProtected && !provider.CheckPermissions(shortcut.Guid, currentURL))
                return false;

            if (shortcut.IsDynamic)
            {
                shortcut.ActionURL = provider.GetAbsoluteWebPathForShortcut(shortcut.Guid, currentURL);
                shortcut.AbsoluteActionURL = shortcut.ActionURL;
            }
            else if (!String.IsNullOrEmpty(shortcut.ActionURL))
            {
                int index = shortcut.ActionURL.IndexOf("?");
                if (index != -1)
                {
                    shortcut.AbsoluteActionURL = VirtualPathUtility.ToAbsolute(shortcut.ActionURL.Substring(0, index))
                                      + shortcut.ActionURL.Substring(index).Replace("[%uid%]", SecurityContext.CurrentAccount.ID.ToString());
                }
                else
                    shortcut.AbsoluteActionURL = VirtualPathUtility.ToAbsolute(shortcut.ActionURL);
            }

            shortcut.Category = this;

            return true;
        }

        public List<Shortcut> GetCreateShortcuts()
        {
            List<Shortcut> shortcuts = new List<Shortcut>();

            string currentURL = "";
            if (HttpContext.Current != null)
                currentURL = HttpContext.Current.Request.Url.AbsoluteUri;
            
            if (this.CreateShortcuts != null)
            {
                foreach (var shortcutRef in this.CreateShortcuts)
                {
                    var shortcut = this.Module.GetShortcutByRefName(shortcutRef.RefName);
                    if (shortcut != null && CheckAccess(ref shortcut, currentURL))
                    {
                        shortcuts.Add(shortcut);
                    }
                }
            }

            return shortcuts;
        }

        public List<Shortcut> GetStuffShortcuts()
        {
            List<Shortcut> shortcuts = new List<Shortcut>();

            string currentURL = "";
            if (HttpContext.Current != null)
                currentURL = HttpContext.Current.Request.Url.AbsoluteUri;
            
            if (this.StuffShortcuts != null)
            {
                foreach (var shortcutRef in this.StuffShortcuts)
                {
                    var shortcut = this.Module.GetShortcutByRefName(shortcutRef.RefName);
                    if (shortcut != null && CheckAccess(ref shortcut, currentURL))
                    {
                        shortcuts.Add(shortcut);
                    }
                }
            }

            return shortcuts;
        }

        public List<Shortcut> GetCurrentShortcuts()
        {
            return GetCurrentShortcuts(false);
        }
        public List<Shortcut> GetCurrentShortcuts(bool isMenu)
        {
            List<Shortcut> shortcuts = new List<Shortcut>();

            string currentURL = "";
                if (HttpContext.Current != null)
                    currentURL = HttpContext.Current.Request.Url.AbsoluteUri;

            if (this.NavigationShortcuts != null)
            {
                foreach (var shortcutRef in this.NavigationShortcuts)
                {
                    var shortcut = this.Module.GetShortcutByRefName(shortcutRef.RefName);
                    if (shortcut != null && CheckAccess(ref shortcut, currentURL, isMenu))
                    {
                        shortcuts.Add(shortcut);
                    }
                }
            }

            if (this.PageConfigurations != null)
            {
                foreach (var pageConfig in this.PageConfigurations)
                {
                    if (!String.IsNullOrEmpty(pageConfig.URL)
                        && currentURL.IndexOf(VirtualPathUtility.ToAbsolute(pageConfig.URL), StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        if (pageConfig.AddShortcuts != null)
                        {
                            foreach (var addShortcutRef in pageConfig.AddShortcuts)
                            {
                                var shortcut = this.Module.GetShortcutByRefName(addShortcutRef.RefName);
                                if (shortcut != null && CheckAccess(ref shortcut, currentURL, isMenu))
                                {
                                    shortcuts.Add(shortcut);
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return shortcuts;
        }

        public string AbsoluteStartURL
        {
            get
            {
                return VirtualPathUtility.ToAbsolute(this.StartURL);
            }
        }
    }
}
