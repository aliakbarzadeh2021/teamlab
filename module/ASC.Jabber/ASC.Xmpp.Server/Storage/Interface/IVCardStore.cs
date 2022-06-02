using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.iq.vcard;

namespace ASC.Xmpp.Server.Storage.Interface {

	public interface IVCardStore {

		void SetVCard(Jid jid, Vcard vcard);

		Vcard GetVCard(Jid jid);

		ICollection<Vcard> Search(Vcard pattern);
	}
}