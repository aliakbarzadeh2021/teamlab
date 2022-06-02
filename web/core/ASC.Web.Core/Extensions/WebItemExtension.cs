using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core
{
    public static class WebItemExtension
    {
        public static string GetSysName(this IWebItem webitem)
        {
            if (string.IsNullOrEmpty(webitem.StartURL)) return string.Empty;

            var sysname = string.Empty;
            var parts = webitem.StartURL.ToLower().Split('/', '\\').ToList();

            var index = parts.FindIndex(s => "products".Equals(s));
            if (0 <= index && index < parts.Count - 1)
            {
                sysname = parts[index + 1];
                index = parts.FindIndex(s => "modules".Equals(s));
                if (0 <= index && index < parts.Count - 1)
                {
                    sysname += "-" + parts[index + 1];
                }
                else if (index == parts.Count - 1)
                {
                    sysname = parts[index].Split('.')[0];
                }
                return sysname;
            }

            index = parts.FindIndex(s => "addons".Equals(s));
            if (0 <= index && index < parts.Count - 1)
            {
                sysname = parts[index + 1];
            }

            return sysname;
        }


        public static string GetSmallIconAbsoluteURL(this IWebItem item)
        {
            if (item == null || item.Context == null || String.IsNullOrEmpty(item.Context.SmallIconFileName)) return string.Empty;

            return WebImageSupplier.GetAbsoluteWebPath(item.Context.SmallIconFileName, item.ID);
        }

        public static string GetIconAbsoluteURL(this IWebItem item)
        {
            if (item == null || item.Context == null || String.IsNullOrEmpty(item.Context.IconFileName)) return string.Empty;

            return WebImageSupplier.GetAbsoluteWebPath(item.Context.IconFileName, item.ID);
        }

        public static string GetLargeIconAbsoluteURL(this IWebItem item)
        {
            if (item == null || item.Context == null || String.IsNullOrEmpty(item.Context.LargeIconFileName)) return string.Empty;

            return WebImageSupplier.GetAbsoluteWebPath(item.Context.LargeIconFileName, item.ID);
        }


        public static IHtmlInjectionProvider GetInjectionProvider(this IWebItem item)
        {
            if (item != null && item.Context != null && item.Context.HtmlInjectionProviderType != null)
            {
                var type = item.Context.HtmlInjectionProviderType;
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.Equals(typeof(IHtmlInjectionProvider)))
                    {
                        return (IHtmlInjectionProvider)Activator.CreateInstance(type);
                    }
                }
            }
            return null;
        }


        public static bool IsDisabled(this IWebItem item)
        {
            return item.IsDisabled(SettingsManager.Instance.LoadSettings<WebItemSettings>(CoreContext.TenantManager.GetCurrentTenant().TenantId));
        }

        public static bool IsDisabled(this IWebItem item, WebItemSettings settings)
        {
            if (item == null) return false;

            var s = settings.SettingsCollection.Find(o => o.ItemID == item.ID);
            return s != null ? s.Disabled : false;
        }


        public static bool IsDisabledForPublic(this IWebItem item)
        {
            if (item == null) return false;
            if (SecurityContext.CurrentAccount.IsAuthenticated) return false;

            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            if (!tenant.Public) return false;

            var result = !tenant.PublicVisibleProducts.Exists(s => string.Equals(s, item.ID.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (result)
            {
                var parentID = WebItemManager.Instance.GetParentItemID(item.ID);
                if (!parentID.Equals(Guid.Empty))
                {
                    return !tenant.PublicVisibleProducts.Exists(s => string.Equals(s, parentID.ToString(), StringComparison.InvariantCultureIgnoreCase));
                }
            }
            return result;

        }


        public static List<IWebItem> SortItems(this IEnumerable<IWebItem> items, WebItemSettings settings)
        {
            return items.OrderBy(i => WebItemManager.GetSortOrder(i, settings)).ToList();
        }

        public static bool IsSubItem(this IWebItem item)
        {
            return item is IModule && !(item is IProduct); // module and not product
        }
    }
}
