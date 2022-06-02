using System;
using System.Collections.Generic;
using System.Text;
using ASC.Core;
using ASC.Core.Notify;

namespace ASC.Blogs.Core
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
        NewBlog = 0,
        NewBlogCorp = 1,
        NewBlogPers = 2,
        NewLoveAuthor = 3,
        NewComment = 4
    }
}
