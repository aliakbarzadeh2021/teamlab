using System;
using System.Collections;
using System.IO;
using System.Threading;
using ASC.Core.Common.Publisher;

namespace ASC.Web.Studio.Core.Publisher
{
    public class PublisherHolder
    {
        static object SyncRoot = new object();

        public static IPublisher Instance {
            get {

                //todo: default publisher temporary
                return new DefaultPublisher();

                /*if (_host == null)
                {
                    lock (SyncRoot)
                    {
                        if (_host == null)
                        {
                            _host = new PublisherHost();
                            _host.Initialize(
                                    new DefaultPublisher(),
                                    GetProperties()
                                );
                        }
                    }
                }
                
                return _host.GetPublisher();*/
            }
        }

        private static Hashtable GetProperties()
        {
            var props = new Hashtable
            {
                {"pubhost.pubtype","ASC.Publisher.Publisher, ASC.Publisher"},
                {"pub.updurl",System.Configuration.ConfigurationManager.AppSettings["pub.updurl"]},
                {"pub.checkinterval",System.Configuration.ConfigurationManager.AppSettings["pub.checkinterval"]},
                {"pub.datapath",Path.Combine(AppDomain.CurrentDomain.BaseDirectory,Path.Combine("App_Data","pubdata")) },
                {"pub.lang",Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName}
            };

            return props;
        }


    }
}
