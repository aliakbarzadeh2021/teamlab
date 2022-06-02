using System;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;

namespace ASC.Web.Files.Configuration
{
    public class SearchHandler : BaseSearchHandlerEx
    {
        public override Guid ProductID
        {
			get { return ProductEntryPoint.ID; }
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions() { ImageFileName = "common_search_icon.png" }; }
        }

        public override string SearchName
        {
            get { return FilesCommonResource.SearchText; }
        }

        public override string AbsoluteSearchURL
        {
            get;
            set;
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