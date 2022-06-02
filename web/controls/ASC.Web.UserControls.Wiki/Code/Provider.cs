using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.UserControls.Wiki
{
    public class Provider
    {
        protected static string _connectionStringName = WikiSection.Section != null ? WikiSection.Section.DB.ConnectionStringName : null;
        public static void SetConnectionStringName(string conn)
        {
            if(string.IsNullOrEmpty(_connectionStringName) && !string.IsNullOrEmpty(conn))
            {
                _connectionStringName = conn;
            }
        }
        protected static void InitWikiDao(WikiDAO wikiDAO, int tenantId)
        {
            if (_connectionStringName == null && WikiSection.Section != null)
            {
                _connectionStringName = WikiSection.Section.DB.ConnectionStringName;
            }
            wikiDAO.ConnectionStringName = _connectionStringName;
            wikiDAO.InitDAO(tenantId);
        }
    }
}
