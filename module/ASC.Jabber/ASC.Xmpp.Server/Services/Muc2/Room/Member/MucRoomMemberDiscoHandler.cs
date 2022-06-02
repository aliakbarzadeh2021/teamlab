using agsXMPP;

namespace ASC.Xmpp.Server.Services.Muc2.Room.Member
{
	using ASC.Xmpp.Server.Streams;
	using Handler;
	using agsXMPP.protocol.client;
	using agsXMPP.protocol.iq.disco;

	class MucRoomMemberDiscoHandler : ServiceDiscoHandler
	{
		private readonly Jid realJid;

		public MucRoomMemberDiscoHandler(Jid jid, Jid realJid)
			: base(jid)
		{
			this.realJid = realJid;
		}

		protected override IQ GetDiscoItems(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (((DiscoItems)iq.Query).Node != null) return XmppStanzaError.ToServiceUnavailable(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.From = Jid;
			answer.To = iq.From;
			var items = new DiscoItems();
			answer.Query = items;
			if (realJid != null)
			{
				items.AddDiscoItem().Jid = realJid;
			}
			return answer;
		}
	}
}