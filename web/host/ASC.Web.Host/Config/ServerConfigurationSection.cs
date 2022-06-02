using System;
using System.Configuration;
using System.Net;

namespace ASC.Web.Host.Config
{
    class ServerSection : ConfigurationSection
    {
        [ConfigurationProperty("authSchemes", DefaultValue = AuthenticationSchemes.Anonymous)]
        public AuthenticationSchemes AuthenticationSchemes
        {
            get { return (AuthenticationSchemes)base["authSchemes"]; }
            set { base["authSchemes"] = value; }
        }

        [ConfigurationProperty("port", DefaultValue = 8080)]
        public int Port
        {
            get { return (int)base["port"]; }
            set { base["port"] = value; }
        }

        [ConfigurationProperty("virtualPath", DefaultValue = "/")]
        public string VirtualPath
        {
            get { return (string)base["virtualPath"]; }
            set { base["virtualPath"] = value; }
        }

        [ConfigurationProperty("physicalPath", IsRequired = true)]
        public string PhysicalPath
        {
            get { return (string)base["physicalPath"]; }
            set { base["physicalPath"] = value; }
        }

        [ConfigurationProperty("defaultDocument", DefaultValue = "Default.aspx")]
        public string DefaultDocument
        {
            get { return (string)base["defaultDocument"]; }
            set { base["defaultDocument"] = value; }
        }

        [ConfigurationProperty("scheme", DefaultValue = "http")]
        public string Scheme
        {
            get { return (string)base["scheme"]; }
            set { base["scheme"] = value; }
        }

        [ConfigurationProperty("defaultFileNames", DefaultValue = "default.aspx,default.htm,default.html,index.htm,index.html")]
        public string DefaultFileNames
        {
            get { return (string)base["defaultFileNames"]; }
            set { base["defaultFileNames"] = value; }
        }

        [ConfigurationProperty("restrictedDirs", DefaultValue = "/bin,/_private_folder,/app_browsers,/app_code,/app_data,/app_localresources,/app_globalresources,/app_webreferences")]
        public string RestrictedDirs
        {
            get { return (string)base["restrictedDirs"]; }
            set { base["restrictedDirs"] = value; }
        }

        [ConfigurationProperty("httpHandlers")]
        public HttpHandlerElementCollection HttpHandlers
        {
            get { return (HttpHandlerElementCollection)base["httpHandlers"]; }
            set { base["httpHandlers"] = value; }
        }

        [ConfigurationProperty("bufferSize", DefaultValue = 65536)]
        public int BufferSize
        {
            get { return (int)base["bufferSize"]; }
            set { base["bufferSize"] = value; }
        }

        [ConfigurationProperty("hackWCFBinding", DefaultValue = true)]
        public bool HackWCFBinding
        {
            get { return (bool)base["hackWCFBinding"]; }
            set { base["hackWCFBinding"] = value; }
        }
    }

    class HttpHandlerElement : ConfigurationElement
    {

        [ConfigurationProperty("extension", IsRequired = true, IsKey = true)]
        public string Extension
        {
            get { return (string)base["extension"]; }
            set { base["extension"] = value; }
        }

        [ConfigurationProperty("handler", IsRequired = true)]
        public string HandlerType
        {
            get { return (string)base["handler"]; }
            set { base["handler"] = value; }
        }
    }

    class HttpHandlerElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new HttpHandlerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((HttpHandlerElement)element).Extension;
        }

        public void Add(string extension, Type handlerType)
        {
            base.BaseAdd(new HttpHandlerElement { Extension = extension, HandlerType = handlerType.FullName });
        }
    }
}