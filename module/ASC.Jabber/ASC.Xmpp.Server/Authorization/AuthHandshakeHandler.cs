using System;
using agsXMPP.protocol.component;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using agsXMPP.util;
using agsXMPP.Xml.Dom;

namespace ASC.Xmpp.Server.Authorization
{
	[XmppHandler(typeof(Handshake))]
	class AuthHandshakeHandler : XmppStreamHandler
	{
		private string password;

		public AuthHandshakeHandler(string password)
		{
			this.password = password;
		}

		public override void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context)
		{
			if (stream.Authenticated) return;

			var handshake = (Handshake)element;
			string digest = handshake.Digest;
			string hash = Hash.Sha1Hash(stream.Id + password);
			if (string.Compare(hash, digest, StringComparison.OrdinalIgnoreCase) == 0)
			{
				context.Sender.SendTo(stream, new Handshake()); //TODO: auth with sha1
				//stream.Authenticated = true;
			}
			else
			{
				context.Sender.SendToAndClose(stream, XmppStreamError.NotAuthorized);
			}
		}
	}
}