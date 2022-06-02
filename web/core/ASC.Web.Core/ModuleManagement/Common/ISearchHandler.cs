using System;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core.ModuleManagement.Common
{
    public class SearchResultItem
    {
        /// <summary>
        /// Absolute URL
        /// </summary>
        public string URL { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public interface ISearchHandler
    {
        /// <summary>
        /// Do search
        /// </summary>
        /// <param name="text">Search text</param>
        /// <returns>If nothing found - empty array</returns>
        SearchResultItem[] Search(string text);
    }

    public interface ISearchHandlerEx : ISearchHandler
    {
        /// <summary>
        /// Module search paths. May be more than one. 
        /// </summary>
        string PlaceVirtualPath { get; }

        Guid ProductID { get; }

        Guid ModuleID { get; }

        /// <summary>
        /// Interface log 
        /// </summary>
        ImageOptions Logo { get; }

        /// <summary>
        /// Search display name
        /// <remarks>Ex: "forum search"</remarks>
        /// </summary>
        string SearchName { get; }

        /// <summary>
        /// Concrete search page url 
        /// </summary>        
        /// <remarks>search - url parameter name with search text</remarks>
        string AbsoluteSearchURL{get;}
    }
}
