using System;
using System.Collections.Generic;
using System.Text;
using ASC.Core;
using ASC.Core.Notify;

namespace ASC.PhotoManager
{
	[Serializable]
	public class NotifySenderSettings : ISettings
	{
		public List<SubscriptionNotitySenders> SubscriptionNotitySenders { get; set; }

		#region ISettings Members

		public Guid ID
		{
			get { return new Guid("{31FA4B26-3874-48bb-AA10-054913BC0CEA}"); }
		}

		public ISettings GetDefault()
		{
			return new NotifySenderSettings()
			{
				SubscriptionNotitySenders = new List<SubscriptionNotitySenders>()
                {
                    new SubscriptionNotitySenders(){ SubscriptionType = SubscriptionType.NewPhoto, 
                                                       Senders = new List<NotifySenderDescription>(WorkContext.AvailableNotifySenders) },
                     
                    new SubscriptionNotitySenders(){ SubscriptionType = SubscriptionType.NewComment, 
                                                       Senders = new List<NotifySenderDescription>(WorkContext.AvailableNotifySenders) }
                }
			};
		}

		#endregion
	}
}
