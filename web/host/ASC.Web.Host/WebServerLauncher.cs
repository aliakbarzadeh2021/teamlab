using System;
using ASC.Common.Module;
using log4net;

namespace ASC.Web.Host
{
    public class WebServerLauncher : IServiceController
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Host");
        private IServer webServer;


        public string ServiceName
        {
            get { return "Web server"; }
        }
        
        
        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            webServer = ServerFactory.CreateHTTPWebServer();
            webServer.Start();

            log.InfoFormat("Web server start site {0} on {1}", webServer.PhysicalPath, string.Format("{0}://*:{1}{2}", webServer.Scheme, webServer.Port, webServer.VirtualPath));
        }

        public void Stop()
        {
            if (webServer != null)
            {
                webServer.Stop();
                webServer = null;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error(e.ExceptionObject);
        }
    }
}
