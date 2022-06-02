using System;
using agsXMPP;
using agsXMPP.Idn;
using agsXMPP.protocol.Base;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.auth;
using agsXMPP.protocol.iq.register;
using ASC.Xmpp.Server.Streams;
using agsXMPP.Xml.Dom;
using Uri = agsXMPP.Uri;

namespace ASC.Xmpp.Server.Handler
{
    class XmppStreamValidator
    {
        public bool ValidateStanza(Stanza stanza, XmppStream stream, XmppHandlerContext context)
        {
            Element result = null;
            if (stream.Namespace == Uri.CLIENT) result = ValidateClientStanza(stanza, stream);
            if (stream.Namespace == Uri.SERVER) result = ValidateServerStanza(stanza, stream);

            if (result == null) return true;

            if (result is Stanza)
            {
                context.Sender.SendTo(stream, result);
            }
            else if (result is agsXMPP.protocol.Error)
            {
                context.Sender.SendToAndClose(stream, result);
            }
            else
            {
                return true;
            }
            return false;
        }

        private Element ValidateClientStanza(Stanza stanza, XmppStream stream)
        {
            if (!stream.Authenticated)
            {
                if (!(stanza is AuthIq) && (stanza is IQ && !(((IQ)stanza).Query is Register))) return XmppStanzaError.ToNotAuthorized(stanza);
            }

            //remove empty jids
            if (stanza.HasFrom && string.IsNullOrEmpty(stanza.From.ToString())) stanza.From = null;
            if (stanza.HasTo && string.IsNullOrEmpty(stanza.To.ToString())) stanza.To = null;

            //prep strings
            stanza.From = NodePrep(stanza.From);
            stanza.To = NodePrep(stanza.To);

            if (!ValidateJid(stanza.From) || !ValidateJid(stanza.To)) return XmppStanzaError.ToBadRequest(stanza);

            if (stanza.HasFrom)
            {
                if (!stream.JidBinded(stanza.From)) return XmppStreamError.InvalidFrom;
            }
            else
            {
                if (stream.MultipleResources) return XmppStanzaError.ToConflict(stanza);
                stanza.From = new Jid(string.Format("{0}@{1}/{2}", stream.User, stream.Domain, 0 < stream.Resources.Count ? stream.Resources[0] : null));
            }

            if (stanza is Message)
            {
                var message = (Message)stanza;
                if (message.Type == MessageType.chat && message.To == null) return XmppStanzaError.ToRecipientUnavailable(stanza);
            }
            return null;
        }

        private bool ValidateJid(Jid jid)
        {
            if (jid == null) return true;
            if (jid.HasUser)
            {
                if (jid.User != Jid.EscapeNode(jid.User)) return false;
                if (jid.User != Stringprep.NodePrep(jid.User)) return false;
            }
            return true;
        }

        private Jid NodePrep(Jid jid)
        {
            if (jid != null)
            {
                if (jid.HasUser)
                {
                    jid.User = Stringprep.NodePrep(jid.User);
                }
                if (jid.HasResource)
                {
                    jid.Resource = Stringprep.ResourcePrep(jid.Resource);
                }
                //BUG: Many faults here in log
                jid.Server = Stringprep.NamePrep(jid.Server);
            }
            return jid;
        }

        private Element ValidateServerStanza(Stanza stanza, XmppStream stream)
        {
            if (!stanza.HasTo || !stanza.HasFrom) return XmppStreamError.ImproperAddressing;
            return null;
        }
    }
}
