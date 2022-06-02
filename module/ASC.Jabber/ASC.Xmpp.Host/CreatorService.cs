using System;
using System.Collections.Generic;
using agsXMPP;
using agsXMPP.Idn;
using agsXMPP.protocol;
using ASC.Core;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Host
{
	[XmppHandler(typeof(agsXMPP.protocol.Base.Stanza))]
	class CreatorStartStreamHandler : IXmppStreamStartHandler
	{
		private readonly Dictionary<string, Type> templates = new Dictionary<string, Type>();

		private XmppServiceManager serviceManager;

		private XmppHandlerManager handlerManager;


		public string Namespace
		{
			get { return agsXMPP.Uri.CLIENT; }
		}


		public CreatorStartStreamHandler(Dictionary<string, Type> instanceTemplate)
		{
			this.templates = instanceTemplate;
		}

		public void StreamStartHandle(XmppStream xmppStream, Stream stream, XmppHandlerContext context)
		{
			lock (this)
			{
				//Check tennats here
				if (ValidateHost(stream.To))
				{
					//Create new services
					foreach (var template in templates)
					{
						var service = (IXmppService)Activator.CreateInstance(template.Value);
						service.Jid = new Jid(Stringprep.NamePrep(string.Format("{0}.{1}", template.Key, stream.To.Server).Trim('.')));

						if (serviceManager.GetService(service.Jid) != null)
						{
							continue;
						}

						service.Name = service.Jid.ToString();
						if (!string.IsNullOrEmpty(template.Key))
						{
							service.ParentService = serviceManager.GetService(new Jid(Stringprep.NamePrep(stream.To.Server)));
						}
						service.Configure(new Dictionary<string, string>());
						serviceManager.RegisterService(service);
					}
					//Reroute
					handlerManager.ProcessStreamStart(stream, agsXMPP.Uri.CLIENT, xmppStream);
				}
				else
				{
					context.Sender.SendToAndClose(xmppStream, XmppStreamError.HostUnknown);
				}
			}
		}

		public void OnRegister(IServiceProvider serviceProvider)
		{
			serviceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));
			handlerManager = (XmppHandlerManager)serviceProvider.GetService(typeof(XmppHandlerManager));
		}

		public void OnUnregister(IServiceProvider serviceProvider)
		{

		}

		private bool ValidateHost(Jid jid)
		{
			if (jid != null && jid.IsServer)
			{
				return CoreContext.TenantManager.GetTenant(jid.Server) != null;
			}
			return false;
		}
	}


	public class CreatorService : XmppServiceBase
	{
		public override void Configure(IDictionary<string, string> properties)
		{
			var template = new Dictionary<string, Type>();
			foreach (var pair in properties)
			{
				template.Add(pair.Key, Type.GetType(pair.Value, true));
			}

			Handlers.Add(new CreatorStartStreamHandler(template));
		}
	}
}