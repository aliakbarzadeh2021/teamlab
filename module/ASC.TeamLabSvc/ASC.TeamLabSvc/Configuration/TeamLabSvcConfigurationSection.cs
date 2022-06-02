using System.Configuration;

namespace ASC.TeamLabSvc.Configuration
{
    class TeamLabSvcConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty(Schema.SERVICES)]
        public TeamLabSvcConfigurationCollection TeamlabServices
        {
            get { return (TeamLabSvcConfigurationCollection)this[Schema.SERVICES]; }
        }
    }
}
