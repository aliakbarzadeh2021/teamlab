using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.UserControls.Wiki.Data;
using System.Text.RegularExpressions;
using System.Linq;

namespace ASC.Web.UserControls.Wiki
{
    public class PagesProvider : Provider
    {
        public static Pages PagesHistGetByNameVersion(string pageName, int version, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesHistGetByNameVersion(pageName, version);
            }
        }

        public static Pages PagesGetByName(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesGetByName(pageName);
            }
        }

        public static List<Pages> PagesGetTopNew(int maxValue, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return ClearSpecialPages(wikiDAO.PagesGetTopNew(maxValue));
            }
        }

        public static List<Pages> PagesGetTopNewAndUserCreated(int maxValue, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return ClearSpecialPages(wikiDAO.PagesGetTopNewAndUserCreated(maxValue));
            }
        }

        public static List<Pages> PagesGetTopFresh(int maxValue, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return ClearSpecialPages(wikiDAO.PagesGetTopFresh(maxValue));
            }
        }


        public static List<Pages> PagesHistGetAllByName(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesHistGetAllByName(pageName);
            }
        }

        public static Pages PagesHistSave(Pages page, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesHistSave(page);
            }
        }

        public static int PagesHistGetMaxVersion(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesHistGetMaxVersion(pageName);
            }
        }




        public static Pages PagesSave(Pages page, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesSave(page);
            }
        }

        /// <summary>
        /// Get All Pages without Body Content withoutSpecialPages
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns>List of Pages</returns>
        public static List<Pages> PagesGetAll(int tenantId)
        {
            return PagesGetAll(tenantId, false);
        }

        public static List<Pages> PagesGetAll(int tenantId, bool withinBodyContent)
        {
            return PagesGetAll(tenantId, withinBodyContent, true);
        }

        public static List<Pages> PagesGetAll(int tenantId, bool withinBodyContent, bool excludeSpecialPages)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                List<Pages> result;
                InitWikiDao(wikiDAO, tenantId);
                if (withinBodyContent)
                {
                    result = wikiDAO.PagesGetAllWithBody();
                }
                else
                {
                    result = wikiDAO.PagesGetAll();
                }

                if(excludeSpecialPages)
                {
                    result = ClearSpecialPages(result);
                }

                return result;
            }
        }



        public static List<Pages> PagesGetByStartName(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return ClearSpecialPages(wikiDAO.PagesGetByStartName(pageName));
            }

        }

        public static List<Pages> PagesSearchByName(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return ClearSpecialPages(wikiDAO.PagesSearchByName(pageName));
            }

        }

        public static List<Pages> PagesGetByCreatedUserId(Guid userID, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesGetByCreatedUserId(userID);
            }

        }

        public static int PagesGetCountByCreatedUserId(Guid userID, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.PagesGetCountByCreatedUserId(userID);
            }

        }


        public static List<Pages> PagesSearchAllByContentEntry(string content, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                if (!string.IsNullOrEmpty(content))
                    return wikiDAO.PagesSearchAllByContentEntry(content);
                return wikiDAO.PagesGetAll();
            }

        }

        public static List<Pages> PagesGetAllByCategoryName(string categoryName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return ClearSpecialPages(wikiDAO.PagesGetAllByCategoryName(categoryName));
            }

        }


        public static List<Files> FilesGetByStartName(string FileName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.FilesGetByStartName(FileName);
            }

        }


        public static void PagesDelete(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                wikiDAO.PagesDelete(pageName);
            }
        }


        public static Files FilesGetByName(string fileName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.FilesGetByName(fileName);
            }
        }

        public static Files FilesSave(Files file, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.FilesSave(file);
            }
        }

        public static List<Files> FilesGetAll(int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.FilesGetAll();
            }
        }



        public static void FilesDelete(string fileName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                wikiDAO.FilesDelete(fileName);
            }
        }

        internal static string RegExCategorySearch = string.Format(@"\[\[{0}:([^\|^\]]+)(\|[^]]+)*\]\]", Constants.WikiCategoryKeyCaption);

        public static List<string> UpdateCategoriesByPageContent(Pages page, int tenantId)
        {
            List<string> result = new List<string>();

            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);

                Regex catReg = new Regex(RegExCategorySearch, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                List<Categories> oldCategories = wikiDAO.CategoriesGetAllByPageName(page.PageName);
                wikiDAO.CategoryDeleteAllByPageName(page.PageName);
                string categoryName;
                string bodyWikiOnly = HtmlWikiUtil.rexNoSection.Replace(page.Body, string.Empty);
                foreach (Match m in catReg.Matches(bodyWikiOnly))
                {
                    categoryName = PageNameUtil.NormalizeNameCase(PageNameUtil.NormalizeNameCase(m.Groups[1].Value.Trim()));
                    if (!oldCategories.Exists(oc => oc.CategoryName.Equals(categoryName, StringComparison.InvariantCulture)))
                    {
                        result.Add(categoryName);
                    }
                    wikiDAO.CategorySave(new Categories() { PageName = page.PageName, CategoryName = categoryName });
                }


            }

            return result;

        }

        private static RegexOptions mainOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private static Regex rgxLinks = new Regex(@"\[([\s\S]*?)\]", mainOptions);
        public static List<string> GetExistingPagesFilesListByBody(string wikiBody, int tenantId)
        {
            List<string> existingPages = new List<string>();
            List<string> existingFiles = new List<string>();
            List<string> result = new List<string>();
            string sLink;
            LinkType lType;
            //Getting all links at the Body
            foreach (Match m in rgxLinks.Matches(wikiBody))
            {
                lType = CheckTheLink(m.Groups[1].Value, out sLink);
                if (lType == LinkType.Page && !existingPages.Exists(s => s.Equals(sLink)))
                {
                    existingPages.Add(sLink);
                }
                else if (lType == LinkType.File && !existingFiles.Exists(s => s.Equals(sLink)))
                {
                    existingFiles.Add(sLink);
                }
            }

            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);

                if (existingPages.Count > 0)
                {
                    foreach (Pages _page in wikiDAO.PagesGetByList(existingPages))
                    {
                        result.Add(_page.PageName);
                    }
                }

                if (existingFiles.Count > 0)
                {
                    foreach (Files _file in wikiDAO.FilesGetByList(existingFiles))
                    {
                        result.Add(_file.FileName);
                    }
                }
            }


            return result;
        }

        private static List<Pages>ClearSpecialPages(List<Pages> source)
        {
            List<Pages> result = new List<Pages>();
            var clearResult = from page in source
                              where !(page.PageName.Contains(':') && BaseUserControl.reservedPrefixes.Any(rp => page.PageName.StartsWith(rp, StringComparison.InvariantCultureIgnoreCase)))
                              select page;
            result.AddRange(clearResult.ToArray());
            return result;
        }

        private static LinkType CheckTheLink(string str, out string sLink)
        {
            sLink = string.Empty;

            if (string.IsNullOrEmpty(str))
                return LinkType.None;

            if (str[0] == '[') //Internal Link
            {
                sLink = str.Trim("[]".ToCharArray()).Split('|')[0].Trim();
            }
            else if (str.StartsWith("image:", StringComparison.InvariantCultureIgnoreCase) || str.StartsWith("file:", StringComparison.InvariantCultureIgnoreCase)) //Process user error like [image:test.png]
            {
                sLink = str.Split('|')[0].Trim();
            }
            sLink = sLink.Split('#')[0].Trim(); //Trim anchors
            if (string.IsNullOrEmpty(str))
                return LinkType.None;

            if (sLink.Contains(":"))
            {
                if ((sLink.StartsWith("image:", StringComparison.InvariantCultureIgnoreCase) ||
                                        sLink.StartsWith("file:", StringComparison.InvariantCultureIgnoreCase)))
                {
                    sLink = sLink.Split(':')[1];
                    return LinkType.File;
                }
                else
                {
                    if (HtmlWikiUtil.IsSpetialExists(sLink))
                    {
                        sLink = string.Empty; //Special link or some thing else
                        return LinkType.None;
                    }
                }
            }

            return LinkType.Page;
        }
    }

    enum LinkType
    {
        None = 0,
        Page,
        File
    }
}
