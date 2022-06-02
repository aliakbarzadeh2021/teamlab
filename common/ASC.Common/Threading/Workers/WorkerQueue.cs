#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;

#endregion

namespace ASC.Common.Threading.Workers
{
    public class WorkerQueuePersistent<T> : WorkerQueue<T>
    {
        public WorkerQueuePersistent(int workerCount, TimeSpan waitInterval)
            : base(workerCount, waitInterval)
        {
        }

        public WorkerQueuePersistent(int workerCount, TimeSpan waitInterval, int errorCount, bool stopAfterFinsih)
            : base(workerCount, waitInterval, errorCount, stopAfterFinsih)
        {
        }

        protected override WorkItem<T> Selector()
        {
            return Items.Where(x => !x.IsProcessed).Where(x => !x.IsCompleted).OrderBy(x => x.Added).FirstOrDefault();
        }

        protected override void PostComplete(WorkItem<T> item)
        {
            item.Completed = DateTime.Now;
            item.IsCompleted = true;
        }

        protected override void ErrorLimit(WorkItem<T> item)
        {
            PostComplete(item);
        }

    }

    public class WorkerSet<T> : WorkerQueue<T>
    {
        private readonly HashSet<WorkItem<T>> _set = new HashSet<WorkItem<T>>();

        public override ICollection<WorkItem<T>> Items
        {
            get
            {
                return _set;
            }
        }

        public WorkerSet(int workerCount, TimeSpan waitInterval)
            : base(workerCount, waitInterval)
        {
        }

        public WorkerSet(int workerCount, TimeSpan waitInterval, int errorCount, bool stopAfterFinsih)
            : base(workerCount, waitInterval, errorCount, stopAfterFinsih)
        {
        }

    }

    public class WorkerQueue<T>
    {
        private readonly AutoResetEvent _emptyEvent = new AutoResetEvent(false);
        private readonly int _errorCount;
        private readonly ICollection<WorkItem<T>> _items = new List<WorkItem<T>>();
        private readonly bool _stopAfterFinsih;

        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly AutoResetEvent _waitEvent = new AutoResetEvent(false);
        private readonly int _waitInterval;
        private int _workerCount;
        private readonly List<Thread> _workerThreads = new List<Thread>();
        private Action<T> _action;
        private bool _isThreadSet;
        private ILog _log = Utils.LogHolder.Log("ASC.WorkerQueue");

        public object SynchRoot { get { return Items; } }

        public WorkerQueue(int workerCount, TimeSpan waitInterval)
            : this(workerCount, waitInterval, 1, false)
        {
        }

        public WorkerQueue(int workerCount, TimeSpan waitInterval, int errorCount, bool stopAfterFinsih)
        {
            WorkerCount = workerCount;
            _errorCount = errorCount;
            _stopAfterFinsih = stopAfterFinsih;
            _waitInterval = (int)waitInterval.TotalMilliseconds;
        }

        private WaitHandle[] WaitObjects()
        {
            return new WaitHandle[] { _stopEvent, _waitEvent };
        }

        public bool IsStarted { get; set; }


        public int WorkerCount
        {
            get { return _workerCount; }
            set
            {
                if (value != _workerCount)
                {
                    _workerCount = value;

                    //Do a restart
                    if (_isThreadSet && _action != null)
                    {
                        Stop();
                        Start(_action);
                    }
                }
            }
        }

        public virtual ICollection<WorkItem<T>> Items
        {
            get { return _items; }
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            lock (Items)
            {
                foreach (var item in items)
                {
                    Items.Add(new WorkItem<T>(item));
                }
            }
            _waitEvent.Set();
            ReviveThreads();
        }

        public virtual void Add(T item)
        {
            lock (Items)
            {
                Items.Add(new WorkItem<T>(item));
            }
            _waitEvent.Set();
            ReviveThreads();
        }

        private void ReviveThreads()
        {
            if (_workerThreads.Count != 0)
            {
                bool haveLiveThread = _workerThreads.Count(x => x.IsAlive) > 0;
                if (!haveLiveThread)
                {
                    Restart();
                }
            }
        }

        private void Restart()
        {
            Stop();
            Start(_action);
        }

        public void Remove(T item)
        {
            lock (Items)
            {
                WorkItem<T> existing = Items.Where(x => Equals(x.Item, item)).SingleOrDefault();
                RemoveInternal(existing);
            }
        }

        public IEnumerable<T> GetItems()
        {
            lock (Items)
            {
                return Items.Select(x => x.Item).ToList();
            }
        }

        public void Start(Action<T> starter)
        {
            lock (Items)
            {
                _action = starter;
            }
            if (!_isThreadSet)
            {
                _log.DebugFormat("Creating threads");
                _isThreadSet = true;
                for (int i = 0; i < WorkerCount; i++)
                {
                    _workerThreads.Add(new Thread(DoWork) { Name = "queue_worker_" + (i + 1) });
                }
            }
            if (!IsStarted)
            {
                IsStarted = true;
                _stopEvent.Reset();
                _waitEvent.Reset();
                _log.DebugFormat("Starting threads");
                foreach (Thread workerThread in _workerThreads)
                {
                    workerThread.Start(_stopAfterFinsih);
                }
            }
        }

        public void WaitForCompletion()
        {
            _emptyEvent.WaitOne();
        }

        public void Terminate()
        {
            if (IsStarted)
            {
                IsStarted = false;
                _stopEvent.Set();
                _waitEvent.Set();
                
                _log.DebugFormat("Stoping queue. Terminating threads");
                foreach (Thread workerThread in
                    _workerThreads.Where(workerThread => workerThread != Thread.CurrentThread))
                {
                    workerThread.Abort();
                }
                _isThreadSet = false;
                if (_workerThreads.Contains(Thread.CurrentThread))
                {
                    _workerThreads.Clear();
                    _log.DebugFormat("Terminate called from current worker thread. Terminating");
                    Thread.CurrentThread.Abort();
                }
                _workerThreads.Clear();
                _log.DebugFormat("Queue stoped. Threads cleared");
            }
        }

        public void Stop()
        {
            if (IsStarted)
            {
                IsStarted = false;
                _stopEvent.Set();
                _waitEvent.Set();

                _log.DebugFormat("Stoping queue. Joining threads");
                foreach (Thread workerThread in _workerThreads)
                {
                    workerThread.Join();
                }
                _isThreadSet = false;
                _workerThreads.Clear();
                _log.DebugFormat("Queue stoped. Threads cleared");
            }
        }

        protected virtual WorkItem<T> Selector()
        {
            return Items.Where(x => !x.IsProcessed).OrderBy(x => x.Added).FirstOrDefault();
        }

        protected virtual void PostComplete(WorkItem<T> item)
        {
            item.Completed = DateTime.Now;
            RemoveInternal(item);
        }

        protected void RemoveInternal(WorkItem<T> item)
        {
            if (item != null)
            {
                Items.Remove(item);
                item.Dispose();
            }
        }

        protected virtual void ErrorLimit(WorkItem<T> item)
        {
            RemoveInternal(item);
        }

        protected virtual void Error(WorkItem<T> item, Exception exception)
        {
            item.Error = exception;
            item.IsProcessed = false;
            item.Added = DateTime.Now;
        }

        private void DoWork(object state)
        {
            bool stopAfterFinsih = false;
            if (state != null && state is bool)
            {
                stopAfterFinsih = (bool)state;
            }
            do
            {
                WorkItem<T> item;
                Action<T> localAction;
                lock (Items)
                {
                    localAction = _action;
                    item = Selector();
                    if (item != null)
                    {
                        item.IsProcessed = true;
                    }
                }
                if (localAction == null)
                    break;//Exit if action is null

                if (item != null)
                {
                    try
                    {
                        localAction(item.Item);
                        bool fallSleep = false;
                        lock (Items)
                        {
                            PostComplete(item);
                            if (Items.Count == 0)
                            {
                                _emptyEvent.Set();
                                fallSleep = QueueEmpty(true);
                            }
                        }
                        if (fallSleep)
                        {
                            if (WaitHandle.WaitAny(WaitObjects(), Timeout.Infinite, false) == 0 || stopAfterFinsih)
                            {
                                break;
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception e)
                    {
                        lock (Items)
                        {
                            Error(item, e);
                            item.ErrorCount++;
                            if (item.ErrorCount > _errorCount)
                            {
                                ErrorLimit(item);
                            }
                        }
                    }
                }
                else
                {
                    if (WaitHandle.WaitAny(WaitObjects(), GetSleepInterval(), false) == 0 || stopAfterFinsih)
                    {
                        break;
                    }
                }
            } while (true);
        }

        protected virtual bool QueueEmpty(bool fallAsleep)
        {
            return fallAsleep;
        }

        protected virtual int GetSleepInterval()
        {
            return _waitInterval;
        }

        public void Clear()
        {
            lock (Items)
            {
                foreach (var workItem in Items)
                {
                    workItem.Dispose();
                }
                Items.Clear();
            }
        }
    }
}