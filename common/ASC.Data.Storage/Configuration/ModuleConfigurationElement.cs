using System;
using System.Configuration;

namespace ASC.Data.Storage.Configuration
{
    public class ModuleConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty(Schema.NAME, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this[Schema.NAME]; }
            set { this[Schema.NAME] = value; }
        }

        [ConfigurationProperty(Schema.TYPE, IsRequired = true)]
        public string Type
        {
            get { return (string) this[Schema.TYPE]; }
            set { this[Schema.TYPE] = value; }
        }

        [ConfigurationProperty(Schema.DOMAINS, IsDefaultCollection = false)]
        public DomainConfigurationCollection Domains
        {
            get { return (DomainConfigurationCollection) base[Schema.DOMAINS]; }
        }

        [ConfigurationProperty(Schema.ACL, IsRequired = false, DefaultValue = ACL.Read)]
        public ACL Acl
        {
            get { return (ACL) this[Schema.ACL]; }
            set { this[Schema.ACL] = value; }
        }

        [ConfigurationProperty(Schema.EXPIRES, IsRequired = false)]
        public TimeSpan Expires
        {
            get { return (TimeSpan) this[Schema.EXPIRES]; }
            set { this[Schema.EXPIRES] = value; }
        }

        [ConfigurationProperty(Schema.PATH, IsRequired = false)]
        public string Path
        {
            get { return (string) this[Schema.PATH]; }
            set { this[Schema.PATH] = value; }
        }

        [ConfigurationProperty(Schema.DATA, IsRequired = false)]
        public string Data
        {
            get { return (string) this[Schema.DATA]; }
            set { this[Schema.DATA] = value; }
        }

        [ConfigurationProperty(Schema.QUOTA, IsRequired = false, DefaultValue = long.MaxValue)]
        public long Quota
        {
            get { return (long) this[Schema.QUOTA]; }
            set { this[Schema.QUOTA] = value; }
        }

        [ConfigurationProperty(Schema.OVERWRITE, IsRequired = false, DefaultValue = true)]
        public bool Overwrite
        {
            get { return (bool) this[Schema.OVERWRITE]; }
            set { this[Schema.OVERWRITE] = value; }
        }

        [ConfigurationProperty(Schema.VISIBLE, IsRequired = false, DefaultValue = true)]
        public bool Visible
        {
            get { return (bool) this[Schema.VISIBLE]; }
            set { this[Schema.VISIBLE] = value; }
        }

        [ConfigurationProperty(Schema.COUNT_QUOTA, IsRequired = false, DefaultValue = true)]
        public bool Count
        {
            get { return (bool)this[Schema.COUNT_QUOTA]; }
            set { this[Schema.COUNT_QUOTA] = value; }
        }

        [ConfigurationProperty(Schema.VIRTUALPATH, IsRequired = false)]
        public string VirtualPath
        {
            get { return (string) this[Schema.VIRTUALPATH]; }
            set { this[Schema.VIRTUALPATH] = value; }
        }

        [ConfigurationProperty(Schema.BASEDIR, IsRequired = false)]
        public string BaseDir
        {
            get { return (string) this[Schema.BASEDIR]; }
            set { this[Schema.BASEDIR] = value; }
        }
    }
}