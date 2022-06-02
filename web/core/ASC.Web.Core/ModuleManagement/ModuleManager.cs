using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Globalization;

namespace ASC.Web.Core.ModuleManagement
{
    public class ModuleManager
    {
        private string modulesPhysicalDirPath;

        private string moduleXMLFileName;

        private static IDictionary<string, List<Module>> modulesCache = new Dictionary<string, List<Module>>();

        private static IDictionary<string, string> resourcesCache = new Dictionary<string, string>();


        public List<Module> AllModules
        {
            get
            {
                var key = modulesPhysicalDirPath + moduleXMLFileName;
                return modulesCache.ContainsKey(key) ? modulesCache[key] : new List<Module>();
            }
        }

        public List<Module> Modules
        {
            get { return AllModules; }
        }

        public Module this[Guid id]
        {
            get { return Modules.Find(m => id.Equals(m.Guid)); }
        }


        public ModuleManager(string modulesPhysicalDirPath, string moduleXMLFileName)
        {
            this.modulesPhysicalDirPath = modulesPhysicalDirPath;
            this.moduleXMLFileName = moduleXMLFileName;
        }

        public void LoadModules()
        {
            var key = modulesPhysicalDirPath + moduleXMLFileName;
            if (modulesCache.ContainsKey(key)) return;

            var modules = ModuleXMLSchemaParser.Parse(modulesPhysicalDirPath, moduleXMLFileName);
            modules.Sort((m1, m2) => m1.SortOrder.CompareTo(m2.SortOrder));
            modules.ForEach(m => m.InitializeModule());
            modulesCache[key] = modules;
        }

        public Widget GetWidgetByID(Guid widgetID)
        {
            foreach (var module in Modules)
            {
                if (module.MainWidget != null && widgetID.Equals(module.MainWidget.Guid)) return module.MainWidget;
                if (module.WidgetCollection != null)
                {
                    var widget = Array.Find(module.WidgetCollection, wc => widgetID.Equals(wc.Guid));
                    if (widget != null) return widget;
                }
            }
            return null;
        }

        public static string GetModuleResource(string resourceClassTypeName, string resourseKey)
        {
            if (string.IsNullOrEmpty(resourceClassTypeName) || string.IsNullOrEmpty(resourseKey)) return string.Empty;

            var key = string.Format("{0}/{1}/{2}", CultureInfo.CurrentCulture.Name, resourceClassTypeName, resourseKey);
            if (resourcesCache.ContainsKey(key))
            {
                return resourcesCache[key];
            }
            lock (resourcesCache)
            {
                var value = string.Empty;
                if (!resourcesCache.TryGetValue(key, out value))
                {
                    value = (string)Type.GetType(resourceClassTypeName, true).GetProperty(resourseKey, BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
                    resourcesCache[key] = value;
                }
                return value;
            }
        }

        public List<Shortcut> GetUserProfileShortcuts(Guid userID)
        {
            var shortcuts = new List<Shortcut>();
            foreach (var module in this.Modules)
            {
                var shortcut = module.GetUserProfileShortcut(userID);
                if (shortcut != null) shortcuts.Add(shortcut);
            }
            return shortcuts;
        }

        public List<Shortcut> GetCreateShortcuts()
        {
            var shortcuts = new List<Shortcut>();
            foreach (var module in Modules)
            {
                foreach (var shCateg in module.ShortcutCategoryCollection)
                {
                    if (shCateg.CreateShortcuts != null) shortcuts.AddRange(shCateg.GetCreateShortcuts());
                }
            }
            return shortcuts;
        }
    }
}
