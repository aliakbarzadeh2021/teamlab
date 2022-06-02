using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Xml;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System.Reflection;

namespace ASC.Web.Studio.Core.Users
{
    [Serializable]
    public class PeopleNamesSettings : ISettings
    {
        public PeopleNamesItem Item { get; set; }
        public string ItemID { get; set; }

        #region ISettings Members

        public ISettings GetDefault()
        {
            var settings = new PeopleNamesSettings();
            settings.ItemID = PeopleNamesItem.DefaultID;
            settings.Item = null;

            return settings;
        }

        public Guid ID
        {
            get { return new Guid("47F34957-6A70-4236-9681-C8281FB762FA"); }
        }

        #endregion
    }

    [Serializable]
    public class PeopleNamesException
    {
        public string LngISO2 { get; set; }
        public string ResourcePath { get; set; }
        public string Value { get; set; }
    }
     
    [Serializable]
    public class PeopleNamesItem 
    {
        public static string DefaultID { get { return "default"; } }
        public static string CustomID { get { return "custom"; } }
        public List<PeopleNamesException> Exceptions = new List<PeopleNamesException>();
        
        public string Id { get; set; }
        public int SortOrder { get; set; }

        private static string GetResourceValue(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey)) return string.Empty;
			return (string)typeof(NamingPeopleResource).GetProperty(resourceKey, BindingFlags.Static | BindingFlags.Public)
				            .GetValue(null, null);
        }

        private string _schemaName;
        public string SchemaName
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _schemaName;
                return GetResourceValue(_schemaName);
            }
            set { _schemaName = value; }
        }
        
        private string _userCaption;
        public string UserCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _userCaption;
                return GetResourceValue(_userCaption);
            }
            set { _userCaption = value; }
        }

        private string _usersCaption;
        public string UsersCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _usersCaption;
                return GetResourceValue(_usersCaption);
            }
            set { _usersCaption = value; }
        }

        private string _groupCaption;
        public string GroupCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _groupCaption;
                return GetResourceValue(_groupCaption);
            }
            set { _groupCaption = value; }
        }

        private string _groupsCaption;
        public string GroupsCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _groupsCaption;
                return GetResourceValue(_groupsCaption);
            }
            set { _groupsCaption = value; }
        }

        private string _userPostCaption;
        public string UserPostCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _userPostCaption;
                return GetResourceValue(_userPostCaption);
            }
            set { _userPostCaption = value; }
        }

        private string _globalHeadCaption;
        public string GlobalHeadCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _globalHeadCaption;
                return GetResourceValue(_globalHeadCaption);
            }
            set { _globalHeadCaption = value; }
        }

        private string _groupHeadCaption;
        public string GroupHeadCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _groupHeadCaption;
                return GetResourceValue(_groupHeadCaption);
            }
            set { _groupHeadCaption = value; }
        }

        private string _regDateCaption;
        public string RegDateCaption
        {
            get
            {
                if (this.Id.Equals(CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return _regDateCaption;
                return GetResourceValue(_regDateCaption);
            }
            set { _regDateCaption = value; }
        }
    }

    public class CustomNamingPeople
    {        
        private static bool _isLoad = false;

        private static List<PeopleNamesItem> _items = new List<PeopleNamesItem>();

        public static void Load() 
        {
            var path = HttpContext.Current.Server.MapPath("~/Core/Users/PeopleNames.xml");
            var doc = new XmlDocument();            
            doc.Load(path);

            _items.Clear();
            foreach (XmlNode node in doc.SelectNodes("/root/item"))
            {
                var item = new PeopleNamesItem();
                item.Id = node.SelectSingleNode("id").InnerText;
                item.SortOrder = Convert.ToInt32(node.SelectSingleNode("sortorder").InnerText);
                
                item.SchemaName = node.SelectSingleNode("names/schemaname").InnerText;
                item.GlobalHeadCaption = node.SelectSingleNode("names/globalhead").InnerText;
                item.GroupHeadCaption = node.SelectSingleNode("names/grouphead").InnerText;
                item.GroupCaption = node.SelectSingleNode("names/group").InnerText;
                item.GroupsCaption = node.SelectSingleNode("names/groups").InnerText;
                item.UserCaption = node.SelectSingleNode("names/user").InnerText;
                item.UsersCaption = node.SelectSingleNode("names/users").InnerText;
                item.UserPostCaption = node.SelectSingleNode("names/userpost").InnerText;
                item.RegDateCaption = node.SelectSingleNode("names/regdate").InnerText;
               
                var expNode = node.SelectSingleNode("exceptions");
                if (expNode != null)
                {
                    foreach (XmlNode lngNode in expNode.ChildNodes)
                    {
                        foreach (XmlNode lngExpNode in lngNode.ChildNodes)
                        {
                            item.Exceptions.Add(new PeopleNamesException()
                            {
                                LngISO2 = lngNode.Name,
                                ResourcePath = lngExpNode.Attributes["key"].InnerText,
                                Value = lngExpNode.InnerText
                            });
                        }
                    }
                }

                _items.Add(item);
            }

            _items.Sort((i1,i2)=> Comparer<int>.Default.Compare(i1.SortOrder, i2.SortOrder));
            _isLoad = true;            
        }

        public static Dictionary<string, string> GetSchemas()         
        {
            if (!_isLoad)
                Load();

            var dict = new Dictionary<string, string>();
            foreach (var item in _items)
            {
                if (!dict.ContainsKey(item.Id.ToLower()))
                {
                    var it = GetPeopleNames(item.Id);
                    dict.Add(it.Id.ToLower(), it.SchemaName);
                }
            }

            dict.Add(PeopleNamesItem.CustomID, Resources.Resource.CustomNamingPeopleSchema);

            return dict;
        }

        public static PeopleNamesItem Current
        {
            get
            {
                var settings = SettingsManager.Instance.LoadSettings<PeopleNamesSettings>(TenantProvider.CurrentTenantID);
                if (String.Equals(settings.ItemID, PeopleNamesItem.CustomID, StringComparison.InvariantCultureIgnoreCase))
                    return settings.Item;

                return GetPeopleNames(settings.ItemID);
            }
        }

        public static PeopleNamesItem GetPeopleNames(string schemaId)
        {
            if (string.Equals(schemaId, PeopleNamesItem.CustomID, StringComparison.InvariantCultureIgnoreCase))
            {
                var settings = SettingsManager.Instance.LoadSettings<PeopleNamesSettings>(TenantProvider.CurrentTenantID);
                if (settings.Item == null)
                    return new PeopleNamesItem()
                    {                       
                        Id = PeopleNamesItem.CustomID,
                        GlobalHeadCaption = "",
                        GroupCaption = "",
                        GroupHeadCaption = "",
                        GroupsCaption = "",
                        RegDateCaption = "",
                        SortOrder = 0,
                        UserCaption = "",
                        UserPostCaption = "",
                        UsersCaption = "",
                        SchemaName = Resources.Resource.CustomNamingPeopleSchema
                    };

                return settings.Item;
            }

            if (!_isLoad)
                Load();

            var item = _items.Find(i => i.Id.Equals(schemaId, StringComparison.InvariantCultureIgnoreCase));            
            return item;

        }

        public static void SetPeopleNames(string schemaId)
        {
            var settings = SettingsManager.Instance.LoadSettings<PeopleNamesSettings>(TenantProvider.CurrentTenantID);
            settings.ItemID = schemaId;
            //settings.Item = null;
            SettingsManager.Instance.SaveSettings<PeopleNamesSettings>(settings, TenantProvider.CurrentTenantID);
        }

        public static void SetPeopleNames(PeopleNamesItem custom)
        {
            var settings = SettingsManager.Instance.LoadSettings<PeopleNamesSettings>(TenantProvider.CurrentTenantID);
            custom.Id = PeopleNamesItem.CustomID;
            settings.ItemID = PeopleNamesItem.CustomID;
            settings.Item = custom;
            SettingsManager.Instance.SaveSettings<PeopleNamesSettings>(settings, TenantProvider.CurrentTenantID);
        }


        public static List<PeopleNamesItem> PeopleNamesCollection { get; private set; }

        private static string SubstituteUser(string text)
        {
            var item = Current;

            return text.Replace("{!User}", item.UserCaption)
                        .Replace("{!user}", item.UserCaption.ToLower())
                        .Replace("{!users}", item.UsersCaption.ToLower())
                        .Replace("{!Users}", item.UsersCaption);
        }

        private static string SubstituteGroup(string text)
        {
            var item = Current;

            return text.Replace("{!Group}", item.GroupCaption)
                        .Replace("{!group}", item.GroupCaption.ToLower())
                        .Replace("{!Groups}", item.GroupsCaption)
                        .Replace("{!groups}", item.GroupsCaption.ToLower());
        }

        private static string SubstitutePost(string text)
        {
            var item = Current;

            return text.Replace("{!Post}", item.UserPostCaption)
                        .Replace("{!post}", item.UserPostCaption.ToLower());
        }

        private static string SubstituteGlobalHead(string text)
        {
            var item = Current;

            return text.Replace("{!CEO}", item.GlobalHeadCaption)
                       .Replace("{!ceo}", item.GlobalHeadCaption.ToLower());
        }

        private static string SubstituteGroupHead(string text)
        {
            var item = Current;

            return text.Replace("{!Head}", item.GroupHeadCaption)
                       .Replace("{!head}", item.GroupHeadCaption.ToLower());
        }

        private static string SubstituteRegDate(string text)
        {
            var item = Current;

            return text.Replace("{!Regdate}", item.RegDateCaption)
                       .Replace("{!regdate}", item.RegDateCaption.ToLower());
        }

        private static string SubstituteUserPost(string text)
        {
            var item = Current;

            return text.Replace("{!Userpost}", item.UserPostCaption)
                       .Replace("{!userpost}", item.UserPostCaption.ToLower());
        }

        public static string Substitute<T>(string resourceKey) where T: class
        {
            var item = Current;
            var resourceName = typeof(T).Module.Name.ToLower().Replace(".dll", ".")+ typeof(T).FullName + "." + resourceKey;
            if (typeof(T).Module.Name.IndexOf("App_GlobalResources", StringComparison.InvariantCultureIgnoreCase) >= 0)
                resourceName = "ASC.Web.Studio." + typeof(T).FullName + "." + resourceKey;

            var ex = item.Exceptions.Find(e=> string.Equals(e.LngISO2, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, StringComparison.InvariantCultureIgnoreCase)&&
                                     string.Equals(e.ResourcePath,resourceName, StringComparison.CurrentCultureIgnoreCase));
            if (ex != null)
                return Substitute(ex.Value);


            string text = "";
            try
            {
                text = (string)typeof(T).GetProperty(resourceKey, BindingFlags.Static | BindingFlags.Public)
                                .GetValue(null, null);
            }
            catch {
                text = (string)typeof(T).GetProperty(resourceKey, BindingFlags.Static | BindingFlags.NonPublic)
                                .GetValue(null, null);
            }            
            
            return Substitute(text);
        }

        private static string Substitute(string text)
        {   
            return SubstituteUserPost(SubstituteRegDate(SubstituteGroupHead(SubstituteGlobalHead(SubstitutePost(SubstituteGroup(SubstituteUser(text)))))));
        }
    }
}