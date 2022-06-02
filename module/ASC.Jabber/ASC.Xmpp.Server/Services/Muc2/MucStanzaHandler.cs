using agsXMPP;

namespace ASC.Xmpp.Server.Services.Muc2
{
    using Handler;
    using Helpers;
    using agsXMPP.protocol.Base;
    using agsXMPP.protocol.client;
    using agsXMPP.protocol.x.muc.iq;
    using Room.Settings;
    using Utils;

    [XmppHandler(typeof(Stanza))]
    internal class MucStanzaHandler : XmppStanzaHandler
    {
        public MucService Service { get; set; }

        internal MucStanzaHandler(MucService service)
        {
            Service = service;
        }

        public override IQ HandleIQ(Streams.XmppStream stream, IQ iq, XmppHandlerContext context)
        {
            Unique unique = (Unique)iq.SelectSingleElement(typeof(Unique));
            if (unique != null)
            {
                // Gen unique id
                unique.Value = UniqueId.CreateNewId(16);
                iq.Type = IqType.result;
                iq.SwitchDirection();
                return iq;
            }
            iq.SwitchDirection();
            iq.Type = IqType.error;
            iq.Error = new Error(ErrorType.cancel, ErrorCondition.ItemNotFound);
            return iq;
        }

        private bool IsUserAbleToCreateRooms(Jid @from)
        {
            return true;//TODO: configuration
        }

        public override void HandlePresence(ASC.Xmpp.Server.Streams.XmppStream stream, Presence presence, XmppHandlerContext context)
        {
            //Presence to open new room
            if (IsUserAbleToCreateRooms(presence.From) && MucHelpers.IsJoinRequest(presence))
            {
                //Register
                Service.CreateRoom(new Jid(presence.To.Bare), null);
                Service.HandlerManager.ProcessStreamElement(presence, stream);//Forward to room
            }
            else
            {
                //Return error
                presence.Type = PresenceType.error;
                presence.Error = new Error(ErrorType.cancel, ErrorCondition.NotAllowed);
                presence.SwitchDirection();
                context.Sender.SendTo(stream, presence);
            }
        }

        public override void HandleMessage(ASC.Xmpp.Server.Streams.XmppStream stream, Message msg, XmppHandlerContext context)
        {
            msg.SwitchDirection();
            msg.Type = MessageType.error;
            msg.Error = new Error(ErrorType.cancel, ErrorCondition.ItemNotFound);
            context.Sender.SendTo(stream, msg);

        }
    }
}