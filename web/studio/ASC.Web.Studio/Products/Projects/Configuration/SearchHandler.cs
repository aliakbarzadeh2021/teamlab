using System;
using System.Web;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects.Configuration
{

    public class SearchHandler : BaseSearchHandlerEx
    {
        public override Guid ProductID
        {
            get { return new Guid("{1E044602-43B5-4d79-82F3-FD6208A11960}"); }
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions() { ImageFileName = "common_search_icon.png" }; }
        }

        public override string SearchName
        {
            get { return ProjectsCommonResource.SearchText; }
        }

        public override string AbsoluteSearchURL
        {
            get { return VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "/search.aspx"); }
        }

        public override string PlaceVirtualPath
        {
            get { return PathProvider.BaseVirtualPath; }
        }

        public override SearchResultItem[] Search(string text)
        {
            return new SearchResultItem[] { };
        }
    }
}