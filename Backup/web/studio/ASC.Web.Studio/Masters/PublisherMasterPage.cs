using System;
using System.Collections.Generic;
using ASC.Core.Common.Publisher;
using ASC.Web.Studio.Core.Publisher;

namespace ASC.Web.Studio.Masters
{
    public abstract class PublisherMasterPage : System.Web.UI.MasterPage, IPublishZoneCollection
    {
        protected List<PublishZone> _zonesForLoad { get; private set; }

        protected virtual void InitPageZones()
        {
            _zonesForLoad = new List<PublishZone>();
            var pageZones = new List<PublishZone>();

            if (this.Page is IPublishZoneCollection)
            {
                pageZones = (this.Page as IPublishZoneCollection).PublishZones;
                if (pageZones != null)
                    _zonesForLoad.AddRange(pageZones);
            }

            if (this.PublishZones != null)
            {
                foreach (var mZone in this.PublishZones)
                {
                    if (pageZones.Find(pZone => String.Equals(mZone.Zone.ID, pZone.Zone.ID, StringComparison.InvariantCultureIgnoreCase)) == null)
                        _zonesForLoad.Add(mZone);
                }
            }
        }

        protected virtual void LoadZoneContent(RequestContext requestContext)
        {
            if (_zonesForLoad == null || _zonesForLoad.Count == 0)
                return;

            List<Zone> zones = new List<Zone>();

            _zonesForLoad.ForEach(zh =>
            {
                if (zh.Zone != null &&
                   zones.Find(zone => String.Equals(zone.ID, zh.Zone.ID, StringComparison.InvariantCultureIgnoreCase)) == null)
                    zones.Add(zh.Zone);
            });

            var articles = PublisherHolder.Instance.HandleRequest(requestContext, zones);

            foreach (var zoneHolder in _zonesForLoad)
            {

                var article = articles.Find(art => String.Equals(art.Zone.ID, zoneHolder.Zone.ID, StringComparison.InvariantCultureIgnoreCase));

                if (zoneHolder.Zone.ID == ASC.Core.Common.Publisher.Constants.GreetingZoneID)
                {
                    var defArtList = new DefaultPublisher().HandleRequest(requestContext, new List<Zone> { zoneHolder.Zone });
                    if (defArtList.Count > 0)
                    {
                        Article defArt = defArtList[0];
                        if (defArt != null && 
                                (
                                    article ==null  
                                    ||              
                                    defArt.Type > article.Type 
                                )
                            )
                            article = defArt;
                    }
                }

                if (article != null)
                    zoneHolder.LoadZone(null, new ZoneLoadEventArgs(article));
            }
        }

        #region IPublishZoneCollection Members

        public abstract List<PublishZone> PublishZones { get; }

        #endregion
    }
}
