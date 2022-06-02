﻿using System;
using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.iq.disco;
using agsXMPP.protocol.iq.vcard;
using ASC.Xmpp.Server.Handler;
using agsXMPP.Factory;

namespace ASC.Xmpp.Server.Services
{
    public class XmppServiceBase : IXmppService
    {
        protected IList<IXmppHandler> Handlers
        {
            get;
            private set;
        }

        protected bool Registered
        {
            get;
            private set;
        }

        public Jid Jid
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public IXmppService ParentService
        {
            get;
            set;
        }

        public DiscoInfo DiscoInfo
        {
            get;
            private set;
        }

        public virtual DiscoItem DiscoItem
        {
            get { return new DiscoItem() { Name = Name, Jid = Jid }; }
        }

        public virtual Vcard Vcard
        {
            get { return new Vcard() { Fullname = Name }; }
        }

        public XmppServiceBase()
        {
            Handlers = new List<IXmppHandler>();
            Registered = false;
            DiscoInfo = new DiscoInfo();
        }

        public XmppServiceBase(IXmppService parent)
            : this()
        {
            ParentService = parent;
        }

        public virtual void Configure(IDictionary<string, string> properties)
        {

        }

        public void OnRegister(IServiceProvider serviceProvider)
        {
            var handlerManager = (XmppHandlerManager)serviceProvider.GetService(typeof(XmppHandlerManager));
            var serviceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));

            foreach (var h in Handlers) handlerManager.AddXmppHandler(Jid, h);
            Registered = true;
            OnRegisterCore(handlerManager, serviceManager, serviceProvider);

            DiscoveryFearures(Handlers);
        }

        public void OnUnregister(IServiceProvider serviceProvider)
        {
            var handlerManager = (XmppHandlerManager)serviceProvider.GetService(typeof(XmppHandlerManager));
            var serviceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));

            foreach (var h in Handlers) handlerManager.RemoveXmppHandler(h);
            Registered = false;
            OnUnregisterCore(handlerManager, serviceManager, serviceProvider);
        }

        protected virtual void OnRegisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {

        }

        protected virtual void OnUnregisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider serviceProvider)
        {

        }


        private void DiscoveryFearures(IList<IXmppHandler> handlers)
        {
            foreach (var handler in handlers)
            {
                foreach (XmppHandlerAttribute attribute in handler.GetType().GetCustomAttributes(typeof(XmppHandlerAttribute), true))
                {
                    var nameSpace = ElementFactory.GetElementNamespace(attribute.XmppElementType);
                    if (!string.IsNullOrEmpty(nameSpace) && !DiscoInfo.HasFeature(nameSpace))
                    {
                        DiscoInfo.AddFeature(new DiscoFeature(nameSpace));
                    }
                }
            }
        }
    }
}