using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.commands;
using agsXMPP.protocol.x;
using ASC.Core;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Storage;

namespace ASC.Xmpp.Host.Responders
{
    public class TokenResponder : BaseAscResponder
    {
        public override string Path
        {
            get { return "token"; }
        }

        protected override XElement ProcessRequest(XmppServer server, Jid jid, NameValueCollection values)
        {
            return new XElement("token", server.AuthManager.GetUserToken(jid.User));
        }
    }

    public class UnreadResponder : BaseAscResponder
    {
        public override string Path
        {
            get { return "unread"; }
        }

        protected override XElement ProcessRequest(XmppServer server, Jid jid, NameValueCollection values)
        {
            return new XElement("unread", server.StorageManager.OfflineStorage.GetOfflineMessagesCount(jid));
        }
    }

    public class AvailibleResponder : BaseAscResponder
    {
        public override string Path
        {
            get { return "availible"; }
        }

        protected override XElement ProcessRequest(XmppServer server, Jid jid, NameValueCollection values)
        {
            var user = values["user"];
            if (!string.IsNullOrEmpty(user))
            {
                jid = GetJid(user);
            }
            var session = server.SessionManager.GetSession(jid);
            return new XElement("availible", session != null && session.Available);
        }
    }

    public class MessageResponder : BaseAscResponder
    {
        public override string Path
        {
            get { return "message"; }
        }

        protected override XElement ProcessRequest(XmppServer server, Jid jid, NameValueCollection values)
        {
            var to = values["to"];
            var subject = values["subject"];
            var text = values["message"];

            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(text))
            {
                var jidTo = GetJid(to);
                text = ModifyJabberNotification(text);
                var message = new Message(
                    jidTo,
                    new Jid(CoreContext.TenantManager.GetCurrentTenant().TenantDomain),
                    MessageType.headline,
                    text,
                    subject ?? "") { XDelay = new Delay() { Stamp = DateTime.UtcNow } };

                var session = server.SessionManager.GetSession(jid);
                if (session != null)
                {
                    var sender = (IXmppSender)server.GetService(typeof(IXmppSender));
                    sender.SendTo(session, message);
                }
                else
                {
                    server.StorageManager.OfflineStorage.SaveOfflineMessages(message);
                }
                var archive = server.StorageManager.GetStorage<DbMessageArchive>("archive");
                if (archive != null) archive.SaveMessages(message);
            }
            else
            {
                throw new ArgumentException("argument count mismatch");
            }
            return new XElement("ok");
        }

        private static string ModifyJabberNotification(string message)
        {
            message = message.Replace("\r\n", "\n");
            message = message.Trim('\n', '\r');
            message = Regex.Replace(message, "\n{3,}", "\n\n");
            return message;
        }
    }

    public class CommandResponder : BaseAscResponder
    {
        public override string Path
        {
            get { return "command"; }
        }

        protected override XElement ProcessRequest(XmppServer server, Jid jid, NameValueCollection values)
        {
            var to = values["to"];
            var from = values["from"];
            var command = values["command"];
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(command)) throw new ArgumentException("argument count mismatch");

            var jidTo = GetJid(to);
            var iq = new IQ(IqType.set, GetJid(from), jidTo)
            {
                Query = new Command(command)
            };

            var session = server.SessionManager.GetSession(jidTo);
            if (session != null)
            {
                var sender = (IXmppSender)server.GetService(typeof(IXmppSender));
                sender.SendTo(session, iq);
            }
            return new XElement("ok");
        }

    }
}