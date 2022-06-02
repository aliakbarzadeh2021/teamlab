using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ASC.FullTextIndex
{
    [DataContract]
    public class TextSearchResult
    {
        [DataMember]
        public IDictionary<string, List<string>> results = new Dictionary<string, List<string>>();

        [DataMember]
        private IDictionary<string, string> texts = new Dictionary<string, string>();

        [DataMember]
        public string Module
        {
            get;
            private set;
        }


        public TextSearchResult(string module)
        {
            Module = module;
        }


        public IEnumerable<string> GetIdentifiers()
        {
            return results.Keys.ToList();
        }

        public string GetText(string identifier)
        {
            return texts.ContainsKey(identifier) ? texts[identifier] : null;
        }

        public IEnumerable<string> GetIdentifierDetails(string identifier)
        {
            return results.ContainsKey(identifier) ?
                results[identifier] :
                new List<string>();
        }

        public void AddIdentifier(string identifier)
        {
            AddIdentifier(identifier, null);
        }

        public void AddIdentifier(string identifier, string text)
        {
            var owner = identifier;
            var id = identifier;
            if (identifier.Contains("/"))
            {
                var split = identifier.Split('/');
                id = split[0];
                owner = split[1];
            }
            if (!results.ContainsKey(owner))
            {
                results[owner] = new List<string>();
            }
            results[owner].Add(id);

            if (!string.IsNullOrEmpty(text))
            {
                texts[identifier] = text;
            }
        }


    }
}
