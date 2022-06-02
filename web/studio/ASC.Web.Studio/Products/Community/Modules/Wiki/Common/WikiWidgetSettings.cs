using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Web.Community.Wiki.Common
{
    [Serializable]
    public class WikiWidgetSettings : ISettings
    {
        public int NewPageCount { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{4D52DB1C-2441-46ba-9DB3-CEF649A6D510}"); }
        }

        public ISettings GetDefault()
        {
            return new WikiWidgetSettings() { NewPageCount = 3 };
        }

        #endregion
    }
}
