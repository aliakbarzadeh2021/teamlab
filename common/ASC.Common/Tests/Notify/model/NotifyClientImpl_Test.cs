#if DEBUG
namespace ASC.Common.Tests.Notify.model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rhino.Mocks;
    using NUnit.Framework;
    using NMock2;
    using ASC.Notify;
    using ASC.Notify.Model;
    using ASC.Notify.Patterns;
    using ASC.Notify.Channels;
    using ASC.Notify.Recipients;
    using NMock2.Matchers;
    using NMock2.Actions;
    using ASC.Notify.Messages;
    using ASC.Notify.Tests;
    using ASC.Common.Tests;
    using System.Threading;
    using System.Diagnostics;
    using ASC.Notify.Engine;

    [TestFixture]
    public class NotifyClientImpl_Test
    {
        Mockery mockery = new Mockery();
        MockRepository mocks = new MockRepository();

        Context notofyCtx = null;
        INotifySource notifySource = null;
        INotifyClient notifyClient = null;
        ISubscriptionSource subscriptionSource = null;
        IActionPatternProvider actionPatternProvider = null;
        IPatternProvider patternProvider = null;
        IRecipientProvider recipientProvider = null;

        ISenderChannel senderChannel_email = null;
        ISenderChannel senderChannel_web = null;
        enum E { e = 1 };

        [Test]
        public void AddStaticText()
        {
            RecreateMocks();
            NotifyClientImpl client = new NotifyClientImpl(notofyCtx, notifySource);
            var tv = new TagValue("t1", 1);
            var tv2 = new TagValue("t2", 1);
            var tv_ = new TagValue("t1", 2);

            client.SetStaticTags(new[] { tv });
            //CollectionAssert.AreEqual(new[] { tv }, client._staticTags);

            client.SetStaticTags(null);
            //CollectionAssert.IsEmpty(client._staticTags);

            client.SetStaticTags(new[] { tv });

            //добавляется
            NotifyRequest req = new NotifyRequest(notifySource, Fake.action_employee_die, "1", Fake.recipient_anton);
            req.Arguments.Add(tv2);
            client.AddStaticTegs(req);
            CollectionAssert.AreEqual(new[] { tv2, tv }, req.Arguments);

            //не перебивает явные
            req.Arguments.Clear();
            req.Arguments.Add(tv2);
            req.Arguments.Add(tv_);
            client.AddStaticTegs(req);
            CollectionAssert.AreEqual(new[] { tv2, tv_ }, req.Arguments);

        }

        #region вспомогательные методы
        void RecreateMocks()
        {
            notofyCtx = new Context();
            notifySource = mockery.NewMock<INotifySource>();
            notifyClient = new NotifyClientImpl(notofyCtx, notifySource);

            subscriptionSource = mockery.NewMock<ISubscriptionSource>();
            actionPatternProvider = mockery.NewMock<IActionPatternProvider>();
            patternProvider = mockery.NewMock<IPatternProvider>();
            recipientProvider = mockery.NewMock<IRecipientProvider>();

            Stub.On(notifySource).Method("GetPatternProvider").Will(Return.Value(patternProvider));
            Stub.On(notifySource).Method("GetRecipientsProvider").Will(Return.Value(recipientProvider));
            Stub.On(notifySource).Method("GetSubscriptionSource").Will(Return.Value(subscriptionSource));
            Stub.On(notifySource).Method("GetActionPatternProvider").Will(Return.Value(actionPatternProvider));

            //для любого шаблона будет форматтер-заглушка
            Stub.On(patternProvider).Method("GetFormatter").Will(Return.Value(Fake.pattern_formatter));
            //адрес для email-отправки - email получателя
            Stub.On(recipientProvider).Method("GetRecipientAddresses")
                .With(Is.Anything, Fake.sender_email, Is.Anything)
                .Will(ArgReturnAction.Create<IDirectRecipient, string>((drec, sender) => new[] { drec.Addresses[0] }));
            //адрес для web-отправки - id получателя
            Stub.On(recipientProvider).Method("GetRecipientAddresses")
                .With(Is.Anything, Fake.sender_web, Is.Anything)
                .Will(ArgReturnAction.Create<IDirectRecipient, string>((drec, sender) => new[] { drec.ID }));

            senderChannel_web = mockery.NewMock<ISenderChannel>();
            Stub.On(senderChannel_web).GetProperty("SenderName").Will(Return.Value(Fake.sender_web));
            notofyCtx.SenderHolder.RegisterSender(senderChannel_web);

            senderChannel_email = mockery.NewMock<ISenderChannel>();
            Stub.On(senderChannel_email).GetProperty("SenderName").Will(Return.Value(Fake.sender_email));
            notofyCtx.SenderHolder.RegisterSender(senderChannel_email);
        }


        #endregion

        #region отправка сизвещений

        [Test, Description("проверка непосредственной отправки сообщения")]
        public void SendNoticeDirect()
        {
            RecreateMocks();

            Expect.Once.On(actionPatternProvider)
                .Method("GetPattern")
                .With(Fake.action_new_employee, Fake.sender_email)
                .Will(Return.Value(Fake.pattern_new_employee));

            Stub.On(subscriptionSource)
                .Method("IsUnsubscribe").Will(Return.Value(false));
            Expect.Once.On(subscriptionSource)
                .Method("GetSubscriptionMethod")
                .With(Fake.action_new_employee, Fake.recipient_anton)
                .Will(Return.Value(new[] { Fake.sender_email }));

            Expect.Once.On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));


            NotifyResult res = notifyClient.SendNotice(
                                    Fake.action_new_employee,
                                    "jhon doo",
                                    Fake.recipient_anton,
                                    new ITagValue[0]);

            mockery.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(SendResult.Ok, res.Result);
        }

        [Test, Description("проверка непосредственной отправки сообщения ассинхронно")]
        public void SendNoticeDirectAsync()
        {
            RecreateMocks();

            Expect.Once.On(actionPatternProvider)
                .Method("GetPattern")
                .With(Fake.action_new_employee, Fake.sender_email)
                .Will(Return.Value(Fake.pattern_new_employee));

            Stub.On(subscriptionSource)
                .Method("IsUnsubscribe").Will(Return.Value(false));
            Expect.Once.On(subscriptionSource)
                .Method("GetSubscriptionMethod")
                .With(Fake.action_new_employee, Fake.recipient_anton)
                .Will(Return.Value(new[] { Fake.sender_email }));

            Expect.Once.On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));

            ManualResetEvent stopev = new ManualResetEvent(false);
            NotifyResult res = null;

            notifyClient.SendNoticeAsync(
                                     Fake.action_new_employee,
                                     "jhon doo",
                                     Fake.recipient_anton,
                                     (a, oid, rec, result) =>
                                     {
                                         res = result;
                                         stopev.Set();
                                     },
                                     new ITagValue[0]);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            stopev.WaitOne(500, false);
            sw.Stop();
            Trace.WriteLine("SendNoticeDirectAsync by " + sw.Elapsed.ToString());

            mockery.VerifyAllExpectationsHaveBeenMet();

            Assert.AreEqual(SendResult.Ok, res.Result);
        }

        [Test, Description("проверка непосредственной отправки сообщения конкретному списку адресатов")]
        public void SendNoticeDirectToAntonAndValery()
        {
            RecreateMocks();

            Expect.Exactly(2).On(actionPatternProvider)
                .Method("GetPattern")
                .With(Fake.action_new_employee, Fake.sender_email)
                .Will(Return.Value(Fake.pattern_new_employee));

            Stub.On(subscriptionSource)
                .Method("IsUnsubscribe").Will(Return.Value(false));
            Expect.Exactly(2).On(subscriptionSource)
                .Method("GetSubscriptionMethod")
                .With(Fake.action_new_employee, Is.OneOf(Fake.recipient_anton, Fake.recipient_valery))
                .Will(Return.Value(new[] { Fake.sender_email }));


            Expect.Exactly(2).On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));


            NotifyResult res = notifyClient.SendNoticeTo(
                                    Fake.action_new_employee,
                                    "jhon doo",
                                    new IRecipient[] { Fake.recipient_anton, Fake.recipient_valery },
                                    new ITagValue[0]);

            mockery.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(SendResult.Ok, res.Result);
        }

        [Test, Description("проверка непосредственной отправки сообщения ассинхронно конкретному списку адресатов")]
        public void SendNoticeDirectToAntonAndValeryAsync()
        {
            RecreateMocks();

            Expect.Exactly(2).On(actionPatternProvider)
                .Method("GetPattern")
                .With(Fake.action_new_employee, Fake.sender_email)
                .Will(Return.Value(Fake.pattern_new_employee));

            Stub.On(subscriptionSource)
                .Method("IsUnsubscribe").Will(Return.Value(false));
            Expect.Exactly(2).On(subscriptionSource)
                .Method("GetSubscriptionMethod")
                .With(Fake.action_new_employee, Is.OneOf(Fake.recipient_anton, Fake.recipient_valery))
                .Will(Return.Value(new[] { Fake.sender_email }));


            Expect.Exactly(2).On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));

            ManualResetEvent stopev = new ManualResetEvent(false);
            NotifyResult[] res = new NotifyResult[2];
            int callbackcount = 0;

            notifyClient.SendNoticeToAsync(
                                     Fake.action_new_employee,
                                     "jhon doo",
                                     new IRecipient[] { Fake.recipient_anton, Fake.recipient_valery },
                                     (a, oid, rec, result) =>
                                     {
                                         res[callbackcount] = result;
                                         if (callbackcount == 1)
                                             stopev.Set();
                                         callbackcount++;
                                     },
                                     new ITagValue[0]);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            stopev.WaitOne(500, false);
            sw.Stop();
            Trace.WriteLine("SendNoticeDirectAsync by " + sw.Elapsed.ToString());

            mockery.VerifyAllExpectationsHaveBeenMet();

            Assert.AreEqual(SendResult.Ok, res[0].Result);
            Assert.AreEqual(SendResult.Ok, res[1].Result);
        }

        [Test, Description("проверка непосредственной отправки сообщения конкретному списку адресатов конкретным способом")]
        public void SendNoticeDirectToAntonAndValeryByEmail()
        {
            RecreateMocks();

            Expect.Exactly(2).On(actionPatternProvider)
                .Method("GetPattern")
                .With(Fake.action_new_employee, Fake.sender_email)
                .Will(Return.Value(Fake.pattern_new_employee));

            Stub.On(subscriptionSource)
                .Method("IsUnsubscribe").Will(Return.Value(false));


            Expect.Never.On(subscriptionSource)
                .Method("GetSubscriptionMethod");

            Expect.Exactly(2).On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));


            NotifyResult res = notifyClient.SendNoticeTo(
                                    Fake.action_new_employee,
                                    "jhon doo",
                                    new IRecipient[] { Fake.recipient_anton, Fake.recipient_valery },
                                    new[] { Fake.sender_email },
                                    new ITagValue[0]);

            mockery.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(SendResult.Ok, res.Result);
        }

        [Test, Description("проверка непосредственной отправки сообщения ассинхронно конкретному списку адресатов конкретным способом")]
        public void SendNoticeDirectToAntonAndValeryByEmailAsync()
        {
            RecreateMocks();

            Expect.Exactly(2).On(actionPatternProvider)
                .Method("GetPattern")
                .With(Fake.action_new_employee, Fake.sender_email)
                .Will(Return.Value(Fake.pattern_new_employee));

            Stub.On(subscriptionSource)
                .Method("IsUnsubscribe").Will(Return.Value(false));


            Expect.Never.On(subscriptionSource)
                .Method("GetSubscriptionMethod")
                .With(Fake.action_new_employee, Is.OneOf(Fake.recipient_anton, Fake.recipient_valery))
                .Will(Return.Value(new[] { Fake.sender_email }));

            Expect.Exactly(2).On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));

            ManualResetEvent stopev = new ManualResetEvent(false);
            NotifyResult[] res = new NotifyResult[2];
            int callbackcount = 0;

            notifyClient.SendNoticeToAsync(
                                     Fake.action_new_employee,
                                     "jhon doo",
                                     new IRecipient[] { Fake.recipient_anton, Fake.recipient_valery },
                                     new[] { Fake.sender_email },
                                     (a, oid, rec, result) =>
                                     {
                                         res[callbackcount] = result;
                                         if (callbackcount == 1)
                                             stopev.Set();
                                         callbackcount++;
                                     },
                                     new ITagValue[0]);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            stopev.WaitOne(500, false);
            sw.Stop();
            Trace.WriteLine("SendNoticeDirectAsync by " + sw.Elapsed.ToString());

            mockery.VerifyAllExpectationsHaveBeenMet();

            Assert.AreEqual(SendResult.Ok, res[0].Result);
            Assert.AreEqual(SendResult.Ok, res[1].Result);
        }

        [Test, Description("проверка отправки готового сообщения конкретному списку адресатов конкретным способом")]
        public void SendDirectPureNoticeToAntonAndValeryByEmail()
        {
            RecreateMocks();

            Stub.On(subscriptionSource)
                .Method("IsUnsubscribe").Will(Return.Value(false));
            Expect.Exactly(2).On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));


            NotifyResult res = notifyClient.SendDirectNotice(
                                new PureNoticeMessage(
                                        "title",
                                        "body",
                                        new IRecipient[] { Fake.recipient_anton, Fake.recipient_valery }
                                    ),
                                    new[] { Fake.sender_email }
                                );

            Assert.AreEqual(SendResult.Ok, res.Result);
            mockery.VerifyAllExpectationsHaveBeenMet();
        }

        [Test, Description("проверка отправки готового сообщения конкретному списку адресатов ассинхронно конкретным способом")]
        public void SendDirectPureNoticeToAntonAndValeryByEmailAsync()
        {
            RecreateMocks();
            Stub.On(subscriptionSource)
               .Method("IsUnsubscribe").Will(Return.Value(false));
            Expect.Exactly(2).On(senderChannel_email)
                .Method("Send")
                .Will(ArgReturnAction.Create<INoticeMessage, INotifySource>((nm, s) => new SendResponse(nm) { Result = SendResult.Ok }));

            PureNoticeMessage pure = new PureNoticeMessage("title", "body",
                            new IRecipient[] { Fake.recipient_anton, Fake.recipient_valery });

            ManualResetEvent stopev = new ManualResetEvent(false);
            NotifyResult[] res = new NotifyResult[2];
            int callbackcount = 0;

            notifyClient.SendDirectNoticeAsync(
                                pure,
                                (a, oid, rec, result) =>
                                {
                                    res[callbackcount] = result;
                                    if (callbackcount == 1)
                                        stopev.Set();
                                    callbackcount++;
                                },
                                new[] { Fake.sender_email }
                                );

            Stopwatch sw = new Stopwatch();
            sw.Start();
            stopev.WaitOne(500, false);
            sw.Stop();
            Trace.WriteLine("SendDirectNoticeAsync by " + sw.Elapsed.ToString());

            mockery.VerifyAllExpectationsHaveBeenMet();

            Assert.AreEqual(SendResult.Ok, res[0].Result);
            Assert.AreEqual(SendResult.Ok, res[1].Result);
        }
        #endregion


        Context _nctx = null;
        NotifyEngine _negine = null;
        INotifyClient _nclient = null;
        INotifySource _nsource = null;
        void CreateRhinoMocks()
        {
            _nctx = new Context();

            _negine = mocks.StrictMock<NotifyEngine>(_nctx);
            _nsource = mocks.StrictMock<INotifySource>();
            _nctx.NotifyEngine = _negine;

            _nclient = new NotifyClientImpl(_nctx, _nsource);

        }
        #region события


        #endregion
    }
}
#endif