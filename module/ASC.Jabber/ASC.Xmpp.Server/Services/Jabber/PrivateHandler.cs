using System.Collections.Generic;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.@private;
using agsXMPP.Xml.Dom;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Private))]
	class PrivateHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.To != null && iq.From != iq.To) return XmppStanzaError.ToForbidden(iq);

			if (iq.Type == IqType.get) return GetPrivate(stream, iq, context);
			else if (iq.Type == IqType.set) return SetPrivate(stream, iq, context);
			else return XmppStanzaError.ToBadRequest(iq);
		}

		private IQ SetPrivate(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var @private = (Private)iq.Query;
			
            if (!@private.HasChildElements) return XmppStanzaError.ToNotAcceptable(iq);

			foreach (var childNode in @private.ChildNodes)
			{
				if (childNode is Element)
				{
                    context.StorageManager.PrivateStorage.SetPrivate(iq.From, (Element)childNode);
				}
			}
            iq.Query = null;
			iq.SwitchDirection();
			iq.Type = IqType.result;
			return iq;
		}

		private IQ GetPrivate(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var privateStore = (Private)iq.Query;
			
            if (!privateStore.HasChildElements) return XmppStanzaError.ToNotAcceptable(iq);

			var retrived = new List<Element>();
			foreach (var childNode in privateStore.ChildNodes)
			{
				if (childNode is Element)
				{
					var elementToRetrive = (Element)childNode;
                    var elementRestored = context.StorageManager.PrivateStorage.GetPrivate(iq.From, elementToRetrive);
					retrived.Add(elementRestored ?? elementToRetrive);
				}
			}

            privateStore.RemoveAllChildNodes();
			foreach (var element in retrived)
			{
				privateStore.AddChild(element);
			}
			
            iq.SwitchDirection();
			iq.Type = IqType.result;
			return iq;
		}
	}
}