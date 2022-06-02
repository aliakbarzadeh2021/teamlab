#if DEBUG
namespace ASC.Common.Tests.Notify
{
    using System.Collections.Generic;
    using ASC.Notify;
    using ASC.Notify.Messages;
    using NUnit.Framework;

    [TestFixture]
    public class NotifyResult_Test
    {
        [Test]
        public void Merge()
        { 
            SendResponse sr1 = new SendResponse("1",Fake.recipient_anton,SendResult.Ok);
            SendResponse sr2 = new SendResponse("2",Fake.recipient_anton,SendResult.Impossible);

            NotifyResult res1 = new NotifyResult(SendResult.Ok, new List<SendResponse>{sr1});
            NotifyResult res2 = new NotifyResult(SendResult.Impossible, new List<SendResponse> { sr2 });

            res1.Merge(res2);

            Assert.AreEqual(SendResult.Ok | SendResult.Impossible, res1.Result);
            Assert.AreEqual(2, res1.Responses.Count);
            CollectionAssert.Contains(res1.Responses, sr1);
            CollectionAssert.Contains(res1.Responses, sr2);

            res1 = new NotifyResult();
            res2 = new NotifyResult(SendResult.Impossible, new List<SendResponse> { sr2 });
            res1.Merge(res2);

            Assert.AreEqual(SendResult.Impossible, res1.Result);
            Assert.AreEqual(1, res1.Responses.Count);
            CollectionAssert.Contains(res1.Responses, sr2);

            res1 = new NotifyResult(SendResult.Ok, new List<SendResponse> { sr1 });
            res2 = new NotifyResult();
            res1.Merge(res2);

            Assert.AreEqual(SendResult.Ok, res1.Result);
            Assert.AreEqual(1, res1.Responses.Count);
            CollectionAssert.Contains(res1.Responses, sr1);

        }
    }
}
#endif