using System;
using System.IO;
using System.Threading;
using ASC.FullTextIndex.Service.Cfg;
using log4net;

namespace ASC.FullTextIndex.Service
{
    public class TextIndexerService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TextIndexerService));

        private readonly TextIndexCfg configuration;
        private readonly TenantsProvider tenantsProvider;

        private Thread worker;


        public TextIndexerService()
        {
            configuration = new TextIndexCfg();
            tenantsProvider = new TenantsProvider(configuration.ConnectionString);
        }


        public void Start()
        {
            if (worker != null) return;

            worker = new Thread(DoWork)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest,
                Name = "Full Text Indexer",
            };
            worker.Start();
        }

        public void Stop()
        {
            if (worker == null) return;
            worker.Abort();
            worker.Join();
            worker = null;
        }


        private void DoWork()
        {
            var period = TimeSpan.FromSeconds(1);
            var action = TextIndexAction.None;
            do
            {
                try
                {
                    Thread.Sleep(period);

                    DoIndex(action);

                    var now = DateTime.UtcNow;
                    var indexDateTime = configuration.ChangedCron.GetTimeAfter(now) ?? DateTime.MaxValue;
                    var removeDateTime = configuration.RemovedCron.GetTimeAfter(now) ?? DateTime.MaxValue;

                    action = TextIndexAction.Index | TextIndexAction.Remove;
                    if (indexDateTime < removeDateTime) action = TextIndexAction.Index;
                    if (indexDateTime > removeDateTime) action = TextIndexAction.Remove;

                    period = ((indexDateTime < removeDateTime ? indexDateTime : removeDateTime) - now).Add(TimeSpan.FromSeconds(1));

                    log.DebugFormat("Next action '{0}' over {1}", action, period);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error in DoIndex: {0}", ex);
                    period = TimeSpan.FromMinutes(10);
                }
            }
            while (true);
        }

        private void DoIndex(TextIndexAction action)
        {
            if (action == TextIndexAction.None) return;

            foreach (var t in tenantsProvider.GetTenants())
            {
                foreach (var m in configuration.Modules)
                {
                    var indexPath = configuration.GetIndexPath(t.TenantId, m.Name);
                    using (var indexer = new TextIndexer(indexPath, t, m))
                    {
                        try
                        {
                            Thread.Sleep(configuration.Delay);
                            if (TextIndexAction.Index == (action & TextIndexAction.Index))
                            {
                                var affected = indexer.FindChangedAndIndex();
                                log.DebugFormat("Indexed {0} objects at tenant {1} in module {2}", affected, t, m);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Error FindChangedAndIndex in tenant {0}: {1}", t, ex);
                        }

                        try
                        {
                            Thread.Sleep(configuration.Delay);
                            if (TextIndexAction.Remove == (action & TextIndexAction.Remove))
                            {
                                var affected = indexer.FindRemovedAndIndex();
                                log.DebugFormat("Removed {0} objects at tenant {1} in module {2}", affected, t, m);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Error FindRemovedAndIndex in tenant {0}: {1}", t, ex);
                        }
                    }
                }
            }
        }


        [Flags]
        enum TextIndexAction
        {
            None = 0,
            Index = 1,
            Remove = 2,
        }
    }
}
