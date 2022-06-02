using System;
using System.Globalization;
using System.Web;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Files.Classes
{
    public static class PathProvider
    {
        public static readonly String BaseVirtualPath = "~/products/files/".ToLower();

        public static readonly String ProjectVirtualPath = "~/products/projects/tmdocs.aspx".ToLower();

        public static readonly String BaseAbsolutePath = VirtualPathUtility.ToAbsolute(BaseVirtualPath).ToLower();

        public static readonly String JsPhysicalResourcesPath = HttpContext.Current.Server.MapPath(@"~\products\files\js\");

        public static readonly String TemplatePath = "/products/files/templates/".ToLower();


        public static String StartURL()
        {
            return "~/products/files/".ToLower();
        }

        public static String GetDocServicePath()
        {
            return VirtualPathUtility.ToAbsolute("~/products/files/services/docservice.asmx");
        }

        public static String GetFileServicePath()
        {
            return VirtualPathUtility.ToAbsolute("~/products/files/services/wcfservice/service.svc/");
        }

        public static String GetPluginPath()
        {
            return VirtualPathUtility.ToAbsolute("~/products/files/plugin/install.msi");
        }

        public static string GetImagePath(string imgFileName)
        {
            return WebImageSupplier.GetAbsoluteWebPath(imgFileName, Configuration.ProductEntryPoint.ID);
        }

        public static String GetResourcesPath()
        {
            return GetFileStaticRelativePath(String.Format("resources.{0}.js", CultureInfo.CurrentCulture.Name.ToLower()));
        }

        public static String GetFileStaticRelativePath(String fileName)
        {
            if (fileName.EndsWith(".js"))
            {
                return WebPath.GetPath("/products/files/js/" + fileName);
            }
            if (fileName.EndsWith(".ascx"))
            {
                return VirtualPathUtility.ToAbsolute("~/products/files/controls/" + fileName);
            }
            if (fileName.EndsWith(".css"))
            {
                return WebSkin.GetUserSkin().GetAbsoluteWebPath("/products/files/app_themes/<theme_folder>/" + fileName);
            }
            return fileName;
        }
    }
}