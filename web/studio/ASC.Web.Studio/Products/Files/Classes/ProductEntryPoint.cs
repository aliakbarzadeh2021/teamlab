using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Xml;
using ASC.Web.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Configuration
{
    public class ProductEntryPoint : AbstractProduct, IModule
    {
        #region Members

        public new static readonly Guid ID = new Guid("{E67BE73D-F9AE-4ce1-8FEC-1880CB518CB4}");

        private ProductContext _productContext;

        private ModuleContext _moduleContext;

        #endregion

        public override void Init(ProductContext productContext)
        {
            new SearchHandler();

            JavaScriptResourcePublisher.Execute(PathProvider.JsPhysicalResourcesPath, "ASC.Files.Resources", typeof(FilesJSResource));

            productContext.ThemesFolderVirtualPath = String.Concat(PathProvider.BaseVirtualPath, "App_Themes");
            productContext.ImageFolder = "images";
            productContext.GlobalHandler = new GlobalHandler();
            productContext.MasterPageFile = String.Concat(PathProvider.BaseVirtualPath, "Masters/BasicTemplate.Master").ToLower();
            productContext.IconFileName = "product_logo.png";
            productContext.LargeIconFileName = "product_logolarge.png";
            productContext.DefaultSortOrder = 30;

            productContext.SubscriptionManager = new SubscriptionManager();
            productContext.SpaceUsageStatManager = new FilesSpaceUsageStatManager();

            _moduleContext = new ModuleContext();
            _productContext = productContext;
        }

        public String GetModuleResource(String ResourceClassTypeName, String ResourseKey)
        {
            if (string.IsNullOrEmpty(ResourseKey))
                return string.Empty;
            try
            {
                return (String)Type.GetType(ResourceClassTypeName).GetProperty(ResourseKey, BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        private static Dictionary<String, XmlDocument> _xslTemplates = null;

        public void ProcessRequest(HttpContext context)
        {
            if (_xslTemplates == null)
                _xslTemplates = new Dictionary<String, XmlDocument>();

            if (String.IsNullOrEmpty(context.Request["id"]) || String.IsNullOrEmpty(context.Request["name"]))
                return;

            String TemplateName = context.Request["name"];
            String TemplatePath = context.Request["id"];
            XmlDocument Template = new XmlDocument();
            try
            {
                Template.Load(context.Server.MapPath(String.Format("~{0}{1}.xsl", TemplatePath, TemplateName)));
            }
            catch (Exception)
            {
                return;
            }
            if (Template.GetElementsByTagName("xsl:stylesheet").Count == 0)
                return;

            Dictionary<String, String> Aliases = new Dictionary<String, String>();

            XmlNodeList RegisterAliases = Template.GetElementsByTagName("register");
            while ((RegisterAliases = Template.GetElementsByTagName("register")).Count > 0)
            {
                XmlNode RegisterAlias = RegisterAliases.Item(0);
                if (!String.IsNullOrEmpty(RegisterAlias.Attributes["alias"].Value) && !String.IsNullOrEmpty(RegisterAlias.Attributes["type"].Value))
                    Aliases.Add(RegisterAlias.Attributes["alias"].Value, RegisterAlias.Attributes["type"].Value);
                RegisterAlias.ParentNode.RemoveChild(RegisterAlias);
            }

            XmlNodeList CurrentResources = Template.GetElementsByTagName("resource");

            while ((CurrentResources = Template.GetElementsByTagName("resource")).Count > 0)
            {
                XmlNode CurrentResource = CurrentResources.Item(0);
                if (!String.IsNullOrEmpty(CurrentResource.Attributes["name"].Value))
                {
                    String[] FullName = CurrentResource.Attributes["name"].Value.Split('.');
                    if (FullName.Length == 2 && Aliases.ContainsKey(FullName[0]))
                    {
                        XmlText ResourceValue = Template.CreateTextNode(GetModuleResource(Aliases[FullName[0]], FullName[1]));
                        CurrentResource.ParentNode.InsertBefore(ResourceValue, CurrentResource);
                    }
                }
                CurrentResource.ParentNode.RemoveChild(CurrentResource);
            }

            //_xslTemplates.Add(TemlateKey, Template);

            context.Response.ContentType = "text/xml";
            context.Response.Write(Template.InnerXml);
        }

        public override void Shutdown()
        {

        }

        public override Guid ProductID
        {
            get { return ID; }
        }

        public override string ProductName
        {
            get { return FilesCommonResource.ProductName; }
        }

        public override string ProductDescription
        {
            get { return FilesCommonResource.ProductDescription; }
        }

        public Guid ModuleID
        {
            get { return ID; }
        }

        public string ModuleName
        {
            get { return FilesCommonResource.ProductName; }
        }

        public string ModuleDescription
        {
            get { return FilesCommonResource.ProductDescription; }
        }

        public override string StartURL
        {
            get { return PathProvider.StartURL(); }
        }

        ModuleContext IModule.Context
        {
            get { return _moduleContext; }
        }

        public override IModule[] Modules
        {
            get { return new IModule[] {  }; }
        }

        public override ProductContext Context
        {
            get { return _productContext; }
        }
    }
}