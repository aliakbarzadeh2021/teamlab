using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using ASC.Common.Module;
using ASC.TeamLabSvc.Configuration;
using log4net;
using log4net.Config;

namespace ASC.TeamLabSvc
{
    class Program : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.TeamLabSvc");
        private static List<IServiceController> services = new List<IServiceController>();

        private static void Main(string[] args)
        {
#if DEBUG
            if (bool.TrueString.Equals(ConfigurationManager.AppSettings["debugBreak"], StringComparison.InvariantCultureIgnoreCase))
            {
                Debugger.Launch();
            }
#endif
            XmlConfigurator.Configure();

            if (Environment.UserInteractive)
            {
                StartServices();

                Console.WriteLine("\r\nPRESS ANY KEY TO STOP...\r\n");
                Console.ReadKey();

                StopServices();
            }
            else
            {
                Run(new Program());
            }
        }

        private static void StartServices()
        {
            try
            {
                var section = (TeamLabSvcConfigurationSection)ConfigurationManager.GetSection(Schema.SECTION_NAME);
                foreach (TeamLabSvcConfigurationElement e in section.TeamlabServices)
                {
                    if (!e.Disable)
                    {
                        services.Add((IServiceController)Activator.CreateInstance(Type.GetType(e.Type, true)));
                    }
                    else
                    {
                        log.InfoFormat("Skip service {0}", e.Type);
                    }
                }
            }
            catch (Exception error)
            {
                log.ErrorFormat("Can not start services: {0}", error);
                return;
            }

            foreach (var s in services)
            {
                try
                {
                    s.Start();
                    log.InfoFormat("Service '{0}' started.", s.ServiceName);
                }
                catch (Exception error)
                {
                    log.ErrorFormat("Can not start service '{0}': {1}", s.ServiceName, error);
                }
            }
        }

        private static void StopServices()
        {
            foreach (var s in services)
            {
                try
                {
                    s.Stop();
                    log.InfoFormat("Service '{0}' stopped.", s.ServiceName);
                }
                catch (Exception error)
                {
                    log.ErrorFormat("Can not stop service '{0}': {1}", s.ServiceName, error);
                }
            }

            services.Clear();
        }


        protected override void OnStart(string[] args)
        {
            StartServices();
        }

        protected override void OnStop()
        {
            StopServices();
        }
    }
}
