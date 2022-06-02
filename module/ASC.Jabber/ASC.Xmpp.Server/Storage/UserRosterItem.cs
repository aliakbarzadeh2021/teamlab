using System;
using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.Xml.Dom;
using RosterItem = agsXMPP.protocol.iq.roster.RosterItem;

namespace ASC.Xmpp.Server.Storage
{
	public class UserRosterItem
	{
		public Jid Jid
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			set;
		}

		public SubscriptionType Subscribtion
		{
			get;
			set;
		}

		public AskType Ask
		{
			get;
			set;
		}

		public List<string> Groups
		{
			get;
			private set;
		}

		public UserRosterItem(Jid jid)
		{
			if (jid == null) throw new ArgumentNullException("jid");

			Jid = new Jid(jid.Bare.ToLowerInvariant());
			Groups = new List<string>();
		}

		public RosterItem ToRosterItem()
		{
			var ri = new RosterItem(Jid, Name)
			{
				Subscription = Subscribtion,
				Ask = Ask,
			};
			Groups.ForEach(g => ri.AddGroup(g));
			return ri;
		}

		public static UserRosterItem FromRosterItem(RosterItem ri)
		{
			var item = new UserRosterItem(ri.Jid)
			{
				Name = ri.Name,
				Ask = ri.Ask,
				Subscribtion = ri.Subscription,
			};
			foreach (Element element in ri.GetGroups())
			{
				item.Groups.Add(element.Value);
			}
			return item;
		}

		public IQ GetRosterIq(Jid to)
		{
			var iq = new IQ(IqType.set);
			var roster = new Roster();
			roster.AddRosterItem(ToRosterItem());
			iq.Query = roster;
			iq.To = to.BareJid;
			return iq;
		}

		public override string ToString()
		{
			return string.IsNullOrEmpty(Name) ? Jid.ToString() : Name;
		}

		public override bool Equals(object obj)
		{
			var i = obj as UserRosterItem;
			return i != null && i.Jid == Jid;
		}

		public override int GetHashCode()
		{
			return Jid.GetHashCode();
		}
	}
}