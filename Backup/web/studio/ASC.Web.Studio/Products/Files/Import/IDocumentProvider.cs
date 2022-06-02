﻿using System.Collections.Generic;
using System.IO;

namespace ASC.Web.Files.Import
{
    interface IDocumentProvider
    {
        string Name
        {
            get;
        }

        IEnumerable<Document> GetDocuments();

        Stream GetDocumentStream(string contentLink, out long size);
    }
}
