using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.bytestreams;
using agsXMPP.protocol.extensions.filetransfer;
using agsXMPP.protocol.extensions.ibb;
using agsXMPP.protocol.extensions.jivesoftware.phone;
using agsXMPP.protocol.extensions.si;
using agsXMPP.protocol.iq.jingle;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services.Jabber
{
    //si
	[XmppHandler(typeof(SI))]
	
    //bytestreams
    [XmppHandler(typeof(Activate))]
	[XmppHandler(typeof(ByteStream))]
	[XmppHandler(typeof(StreamHost))]
	[XmppHandler(typeof(StreamHostUsed))]
	[XmppHandler(typeof(UdpSuccess))]

    //filetransfer
    [XmppHandler(typeof(File))]
    [XmppHandler(typeof(Range))]

    //ibb
    [XmppHandler(typeof(Base))]
    [XmppHandler(typeof(Close))]
    [XmppHandler(typeof(agsXMPP.protocol.extensions.ibb.Data))]
    [XmppHandler(typeof(Open))]

    //livesoftware.phone
    [XmppHandler(typeof(PhoneAction))]
    [XmppHandler(typeof(PhoneEvent))]
    [XmppHandler(typeof(PhoneStatus))]

    //jingle
    [XmppHandler(typeof(GoogleJingle))]
    [XmppHandler(typeof(Jingle))]
    [XmppHandler(typeof(agsXMPP.protocol.iq.jingle.Server))]
    [XmppHandler(typeof(Stun))]
    class TransferHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (!iq.HasTo || !iq.To.HasUser) return XmppStanzaError.ToServiceUnavailable(iq);

			var session = context.SessionManager.GetSession(iq.To);
			if (session != null) context.Sender.SendTo(session, iq);
			return null;
		}
	}
}