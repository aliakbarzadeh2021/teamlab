﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;

namespace ASC.Web.UserControls.Wiki.Handlers
{
    public class WikiSection : ConfigurationSection
    {
        public static WikiSection Section
        {
            get
            {
                return (ConfigurationManager.GetSection("wikiSettings") as WikiSection);

            }
        }

        //[ConfigurationProperty("mainPage")]
        //public MainPageElement MainPage
        //{
        //    get
        //    {
        //        return (MainPageElement)this["mainPage"];
        //    }
        //    set
        //    {
        //        this["mainPage"] = value;
        //    }
        //}

        [ConfigurationProperty("dbase")]
        public DBElement DB
        {
            get
            {
                return (DBElement)this["dbase"];
            }
            set
            {
                this["dbase"] = value;
            }
        }

        [ConfigurationProperty("fckeditorInfo")]
        public FckeditorInfoElement FckeditorInfo
        {
            get
            {
                return (FckeditorInfoElement)this["fckeditorInfo"];
            }
            set
            {
                this["fckeditorInfo"] = value;
            }
        }

        [ConfigurationProperty("dataStorage")]
        public DataStorageElement DataStorage
        {
            get
            {
                return (DataStorageElement)this["dataStorage"];
            }
            set
            {
                this["dataStorage"] = value;
            }
        }

        [ConfigurationProperty("imageHangler")]
        public ImageHanglerElement ImageHangler
        {
            get
            {
                return (ImageHanglerElement)this["imageHangler"];
            }
            set
            {
                this["imageHangler"] = value;
            }
        }

    }

    public class MainPageElement : ConfigurationElement
    {
        [ConfigurationProperty("url", DefaultValue = "", IsRequired = true)]
        public string Url
        {
            get
            {
                return (String)this["url"];
            }
            set
            {
                this["url"] = value;
            }
        }

        [ConfigurationProperty("wikiView", DefaultValue = "", IsRequired = true)]
        public string WikiView
        {
            get
            {
                return (string)this["wikiView"];
            }
            set
            {
                this["wikiView"] = value;
            }
        }

        [ConfigurationProperty("wikiEdit", DefaultValue = "", IsRequired = true)]
        public string WikiEdit
        {
            get
            {
                return (string)this["wikiEdit"];
            }
            set
            {
                this["wikiEdit"] = value;
            }
        }
    }

    public class DBElement : ConfigurationElement
    {
        [ConfigurationProperty("connectionStringName", DefaultValue = "", IsRequired = true)]
        public string ConnectionStringName
        {
            get
            {
                return (string)this["connectionStringName"];
            }
            set
            {
                this["connectionStringName"] = value;
            }
        }
    }

    public class FileStorageElement : ConfigurationElement
    {
        [ConfigurationProperty("location", DefaultValue = "", IsRequired = true)]
        public string Location
        {
            get
            {
                return (string)this["location"];
            }
            set
            {
                this["location"] = value;
            }
        }
    }

    public class DataStorageElement : ConfigurationElement
    {
        [ConfigurationProperty("moduleName", DefaultValue = "", IsRequired = true)]
        public string ModuleName
        {
            get
            {
                return (string)this["moduleName"];
            }
            set
            {
                this["moduleName"] = value;
            }
        }

        [ConfigurationProperty("defaultDomain", DefaultValue = "", IsRequired = true)]
        public string DefaultDomain
        {
            get
            {
                return (string)this["defaultDomain"];
            }
            set
            {
                this["defaultDomain"] = value;
            }
        }

        [ConfigurationProperty("tempDomain", DefaultValue = "", IsRequired = true)]
        public string TempDomain
        {
            get
            {
                return (string)this["tempDomain"];
            }
            set
            {
                this["tempDomain"] = value;
            }
        }
    }

    public class ImageHanglerElement : ConfigurationElement
    {
        [ConfigurationProperty("urlFormat", DefaultValue = "", IsRequired = true)]
        public string UrlFormat
        {
            get
            {
                return (string)this["urlFormat"];
            }
            set
            {
                this["urlFormat"] = value;
            }
        }
    }

    public class FckeditorInfoElement : ConfigurationElement
    {
        [ConfigurationProperty("pathFrom", DefaultValue = "", IsRequired = true)]
        public string PathFrom
        {
            get
            {
                return (string)this["pathFrom"];
            }
            set
            {
                this["pathFrom"] = value;
            }
        }

        [ConfigurationProperty("baseRelPath", DefaultValue = "", IsRequired = true)]
        public string BaseRelPath
        {
            get
            {
                return (string)this["baseRelPath"];
            }
            set
            {
                this["baseRelPath"] = value;
            }
        }
        
    }

    

}
