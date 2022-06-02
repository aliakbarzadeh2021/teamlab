using System.Configuration;

namespace ASC.TeamLabSvc.Configuration
{
    class TeamLabSvcConfigurationElement: ConfigurationElement
    {
        [ConfigurationProperty(Schema.TYPE, IsKey = true, IsRequired = true)]
		public string Type
		{
			get { return (string)this[Schema.TYPE]; }
			set { this[Schema.TYPE] = value; }
		}

        [ConfigurationProperty(Schema.DISABLE, DefaultValue = false)]
        public bool Disable
        {
            get { return (bool)this[Schema.DISABLE]; }
            set { this[Schema.DISABLE] = value; }
        }
    }
}
