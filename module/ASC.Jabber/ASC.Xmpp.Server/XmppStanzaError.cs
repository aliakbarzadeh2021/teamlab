﻿using System;
using agsXMPP.protocol.Base;
using agsXMPP.protocol.client;

namespace ASC.Xmpp.Server
{
	public static class XmppStanzaError
	{
		public static Stanza ToBadRequest(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.BadRequest);
		}

		public static Stanza ToConflict(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.Conflict);
		}

		public static Stanza ToNotAuthorized(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.Unauthorized);
		}

		public static Stanza ToNotFound(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.NotFound);
		}

		public static Stanza ToRecipientUnavailable(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorType.wait, ErrorCondition.RecipientUnavailable);
		}

		public static Stanza ToServiceUnavailable(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.ServiceUnavailable);
		}

		public static Stanza ToNotAcceptable(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.NotAcceptable);
		}

		public static Stanza ToItemNotFound(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorType.wait, ErrorCondition.ItemNotFound);
		}

		public static Stanza ToForbidden(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.Forbidden);
		}


		public static T ToBadRequest<T>(T stanza) where T : Stanza
		{
			return (T)ToBadRequest((Stanza)stanza);
		}

		public static T ToConflict<T>(T stanza) where T : Stanza
		{
			return (T)ToConflict((Stanza)stanza);
		}

		public static T ToNotAuthorized<T>(T stanza) where T : Stanza
		{
			return (T)ToNotAuthorized((Stanza)stanza);
		}

		public static T ToNotFound<T>(T stanza) where T : Stanza
		{
			return (T)ToNotFound((Stanza)stanza);
		}

		public static T ToRecipientUnavailable<T>(T stanza) where T : Stanza
		{
			return (T)ToRecipientUnavailable((Stanza)stanza);
		}

		public static T ToServiceUnavailable<T>(T stanza) where T : Stanza
		{
			return (T)ToServiceUnavailable((Stanza)stanza);
		}

		public static T ToNotAcceptable<T>(T stanza) where T : Stanza
		{
			return (T)ToNotAcceptable((Stanza)stanza);
		}

		public static T ToItemNotFound<T>(T stanza) where T : Stanza
		{
			return (T)ToItemNotFound((Stanza)stanza);
		}

		public static T ToForbidden<T>(T stanza) where T : Stanza
		{
			return (T)ToForbidden((Stanza)stanza);
		}


		public static Stanza ToErrorStanza(Stanza stanza, Error error)
		{
			if (stanza == null) throw new ArgumentNullException("stanza");
			if (error == null) throw new ArgumentNullException("error");

			stanza.SetAttribute("type", "error");
			stanza.ReplaceChild(error);
			if (!stanza.Switched) stanza.SwitchDirection();
			return stanza;
		}

		private static Stanza ToErrorStanzaWithError(Stanza stanza, ErrorCode code)
		{
			return ToErrorStanza(stanza, new Error(code));
		}

		private static Stanza ToErrorStanzaWithError(Stanza stanza, ErrorType type, ErrorCondition condition)
		{
			return ToErrorStanza(stanza, new Error(type, condition));
		}
	}
}
