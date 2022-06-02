using System;
using System.Linq;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.Utility.Settings;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ASC.Web.Core
{
    public static class ProductModuleExtension
    {
        
        
        public static string GetSmallIconAbsoluteURL(this IModule module)
        {
            if (module == null || module.Context == null || String.IsNullOrEmpty(module.Context.SmallIconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(module.Context.SmallIconFileName, module.ModuleID);
        }

        public static string GetSmallIconAbsoluteURL(this IProduct product)
        {
            if (product == null || product.Context == null || String.IsNullOrEmpty(product.Context.SmallIconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(product.Context.SmallIconFileName, product.ProductID);
        }

        public static string GetIconAbsoluteURL(this IModule module)
        {
            if (module == null || module.Context == null || String.IsNullOrEmpty(module.Context.IconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(module.Context.IconFileName, module.ModuleID);
        }

        public static string GetIconAbsoluteURL(this IProduct product)
        {
            if (product == null || product.Context == null || String.IsNullOrEmpty(product.Context.IconFileName))
                return "";

            return WebImageSupplier.GetAbsoluteWebPath(product.Context.IconFileName, product.ProductID);
        }

        public static IHtmlInjectionProvider GetInjectionProvider(this IProduct product)
        {
            if (product != null && product.Context != null)
                return GetInjectionProvider(product.Context.HtmlInjectionProviderType);

            return null;
        }

        public static IHtmlInjectionProvider GetInjectionProvider(this IModule module)
        {
            if (module != null && module.Context != null)
                return GetInjectionProvider(module.Context.HtmlInjectionProviderType);

            return null;
        }

        private static IHtmlInjectionProvider GetInjectionProvider(Type type)
        {
            if (type == null)
                return null;

            foreach (var interfaceType in type.GetInterfaces())
            { 
                if(interfaceType.Equals(typeof(IHtmlInjectionProvider)))
                    return (IHtmlInjectionProvider)Activator.CreateInstance(type);
            }

            return null;
        }

    }
}
