#if DEBUG
namespace ASC.Common.Tests.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NUnit.Framework;
    using ASC.Collections;
    using System.Threading;

    [TestFixture]
    public class EventQueue_Test
    {
        [Test]
        public void TestCompare()
        {
            Assert.AreEqual(
                    DateTime.Compare(DateTime.MinValue, DateTime.MaxValue),
                    EventQueue<DateTime>.ComparerMethod((dt, dt2) => dt, DateTime.MinValue, DateTime.MaxValue)
                );

            Assert.AreEqual(
                    -1,
                    EventQueue<DateTime>.ComparerMethod(
                        (dt, dt2) => dt == DateTime.MaxValue ? (Nullable<DateTime>)DateTime.MaxValue : null,
                        DateTime.MinValue,
                        DateTime.MaxValue)
                );
            Assert.AreEqual(
                    +1,
                    EventQueue<DateTime>.ComparerMethod(
                        (dt, dt2) => dt == DateTime.MinValue ? (Nullable<DateTime>)DateTime.MinValue : null,
                        DateTime.MinValue,
                        DateTime.MaxValue)
                );
            Assert.AreEqual(
                    0,
                    EventQueue<DateTime>.ComparerMethod(
                        (dt, dt2) => null,
                        DateTime.MinValue,
                        DateTime.MaxValue)
                );
        }

        [Test]
        public void EnqueuePastEvent()
        {
            EventQueue<int> q = new EventQueue<int>((i, d) => DateTime.MinValue);
            q.Enqueue(1);
            Assert.AreEqual(0, q.Count);
        }

        [Test]
        public void EnqueueNullEvent()
        {
            EventQueue<object> q = new EventQueue<object>((i, d) => DateTime.MaxValue);
            q.Enqueue(null);
            Assert.AreEqual(0, q.Count);
        }

        [Test]
        public void EnqueueTomorrowEvent()
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);
            EventQueue<object> q = new EventQueue<object>((i, d) => tomorrow);
            q.Enqueue(1);
            Assert.AreEqual(1, q.Count);
            Assert.AreEqual(tomorrow, q.FirstEventDate);
        }

        [Test]
        public void DequeueTomorrowEvent()
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);
            EventQueue<object> q = new EventQueue<object>((i, d) => tomorrow);
            q.Enqueue(1);

            Assert.IsNull(q.Dequeue());
            Assert.AreEqual(1, q.Count);
        }

        [Test]
        public void DequeuePeriodicEvent()
        {
            bool stop = false;
            EventQueue<object> q = new EventQueue<object>(
                (i, d) => (!stop) ? (DateTime?)DateTime.Now.AddMilliseconds(0.01) : null);
            q.Enqueue(1);

            Thread.Sleep(2);
            Assert.AreEqual(1, q.Dequeue());
            Assert.AreEqual(1, q.Count);

            Thread.Sleep(2);
            stop = true;
            Assert.AreEqual(1, q.Dequeue());
            Assert.AreEqual(0, q.Count);

        }

        class Evnt
        {
            public TimeSpan span;
        }

        [Test]
        public void ReadyCount()
        {
            TimeSpan everyMinuteSpan = new TimeSpan(0, 0, 1, 0); //раз в минуту
            TimeSpan everyDaySpan = new TimeSpan(1, 0, 0, 0); //раз в день
            DateTime enqDate = new DateTime(1983, 1, 19);

            EventQueue<Evnt> q = new EventQueue<Evnt>((evnt, from) => from + evnt.span);
            q.Enqueue(new Evnt { span = everyMinuteSpan }, enqDate);
            q.Enqueue(new Evnt { span = everyDaySpan }, enqDate);

            Assert.AreEqual(0, q.ReadyCountImpl(enqDate + new TimeSpan(0, 0, 0, 1)));
            Assert.AreEqual(1, q.ReadyCountImpl(enqDate + new TimeSpan(0, 0, 1, 1)));
            Assert.AreEqual(2, q.ReadyCountImpl(enqDate + new TimeSpan(1, 0, 1, 1)));
        }

        [Test]
        public void FirstEventSpan()
        {
            TimeSpan everyMinuteSpan = new TimeSpan(0, 0, 1, 0); //раз в минуту
            TimeSpan everyDaySpan = new TimeSpan(1, 0, 0, 0); //раз в день
            TimeSpan everyWeekSpan = new TimeSpan(7, 0, 0, 0); //раз в неделю
            DateTime enqDate = new DateTime(1983,1,19);
            DateTime? schDate;

            EventQueue<Evnt> q = new EventQueue<Evnt>((evnt, from) => from + evnt.span);
            q.Enqueue(new Evnt { span = everyMinuteSpan }, enqDate);
            q.Enqueue(new Evnt { span = everyDaySpan }, enqDate);
            q.Enqueue(new Evnt { span = everyWeekSpan }, enqDate);

            TimeSpan? span =q.FirstEventSpanImpl(enqDate);
            Assert.IsTrue(span.HasValue);
            Assert.AreEqual(everyMinuteSpan,span.Value);

            span = q.FirstEventSpanImpl(enqDate + new TimeSpan(0, 0,0 , 40)); //19.01.1983 00:00:40
            Assert.IsTrue(span.HasValue);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 20), span.Value );         //19.01.1983 00:01:00

            q.Dequeue(out schDate, enqDate + new TimeSpan(0, 0, 1, 00));    //19.01.1983 00:01:00

            span = q.FirstEventSpanImpl(enqDate + new TimeSpan(0, 0, 01, 55)); //19.01.1983 00:01:55
            Assert.IsTrue(span.HasValue);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 5),span.Value);         //19.01.1983 00:02:00

            //забыли вытаскивать целый день, посмотрим
            Evnt e1 = q.Dequeue(out schDate, enqDate + new TimeSpan(1, 0, 0, 02));    //20.01.1983 00:00:02
            Assert.AreEqual(e1.span, everyMinuteSpan);
            Evnt e2 = q.Dequeue(out schDate, enqDate + new TimeSpan(1, 0, 0, 02));    //20.01.1983 00:00:02
            Assert.AreEqual(e2.span, everyDaySpan);
            Evnt e3 = q.Dequeue(out schDate, enqDate + new TimeSpan(1, 0, 0, 02));    //20.01.1983 00:00:02
            Assert.IsNull(e3);
        }
    }
}
#endif