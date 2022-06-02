using System;
using System.Collections.Generic;
using System.Text;
using ASC.Core.Notify;

namespace ASC.PhotoManager
{
    [Serializable]
    public class SubscriptionNotitySenders
    {
        public SubscriptionType SubscriptionType { get; set; }

        public List<NotifySenderDescription> Senders { get; set; }

        public SubscriptionNotitySenders()
        {
            Senders = new List<NotifySenderDescription>(0);
        }
    }

    public enum SubscriptionType
    {
        NewPhoto = 0,
        NewComment = 1
    }
}