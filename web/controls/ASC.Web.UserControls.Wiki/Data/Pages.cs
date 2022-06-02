using System;
using System.Collections.Generic;
using System.Web;
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    public class Pages : BaseContainer
    {
        public string PageName { get; set; }
        public string Body { get; set; }
        

        public Pages()
        {
            UserID = Guid.Empty;
            OwnerID = Guid.Empty;
            PageName = Body = string.Empty;
            Version = 0;
            Date = TenantUtil.DateTimeNow();
            Tenant = 0;
        }

        public override object GetObjectId()
        {
            return PageName;
        }
    }
}
