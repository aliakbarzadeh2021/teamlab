// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="MucService.cs">
//   
// </copyright>
// <summary>
//   (c) Copyright Ascensio System Limited 2008-2009
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using agsXMPP;

namespace ASC.Xmpp.Server.Services.Muc2
{
	using System;
	using System.Collections.Generic;
	using ASC.Xmpp.Server.Services.Jabber;
	using ASC.Xmpp.Server.Services.Muc2.Room.Settings;
	using Handler;
	using agsXMPP.protocol.iq.disco;
	using Room;
	using Storage;
	using Storage.Interface;
    using Uri = agsXMPP.Uri;

	internal class MucService : XmppServiceBase
	{
		private XmppHandlerManager handlerManager;

		#region Properties

		public IMucStore MucStorage
		{
			get
			{
				return ((StorageManager)context.GetService(typeof(StorageManager))).MucStorage;
			}
		}

		public IVCardStore VcardStorage
		{
			get
			{
				return ((StorageManager)context.GetService(typeof(StorageManager))).VCardStorage;
			}
		}

		public XmppServiceManager ServiceManager
		{
			get { return ((XmppServiceManager)context.GetService(typeof(XmppServiceManager))); }
		}

		public XmppHandlerManager HandlerManager
		{
			get { return handlerManager; }
		}

		#endregion

		public override void Configure(IDictionary<string, string> properties)
		{
			base.Configure(properties);
	
			DiscoInfo.AddIdentity(new DiscoIdentity("text", Name, "conference"));
			DiscoInfo.AddFeature(new DiscoFeature(Uri.MUC));
			DiscoInfo.AddFeature(new DiscoFeature(Features.FEAT_MUC_ROOMS));

			Handlers.Add(new MucStanzaHandler(this));
			Handlers.Add(new VCardHandler());
			Handlers.Add(new ServiceDiscoHandler(Jid));
		}

		protected override void OnRegisterCore(XmppHandlerManager handlerManager, XmppServiceManager serviceManager, IServiceProvider provider)
		{
			context = provider;
			this.handlerManager = handlerManager;
			LoadRooms();
		}

		private void LoadRooms()
		{
			List<MucRoomInfo> rooms = MucStorage.GetMucs(Jid.Server);
			foreach (MucRoomInfo room in rooms)
			{
				CreateRoom(room.Jid, room.Description);
			}
		}


		private IServiceProvider context;

		/// <summary>
		/// </summary>
		/// <param name="name">
		/// </param>
		/// <param name="description">
		/// </param>
		/// <returns>
		/// </returns>
		internal MucRoom CreateRoom(Jid roomJid, string description)
		{
            MucRoom room = new MucRoom(roomJid, roomJid.User, this, context);
			room.ParentService = this;
			ServiceManager.RegisterService(room);
			return room;
		}

		public void RemoveRoom(MucRoom room)
		{
			ServiceManager.UnregisterService(room.Jid);
            MucStorage.RemoveMuc(room.Jid);
		}
	}
}