using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Common.Security.Authorizing;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiAuthCategoriesProvider : IAuthCategoriesProvider
    {
        public virtual AuthCategory[] GetAuthCategories()
        {
            return Constants.AuthorizingCategories;
        }
    }
}
