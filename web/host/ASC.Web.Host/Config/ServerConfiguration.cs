using System;
using System.Configuration;
using System.Net;
using ASC.Web.Host.HttpHandlers;

namespace ASC.Web.Host.Config
{
    class ServerConfiguration
    {
        public static string Scheme
        {
            get;
            private set;
        }

        public static int Port
        {
            get;
            private set;
        }

        public static string VirtualPath
        {
            get;
            private set;
        }

        public static string PhysicalPath
        {
            get;
            private set;
        }

        public static AuthenticationSchemes AuthenticationSchemes
        {
            get;
            private set;
        }

        public static string DefaultDocument
        {
            get;
            private set;
        }

        public static HttpHandlerElementCollection HttpHandlers
        {
            get;
            private set;
        }

        public static int BufferSize
        {
            get;
            set;
        }

        public static bool HackWCFBinding
        {
            get;
            set;
        }

        static ServerConfiguration()
        {
            var section = ConfigurationManager.GetSection("webServer") as ServerSection ?? new ServerSection();

            AuthenticationSchemes = section.AuthenticationSchemes;
            Port = section.Port;
            VirtualPath = section.VirtualPath;
            PhysicalPath = section.PhysicalPath;
            DefaultDocument = section.DefaultDocument;
            Scheme = section.Scheme;
            RequestConfiguration.DefaultFileNames = section.DefaultFileNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            RequestConfiguration.RestrictedDirs = section.RestrictedDirs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            BufferSize = section.BufferSize;
            HackWCFBinding = section.HackWCFBinding;

            if (section.HttpHandlers.Count == 0)
            {
                HttpHandlers = new HttpHandlerElementCollection();
                HttpHandlers.Add("*", typeof(StaticFileHttpHandler));
                foreach (var ext in aspHttpExtensions)
                {
                    HttpHandlers.Add(ext, typeof(AspNetHttpHandler));
                }
            }
            else
            {
                HttpHandlers = section.HttpHandlers;
            }
        }


        private static readonly string[] aspHttpExtensions = new[]
        {
            "/",
            "ad",
            "adprototype",
            "asa",
            "asax",
            "ascx",
            "ashx",
            "asmx",
            "asp",
            "aspx",
            "axd",
            "browser",
            "cd",
            "cdx",
            "cer",
            "compiled",
            "dll.config",
            "exe.config",
            "config",
            "cs",
            "csproj",
            "dd",
            "exclude",
            "jdc",
            "java",
            "jsl",
            "ldb",
            "ldd",
            "lddprototype",
            "ldf",
            "lic",
            "licx",
            "master",
            "mdb",
            "mdf",
            "msgx",
            "refresh",
            "rem",
            "resources",
            "resx",
            "sd",
            "sdm",
            "sdmDocument",
            "shtm",
            "shtml",
            "sitemap",
            "skin",
            "soap",
            "stm",
            "svc",
            "vb",
            "vbproj",
            "vjsproj",
            "vsdisco",
            "xoml",
            "xsd",
            "webinfo",
            "wsdl",
        };
    }
}