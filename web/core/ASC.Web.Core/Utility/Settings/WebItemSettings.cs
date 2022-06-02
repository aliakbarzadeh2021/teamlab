using System;
using System.Collections.Generic;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core.Utility.Settings
{
    [Serializable]
    public class WebItemSettings : ISettings
    {
        [Serializable]
        public class WebItemOption
        {
            public Guid ItemID { get; set; }
            public int SortOrder { get; set; }
            public List<Guid> ChildItemIDs { get; set; }
            public string CustomName { get; set; }
            public bool Disabled { get; set; }

            public WebItemOption()
            {
                ChildItemIDs = new List<Guid>();
                this.SortOrder = int.MinValue;
                CustomName = "";
                this.Disabled = false;
            }
        }


        public WebItemSettings()
        {
            SettingsCollection = new List<WebItemOption>();
        }

        #region ISettings Members

        public List<WebItemOption> SettingsCollection { get; set; }

        public ISettings GetDefault()
        {
            var settings = new WebItemSettings();
            WebItemManager.Instance.GetItemsInternal().ForEach(w =>
            {
                var opt = new WebItemOption()
                {
                    CustomName = w.Name,
                    ItemID = w.ID,
                    SortOrder = WebItemManager.GetSortOrder(w),
                    Disabled = false,
                };
                settings.SettingsCollection.Add(opt);
            });
            return settings;
        }

        public Guid ID
        {
            get { return new Guid("{C888CF56-585B-4c78-9E64-FE1093649A62}"); }
        }

        #endregion
    }
}
