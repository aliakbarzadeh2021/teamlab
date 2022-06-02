﻿using System.Collections.Generic;
using agsXMPP.Xml.Dom;
using ASC.Xmpp.Server.Streams;


namespace ASC.Xmpp.Server.Handler
{
	public interface IXmppStreamHandler : IXmppHandler
	{
		void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context);

		void StreamEndHandle(XmppStream stream, ICollection<Node> notSendedBuffer, XmppHandlerContext context);
	}
}
