using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core
{
    [Flags]
    public enum ItemAvailableState
    {
        Normal = 1,
        Disabled = 2,
        NonPublicForPortal = 8,
        All = Normal | Disabled | NonPublicForPortal
    }

    public class WebItemManager
    {
        private readonly static List<string> allowedItems;

        private readonly List<IWebItem> items;


        public static WebItemManager Instance
        {
            get;
            private set;
        }


        public IGlobalHandler ItemGlobalHandlers
        {
            get;
            private set;
        }

        public IWebItem this[Guid id]
        {
            get { return items.Find(i => id.Equals(i.ID)); }
        }


        static WebItemManager()
        {
            allowedItems = (ConfigurationManager.AppSettings["webitems"] ?? string.Empty)
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            Instance = new WebItemManager();
        }

        private WebItemManager()
        {
            items = new List<IWebItem>();
        }

        public void LoadItems(IGlobalHandler mainGlobalHandler)
        {
            var handlers = new List<IGlobalHandler>();
            ItemGlobalHandlers = new GlobalHandlerComposite(handlers);

            if (mainGlobalHandler != null)
            {
                handlers.Add(mainGlobalHandler);
            }

            ProductManager.Instance.LoadProducts();
            handlers.Add(ProductManager.Instance.ProductGlobalHandlers);

            AddonManager.Instance.LoadAddons();
            handlers.Add(AddonManager.Instance.AddonGlobalHandlers);

            try
            {
                ItemGlobalHandlers.InitItemsComplete(items.ToArray());
            }
            catch (Exception exc)
            {
                LogHolder.Log("ASC.Web").Error("InitProductsComplete failed", exc);
            }
        }

        public bool RegistryItem(IWebItem webItem)
        {
            var systemname = GetSystemName(webItem);
            if (!items.Contains(webItem) && allowedItems.Contains(systemname))
            {
                items.Add(webItem);
                items.Sort((x, y) => GetSortOrder(x, null).CompareTo(GetSortOrder(y, null)));
                return true;
            }
            return false;
        }

        public List<IWebItem> GetItems(WebZoneType webZone)
        {
            return GetItems(webZone, ItemAvailableState.Normal);
        }

        public List<IWebItem> GetItems(WebZoneType webZone, ItemAvailableState avaliableState)
        {
            var settings = GetCurrentSettings();
            var webitems = items.FindAll(item =>
            {
                if ((avaliableState & ItemAvailableState.Disabled) != ItemAvailableState.Disabled && item.IsDisabled(settings))
                {
                    return false;
                }
                if ((avaliableState & ItemAvailableState.NonPublicForPortal) != ItemAvailableState.NonPublicForPortal && item.IsDisabledForPublic())
                {
                    return false;
                }
                var attribute = Attribute.GetCustomAttribute(item.GetType(), typeof(WebZoneAttribute), true) as WebZoneAttribute;
                return attribute != null && (attribute.Type & webZone) != 0;
            });
            return webitems.SortItems(settings);
        }

        public List<IWebItem> GetSubItems(Guid parentItemID)
        {
            return GetSubItems(parentItemID, ItemAvailableState.Normal);
        }

        public List<IWebItem> GetSubItems(Guid parentItemID, ItemAvailableState avaliableState)
        {
            var items = GetItems(WebZoneType.All, avaliableState);
            var modules = items.OfType<IProduct>()
                .Where(p => p.ProductID == parentItemID)
                .SelectMany(p => p.Modules.Select(m => m.ModuleID));

            return items.FindAll(item => modules.Contains(item.ID));

        }

        public Guid GetParentItemID(Guid itemID)
        {
            foreach (var p in items.OfType<IProduct>())
            {
                if (p.Modules.Any(m => m.ModuleID == itemID)) return p.ProductID;
            }
            return Guid.Empty;
        }

        public static int GetSortOrder(IWebItem item, WebItemSettings settings)
        {
            if (item == null) return 0;

            if (settings != null && item.IsSubItem())
            {
                var s = settings.SettingsCollection.Find(o => o.ItemID == item.ID);
                if (s != null && s.SortOrder != int.MinValue) return s.SortOrder;
            }

            var index = allowedItems.IndexOf(GetSystemName(item));
            if (index != -1) return index;

            return item.Context != null ? item.Context.DefaultSortOrder : 0;
        }

        public static int GetSortOrder(IWebItem item)
        {
            if (item == null) return 0;

            var index = allowedItems.IndexOf(GetSystemName(item));
            return index != -1 ? index : int.MinValue;
        }

        internal List<IWebItem> GetItemsInternal()
        {
            return items.ToList();
        }

        private WebItemSettings GetCurrentSettings()
        {
            return SettingsManager.Instance.LoadSettings<WebItemSettings>(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        private static string GetSystemName(IWebItem webitem)
        {
            if (webitem == null)
            {
                return string.Empty;
            }

            if (webitem is IRenderMyTools)
            {
                return ((IRenderMyTools)webitem).ParameterName;
            }

            if (string.IsNullOrEmpty(webitem.StartURL))
            {
                return webitem.GetType().Name;
            };

            var parts = webitem.StartURL.ToLower().Split('/', '\\').ToList();

            if (webitem is IProduct)
            {
                var index = parts.FindIndex(s => "products".Equals(s));
                if (0 <= index && index < parts.Count - 1)
                {
                    return parts[index + 1];
                }
            }

            if (webitem is IModule)
            {
                var index = parts.FindIndex(s => "modules".Equals(s));
                if (0 <= index && index < parts.Count - 1)
                {
                    return parts[index + 1];
                }
            }

            if (webitem is IAddon)
            {
                var index = parts.FindIndex(s => "addons".Equals(s));
                if (0 <= index && index < parts.Count - 1)
                {
                    return parts[index + 1];
                }
            }

            return parts[parts.Count - 1].Split('.')[0];
        }
    }
}
