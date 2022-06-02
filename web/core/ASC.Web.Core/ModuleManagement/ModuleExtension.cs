using System;
using System.Collections.Generic;
using System.Web;
using ASC.Common.Security.Authorizing;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core.ModuleManagement
{
    [WebZoneAttribute(WebZoneType.Nowhere)]
    public partial class Module : IModule, IWebItem
    {
        private class DefaultShortcutProvider : IShortcutProvider
        {
            #region IShortcutProvider Members

            public string GetAbsoluteWebPathForShortcut(Guid shortcutID, string currentUrl)
            {
                return "";
            }

            public bool CheckPermissions(Guid shortcutID, string currentUrl)
            {
                return false;
            }

            #endregion
        }

        public Guid Guid
        {
            get { return new Guid(this.ID); }
        }

        public string Name
        {
            get
            {
                return ModuleManager.GetModuleResource(this.ResourceClassTypeName, this.NameResourceKey);
            }
        }

        public string Description
        {
            get
            {
                return ModuleManager.GetModuleResource(this.ResourceClassTypeName, this.DescriptionResourceKey);
            }
        }

        private List<ShortcutCategory> _shortcutCategoryCollection;
        public List<ShortcutCategory> ShortcutCategoryCollection
        {
            get
            {
                if (_shortcutCategoryCollection == null)
                {

                    _shortcutCategoryCollection = new List<ShortcutCategory>();
                    _shortcutCategoryCollection.Add(this.MainShortcutCategory);
                    if (this.ShortcutCategories != null)
                        _shortcutCategoryCollection.AddRange(this.ShortcutCategories);
                }

                return _shortcutCategoryCollection;
            }
        }


        [NonSerialized]
        private IShortcutProvider _shortcutProvider;
        public IShortcutProvider ShortcutProvider { get { return _shortcutProvider; } }

        [NonSerialized]
        private ISearchHandler _searchHandler;
        public ISearchHandler SearchHandler { get { return _searchHandler; } }

        [NonSerialized]
        private ASC.Web.Core.IGlobalHandler _globalHandler;
        public ASC.Web.Core.IGlobalHandler GlobalHandler { get { return _globalHandler; } }

        [NonSerialized]
        private ISubscriptionManager _subscriptionManager;
        public ISubscriptionManager SubscriptionManager { get { return _subscriptionManager; } }

        [NonSerialized]
        private IUserActivityPublisher _userActivityPublisher;
        public IUserActivityPublisher UserActivityPublisher { get { return _userActivityPublisher; } }

        [NonSerialized]
        private IStatisticProvider _statisticProvider;
        public IStatisticProvider StatisticProvider { get { return _statisticProvider; } }

        [NonSerialized]
        private IAuthCategoriesProvider authCategoriesProvider;
        public IAuthCategoriesProvider AuthCategoriesProvider
        {
            get { return authCategoriesProvider; }
        }

        private ModuleContext _moduleContext;

        public void InitializeModule()
        {
            _moduleContext = new ModuleContext();

            _shortcutCategoryCollection = null;

            //set default icon
            this.SmallIconFileName = this.SmallIconFileName ?? "";

            this.IconFileName = this.IconFileName ?? "";

            if (this.MainWidget != null)
                this.MainWidget.Module = this;

            if (this.WidgetCollection != null)
            {
                foreach (var widget in this.WidgetCollection)
                    widget.Module = this;
            }

            if (this.Shortcuts != null)
            {
                foreach (var shortcut in this.Shortcuts)
                    shortcut.Module = this;
            }

            this.MainShortcutCategory.Module = this;
            if (this.ShortcutCategories != null)
            {
                foreach (var categ in this.ShortcutCategories)
                    categ.Module = this;
            }

            if (!String.IsNullOrEmpty(this.GlobalHandlerFullTypeName))
                this._globalHandler = (ASC.Web.Core.IGlobalHandler)Activator.CreateInstance(Type.GetType(this.GlobalHandlerFullTypeName, true));
            else
                this._globalHandler = null;


            if (!String.IsNullOrEmpty(this.ShortcutProviderFullTypeName))
                this._shortcutProvider = (IShortcutProvider)Activator.CreateInstance(Type.GetType(this.ShortcutProviderFullTypeName, true));
            else
                this._shortcutProvider = new DefaultShortcutProvider();

            if (!String.IsNullOrEmpty(this.SearchHandlerFullTypeName))
                this._searchHandler = (ISearchHandler)Activator.CreateInstance(Type.GetType(this.SearchHandlerFullTypeName, true));
            else
                this._searchHandler = null;

            if (!String.IsNullOrEmpty(this.ImageFolder))
                _moduleContext.ImageFolder = this.ImageFolder;

            if (!String.IsNullOrEmpty(this.SubscriptionManagerFullTypeName))
            {
                this._subscriptionManager = (ISubscriptionManager)Activator.CreateInstance(Type.GetType(this.SubscriptionManagerFullTypeName, true));
                _moduleContext.SubscriptionManager = this._subscriptionManager;
            }
            else
                this._subscriptionManager = null;

            if (!String.IsNullOrEmpty(this.UserActivityPublisherFullTypeName))
            {
                this._userActivityPublisher = (IUserActivityPublisher)Activator.CreateInstance(Type.GetType(this.UserActivityPublisherFullTypeName, true));
                _moduleContext.UserActivityPublisher = this._userActivityPublisher;
            }
            else
                this._userActivityPublisher = null;

            if (this.UserActivityPublisherTypes != null)
            {
                foreach (var uap in this.UserActivityPublisherTypes)
                {
                    if (!String.IsNullOrEmpty(uap))
                        _moduleContext.UserActivityPublishers.Add((IUserActivityPublisher)Activator.CreateInstance(Type.GetType(uap, true)));
                }
            }

            if (!String.IsNullOrEmpty(this.StatisticProviderFullTypeName))
            {
                this._statisticProvider = (IStatisticProvider)Activator.CreateInstance(Type.GetType(this.StatisticProviderFullTypeName, true));
                _moduleContext.StatisticProvider = this._statisticProvider;
            }
            else
                this._statisticProvider = null;

            if (string.IsNullOrEmpty(AuthCategoriesProviderFullTypeName))
            {
                authCategoriesProvider = new EmptyAuthCategoriesProvider();
            }
            else
            {
                authCategoriesProvider = (IAuthCategoriesProvider)Activator.CreateInstance(Type.GetType(AuthCategoriesProviderFullTypeName, true));
            }

            if (!string.IsNullOrEmpty(HtmlInjectionProviderFullTypeName))
            {
                try
                {
                    _moduleContext.HtmlInjectionProviderType = Type.GetType(HtmlInjectionProviderFullTypeName, true);
                }
                catch { _moduleContext.HtmlInjectionProviderType = null; }
            }

            if (GetCreateContentPageAbsoluteUrlMethod != null)
            {
                try
                {
                    _moduleContext.GetCreateContentPageAbsoluteUrl = (ModuleContext.GetCreateContentPageAbsoluteUrlDelegate)Delegate.CreateDelegate(typeof(ModuleContext.GetCreateContentPageAbsoluteUrlDelegate),
                                                                                                   Type.GetType(GetCreateContentPageAbsoluteUrlMethod.ClassType, true),
                                                                                                   GetCreateContentPageAbsoluteUrlMethod.MethodName);
                }
                catch { _moduleContext.GetCreateContentPageAbsoluteUrl = null; }
            }

            _moduleContext.AuthCategoriesProvider = authCategoriesProvider;
            _moduleContext.AssemblyName = this.AssemblyName;
            _moduleContext.SmallIconFileName = this.SmallIconFileName;
            _moduleContext.IconFileName = this.IconFileName;
            _moduleContext.ImageFolder = this.ImageFolder;
            _moduleContext.ThemesFolderVirtualPath = this.ThemesFolderVirtualPath;
            _moduleContext.DefaultSortOrder = this.SortOrder;
        }

        /// <summary>
        /// Get module resource value
        /// </summary>
        /// <param name="resourseKey">resource key</param>
        public string this[string resourseKey]
        {
            get
            {
                return ModuleManager.GetModuleResource(this.ResourceClassTypeName, resourseKey);
            }
        }

        public Shortcut GetShortcutByRefName(string refName)
        {
            Shortcut shortcut = null;
            if (this.Shortcuts != null)
            {
                shortcut = Array.Find<Shortcut>(this.Shortcuts, sc => String.Equals(sc.RefName, refName, StringComparison.InvariantCultureIgnoreCase));
                if (shortcut != null)
                    return shortcut.Clone() as Shortcut;
            }

            return shortcut;
        }

        public Shortcut GetUserProfileShortcut(Guid userID)
        {
            if (this.UserProfileShortcut == null)
                return null;

            var shortcut = this.GetShortcutByRefName(this.UserProfileShortcut.RefName);
            if (shortcut != null)
            {
                shortcut.Category = this.MainShortcutCategory;

                if (!String.IsNullOrEmpty(shortcut.ActionURL))
                {
                    int index = shortcut.ActionURL.IndexOf("?");
                    if (index != -1)
                    {
                        shortcut.AbsoluteActionURL = VirtualPathUtility.ToAbsolute(shortcut.ActionURL.Substring(0, index))
                                          + shortcut.ActionURL.Substring(index).Replace("[%uid%]", userID.ToString());
                    }
                    else
                        shortcut.AbsoluteActionURL = VirtualPathUtility.ToAbsolute(shortcut.ActionURL);
                }

                return shortcut;
            }
            return null;
        }

        public List<Shortcut> GetMenuShortcuts()
        {
            List<Shortcut> shortcuts = new List<Shortcut>();

            foreach (var shCateg in this.ShortcutCategoryCollection)
                shortcuts.AddRange(shCateg.GetCurrentShortcuts(true));

            return shortcuts;
        }

        #region IModule Members


        public string ModuleDescription
        {
            get { return this.Description; }
        }

        public Guid ModuleID
        {
            get { return this.Guid; }
        }

        public string ModuleName
        {
            get { return this.Name; }
        }

        public string StartURL
        {
            get { return this.MainShortcutCategory.AbsoluteStartURL; }
        }

        public ModuleContext Context
        {
            get
            {
                return _moduleContext;
            }
        }

        #endregion

        #region IWebItem Members

        Guid IWebItem.ID
        {
            get { return ModuleID; }
        }

        WebItemContext IWebItem.Context
        {
            get { return _moduleContext; }
        }

        #endregion
    }
}
