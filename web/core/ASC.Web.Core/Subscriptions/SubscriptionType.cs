﻿using System;
using ASC.Notify.Model;
using System.Collections.Generic;

namespace ASC.Web.Core.Subscriptions
{
    public delegate bool IsEmptySubscriptionTypeDelegate(Guid productID, Guid moduleOrGroupID, Guid typeID);

    public delegate List<SubscriptionObject> GetSubscriptionObjectsDelegate(Guid productID, Guid moduleOrGroupID, Guid typeID);

    public class SubscriptionType
    {
        public INotifyAction NotifyAction { get; set; }

        public Guid ID { get; set; }

        public string Name { get; set; }

        public bool Single { get; set; }

        public IsEmptySubscriptionTypeDelegate IsEmptySubscriptionType;

        public GetSubscriptionObjectsDelegate GetSubscriptionObjects;
        
    }
}
