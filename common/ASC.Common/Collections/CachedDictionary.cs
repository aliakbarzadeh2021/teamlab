using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace ASC.Collections
{
    public abstract class CachedDictionaryBase<T>
    {
        protected string _baseKey;
        protected Func<T, bool> _cacheCodition;
        protected Guid _clearId = Guid.NewGuid();

        public T this[string key]
        {
            get { return Get(key); }
        }

        public T this[Func<T> @default]
        {
            get { return Get(@default); }
        }

        protected abstract void InsertRootKey(string rootKey);

        public void Clear()
        {
            _clearId = Guid.NewGuid();
            InsertRootKey(_baseKey);
        }

        public void Clear(string rootKey)
        {
            _clearId = Guid.NewGuid();
            InsertRootKey(BuildKey(string.Empty, rootKey));
        }

        public void Reset(string key)
        {
            Reset(string.Empty, key);
        }

        public abstract void Reset(string rootKey, string key);

        public T Get(string key)
        {
            return Get(string.Empty, key, null);
        }

        public T Get(string key, Func<T> defaults)
        {
            return Get(string.Empty, key, defaults);
        }


        public void Add(string key, T newValue)
        {
            Add(string.Empty, key, newValue);
        }

        public abstract void Add(string rootkey, string key, T newValue);

        public bool HasItem(string key)
        {
            return !Equals(Get(key), default(T));
        }

        protected string BuildKey(string key, string rootkey)
        {
            return string.Format("{0}-{1}-{2}", _baseKey, rootkey, key);
        }

        protected abstract object GetObjectFromCache(string fullKey);

        public T Get(Func<T> @default)
        {
            string key = string.Format("func {0} {2}.{1}({3})", @default.Method.ReturnType, @default.Method.Name,
                                       @default.Method.DeclaringType.FullName,
                                       string.Join(",",
                                                   @default.Method.GetGenericArguments().Select(x => x.FullName).ToArray
                                                       ()));
            return Get(key, @default);
        }

        protected virtual bool FitsCondition(object cached)
        {
            return cached != null && cached is T;
        }

        public virtual T Get(string rootkey, string key, Func<T> defaults)
        {
            string fullKey = BuildKey(key, rootkey);
            object objectCache = GetObjectFromCache(fullKey);
            if (FitsCondition(objectCache))
            {
#if (DEBUG)
                OnHit(fullKey);
#endif
                return ReturnCached(objectCache);
            }
            if (defaults != null)
            {
#if (DEBUG)
                OnMiss(fullKey);
#endif
                T newValue = defaults();
                if (_cacheCodition==null || _cacheCodition(newValue))
                {
                    Add(rootkey, key, newValue);
                }
                return newValue;
            }
            return default(T);
        }

        protected virtual T ReturnCached(object objectCache)
        {
            return (T)objectCache;
        }

        protected virtual void OnHit(string fullKey)
        {
            Debug.Print("cache hit:{0}", fullKey);
        }

        protected virtual void OnMiss(string fullKey)
        {
            Debug.Print("cache miss:{0}",fullKey);
        }
    }

    public sealed class CachedDictionary<T> : CachedDictionaryBase<T>
    {
        private readonly DateTime _absoluteExpiration;
        private readonly TimeSpan _slidingExpiration;

        public CachedDictionary(string baseKey, DateTime absoluteExpiration, TimeSpan slidingExpiration,
                                Func<T, bool> cacheCodition)
        {
            if (cacheCodition == null) throw new ArgumentNullException("cacheCodition");
            _baseKey = baseKey;
            _absoluteExpiration = absoluteExpiration;
            _slidingExpiration = slidingExpiration;
            _cacheCodition = cacheCodition;
            InsertRootKey(_baseKey);
        }

        public CachedDictionary(string baseKey)
            : this(baseKey, (x) => true)
        {
        }

        public CachedDictionary(string baseKey, Func<T, bool> cacheCodition)
            : this(baseKey, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, cacheCodition)
        {
        }

        protected override void InsertRootKey(string rootKey)
        {
#if (DEBUG)
            Debug.Print("inserted root key {0}", rootKey);
#endif
            HttpRuntime.Cache.Insert(rootKey, _clearId, null, _absoluteExpiration, _slidingExpiration,
                                     CacheItemPriority.NotRemovable, null);
        }

        public override void Reset(string rootKey, string key)
        {
            HttpRuntime.Cache[BuildKey(key, rootKey)] = null;
        }

        protected override object GetObjectFromCache(string fullKey)
        {
            return HttpRuntime.Cache.Get(fullKey);
        }

        public override void Add(string rootkey, string key, T newValue)
        {
            var builtrootkey = BuildKey(string.Empty, rootkey);
            if (HttpRuntime.Cache[builtrootkey] == null)
            {
#if (DEBUG)
                Debug.Print("added root key {0}",builtrootkey);
#endif
                //Insert root if no present
                HttpRuntime.Cache.Insert(builtrootkey, Guid.NewGuid(), null, _absoluteExpiration, _slidingExpiration,
                                         CacheItemPriority.NotRemovable, null);
            }
            CacheItemRemovedCallback removeCallBack = null;
#if (DEBUG)
            removeCallBack = ItemRemoved;
#endif

            HttpRuntime.Cache.Insert(BuildKey(key, rootkey), newValue, new CacheDependency(null, new[] { _baseKey, builtrootkey }),
                                     _absoluteExpiration, _slidingExpiration,
                                     CacheItemPriority.Normal, removeCallBack);
        }

        private static void ItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            Debug.Print("key: {0} removed. reason: {1}",key,reason);
        }
    }

    public sealed class HttpRequestDictionary<T> : CachedDictionaryBase<T>
    {
        private class CachedItem
        {
            internal T Value { get; set; }

            internal CachedItem(T value)
            {
                Value = value;
            }


        }

        public HttpRequestDictionary(string baseKey)
        {
            _cacheCodition = (T) => true;
            _baseKey = baseKey;
        }

        protected override void InsertRootKey(string rootKey)
        {
            //We can't expire in HtppContext in such way
        }

        public override void Reset(string rootKey, string key)
        {
            if (HttpContext.Current != null)
            {
                var builtkey = BuildKey(key, rootKey);
                HttpContext.Current.Items[builtkey] = null;
            }
        }

        public override void Add(string rootkey, string key, T newValue)
        {
            if (HttpContext.Current != null)
            {
                var builtkey = BuildKey(key, rootkey);
                HttpContext.Current.Items[builtkey] = new CachedItem(newValue);
            }
        }

        protected override object GetObjectFromCache(string fullKey)
        {
            return HttpContext.Current != null ? HttpContext.Current.Items[fullKey] : null;
        }

        protected override bool FitsCondition(object cached)
        {
            return cached is CachedItem;
        }
        protected override T ReturnCached(object objectCache)
        {
            return ((CachedItem) objectCache).Value;
        }

        protected override void OnHit(string fullKey)
        {
            Debug.Print("{0} http cache hit:{1}",HttpContext.Current.Request.Url,fullKey);
        }

        protected override void OnMiss(string fullKey)
        {
            Uri uri = null;
            if (HttpContext.Current!=null)
            {
                uri = HttpContext.Current.Request.Url;
            }
            Debug.Print("{0} http cache miss:{1}", uri==null?"no-context":uri.AbsolutePath, fullKey);
        }

    }
}