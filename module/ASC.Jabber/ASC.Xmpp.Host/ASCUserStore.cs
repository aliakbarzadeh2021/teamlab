﻿using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.client;
using ASC.Core.Users;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Storage.Interface;
using ASC.Xmpp.Server.Users;

namespace ASC.Xmpp.Host
{
	class ASCUserStore : IUserStore
	{
		#region IUserStore Members

		public ICollection<User> GetUsers(string domain)
		{
			ASCContext.SetCurrentTenant(domain);
			var users = new List<User>();
			foreach (var ui in ASCContext.UserManager.GetUsers())
			{
				var u = ToUser(ui, domain);
				if (u != null) users.Add(u);
			}
			return users;
		}

		public User GetUser(Jid jid)
		{
			ASCContext.SetCurrentTenant(jid.Server);
			var u = ASCContext.UserManager.GetUserByUserName(jid.User);
			if (Constants.LostUser.Equals(u) || u.Status == EmployeeStatus.Terminated) return null;
			return ToUser(u, jid.Server);
		}

		public void SaveUser(User user)
		{
			throw new JabberException(ErrorCode.Forbidden);
		}

		public void RemoveUser(Jid jid)
		{
			throw new JabberException(ErrorCode.Forbidden);
		}

		#endregion

		private User ToUser(UserInfo userInfo, string domain)
		{
			try
			{
				if (string.IsNullOrEmpty(userInfo.UserName)) return null;
				return new User(
					new Jid(userInfo.UserName.ToLowerInvariant() + "@" + domain.ToLowerInvariant()),
					ASCContext.Authentication.GetUserPasswordHash(userInfo.ID),
					ASCContext.UserManager.IsUserInGroup(userInfo.ID, Constants.GroupAdmin.ID)
				);
			}
			catch { }
			return null;
		}
	}
}