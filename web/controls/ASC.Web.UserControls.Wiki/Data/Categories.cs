using System;
using System.Collections.Generic;
using System.Web;

namespace ASC.Web.UserControls.Wiki.Data
{
    public class Categories
    {
        public string PageName { get; set; }
        public string CategoryName { get; set; }
        public int Tenant { get; set; }

        public Categories()
        {
            PageName = CategoryName = string.Empty;
            Tenant = 0;
        }
    }
}
