﻿using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Web.Hosting;
using log4net;

namespace ASC.Web.Host.HttpRequestProcessor
{
    internal class Host : MarshalByRefObject, IRegisteredObject
    {
        private string _lowerCasedVirtualPath;
        private string _lowerCasedVirtualPathWithTrailingSlash;
        private volatile int _pendingCallsCount;
        private string _physicalPath;
        private int _port;
        private Server _server;
        private string _virtualPath;
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Host");

        public Host()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Configure(Server server, int port, string virtualPath, string physicalPath)
        {

            _server = server;
            _port = port;
            _virtualPath = virtualPath;
            _lowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(_virtualPath);
            _lowerCasedVirtualPathWithTrailingSlash = virtualPath.EndsWith("/", StringComparison.Ordinal)
                                                          ? virtualPath
                                                          : (virtualPath + "/");
            _lowerCasedVirtualPathWithTrailingSlash =
                CultureInfo.InvariantCulture.TextInfo.ToLower(_lowerCasedVirtualPathWithTrailingSlash);
            _physicalPath = physicalPath;
        }

        public bool IsVirtualPathAppPath(string path)
        {
            if (path == null)
            {
                return false;
            }
            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
            if (!(path == _lowerCasedVirtualPath))
            {
                return (path == _lowerCasedVirtualPathWithTrailingSlash);
            }
            return true;
        }

        public bool IsVirtualPathInApp(string path)
        {
            if (path != null)
            {

                if ((_virtualPath == "/") && path.StartsWith("/", StringComparison.Ordinal))
                {
                    return true;
                }
                path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
                if (path.StartsWith(_lowerCasedVirtualPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    return true;
                }
                if (path == _lowerCasedVirtualPath)
                {
                    return true;
                }
            }
            return false;
        }

        public void ProcessRequest(Connection conn, IIdentity identity)
        {
            AddPendingCall();
            try
            {
                new Request(_server, this, conn, identity).Process();
            }
            catch (Exception ex)
            {
                log.Warn(ServerMessages.ExceptionWhileProcessing, ex);
            }
            finally
            {
                RemovePendingCall();
            }
        }

        public void Shutdown()
        {
            HostingEnvironment.InitiateShutdown();
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            if (_server != null)
            {
                _server.HostStopped();
            }
            WaitForPendingCallsToFinish();
            HostingEnvironment.UnregisterObject(this);
        }

        private void AddPendingCall()
        {
#pragma warning disable 420
            Interlocked.Increment(ref _pendingCallsCount);
#pragma warning restore 420
        }

        private void RemovePendingCall()
        {
#pragma warning disable 420
            Interlocked.Decrement(ref _pendingCallsCount);
#pragma warning restore 420
        }

        private void WaitForPendingCallsToFinish()
        {
            while (_pendingCallsCount > 0)
            {
                Thread.Sleep(250);
            }
        }

        public string DefaultDocument
        {
            get { return _server.DefaultDocument; }
        }

        public string NormalizedVirtualPath
        {
            get { return _lowerCasedVirtualPathWithTrailingSlash; }
        }

        public string PhysicalPath
        {
            get { return _physicalPath; }
        }

        public int Port
        {
            get { return _port; }
        }

        public string VirtualPath
        {
            get { return _virtualPath; }
        }
    }
}