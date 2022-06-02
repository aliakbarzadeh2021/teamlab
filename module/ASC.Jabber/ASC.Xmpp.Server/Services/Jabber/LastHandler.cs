using System;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.last;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Storage.Interface;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Last))]
	class LastHandler : XmppStanzaHandler
	{
		private DateTime startedTime = DateTime.UtcNow;


		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.Type != IqType.get || !iq.HasTo) return XmppStanzaError.ToNotAcceptable(iq);

			var currSession = context.SessionManager.GetSession(iq.From);
			if (currSession == null || !currSession.Available) return XmppStanzaError.ToForbidden(iq);

            double seconds = 0;//available
			
            if (iq.To.IsServer)
			{
				seconds = (DateTime.UtcNow - startedTime).TotalSeconds;
			}
			else
			{
				var session = context.SessionManager.GetSession(iq.To);
				if (session == null || !session.Available)
				{
					var lastActivity = context.StorageManager.OfflineStorage.GetLastActivity(iq.To);
					if (lastActivity != null)
					{
						seconds = (DateTime.UtcNow - lastActivity.LogoutDateTime).TotalSeconds;
						iq.Query.Value = lastActivity.Status;
					}
					else
					{
						return XmppStanzaError.ToRecipientUnavailable(iq);
					}
				}
			}

			((Last)(iq.Query)).Seconds = (int)seconds;
			iq.Type = IqType.result;
			iq.SwitchDirection();
			return iq;
		}
	}
}