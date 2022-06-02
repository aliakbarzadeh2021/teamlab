﻿using System;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.auth;
using agsXMPP.Xml.Dom;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Users;
using agsXMPP.util;

namespace ASC.Xmpp.Server.Authorization
{
	[XmppHandler(typeof(AuthIq))]
	class AuthIQHandler : XmppStreamHandler
	{
		public override void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context)
		{
			var iq = (AuthIq)element;

			if (stream.Authenticated)
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToConflict(iq));
				return;
			}

			if (iq.Type == IqType.get) ProcessAuthIQGet(stream, iq, context);
			else if (iq.Type == IqType.set) ProcessAuthIQSet(stream, iq, context);
			else context.Sender.SendTo(stream, XmppStanzaError.ToNotAcceptable(iq));
		}

		private void ProcessAuthIQSet(XmppStream stream, AuthIq iq, XmppHandlerContext context)
		{
			if (string.IsNullOrEmpty(iq.Query.Username) || string.IsNullOrEmpty(iq.Query.Resource))
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToNotAcceptable(iq));
				return;
			}

			bool authorized = false;
			if (!string.IsNullOrEmpty(iq.Query.Digest))
			{
				authorized = AuthDigest(iq.Query.Username, iq.Query.Digest, stream, context.UserManager);
			}
			if (!string.IsNullOrEmpty(iq.Query.Password))
			{
				authorized = AuthPlain(iq.Query.Username, iq.Query.Password, stream, context.UserManager);
			}
			if (authorized)
			{
				stream.Authenticate(iq.Query.Username);

				var answer = new IQ(IqType.result);
				answer.Id = iq.Id;
				answer.To = iq.From;
				answer.From = iq.To;
				context.Sender.SendTo(stream, answer);
			}
			else
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToNotAuthorized(iq));
			}
		}

		private void ProcessAuthIQGet(XmppStream stream, AuthIq iq, XmppHandlerContext context)
		{
			iq.SwitchDirection();
			iq.Type = IqType.result;
			iq.Query.AddChild(new Element("password"));
			iq.Query.AddChild(new Element("digest"));
			iq.Query.AddChild(new Element("resource"));
			context.Sender.SendTo(stream, iq);
		}

		private bool AuthDigest(string username, string hash, XmppStream stream, UserManager userManager)
		{
			var user = userManager.GetUser(new Jid(username, stream.Domain, null));
			if (user != null)
			{
				string serverhash = Hash.Sha1Hash(stream.Id + user.Password);
				return string.Compare(serverhash, hash, StringComparison.OrdinalIgnoreCase) == 0;
			}
			return false;
		}

		private bool AuthPlain(string username, string password, XmppStream stream, UserManager userManager)
		{
			var user = userManager.GetUser(new Jid(username, stream.Domain, null));
			if (user != null)
			{
				return string.Compare(user.Password, password, StringComparison.Ordinal) == 0;
			}
			return false;
		}
	}
}