using System;
using System.Collections.Generic;
using System.Xml;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Users;
using ASC.Core.Tenants;

namespace ASC.Web.Studio.Core.Publisher.Greetings
{
    public delegate string TagResolver(string tag);

    public sealed class StupidRandomWelcome
    {
        
        #region Singleton
        static StupidRandomWelcome _defaultInstance = null;
        static object _syncRoot = new object();

        public static StupidRandomWelcome DefaultInstance
        {
            get
            {
                if (_defaultInstance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_defaultInstance == null)
                        {
                            _defaultInstance = new StupidRandomWelcome(
                                    Greetings.GreetingsResource.greetings,
                                    new Dictionary<string, TagResolver> { 
                                        {"username",ASCTagResolver},
                                        {"personalcount",ASCTagResolver},
                                        {"joindays",ASCTagResolver}
                                    }
                                );
                        }
                    }
                }
                return _defaultInstance;
            }
        }
        static string ASCTagResolver(string tag)
        {
            string result = null;
            if (tag == "username")
            {
                if (SecurityContext.IsAuthenticated)
                {
                    result = SecurityContext.CurrentAccount.Name;
                    if (CoreContext.UserManager.UserExists(SecurityContext.CurrentAccount.ID))
                        result = DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID);
                }
                else result = "%username%";
            }
            if (tag == "personalcount")
            {
                if (SecurityContext.IsAuthenticated)
                {
                    result = CoreContext.UserManager.GetUsers().Length.ToString();
                }
                else result = "Many";
            }
            if (tag == "joindays")
            {
                result = "Many";
                if (SecurityContext.IsAuthenticated)
                {
                    if (CoreContext.UserManager.UserExists(SecurityContext.CurrentAccount.ID))
                    {
                        UserInfo user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                        if (user.WorkFromDate.HasValue)
                            result = ((int)(TenantUtil.DateTimeNow() - user.WorkFromDate.Value).TotalDays).ToString();

                    }
                }
            }
            return result;
        }
        #endregion

        readonly IDictionary<string, TagResolver> _externalTagResolvers = null;
        readonly List<Greeting> _staticGreetings = null;
        readonly Random _rnd = new Random();

        public StupidRandomWelcome(string xml)
        {
            if (String.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");

            Xml = xml;
            _xmlEntrys = ParseXmlEntrys(Xml);
            _staticGreetings = ParseStaticGreetings(_xmlEntrys);
        }

        public StupidRandomWelcome(string xml, IDictionary<string, TagResolver> externalTagResolvers)
            : this(xml)
        {
            _externalTagResolvers = externalTagResolvers;
        }


        public string Xml { get; private set; }

        public List<Greeting> GetAllGreetings()
        {
            return _staticGreetings.ConvertAll((gr) => ParseDynamicGreeting(gr, _externalTagResolvers));
        }

        public Greeting GetRandomGreeting()
        {
            if (_staticGreetings.Count == 0) return new Greeting() { Phrase = "Hello!", Description = "" };

            Greeting rand = _staticGreetings[_rnd.Next(_staticGreetings.Count - 1)];

            return ParseDynamicGreeting(rand, _externalTagResolvers);
        }

        #region xml parsing
        internal class XmlEntry
        {
            public string FormPhrase;
            public string FormDescription;
            public Dictionary<string, string> Attributes;
        }

        internal List<XmlEntry> _xmlEntrys = null;

        internal List<XmlEntry> ParseXmlEntrys(string xml)
        {
            List<XmlEntry> result = new List<XmlEntry>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var nav = doc.CreateNavigator();
            var nodes = nav.Select(@"//greetings/form");

            while (nodes.MoveNext())
            {
                var node = nodes.Current.Clone();

                string phrase = null, description = null;
                if (node.MoveToAttribute("phrase", ""))
                {
                    phrase = node.Value;
                    node.MoveToParent();
                }
                if (node.MoveToAttribute("description", ""))
                {
                    description = node.Value;
                    node.MoveToParent();
                }

                if (!String.IsNullOrEmpty(phrase))
                {
                    if (node.MoveToChild("phrase", ""))
                    {
                        do
                        {
                            Dictionary<string, string> attrs = new Dictionary<string, string>();
                            if (node.MoveToFirstAttribute())
                            {
                                do
                                {
                                    attrs.Add(node.Name, node.Value);
                                } while (node.MoveToNextAttribute());

                                node.MoveToParent();
                            }

                            if (attrs.Count > 0)
                                result.Add(new XmlEntry() { FormPhrase = phrase, FormDescription = description, Attributes = attrs });
                        } while (node.MoveToNext());
                    }
                    else
                    {
                        result.Add(new XmlEntry() { FormPhrase = phrase, FormDescription = description });
                    }
                }
            }

            return result;
        }

        #endregion

        internal List<Greeting> ParseStaticGreetings(List<XmlEntry> xmlEntrys)
        {
            List<Greeting> result = new List<Greeting>();

            foreach (var entry in xmlEntrys)
                result.Add(ParseXmlEntry(entry));

            return result;
        }

        internal Greeting ParseXmlEntry(XmlEntry entry)
        {
            return new Greeting()
            {
                Phrase = ParseXmlEntryString(entry.FormPhrase, entry.Attributes),
                Description = ParseXmlEntryString(entry.FormDescription, entry.Attributes) ?? ""
            };
        }

        internal string ParseXmlEntryString(string str, Dictionary<string, string> attributes)
        {
            if (attributes == null || attributes.Count == 0) return str;

            foreach (var kvp in attributes)
                str = str.Replace("{" + kvp.Key + "}", kvp.Value);

            return str;
        }

        internal Greeting ParseDynamicGreeting(Greeting greeting, IDictionary<string, TagResolver> resolvers)
        {
            Greeting result = new Greeting() { Phrase = greeting.Phrase, Description = greeting.Description };

            if (resolvers == null || resolvers.Count == 0) return result;

            foreach (var kvp in resolvers)
            {
                try
                {
                    result.Phrase = result.Phrase.Replace("{" + kvp.Key + "}", kvp.Value(kvp.Key));
                }
                catch { }
                try
                {
                    result.Description = result.Description.Replace("{" + kvp.Key + "}", kvp.Value(kvp.Key));
                }
                catch { }
            }

            return result;
        }
    }


    public class Greeting
    {
        public string Phrase { get; set; }
        public string Description { get; set; }
    }
}
