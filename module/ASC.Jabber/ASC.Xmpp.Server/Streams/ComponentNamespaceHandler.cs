using System;
using System.Text;
using agsXMPP.protocol;
using agsXMPP.protocol.component;
using ASC.Xmpp.Server.Handler;
using Uri = agsXMPP.Uri;

namespace ASC.Xmpp.Server.Streams
{
	[XmppHandler(typeof(Handshake))]
	class ComponentNamespaceHandler : IXmppStreamStartHandler
	{
		public string Namespace
		{
			get { return Uri.ACCEPT; }
		}

		public void StreamStartHandle(XmppStream xmppStream, Stream stream, XmppHandlerContext context)
		{
			var streamHeader = new StringBuilder();
			streamHeader.AppendLine("<?xml version='1.0' encoding='UTF-8'?>");
			streamHeader.AppendFormat("<stream:{0} xmlns:{0}='{1}' xmlns='{2}' ", Uri.PREFIX, Uri.STREAM, Uri.ACCEPT);
			streamHeader.AppendFormat("from='{0}' id='{1}' version='1.0'>", stream.To, xmppStream.Id);

			context.Sender.SendTo(xmppStream, streamHeader.ToString());
		}

		public void OnRegister(IServiceProvider serviceProvider)
		{

		}

		public void OnUnregister(IServiceProvider serviceProvider)
		{

		}
	}
}