﻿using agsXMPP.protocol.sasl;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using agsXMPP.Xml.Dom;

namespace ASC.Xmpp.Server.Authorization
{
	[XmppHandler(typeof(TMToken))]
	class AuthTMTokenHandler : XmppStreamHandler
	{
		public override void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context)
		{
			if (stream.Authenticated) return;

			var user = context.AuthManager.RestoreUserToken(((TMToken)element).Value);
			if (!string.IsNullOrEmpty(user))
			{
				stream.Authenticate(user);
				context.Sender.ResetStream(stream);
				context.Sender.SendTo(stream, new Success());
			}
			else
			{
				context.Sender.SendToAndClose(stream, XmppFailureError.NotAuthorized);
			}
		}
	}
}