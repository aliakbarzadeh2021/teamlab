namespace ASC.Xmpp.Server.Services.Muc2.Exceptions
{
    using System;
    using agsXMPP.protocol.client;

    public class MucMemberExistsException : Exception
    {
        public agsXMPP.protocol.client.Error GetError()
        {
            return new Error(ErrorCondition.Conflict);
        }
    }


    public class MucMemberNotFoundException : Exception
    {
        public agsXMPP.protocol.client.Error GetError()
        {
            return new Error(ErrorCondition.ItemNotFound);
        }
    }
}