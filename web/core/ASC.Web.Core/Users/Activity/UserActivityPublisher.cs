using System;
using System.Collections;

namespace ASC.Web.Core.Users.Activity
{
    public static class UserActivityPublisher
    {
        private static Hashtable _publishers = Hashtable.Synchronized(new Hashtable());

        internal static void Registry(BaseUserActivityPublisher publisher)
        {
            lock (_publishers)
            {
                if (!_publishers.ContainsKey(publisher.GetType()))
                    _publishers.Add(publisher.GetType(), publisher);
            }
        }

        internal static void UnRegistry(BaseUserActivityPublisher publisher)
        {
            lock (_publishers)
            {
                if (_publishers.ContainsKey(publisher.GetType()))
                    _publishers.Remove(publisher.GetType());
            }
        }

        public static void Publish<T>(UserActivity userActivity) where T : BaseUserActivityPublisher
        { 
            Type publisherType = typeof(T);
            if (_publishers.Contains(publisherType) && _publishers[publisherType]!=null)
                (_publishers[publisherType] as BaseUserActivityPublisher).Publish(userActivity);
        }

        public static void Publish(Type publisherType, UserActivity userActivity)
        {   
            if (_publishers.Contains(publisherType) && _publishers[publisherType] != null)
                (_publishers[publisherType] as BaseUserActivityPublisher).Publish(userActivity);
        }
    }

    public abstract class BaseUserActivityPublisher : IUserActivityPublisher, IDisposable
    {
        public BaseUserActivityPublisher()
        {
            UserActivityPublisher.Registry(this);
        }

        public virtual void Publish(UserActivity userActivity)
        {
            if (this.DoUserActivity != null)
                this.DoUserActivity(this, new UserActivityEventArgs() { UserActivity = userActivity });
        }

        #region IUserActivityPublisher Members

        public virtual event EventHandler<UserActivityEventArgs> DoUserActivity;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            UserActivityPublisher.UnRegistry(this);
        }

        #endregion
    }
}
