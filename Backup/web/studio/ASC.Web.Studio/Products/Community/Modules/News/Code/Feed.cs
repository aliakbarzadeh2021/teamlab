using System;
using System.Collections.Generic;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Web.Community.News.Code
{
	[Serializable]
	public class Feed : ISecurityObject
	{
		public long Id
		{
			get;
			set;
		}

		public FeedType FeedType
		{
			get;
			set;
		}

		public string Caption
		{
			get;
			set;
		}

		public string Text
		{
			get;
			set;
		}

		public DateTime Date
		{
			get;
			set;
		}

		public string Creator
		{
			get;
			set;
		}

		public virtual bool Readed
		{
			get;
			internal set;
		}

		public Feed()
		{
			FeedType = FeedType.News;
			Creator = SecurityContext.CurrentAccount.ID.ToString();
			Date = TenantUtil.DateTimeNow();
			Readed = false;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var feed = obj as Feed;
			return feed != null && Id == feed.Id;
		}

		public override string ToString()
		{
			return Caption;
		}


		public Type ObjectType
		{
			get { return GetType(); }
		}

		public object SecurityId
		{
			get { return Id; }
		}

		public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
		{
			var roles = new List<IRole>();

			if (Equals(account.ID, new Guid(Creator)))
			{
				roles.Add(ASC.Common.Security.Authorizing.Constants.Owner);
			}

			return roles;
		}

		public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
		{
			throw new NotImplementedException();
		}

		public bool InheritSupported
		{
			get { return false; }
		}

		public bool ObjectRolesSupported
		{
			get { return true; }
		}
	}
}