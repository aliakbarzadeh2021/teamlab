﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using ASC.Web.Host.Config;
using ASC.Web.Host.HttpHandlers;
using log4net;

namespace ASC.Web.Host.HttpRequestProcessor
{
    sealed class Server : MarshalByRefObject, IServer
    {
        private readonly AuthenticationSchemes authSchemes;

        private volatile Host _host;
        private readonly HttpListener listener;
        private CultureInfo currentThreadCulture;

        private readonly WaitCallback onStart;
        private readonly WaitCallback onGetContext;

        private volatile bool shutdownInProgress;
        private readonly ManualResetEvent waitContext = new ManualResetEvent(false);

        private static readonly IList<string> supportedSchemas = new List<string>();
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Host");

        static Server()
        {
            supportedSchemas.Add(Uri.UriSchemeHttp);
        }


        public static bool IsSupported
        {
            get { return HttpListener.IsSupported; }
        }

        public string Scheme
        {
            get;
            private set;
        }

        public int Port
        {
            get;
            private set;
        }

        public string VirtualPath
        {
            get;
            private set;
        }

        public string PhysicalPath
        {
            get;
            private set;
        }

        public string DefaultDocument
        {
            get;
            private set;
        }

        public bool IsCustomOutput
        {
            get;
            private set;
        }

        public string CustomHtml
        {
            get;
            private set;
        }

        public int CustomCode
        {
            get;
            private set;
        }

        public bool HackWCFBinding
        {
            get;
            set;
        }

        internal Server()
            : this(
                ServerConfiguration.Port,
                ServerConfiguration.VirtualPath,
                ServerConfiguration.PhysicalPath,
                ServerConfiguration.DefaultDocument,
                ServerConfiguration.Scheme,
                ServerConfiguration.AuthenticationSchemes) { }

        internal Server(int port, string virtualPath, string physicalPath)
            : this(
                port,
                virtualPath,
                physicalPath,
                string.Empty,
                Uri.UriSchemeHttp,
                ServerConfiguration.AuthenticationSchemes) { }

        internal Server(int port, string virtualPath, string physicalPath, string defaultDocument, string scheme)
            : this(
                port,
                virtualPath,
                physicalPath,
                defaultDocument,
                scheme,
                ServerConfiguration.AuthenticationSchemes) { }

        internal Server(int port,
                      string virtualPath,
                      string physicalPath,
                      string defaultDocument,
                      string scheme,
                      AuthenticationSchemes authSchemes)
        {

            if (!supportedSchemas.Contains(scheme.ToLowerInvariant()))
            {
                throw new ArgumentException(ServerMessages.SchemeNotSupported, "scheme");
            }

            DefaultDocument = defaultDocument ?? string.Empty;

            Scheme = scheme;

            Port = port;

            VirtualPath = virtualPath.EndsWith(@"/", StringComparison.Ordinal) ? virtualPath : (virtualPath + @"/");

            this.authSchemes = authSchemes;

            HackWCFBinding = ServerConfiguration.HackWCFBinding;

            if (!physicalPath.EndsWith(@"\")) physicalPath += @"\";
            PhysicalPath = Path.IsPathRooted(physicalPath) ? physicalPath : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, physicalPath));

            onGetContext = OnGetContext;
            onStart = OnStart;

            listener = new HttpListener()
            {
                AuthenticationSchemes = authSchemes,
                IgnoreWriteExceptions = true,
            };
            listener.Prefixes.Add(string.Format("{0}://*:{1}{2}", Scheme, Port, VirtualPath));
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            shutdownInProgress = false;
            currentThreadCulture = Thread.CurrentThread.CurrentCulture;
            if (!listener.IsListening)
            {
                listener.Start();
                ThreadPool.QueueUserWorkItem(onStart);
            }
        }

        public void Stop()
        {
            shutdownInProgress = true;
            try
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
            catch { }
            try
            {
                if (_host != null)
                {
                    _host.Shutdown();
                }
                for (int i = 0; (i < 10) && (_host != null); i++)
                {
                    Thread.Sleep(100);
                }
            }
            catch { }
            finally
            {
                _host = null;
            }
        }

        public void SetCustomOutput(string htmlPage)
        {
            lock (this)
            {
                CustomHtml = htmlPage;
                IsCustomOutput = true;
            }
        }

        public void CancelCustomOutput()
        {
            IsCustomOutput = false;
        }

        private AuthenticationSchemes AuthSelector(HttpListenerRequest httpRequest)
        {
            return authSchemes;
        }

        private void OnStart(object unused)
        {
            waitContext.Set();
            while (!shutdownInProgress)
            {
                try
                {
                    var ctx = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(onGetContext, ctx);
                    continue;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        private void OnGetContext(object ctx)
        {
            if (shutdownInProgress) return;

            try
            {
                var context = (HttpListenerContext)ctx;
                var conn = new Connection(this, context);

                if (!Thread.CurrentThread.CurrentCulture.Equals(currentThreadCulture))
                {
                    Thread.CurrentThread.CurrentCulture = currentThreadCulture;
                    Thread.CurrentThread.CurrentUICulture = currentThreadCulture;
                }

                if (conn.WaitForRequestBytes() == 0)
                {
                    conn.WriteErrorAndClose(400);
                }
                else
                {
                    var host = GetHost();
                    if (host == null)
                    {
                        conn.WriteErrorAndClose(500);
                    }
                    else
                    {
                        HttpHandlerFactory.GetHttpHandler(context)
                            .ProcessRequest(new HttpHandlerContext(this, host, conn, Thread.CurrentPrincipal.Identity));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(ServerMessages.ExceptionWhileProcessing, ex);
            }
        }

        private Host GetHost()
        {
            if (shutdownInProgress)
            {
                return null;
            }

            Host host = _host;
            if (host == null)
            {
                lock (this)
                {
                    host = _host;
                    if (host == null)
                    {
                        _host = CreateWorkerAppDomainWithHost<Host>(VirtualPath, PhysicalPath);
                        _host.Configure(this, Port, VirtualPath, PhysicalPath);
                        host = _host;
                    }
                }
            }
            return host;
        }

        private T CreateWorkerAppDomainWithHost<T>(string virtualPath, string physicalPath) where T : IRegisteredObject
        {
            string appId = (VirtualPath + PhysicalPath).ToLowerInvariant()
                .GetHashCode().ToString("x", CultureInfo.InvariantCulture);

            var hostType = typeof(T);
            var appManager = ApplicationManager.GetApplicationManager();
            var buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
            var buildManagerHost = appManager.CreateObject(appId, buildManagerHostType, virtualPath, physicalPath, false);

            // call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
            buildManagerHostType.InvokeMember("RegisterAssembly",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null,
                buildManagerHost,
                new object[] { hostType.Assembly.FullName, hostType.Assembly.Location }
            );

            return (T)appManager.CreateObject(appId, hostType, virtualPath, physicalPath, false);
        }

        internal void HostStopped()
        {
            _host = null;
        }

        internal void OnRequestEnd(Connection conn)
        {
            RemotingServices.Disconnect(conn);
        }
    }
}