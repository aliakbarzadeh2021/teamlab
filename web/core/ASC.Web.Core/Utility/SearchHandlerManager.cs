using System;
using System.Collections;
using ASC.Web.Core.ModuleManagement.Common;
using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core.Utility
{
    public static class SearchHandlerManager
    {
        private static Hashtable _searchHandlers = Hashtable.Synchronized(new Hashtable());
        private static object _syncObj = new object();

        public static void Registry(ISearchHandler searchHandler)
        {
            lock (_syncObj)
            {
                if (!_searchHandlers.ContainsKey(searchHandler.GetType()))
                    _searchHandlers.Add(searchHandler.GetType(), searchHandler);
            }
        }

        public static void UnRegistry(ISearchHandler searchHandler)
        {
            lock (_syncObj)
            {
                if (_searchHandlers.ContainsKey(searchHandler.GetType()))
                    _searchHandlers.Remove(searchHandler.GetType());
            }
        }

        public static List<ISearchHandler> HandlerCollection
        {
            get 
            {
                var collection = new List<ISearchHandler>();
                lock (_syncObj)
                {
                    foreach (ISearchHandler sh in _searchHandlers.Values)
                        collection.Add(sh);
                    
                }
                return collection;
            }
        }

        public static List<ISearchHandlerEx> HandlerExCollection
        {
            get
            {
                var collection = new List<ISearchHandlerEx>();
                lock (_syncObj)
                {
                    foreach (ISearchHandler sh in _searchHandlers.Values)
                    {
                        if (sh is ISearchHandlerEx)
                            collection.Add((ISearchHandlerEx)sh);
                    }

                }
                return collection;
            }
        }

        public static ISearchHandlerEx GetActiveHandlerEx()
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return null;

            string cur_vp = HttpContext.Current.Request.CurrentExecutionFilePath.TrimEnd('/');

            ISearchHandlerEx result = null;
            int matchCount = int.MaxValue;
            foreach (var sh in HandlerExCollection)
            {
                foreach (var virtualPath in sh.PlaceVirtualPath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var vp = VirtualPathUtility.ToAbsolute(virtualPath).TrimEnd('/');
                    if (cur_vp.IndexOf(vp, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        int diff = cur_vp.Length - vp.Length;
                        if (diff == 0)
                            return sh;

                        else if (matchCount > diff)
                        {
                            matchCount = diff;
                            result = sh;
                        }
                    }
                }
            }

            return result;
        }

        public static List<ISearchHandlerEx> GetHandlersExForProduct(Guid productID)
        {
            List<ISearchHandlerEx> handlers = new List<ISearchHandlerEx>();
            foreach (var sh in HandlerExCollection)
            {
                if(sh.ProductID.Equals(productID) || sh.ProductID.Equals(Guid.Empty))
                    handlers.Add(sh);
            }

            return handlers;
        }
    }

    public abstract class BaseSearchHandler : ISearchHandler, IDisposable
    {
        public BaseSearchHandler()
        {
            SearchHandlerManager.Registry(this);
        }
       
        #region IDisposable Members

        public void Dispose()
        {
            SearchHandlerManager.UnRegistry(this);
        }

        #endregion

        #region ISearchHandler Members

        public abstract SearchResultItem[] Search(string text);

        #endregion
    }

    public abstract class BaseSearchHandlerEx : BaseSearchHandler, ISearchHandlerEx
    {        
        #region ISearchHandlerEx Members

        public abstract ImageOptions Logo{get;}
       
        public abstract string SearchName{get;}

        public virtual string AbsoluteSearchURL { get; set; }

        public abstract string PlaceVirtualPath{get;}

        public virtual Guid ProductID { get { return Guid.Empty; } }

        public virtual Guid ModuleID { get { return Guid.Empty; } }

        public BaseSearchHandlerEx()
        {
            this.AbsoluteSearchURL = "";
        }

        #endregion
    }
}
