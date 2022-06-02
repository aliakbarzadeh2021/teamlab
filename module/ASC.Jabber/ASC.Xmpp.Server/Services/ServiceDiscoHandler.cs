﻿using System;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.disco;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services
{
	[XmppHandler(typeof(DiscoInfo))]
	[XmppHandler(typeof(DiscoItems))]
	class ServiceDiscoHandler : XmppStanzaHandler
	{
		protected Jid Jid
		{
			get;
			private set;
		}

		protected XmppServiceManager ServiceManager
		{
			get;
			private set;
		}

		public ServiceDiscoHandler(Jid jid)
		{
			Jid = jid;
		}

		public override void OnRegister(IServiceProvider serviceProvider)
		{
			ServiceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));
		}

		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.HasTo && iq.To != Jid) return XmppStanzaError.ToServiceUnavailable(iq);
			if (iq.Query is DiscoInfo && iq.Type == IqType.get) return GetDiscoInfo(stream, iq, context);
			if (iq.Query is DiscoItems && iq.Type == IqType.get) return GetDiscoItems(stream, iq, context);
			return XmppStanzaError.ToServiceUnavailable(iq);
		}

		protected virtual IQ GetDiscoInfo(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (((DiscoInfo)iq.Query).Node != null) return XmppStanzaError.ToServiceUnavailable(iq);

			var service = ServiceManager.GetService(Jid);
			if (service == null) return XmppStanzaError.ToItemNotFound(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.From = Jid;
			answer.To = iq.From;
			answer.Query = service.DiscoInfo;
			return answer;
		}

		protected virtual IQ GetDiscoItems(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (((DiscoItems)iq.Query).Node != null) return XmppStanzaError.ToServiceUnavailable(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.From = Jid;
			answer.To = iq.From;
			var items = new DiscoItems();
			answer.Query = items;
			foreach (var service in ServiceManager.GetChildServices(Jid))
			{
				if (service.DiscoItem != null) items.AddDiscoItem(service.DiscoItem);
			}
			return answer;
		}
	}
}
