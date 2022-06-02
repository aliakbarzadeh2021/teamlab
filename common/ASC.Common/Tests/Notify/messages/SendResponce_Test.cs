#if DEBUG
namespace ASC.Common.Tests.Notify.messages
{
    using System;
    using ASC.Notify.Messages;
    using NUnit.Framework;
    
    [TestFixture]
    public class SendResponce_Test
    {
        [Test]
        public void Test_Ctor()
        {
            SendResponse resp = null;
            Exception exc = new Exception();

            resp = new SendResponse();
            Assert.AreEqual(SendResult.Ok, resp.Result);
            Assert.IsNull(resp.Exception);
            Assert.IsNull(resp.NoticeMessage);
            Assert.IsNull(resp.Recipient);
            Assert.IsNull(resp.SenderName);
            Assert.IsNull(resp.NotifyAction);

            resp = new SendResponse(Fake.recipient_anton, exc);
            Assert.AreEqual(SendResult.Impossible, resp.Result);
            Assert.AreEqual(exc,resp.Exception);
            Assert.IsNull(resp.NoticeMessage);
            Assert.AreEqual(Fake.recipient_anton, resp.Recipient);
            Assert.IsNull(resp.SenderName);
            Assert.IsNull(resp.NotifyAction);

            resp = new SendResponse(Fake.action_new_employee,Fake.recipient_anton, exc);
            Assert.AreEqual(SendResult.Impossible, resp.Result);
            Assert.AreEqual(exc, resp.Exception);
            Assert.IsNull(resp.NoticeMessage);
            Assert.AreEqual(Fake.recipient_anton, resp.Recipient);
            Assert.IsNull(resp.SenderName);
            Assert.AreEqual(Fake.action_new_employee, resp.NotifyAction);

            resp = new SendResponse("email",Fake.recipient_anton, exc);
            Assert.AreEqual(SendResult.Impossible, resp.Result);
            Assert.AreEqual(exc, resp.Exception);
            Assert.IsNull(resp.NoticeMessage);
            Assert.AreEqual(Fake.recipient_anton, resp.Recipient);
            Assert.AreEqual("email",resp.SenderName);
            Assert.IsNull(resp.NotifyAction);

            resp = new SendResponse(Fake.action_new_employee, "email", Fake.recipient_anton, exc);
            Assert.AreEqual(SendResult.Impossible, resp.Result);
            Assert.AreEqual(exc, resp.Exception);
            Assert.IsNull(resp.NoticeMessage);
            Assert.AreEqual(Fake.recipient_anton, resp.Recipient);
            Assert.AreEqual("email", resp.SenderName);
            Assert.AreEqual(Fake.action_new_employee, resp.NotifyAction);

            resp = new SendResponse("email", Fake.recipient_anton, SendResult.IncorrectRecipient);
            Assert.AreEqual(SendResult.IncorrectRecipient, resp.Result);
            Assert.IsNull( resp.Exception);
            Assert.IsNull(resp.NoticeMessage);
            Assert.AreEqual(Fake.recipient_anton, resp.Recipient);
            Assert.AreEqual("email", resp.SenderName);
            Assert.IsNull(resp.NotifyAction);

            resp = new SendResponse(Fake.action_new_employee, "email", Fake.recipient_anton, SendResult.IncorrectRecipient);
            Assert.AreEqual(SendResult.IncorrectRecipient, resp.Result);
            Assert.IsNull(resp.Exception);
            Assert.IsNull(resp.NoticeMessage);
            Assert.AreEqual(Fake.recipient_anton, resp.Recipient);
            Assert.AreEqual("email", resp.SenderName);
            Assert.AreEqual(Fake.action_new_employee, resp.NotifyAction);
        }
    }
}
#endif