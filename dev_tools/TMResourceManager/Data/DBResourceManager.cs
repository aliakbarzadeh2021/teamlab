using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Web;

namespace TMResourceData
{
    public class DBResourceManager : ResourceManager
    {
        string FileName = "";
        ResourceManager ResManager;

        static DateTime UpdateDate = DateTime.UtcNow;
        static Hashtable ResData = GetResource.GetAllData();

        public DBResourceManager(string fileName, ResourceManager resManager)
        {
            ResourceSets = new Hashtable();
            FileName = fileName;
            ResManager = resManager;
        }

        public override ResourceSet GetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            DBResourceSet _databaseResourceSet = null;

            if (ResourceSets.Contains(culture.Name))
            {
                while (true)
                {
                    if ((ResourceSets[culture.Name] as DBResourceSet).TableCount != 0 || culture.Equals(CultureInfo.InvariantCulture))
                        break;

                    culture = culture.Parent;
                }

                _databaseResourceSet = ResourceSets[culture.Name] as DBResourceSet;
            }
            else
            {
                _databaseResourceSet = new DBResourceSet(FileName, culture);
                ResourceSets.Add(culture.Name, _databaseResourceSet);
            }

            if (DateTime.UtcNow > UpdateDate.AddSeconds(2))
            {
                GetResource.UpdateDBRS(_databaseResourceSet, FileName, culture.Name, UpdateDate);
                UpdateDate = DateTime.UtcNow;
            }

            return _databaseResourceSet;

        }

        public override string GetString(string name, CultureInfo culture)
        {
            try
            {
                var pageLink = string.Empty;
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    var uri = HttpContext.Current.Request.Url;
                    var updatePortals = ConfigurationManager.AppSettings["UpdatePortals"].Split(';').ToList();
                    var updateSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["UpdateSeconds"]);
                    var getPagePortal = ConfigurationManager.AppSettings["GetPagePortal"];

                    if (updatePortals.Contains(uri.Host, StringComparer.InvariantCultureIgnoreCase) && DateTime.UtcNow > UpdateDate.AddSeconds(updateSeconds))
                    {
                        GetResource.UpdateHashTable(ref ResData, UpdateDate);
                        UpdateDate = DateTime.UtcNow;
                    }
                    if (uri.Host == getPagePortal)
                    {
                        pageLink = uri.AbsolutePath;
                    }
                }

                var ci = culture ?? CultureInfo.CurrentUICulture;
                while (true)
                {
                    var language = !string.IsNullOrEmpty(ci.Name) ? ci.Name : "Neutral";
                    var resdata = ResData[name + FileName + language];
                    if (resdata != null)
                    {
                        if (!string.IsNullOrEmpty(pageLink))
                        {
                            GetResource.AddLink(name, FileName, pageLink);
                        }
                        return resdata.ToString();
                    }

                    if (ci.Equals(CultureInfo.InvariantCulture))
                    {
                        break;
                    }
                    ci = ci.Parent;
                }
            }
            catch { }

            return ResManager.GetString(name, culture);
        }
    }
}
