﻿using System;
using System.Collections.Generic;
using System.Text;
using ASC.Web.Core.Subscriptions;
using ASC.Bookmarking.Common;
using ASC.Bookmarking.Resources;
using ASC.Core;
using System.Web;
using ASC.Notify.Recipients;


namespace ASC.Bookmarking.Business.Subscriptions
{
	public class BookmarkingSubscriptionManager : ISubscriptionManager
	{	

		public BookmarkingSubscriptionManager()
		{
			
		}

		#region ISubscriptionManager Members

		#region Subscription Objects
		private List<SubscriptionObject> GetNewBookmarkSubscriptionObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			return GetSubscriptionObjects(true, false);
		}

		private List<SubscriptionObject> GetNewCommentSubscriptionObjects(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			return GetSubscriptionObjects(false, true);
		}

		private List<SubscriptionObject> GetSubscriptionObjects(bool includeBookmarks, bool includeComments)
		{
			List<SubscriptionObject> subscriptionObjects = new List<SubscriptionObject>();

			IList<string> list = new List<string>();

			var currentAccount = SecurityContext.CurrentAccount;

			if (includeBookmarks)
			{

				list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewBookmark,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name))
						);
				if (list.Count > 0)
				{
					foreach (string id in list)
					{
						if (!string.IsNullOrEmpty(id))
						{
							subscriptionObjects.Add(new SubscriptionObject()
							{
								ID = id,
								Name = BookmarkingBusinessResources.SubscriptionTypeNewBookmark,
								URL = string.Empty,
                                SubscriptionType = GetSubscriptionTypes()[0]
							});
						}
					}
				}
			}

			if (includeComments)
			{

				list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewComment,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name))
						);
				if (list.Count > 0)
				{
					var service = BookmarkingService.GetCurrentInstanse();
					foreach (string id in list)
					{
						if (!string.IsNullOrEmpty(id))
						{
							try
							{
								var bookmark = service.GetBookmarkByID(Int32.Parse(id));
								subscriptionObjects.Add(new SubscriptionObject()
								{
									ID = id,
									Name = bookmark.Name,
									URL = VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Bookmarking/BookmarkInfo.aspx") + "?Url=" + HttpUtility.UrlEncode(bookmark.URL),
                                    SubscriptionType = GetSubscriptionTypes()[1]
								});
							}
							catch { }
						}
					}
				}
			}
			return subscriptionObjects;
		}



		public List<SubscriptionObject> GetSubscriptionObjects()
		{
			return GetSubscriptionObjects(true, true);

		} 
		#endregion

		#region Subscription Types
		public List<SubscriptionType> GetSubscriptionTypes()
		{
            var subscriptionTypes = new List<SubscriptionType>();
            subscriptionTypes.Add(new SubscriptionType()
            {
				ID = BookmarkingBusinessConstants.NotifyActionNewBookmarkID,
                NotifyAction = BookmarkingBusinessConstants.NotifyActionNewBookmark,
                Name = BookmarkingBusinessResources.SubscriptionTypeNewBookmark,
                GetSubscriptionObjects = GetNewBookmarkSubscriptionObjects,
                IsEmptySubscriptionType = IsEmptyNewBookmarkSubscriptionType,
                Single = true
            });

            subscriptionTypes.Add(new SubscriptionType()
            {
				ID = BookmarkingBusinessConstants.NotifyActionNewCommentID,
                NotifyAction = BookmarkingBusinessConstants.NotifyActionNewComment,
                Name = BookmarkingBusinessResources.SubscriptionTypeNewComments,
                GetSubscriptionObjects = GetNewCommentSubscriptionObjects,
                IsEmptySubscriptionType = IsEmptyNewCommentsSubscriptionType
            });

			return subscriptionTypes;
		}

		private bool IsEmptyNewBookmarkSubscriptionType(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			var currentAccount = SecurityContext.CurrentAccount;
			var list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewBookmark,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name))
						);
			return list.Count == 0;			
		}

		private bool IsEmptyNewCommentsSubscriptionType(Guid productID, Guid moduleOrGroupID, Guid typeID)
		{
			var currentAccount = SecurityContext.CurrentAccount;
			var list = new List<string>(
					SubscriptionProvider.GetSubscriptions(
						BookmarkingBusinessConstants.NotifyActionNewComment,
						new DirectRecipient(currentAccount.ID.ToString(), currentAccount.Name))
						);
			return list.Count == 0;
		}
		#endregion

		public ASC.Notify.Model.ISubscriptionProvider SubscriptionProvider
		{
			get
			{
				return BookmarkingNotifySource.Instance.GetSubscriptionProvider();
			}
		}

		#endregion
	}
}
