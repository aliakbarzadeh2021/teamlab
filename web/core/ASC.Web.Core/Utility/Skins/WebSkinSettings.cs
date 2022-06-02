using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Settings;
using System.Reflection;

namespace ASC.Web.Core.Utility.Skins
{
	[Serializable]
	public class WebSkin
	{
        private string _name;
        public string Name
        {
            get
            {
                if (!String.IsNullOrEmpty(_name) && _name.IndexOf(";") >= 0)
                {
                    try
                    {                      
                        var typeName = _name.Split(';')[0];
                        var resKey = _name.Split(';')[1];

                        return (string)Type.GetType(typeName).GetProperty(resKey, BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
                    }
                    catch { return _name; };
                }

                return _name;
            }
            set { _name = value; }
        }

		public string ID { get; set; }

		public List<string> CSSFileNames { get; set; }

		public string BaseCSSFileName { get; set; }

		public string FolderName { get; set; }

        public string ASPTheme { get; set; }

        public int SortOrder { get; set; }

        public string GetAbsoluteWebPath(string path)
        {
            var p = path.ToLower().Replace("<theme_folder>", this.FolderName.ToLower());
            if(WebPath.Exists(p))
                return WebPath.GetPath(p);

            p = path.ToLower().Replace("<theme_folder>", DefaultSkin.FolderName.ToLower());
            return WebPath.GetPath(p);
        }

		public string BaseCSSFileAbsoluteWebPath
		{
			get
			{
                return GetAbsoluteWebPath("/skins/<theme_folder>/" + this.BaseCSSFileName.ToLower());
			}
		}

		public string GetCSSFileAbsoluteWebPath(string fileName)
		{
            return GetAbsoluteWebPath("/skins/<theme_folder>/" + fileName.ToLower());
		}

		public WebSkin()
		{
			CSSFileNames = new List<string>();
		}

		private static WebSkin defaultSkin;

		public static WebSkin DefaultSkin
		{
			get
			{
				if (defaultSkin == null)
				{
					defaultSkin = MakeSkinFromDirectory(HttpContext.Current.Server.MapPath("~/app_themes/default"));
				}
				return defaultSkin;
			}
		}

        public static List<WebSkin> GetSkinCollection() 
        {
            var skins = new List<WebSkin>();

            var basePath = HttpContext.Current.Server.MapPath("~/app_themes");
            var directories = Directory.GetDirectories(basePath);
            foreach (var d in directories)
            {
                var skin = MakeSkinFromDirectory(d);
                if(skin!=null)
                    skins.Add(skin);
            }
            skins.Sort((s1, s2) => Comparer<Int32>.Default.Compare(s1.SortOrder, s2.SortOrder));

            return skins;
        }

		public static WebSkin MakeSkinFromDirectory(string skinDir)
		{
			if (File.Exists(skinDir + "\\skin.xml"))
			{
				WebSkin skin = new WebSkin();
				skin.FolderName = new DirectoryInfo(skinDir).Name;

				XmlDocument doc = new XmlDocument();
				doc.Load(skinDir + "\\skin.xml");

				XmlNodeList list = doc.GetElementsByTagName("ID");
				if (list != null && list.Count > 0)
					skin.ID = list[0].InnerText;

                list = doc.GetElementsByTagName("SortOrder");
                if (list != null && list.Count > 0)
                    skin.SortOrder = Convert.ToInt32(list[0].InnerText);

				list = doc.GetElementsByTagName("Name");
				if (list != null && list.Count > 0)
					skin.Name = list[0].InnerText;

                list = doc.GetElementsByTagName("ASPTheme");
                if (list != null && list.Count > 0)
                    skin.ASPTheme= list[0].InnerText;

				list = doc.GetElementsByTagName("BaseCSSFileName");
				if (list != null && list.Count > 0)
				{
					skin.BaseCSSFileName = list[0].InnerText;
					skin.CSSFileNames.Add(skin.BaseCSSFileName);
				}

				list = doc.GetElementsByTagName("CSSFileName");
				if (list != null && list.Count > 0)
				{
					foreach (XmlNode node in list)
						skin.CSSFileNames.Add(node.InnerText);
				}
				return skin;
			}
			return null;
		}

		internal string GetImageAbsoluteWebPath(string fileName)
		{
			return GetImageAbsoluteWebPath(fileName, Guid.Empty);
		}

		internal string GetImageAbsoluteWebPath(string fileName, Guid partID)
		{
			if (HttpContext.Current == null || string.IsNullOrEmpty(fileName))
				return "";

			string filepath = GetPartImageFolderRel(partID) + "/" + fileName;
			return GetAbsoluteWebPath(filepath);
		}

		private string GetPartImageFolderRel(Guid partID)
		{
            string folderName = "/skins/<theme_folder>/images";
			string itemFolder = null;
			if (!Guid.Empty.Equals(partID))
			{
				var product = ProductManager.Instance[partID];
				if (product != null &&
					product.Context != null &&
				   !String.IsNullOrEmpty(product.Context.ThemesFolderVirtualPath) &&
				   !String.IsNullOrEmpty(product.Context.ImageFolder))
				{

                    itemFolder = product.Context.ThemesFolderVirtualPath.TrimEnd('/') + "/<theme_folder>/" + product.Context.ImageFolder;
				}
				else if (product != null &&  product.Context != null && !String.IsNullOrEmpty(product.Context.ImageFolder))
                    itemFolder = "/skins/<theme_folder>/modules/" + product.Context.ImageFolder;


				if (itemFolder == null)
				{
					var module = ProductManager.Instance.GetModuleByID(partID);
					if (module != null &&
					   module.Context != null &&
					  !String.IsNullOrEmpty(module.Context.ThemesFolderVirtualPath) &&
					  !String.IsNullOrEmpty(module.Context.ImageFolder))
					{
                        itemFolder = module.Context.ThemesFolderVirtualPath.TrimEnd('/') + "/<theme_folder>/" + module.Context.ImageFolder;
					}
					else if (module != null && module.Context!=null && !String.IsNullOrEmpty(module.Context.ImageFolder))
                        itemFolder = "/skins/<theme_folder>/modules/" + module.Context.ImageFolder;
				}

                if (itemFolder == null)
                {
                    var addon = WebItemManager.Instance[partID] as IAddon;
                    if (addon != null &&
                       addon.Context != null &&
                      !String.IsNullOrEmpty(addon.Context.ThemesFolderVirtualPath) &&
                      !String.IsNullOrEmpty(addon.Context.ImageFolder))
                    {
                        itemFolder = addon.Context.ThemesFolderVirtualPath.TrimEnd('/') + "/<theme_folder>/" + addon.Context.ImageFolder;
					}
					else if (addon != null && addon.Context != null && !String.IsNullOrEmpty(addon.Context.ImageFolder))
                        itemFolder = "/skins/<theme_folder>/modules/" + addon.Context.ImageFolder;
				}

				folderName = itemFolder ?? folderName;
			}
			return folderName.TrimStart('~').ToLowerInvariant();
		}

		public static WebSkin GetUserSkin()
		{
            return GetUserSkin(Guid.Empty);
            //if (SecurityContext.IsAuthenticated)
            //    return GetUserSkin(SecurityContext.CurrentAccount.ID);

            //return DefaultSkin;
		}

		public static WebSkin GetUserSkin(Guid userID)
		{
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if(tenant!=null)
			    return SettingsManager.Instance.LoadSettings<WebSkinSettings>(tenant.TenantId).WebSkin;

            return DefaultSkin;
		}

		internal string GetImageFolderAbsoluteWebPath(Guid partID)
		{
			if (HttpContext.Current == null)
				return "";

			string currentThemePath = GetPartImageFolderRel(partID);
            return GetAbsoluteWebPath(currentThemePath);
		}
	}

	[Serializable]
	public class WebSkinSettings : ISettings
	{
		public WebSkin WebSkin { get; set; }

		#region ISettings Members

		public Guid ID
		{
			get { return new Guid("{B209FA64-0607-40ba-9C2D-173378EF3E70}"); }
		}

		public ISettings GetDefault()
		{
			return new WebSkinSettings() { WebSkin = WebSkin.DefaultSkin };
		}

		#endregion
	}
}
