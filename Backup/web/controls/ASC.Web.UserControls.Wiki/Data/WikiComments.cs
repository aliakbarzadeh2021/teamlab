using System;
using System.Collections.Generic;
using System.Web;
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    public class WikiComments: IWikiObjectOwner
    {
        public WikiComments()
        {
            Id = Guid.NewGuid();
            ParentId = UserId = OwnerID = Guid.Empty;
            PageName = Body = string.Empty;
            Date = TenantUtil.DateTimeNow();
            Inactive = false;
            Tenant = 0;
            
        }

        public Guid Id {get; set;}
        public Guid ParentId {get; set;}
        public string PageName { get; set; }
        public string Body { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public bool Inactive { get; set; }
        public int Tenant { get; set; }

        #region IWikiObjectOwner Members
        public Guid OwnerID { get; set; }
        public object GetObjectId()
        {
            return Id;
        }
        #endregion

        
    }
}
