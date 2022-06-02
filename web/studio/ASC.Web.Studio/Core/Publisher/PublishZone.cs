using System;
using System.Collections.Generic;
using ASC.Core.Common.Publisher;

namespace ASC.Web.Studio.Core.Publisher
{
    public interface IPublishZoneCollection
    {
        List<PublishZone> PublishZones { get; }
    }

    public class PublishZone
    {
        public Zone Zone { get; set; }

        public PublishZone(Zone zone)
        {
            this.Zone = zone;
        }

        public event EventHandler<ZoneLoadEventArgs> Load;

        public void LoadZone(object sender, ZoneLoadEventArgs e)
        {
            if (Load != null)
                Load(sender, e);
        }
    }

    public class ZoneLoadEventArgs : EventArgs
    {
        public Article Article { get; private set; }

        public ZoneLoadEventArgs(Article article)
        {
            this.Article = article;
        }
    }
}
