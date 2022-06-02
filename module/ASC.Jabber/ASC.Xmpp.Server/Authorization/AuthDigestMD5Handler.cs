using System;
using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.sasl;
using agsXMPP.sasl.DigestMD5;
using ASC.Collections;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Utils;
using agsXMPP.Xml.Dom;
using log4net;

namespace ASC.Xmpp.Server.Authorization
{
	[XmppHandler(typeof(Auth))]
	[XmppHandler(typeof(Response))]
	[XmppHandler(typeof(Abort))]
	class AuthDigestMD5Handler : XmppStreamHandler
	{
        private IDictionary<string, AuthData> authData = new SynchronizedDictionary<string, AuthData>();
        private static readonly ILog log = LogManager.GetLogger(typeof(AuthDigestMD5Handler));
		private object syncRoot = new object();

		public override void StreamEndHandle(XmppStream stream, ICollection<Node> notSendedBuffer, XmppHandlerContext context)
		{
			lock (syncRoot)
			{
				authData.Remove(stream.Id);
			}
		}

		public override void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context)
		{
			if (stream.Authenticated) return;

			if (element is Auth) ProcessAuth(stream, (Auth)element, context);
			if (element is Response) ProcessResponse(stream, (Response)element, context);
			if (element is Abort) ProcessAbort(stream, (Abort)element, context);
		}

		private void ProcessAuth(XmppStream stream, Auth auth, XmppHandlerContext context)
		{
			lock (syncRoot)
			{
				if (auth.MechanismType != MechanismType.DIGEST_MD5)
				{
					context.Sender.SendToAndClose(stream, XmppFailureError.InvalidMechanism);
				}
				else if (authData.ContainsKey(stream.Id))
				{
					context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
				}
				else
				{
					authData[stream.Id] = new AuthData();
					var challenge = GetChallenge(stream.Domain);
					context.Sender.SendTo(stream, challenge);
				}
			}
		}

		private void ProcessResponse(XmppStream stream, Response response, XmppHandlerContext context)
		{
			lock (syncRoot)
			{
				if (!authData.ContainsKey(stream.Id))
				{
					context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
					return;
				}

				var authStep = authData[stream.Id];
				if (authStep.Step == AuthStep.Step1)
				{
					var challenge = ProcessStep1(stream, response, context);
					if (challenge != null)
					{
						context.Sender.SendTo(stream, challenge);
						authStep.DoStep();
					}
					else
					{
						context.Sender.SendToAndClose(stream, XmppFailureError.NotAuthorized);
					}
				}
				else if (authStep.Step == AuthStep.Step2)
				{
					var success = ProcessStep2(stream, response, context);
					context.Sender.SendTo(stream, success);
				}
				else
				{
					context.Sender.SendToAndClose(stream, XmppFailureError.TemporaryAuthFailure);
				}
			}
		}

		private void ProcessAbort(XmppStream stream, Abort abort, XmppHandlerContext context)
		{
			context.Sender.SendToAndClose(stream, XmppFailureError.Aborted);
		}

		private Challenge GetChallenge(string domain)
		{
			var challenge = new Challenge();
			challenge.TextBase64 = string.Format("realm=\"{0}\",nonce=\"{1}\",qop=\"auth\",charset=utf-8,algorithm=md5-sess", domain, UniqueId.CreateNewId());
			return challenge;
		}

		private Challenge ProcessStep1(XmppStream stream, Response response, XmppHandlerContext ctx)
		{
			var step = new Step2(response.TextBase64);
			var userName = step.Username;
			var user = ctx.UserManager.GetUser(new Jid(userName, stream.Domain, null));

		    log.InfoFormat("User {0} {1}. Realm={2}", userName, user==null?"not found":user.ToString(), step.Realm);

			if (user != null && string.Compare(stream.Domain, step.Realm, StringComparison.OrdinalIgnoreCase) == 0)
			{
                if (step.Authorize(userName, user.Password))
				{
                    log.InfoFormat("User authorized");
                    lock (authData) authData[stream.Id].UserName = userName;
					var challenge = new Challenge();
					challenge.TextBase64 = string.Format("rspauth={0}", step.CalculateResponse(userName, user.Password, string.Empty));
					return challenge;
				}
                else
                {
                    log.InfoFormat("User not authorized");
                }
			}
			return null;
		}

		private Success ProcessStep2(XmppStream stream, Response response, XmppHandlerContext ctx)
		{
			lock (syncRoot)
			{
				stream.Authenticate(authData[stream.Id].UserName);
				authData.Remove(stream.Id);
				ctx.Sender.ResetStream(stream);
				return new Success();
			}
		}

		private enum AuthStep
		{
			Step1,
			Step2,
		}

		private class AuthData
		{
			public string UserName
			{
				get;
				set;
			}

			public AuthStep Step
			{
				get;
				private set;
			}

			public void DoStep()
			{
				Step++;
			}
		}
	}
}