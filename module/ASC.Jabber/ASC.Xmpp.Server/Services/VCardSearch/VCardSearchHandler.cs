using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.search;
using agsXMPP.protocol.iq.vcard;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services.VCardSearch
{
	[XmppHandler(typeof(Search))]
	class VCardSearchHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (!iq.HasTo) iq.To = iq.From;
			if (iq.Type == IqType.get) return GetVCardSearch(stream, iq, context);
			else if (iq.Type == IqType.set) return SetVCardSearch(stream, iq, context);
			else return XmppStanzaError.ToBadRequest(iq);
		}

		private IQ GetVCardSearch(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			answer.From = iq.To;

			var search = new Search();
			search.Nickname = string.Empty;
			search.Firstname = string.Empty;
			search.Lastname = string.Empty;

			answer.Query = search;
			return answer;
		}

		private IQ SetVCardSearch(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			answer.From = iq.To;

			var search = (Search)iq.Query;

			var pattern = new Vcard();
			pattern.Nickname = search.Nickname;
			pattern.Name = new Name(search.Lastname, search.Firstname, null);
			//pattern.AddEmailAddress(new Email() { UserId = search.Email });

			search = new Search();
			foreach (var vcard in context.StorageManager.VCardStorage.Search(pattern))
			{
				var item = new SearchItem();
				item.Jid = vcard.JabberId;
				item.Nickname = vcard.Nickname;
				if (vcard.Name != null)
				{
					item.Firstname = vcard.Name.Given;
					item.Lastname = vcard.Name.Family;
				}
				var email = vcard.GetPreferedEmailAddress();
				if (email != null)
				{
					item.Email = email.UserId;
				}
				search.AddChild(item);
			}

			answer.Query = search;
			return answer;
		}
	}
}