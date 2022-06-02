using System;
using System.Collections.Generic;
using System.Web;

namespace ASC.Web.UserControls.Wiki.Data
{
    public abstract class BaseContainer : IWikiObjectOwner
    {
        public int Tenant { get; set; }
        public Guid UserID { get; set; }
        public int Version { get; set; }
        public DateTime Date { get; set; }

        #region IWikiObjectOwner Members
        public Guid OwnerID { get; set; }
        public abstract object GetObjectId();
        #endregion
    }
}
