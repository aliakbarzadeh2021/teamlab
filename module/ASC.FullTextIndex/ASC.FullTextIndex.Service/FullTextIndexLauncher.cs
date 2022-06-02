using System.ServiceModel;
using ASC.Common.Module;
using log4net;
using log4net.Config;

namespace ASC.FullTextIndex.Service
{
    public class FullTextIndexLauncher: IServiceController
    {
        private static ILog log = LogManager.GetLogger(typeof(Program));

        private TextIndexerService indexer;
        private ServiceHost searcher;


        public string ServiceName
        {
            get { return "Full Text Index"; }
        }
        
        
        public void Start()
        {
            XmlConfigurator.Configure();

            indexer = new TextIndexerService();
            indexer.Start();

            searcher = new ServiceHost(typeof(TextSearcherService));
            searcher.Open();

            log.Debug("Full text index service started.");
        }

        public void Stop()
        {
            searcher.Close();
            indexer.Stop();

            searcher = null;
            indexer = null;

            log.Debug("Full text index service stopped.");
        }
    }
}
