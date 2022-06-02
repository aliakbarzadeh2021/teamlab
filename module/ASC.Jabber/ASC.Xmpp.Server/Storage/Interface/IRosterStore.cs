using System;
using System.Collections.Generic;
using System.Text;
using agsXMPP;
using agsXMPP.protocol.iq.roster;

namespace ASC.Xmpp.Server.Storage.Interface
{
	public interface IRosterStore
	{

		List<UserRosterItem> GetRosterItems(Jid rosterJid);

		List<UserRosterItem> GetRosterItems(Jid rosterJid, SubscriptionType subscriptionType);

		UserRosterItem GetRosterItem(Jid rosterJid, Jid itemJid);

		UserRosterItem SaveRosterItem(Jid rosterJid, UserRosterItem item);

		void RemoveRosterItem(Jid rosterJid, Jid itemJid);
	}
}
