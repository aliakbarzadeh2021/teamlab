using System;
using System.Collections.Generic;
using System.Text;
using ASC.Core;
using ASC.Core.Notify;


namespace ASC.Blogs.Core
{
    [Serializable]
    public class NotifySenderSettings : ISettings
    {
        public List<SubscriptionNotitySenders> SubscriptionNotitySenders { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{2DE87BDA-77D0-4516-9334-FC1A3D1A9D51}"); }
        }

        public ISettings GetDefault()
        {
            return new NotifySenderSettings()
            {
                SubscriptionNotitySenders = new List<SubscriptionNotitySenders>()
                {
                    new SubscriptionNotitySenders(){ SubscriptionType = SubscriptionType.NewBlog, 
                                                       Senders = new List<NotifySenderDescription>(WorkContext.AvailableNotifySenders) },
                    new SubscriptionNotitySenders(){ SubscriptionType = SubscriptionType.NewBlogCorp, 
                                                       Senders = new List<NotifySenderDescription>(WorkContext.AvailableNotifySenders) },
                    new SubscriptionNotitySenders(){ SubscriptionType = SubscriptionType.NewBlogPers, 
                                                       Senders = new List<NotifySenderDescription>(WorkContext.AvailableNotifySenders) },
                    new SubscriptionNotitySenders(){ SubscriptionType = SubscriptionType.NewLoveAuthor, 
                                                       Senders = new List<NotifySenderDescription>(WorkContext.AvailableNotifySenders) },
                    new SubscriptionNotitySenders(){ SubscriptionType = SubscriptionType.NewComment, 
                                                       Senders = new List<NotifySenderDescription>(WorkContext.AvailableNotifySenders) }
                }
            };
        }

        #endregion
    }
}
