using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.Community.Wiki
{
    public class PageDictionary
    {
        public PageDictionary()
        {
            Pages = new List<Pages>();
        }
        public string HeadName { get; set; }
        public List<Pages> Pages { get; set; }
    }
}
