using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.client;

namespace ASC.Xmpp.Server.Storage.Interface
{
	public interface IOfflineStore
	{
		List<Message> GetOfflineMessages(Jid jid);

        int GetOfflineMessagesCount(Jid jid);

		void SaveOfflineMessages(params Message[] messages);

		void RemoveAllOfflineMessages(Jid jid);


		List<Presence> GetOfflinePresences(Jid jid);

		void SaveOfflinePresence(Presence presence);

		void RemoveAllOfflinePresences(Jid jid);


		void SaveLastActivity(Jid jid, LastActivity lastActivity);

		LastActivity GetLastActivity(Jid jid);
	}
}