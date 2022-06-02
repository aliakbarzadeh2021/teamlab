using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using ASC.Notify.Cron;

namespace ASC.FullTextIndex.Service.Cfg
{
    class TextIndexCfg
    {
        private readonly string indexPath;


        public CronExpression ChangedCron
        {
            get;
            private set;
        }

        public CronExpression RemovedCron
        {
            get;
            private set;
        }

        public TimeSpan Delay
        {
            get;
            private set;
        }

        public IList<ModuleInfo> Modules
        {
            get;
            private set;
        }

        public ConnectionStringSettings ConnectionString
        {
            get;
            private set;
        }


        public TextIndexCfg()
            : this(Assembly.GetExecutingAssembly().Location + ".config")
        {

        }

        public TextIndexCfg(string configurationFile)
        {
            var cfg = File.Exists(configurationFile) ?
                ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = configurationFile }, ConfigurationUserLevel.None) :
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var fullTextCfg = (TextIndexCfgSectionHandler)cfg.GetSection("fullTextIndex");

            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            indexPath = fullTextCfg.IndexPath.Trim('\\').Trim();
            if (string.IsNullOrEmpty(indexPath))
            {
                indexPath = currentDirectory;
            }
            if (!Path.IsPathRooted(indexPath))
            {
                indexPath = Path.Combine(currentDirectory, indexPath);
            }

            ChangedCron = new CronExpression(fullTextCfg.ChangedCron);
            RemovedCron = new CronExpression(fullTextCfg.RemovedCron);
            Delay = TimeSpan.FromMilliseconds(fullTextCfg.Delay);

            ConnectionString = cfg.ConnectionStrings.ConnectionStrings["core"] ?? cfg.ConnectionStrings.ConnectionStrings[0];

            Modules = fullTextCfg.Modules
                .Cast<TextIndexCfgModuleElement>()
                .Select(e => new ModuleInfo(e.Name, e.Select, cfg.ConnectionStrings.ConnectionStrings[e.ConnectionStringName]))
                .ToList();
        }


        public string GetIndexPath(int tenantId, string module)
        {
            var path = Path.Combine(indexPath, tenantId.ToString());
            path = Path.Combine(path, module);
            return path;
        }
    }
}
