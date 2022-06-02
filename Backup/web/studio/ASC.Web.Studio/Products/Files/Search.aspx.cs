using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Controls;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Files
{
    public partial class Search : BasePage
    {
        protected int NumberOfStaffFound;
        protected String SearchText = HttpContext.Current.Request["search"];


        protected override void PageLoad()
        {

            Title = HeaderStringHelper.GetPageTitle(FilesCommonResource.ModuleName, Master.BreadCrumbs);
            Master.DisabledSidePanel = true;
            Master.CommonContainerHeaderVisible = true;
            Master.BreadCrumbs.Add(new BreadCrumb(FilesCommonResource.ModuleName, VirtualPathUtility.ToAbsolute(PathProvider.StartURL())));
            Master.BreadCrumbs.Add(new BreadCrumb(HeaderStringHelper.GetHTMLSearchHeader(SearchText)));

            EmployeesSearch();
            ContentSearch();
        }

        protected void EmployeesSearch()
        {
            var users = CoreContext.UserManager.Search(SearchText, EmployeeStatus.Active);
            if (users.Length == 0) return;

            NumberOfStaffFound = users.Length;
            EmployeesSearchRepeater.DataSource = NumberOfStaffFound > 5 ? users.Take(5) : users;
            EmployeesSearchRepeater.DataBind();
        }

        protected void ContentSearch()
        {
            var security = Global.GetFilesSecurity();
            using (var folderDao = Global.DaoFactory.GetFolderDao())
            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var files = fileDao.Search(SearchText, FolderType.USER | FolderType.COMMON)
                    .Where(f => security.CanRead(f.Key))
                    .Select(r => new SearchItem
                    {
                        ID = r.Key.ID,
                        FileTitle = r.Key.Title ?? string.Empty,
                        Body = r.Value ?? string.Empty,
                        Owner = CoreContext.UserManager.GetUsers(r.Key.CreateBy).DisplayUserName() ?? string.Empty,
                        Uploaded = r.Key.ModifiedOn.ToString(DateTimeExtension.ShortDatePattern),
                        Size = FileUtility.ContentLengthToString(r.Key.ContentLength),
                        FolderPathPart = FolderPathBuilder(folderDao.GetParentFolders(r.Key.FolderID)),
                        ItemUrl = r.Key.ViewUrl,
                        IsFolder = false
                    });

                var folders = folderDao.Search(SearchText, FolderType.USER | FolderType.COMMON)
                    .Where(f => security.CanRead(f))
                    .Select(f => new SearchItem
                    {
                        ID = f.ID,
                        FileTitle = f.Title ?? string.Empty,
                        Body = string.Empty,
                        Owner = CoreContext.UserManager.GetUsers(f.CreateBy).DisplayUserName() ?? string.Empty,
                        Uploaded = f.ModifiedOn.ToString(DateTimeExtension.ShortDatePattern),
                        FolderPathPart = FolderPathBuilder(folderDao.GetParentFolders(f.ID)),
                        ItemUrl = VirtualPathUtility.ToAbsolute(string.Concat(PathProvider.BaseVirtualPath, "#", f.ID)),
                        IsFolder = true
                    });

                var result = folders.Concat(files).ToList();
                if (result.Count != 0)
                {
                    ContentSearchRepeater.DataSource = result;
                    ContentSearchRepeater.DataBind();
                }
            }
        }

        protected String GetShortenContent(String fileContent)
        {
            if (string.IsNullOrEmpty(fileContent)) return string.Empty;

            var startIndex = fileContent.IndexOf(SearchText);
            var endIndex = startIndex + SearchText.Length;
            var leftFraming = false;
            var rightFraming = false;

            if (startIndex == -1) return String.Empty;

            startIndex -= 50;

            if (startIndex <= 0) startIndex = 0;
            else leftFraming = true;

            if (endIndex + 50 > fileContent.Length)
            {
                endIndex = fileContent.Length;
            }
            else
            {
                endIndex += 50;
                rightFraming = true;
            }

            return String.Concat(leftFraming ? "..." : "", HtmlUtil.GetText(fileContent.Substring(startIndex, endIndex - startIndex)), rightFraming ? "..." : "");
        }

        private String FolderPathBuilder(List<Folder> folders)
        {
            var titles = folders.Select(f => f.Title).ToList();
            var format = "<span class='separator'>/</span>";
            return 4 < titles.Count ?
                string.Join(format, new[] { titles.First(), "...", titles.ElementAt(titles.Count - 2), titles.Last() }) :
                string.Join(format, titles.ToArray());
        }


        public class SearchItem
        {
            public int ID { get; set; }
            public string FileTitle { get; set; }
            public string Body { get; set; }
            public string Owner { get; set; }
            public string Uploaded { get; set; }
            public string Size { get; set; }
            public string FolderPathPart { get; set; }
            public string ItemUrl { get; set; }
            public bool IsFolder { get; set; }
        }
    }
}
