using System;

namespace ASC.Core.Caching
{
    public class TrustInterval
    {
        private TimeSpan interval;


        public DateTime StartTime
        {
            get;
            private set;
        }

        public bool Expired
        {
            get { return interval == default(TimeSpan) || interval < (DateTime.UtcNow - StartTime).Duration(); }
        }


        public void Start(TimeSpan interval)
        {
            this.interval = interval;
            StartTime = DateTime.UtcNow;
        }

        public void Expire()
        {
            interval = default(TimeSpan);
        }
    }
}
