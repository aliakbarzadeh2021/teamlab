using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.UserControls.Wiki
{
    public class CategoriesProvider : Provider
    {
        public static List<Categories> CategoriesGetAll(int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CategoriesGetAll();
            }
        }

        public static List<Categories> CategoriesSelectCategoriesWillBeDeletedAtAllByPageNam(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CategoriesSelectCategoriesWillBeDeletedAtAllByPageNam(pageName);
            }
        }

        public static void CategoryDeleteByPageCategoryName(string pageName, string categoryName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                wikiDAO.CategoryDeleteByPageCategoryName(pageName, categoryName);
            }
        }

        public static void CategoryDeleteAllByPageName(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                wikiDAO.CategoryDeleteAllByPageName(pageName);
            }
        }
        
        

        
    }
}
