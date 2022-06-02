using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using ASC.Files.Core;
using ASC.Thrdparty;

namespace ASC.Web.Files.Import
{
    class BoxDocumentProvider : IDocumentProvider
    {
        private string authKey;

        private string AuthTicket
        {
            get { return TokenHolder.GetToken("box.net_auth_ticket"); }
            set { TokenHolder.AddToken("box.net_auth_ticket", value); }
        }

        public string Name
        {
            get { return "boxnet"; }
        }


        private void GetAuthTicket()
        {
            if (string.IsNullOrEmpty(AuthTicket)) throw new ArgumentException("No auth ticket");
            var url = string.Format("https://www.box.net/api/1.0/rest?action=get_auth_token&api_key={0}&ticket={1}",
                                    ImportConfiguration.BoxNetApiKey, AuthTicket);
            var response = XDocument.Load(url).Element("response");
            if (response == null)
            {
                throw new ArgumentException("Bad auth tiket responce");
            }

            if (response.ElementValueOrDefault("status") != "get_auth_token_ok")
            {
                throw new Exception("Bad auth token status: " + response.ElementValueOrDefault("status") + ".");
            }
            authKey = response.ElementValueOrDefault("auth_token");
        }

        public BoxDocumentProvider(string authKey)
        {
            if (string.IsNullOrEmpty(authKey))
            {
                //Get auth_ticket
                GetAuthTicket();
            }
            else
            {
                this.authKey = authKey;
            }
        }

        public IEnumerable<Document> GetDocuments()
        {
            var url = string.Format("https://www.box.net/api/1.0/rest?action=get_account_tree&api_key={0}&auth_token={1}&folder_id=0&params[]=nozip", ImportConfiguration.BoxNetApiKey, authKey);
            var response = XDocument.Load(url).Element("response");

            if (response == null)
            {
                throw new ArgumentException("Empty status responce");
            }

            if (response.Element("status").Value != "listing_ok")
            {
                throw new Exception("Bad listing status: " + response.ElementValueOrDefault("status") + ".");
            }

            var docs = new List<Document>();
            if (response.Element("tree") != null)
            {
                foreach (var f in response.Element("tree").Elements("folder"))
                {
                    RetrieveTree(f, docs);
                }
            }
            return docs;
        }

        public Stream GetDocumentStream(string contentLink, out long size)
        {
            var request = WebRequest.Create("https://www.box.net/api/1.0/download/" + authKey + "/" + contentLink);
            var responce = request.GetResponse();

            if (responce == null) throw new IOException("Responce stream empty");

            size = responce.ContentLength;
            return responce.GetResponseStream();
        }


        private void RetrieveTree(XElement root, List<Document> result)
        {
            if (root.Element("folders") != null)
            {
                foreach (var f in root.Element("folders").Elements("folder"))
                {
                    if (f.Attribute("id") != null)
                    {
                        var entry = new Document
                                        {
                                            Id = f.AttributeValueOrDefault("id"),
                                            Title = f.AttributeValueOrDefault("name", "noname"),
                                            Parent = root.AttributeValueOrDefault("id"),
                                            IsFolder = true,
                                        };
                        result.Add(entry);
                    }
                    RetrieveTree(f, result);
                }
            }
            if (root.Element("files") != null)
            {
                result.AddRange(root.Element("files")
                    .Elements("file")
                    .Where(f => f.Attribute("id") != null)
                    .Select(f => new Document
                             {
                                 Id = f.AttributeValueOrDefault("id"),
                                 ContentLink = f.AttributeValueOrDefault("id"),
                                 Title = f.AttributeValueOrDefault("file_name"),
                                 Parent = root.AttributeValueOrDefault("id"),
                                 CreateOn = Utils.FromUnixTime2(long.Parse(f.AttributeValueOrDefault("created"))),
                                 IsFolder = false,
                             }).Where(entry => FileFormats.IsSupported(entry.Title)));
            }
        }
    }
}