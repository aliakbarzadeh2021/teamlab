using agsXMPP;
using agsXMPP.Xml.Dom;

namespace ASC.Xmpp.Server.Storage.Interface
{

	public interface IPrivateStore
	{
		Element GetPrivate(Jid jid, Element element);

        void SetPrivate(Jid jid, Element element);
	}
}