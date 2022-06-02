using System;
using agsXMPP;
using agsXMPP.protocol.iq.disco;
using agsXMPP.protocol.iq.vcard;
using ASC.Xmpp.Common.Configuration;
using ASC.Xmpp.Server.Configuration;

namespace ASC.Xmpp.Server.Services
{
	public interface IXmppService : IConfigurable
	{
		Jid Jid
		{
			get;
			set;
		}

		string Name
		{
			get;
			set;
		}
		
		DiscoInfo DiscoInfo
		{
			get;
		}

		DiscoItem DiscoItem
		{
			get;
		}

		Vcard Vcard
		{
			get;
		}

		IXmppService ParentService
		{
			get;
			set;
		}

		void OnRegister(IServiceProvider serviceProvider);

		void OnUnregister(IServiceProvider serviceProvider);
	}
}
