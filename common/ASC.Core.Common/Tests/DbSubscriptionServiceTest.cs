#if DEBUG
namespace ASC.Core.Common.Tests
{
    using System.Linq;
    using ASC.Core.Data;
    using NUnit.Framework;

    [TestFixture]
    public class DbSubscriptionServiceTest : DbBaseTest<DbSubscriptionService>
    {
        [SetUp]
        public void ClearData()
        {
            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", RecipientId = "recipientId", });
            Service.RemoveSubscriptions(Tenant, "sourceId", "actionId");
        }

        [Test]
        public void SubscriptionMethod()
        {
            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", RecipientId = "recipientId", Methods = new[] { "email.sender" } });
            var m = Service.GetSubscriptionMethods(Tenant, "sourceId", "actionId", "recipientId").First();
            Assert.AreEqual(m.Tenant, Tenant);
            Assert.AreEqual(m.SourceId, "sourceId");
            Assert.AreEqual(m.ActionId, "actionId");
            Assert.AreEqual(m.RecipientId, "recipientId");
            CollectionAssert.AreEquivalent(new[] { "email.sender" }, m.Methods);

            Service.SetSubscriptionMethod(new SubscriptionMethod { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", RecipientId = "recipientId", Methods = null });
            Assert.IsNull(Service.GetSubscriptionMethods(Tenant, "sourceId", "actionId", "recipientId").FirstOrDefault());
            
            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", ObjectId = "object1Id", RecipientId = "recipientId", Subscribed = false });
            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", ObjectId = "object2Id", RecipientId = "recipientId", Subscribed = true });
            var subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", "recipientId", null);
            Assert.AreEqual(subs.Count(), 2);
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", null, "object1Id");
            Assert.AreEqual(subs.Count(), 1);
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", null, "object1Id");
            Assert.AreEqual(subs.Count(), 1);            
            
            Service.RemoveSubscriptions(Tenant, "sourceId", "actionId");
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", "recipientId", null);
            CollectionAssert.IsEmpty(subs);

            Service.SaveSubscription(new SubscriptionRecord { Tenant = this.Tenant, SourceId = "sourceId", ActionId = "actionId", ObjectId = "objectId", RecipientId = "recipientId", Subscribed = true });
            Service.RemoveSubscriptions(Tenant, "sourceId", "actionId", "objectId");
            subs = Service.GetSubscriptions(Tenant, "sourceId", "actionId", "recipientId", null);
            CollectionAssert.IsEmpty(subs);
        }
    }
}
#endif
