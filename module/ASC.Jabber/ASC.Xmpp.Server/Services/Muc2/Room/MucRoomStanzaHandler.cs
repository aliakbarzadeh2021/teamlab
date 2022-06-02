namespace ASC.Xmpp.Server.Services.Muc2.Room
{
    using System;
    using Handler;
    using Helpers;
    using Member;
    using agsXMPP.protocol.Base;
    using agsXMPP.protocol.client;
    using agsXMPP.protocol.x.muc;
    using agsXMPP.protocol.x.muc.iq;
    using agsXMPP.protocol.x.muc.iq.admin;
    using agsXMPP.protocol.x.muc.iq.owner;
    using Streams;
    using Utils;

    [XmppHandler(typeof(Stanza))]
    internal class MucRoomStanzaHandler : XmppStanzaHandler
    {
        public MucRoom Room { get; set; }


        internal MucRoomStanzaHandler(MucRoom room)
        {
            Room = room;
        }

        public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
        {
            //Admins iq

                //New member
                MucRoomMember member = Room.GetRealMember(iq.From);
                if (member!=null)
                {
                    if (iq.Query!=null)
                    {
                        if (iq.Query is Admin && (member.Affiliation==Affiliation.admin || member.Affiliation==Affiliation.owner))
                        {
                            Room.AdminCommand(iq, member);
                        }
                        else if (iq.Query is Owner && (member.Affiliation == Affiliation.owner))
                        {
                            Room.OwnerCommand(iq, member);
                        }
                        else
                        {
                            XmppStanzaError.ToForbidden(iq);
                        }
                    }
                    else
                    {
                        XmppStanzaError.ToBadRequest(iq);
                    }
            }
            else
            {
                XmppStanzaError.ToForbidden(iq);
            }
            if (!iq.Switched)
            {
                iq.SwitchDirection();
            }
            iq.From = Room.Jid;
            return iq;
        }

        public override void HandlePresence(Streams.XmppStream stream, Presence presence, XmppHandlerContext context)
        {
            string userName = presence.To.Resource;
            if (!string.IsNullOrEmpty(userName) && presence.Type == PresenceType.available)
            {
                //New member
                MucRoomMember member = Room.GetRealMember(presence.From);
                if (member != null)
                {
                    if (ReferenceEquals(stream, member.Stream))
                    {
                        if (!Room.TryNickChange(presence))
                        {
                            ErrorPresence(presence, ErrorCondition.NotAcceptable);
                            context.Sender.SendTo(stream, presence);
                        }
                    }
                    else
                    {
                        //Conflict. user with this jid already in room
                        ErrorPresence(presence, ErrorCondition.Conflict);
                        context.Sender.SendTo(stream, presence);
                    }
                }
                else
                {
                    //Doesn't exists
                    MucRoomMember newMember = new MucRoomMember(Room, presence.To, presence.From, stream, context);
                    Room.TryEnterRoom(newMember, presence);
                }
            }
            else
            {
                ErrorPresence(presence, ErrorCondition.BadRequest);
                context.Sender.SendTo(stream, presence);
            }
        }

        private static void ErrorPresence(Presence presence, ErrorCondition condition) {
            presence.SwitchDirection();
            presence.RemoveAllChildNodes();
            presence.AddChild(new Muc());
            presence.Type = PresenceType.error;
            presence.Error = new Error(condition);
        }

        public override void HandleMessage(Streams.XmppStream stream, Message msg, XmppHandlerContext context)
        {
            User user = (User) msg.SelectSingleElement(typeof (User));
            if (user != null)
            {
                HandleUserMessage(msg, user, stream);
            }
            else
            {
                //Groupchat message
                MucRoomMember member = Room.GetRealMember(msg.From);
                if (member != null && ReferenceEquals(member.Stream, stream) && member.Role != Role.none)
                {
                    if (msg.Type == MessageType.groupchat)
                    {
                        if (msg.Subject!=null)
                        {
                            Room.ChangeSubject(member, msg.Subject);
                        }
                        else
                        {
                            MessageBroadcast(msg, member);    
                        }
                    }
                    else
                    {
                        msg.SwitchDirection();
                        msg.Type = MessageType.error;
                        msg.Error = new Error(ErrorCondition.NotAcceptable);
                        context.Sender.SendTo(stream, msg);
                    }
                }
                else
                {
                    msg.SwitchDirection();
                    msg.Type = MessageType.error;
                    msg.Error = new Error(ErrorCondition.Forbidden);
                    context.Sender.SendTo(stream, msg);
                }
            }

        }

        private void HandleUserMessage(Message msg, User user, XmppStream stream)
        {
            if (user.Invite != null)
            {
                Room.InviteUser(msg, user,stream);
            }
            else if (user.Decline != null)
            {
                Room.DeclinedUser(msg, user,stream);
            }
        }

        

        private void MessageBroadcast(Message msg, MucRoomMember member)
        {
            Room.BroadcastMessage(msg, member);
        }
    }
}