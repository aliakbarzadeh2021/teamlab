#if DEBUG
namespace ASC.Common.Tests.Notify.model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NUnit.Framework;
    using Rhino.Mocks;
    using ASC.Notify.Recipients;
    using ASC.Notify.Model;

    [TestFixture]
    public class SubscriptionProviderBase_Test
    {
        MockRepository mocks = new MockRepository();

        IRecipientProvider recProv = null;
        ISubscriptionProvider subProv = null;
        TopSubscriptionProvider topsp = null;

        private void CreateMoks()
        {
            mocks = new MockRepository();
            recProv = mocks.StrictMock<IRecipientProvider>();
            subProv = mocks.StrictMock<ISubscriptionProvider>();
            topsp = new TopSubscriptionProvider(recProv, subProv);
            
        }

        #region WalkUp
        
        [Test]
        public void WalkUp_Single()
        {
            CreateMoks();

            using (mocks.Ordered())
            {
                Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                    .Return(new IRecipientsGroup[] { });
            }

            
            mocks.ReplayAll();
            List<IRecipient> recipients = topsp.WalkUp(Fake.recipient_anton);
            mocks.VerifyAll();
            Assert.AreEqual(0, recipients.Count);
        }

        [Test]
        public void WalkUp_Single_In1Group()
        {
            CreateMoks();

            using (mocks.Ordered())
            {
                Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                    .Return(new IRecipientsGroup[] { Fake.recipient_group_netdep });

                Expect.Call(recProv.GetGroups(Fake.recipient_group_netdep))
                    .Return(new IRecipientsGroup[] {  });
            }


            mocks.ReplayAll();
            List<IRecipient> recipients = topsp.WalkUp(Fake.recipient_anton);
            mocks.VerifyAll();
            Assert.AreEqual(1, recipients.Count);
        }

        [Test]
        public void WalkUp_Single_In2_1Group()
        {
            CreateMoks();

            using (mocks.Ordered())
            {
                Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                    .Return(new IRecipientsGroup[] { Fake.recipient_group_netdep,Fake.recipient_group_users });

                Expect.Call(recProv.GetGroups(Fake.recipient_group_netdep))
                    .Return(new IRecipientsGroup[] { Fake.recipient_group_activeX });

                Expect.Call(recProv.GetGroups(Fake.recipient_group_activeX))
                    .Return(new IRecipientsGroup[] { });

                Expect.Call(recProv.GetGroups(Fake.recipient_group_users))
                    .Return(new IRecipientsGroup[] { });

            }

            mocks.ReplayAll();
            List<IRecipient> recipients = topsp.WalkUp(Fake.recipient_anton);
            mocks.VerifyAll();
            Assert.AreEqual(3, recipients.Count);
        }
        [Test]
        public void WalkUp_Real()
        {
            CreateMoks();
            /*- users
             *  - anton
             *  - lev
             *  - valery
             *- activeX
             *  - dotnetdep
             *      - anton
             *      - valery
             *- administration
             *  - lev
             */


            Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                .Return(new IRecipientsGroup[] { Fake.recipient_group_netdep, Fake.recipient_group_users }).Repeat.Any();

            Expect.Call(recProv.GetGroups(Fake.recipient_valery))
                .Return(new IRecipientsGroup[] { Fake.recipient_group_netdep, Fake.recipient_group_users }).Repeat.Any();
            
            Expect.Call(recProv.GetGroups(Fake.recipient_lev))
                .Return(new IRecipientsGroup[] { Fake.recipient_group_administration, Fake.recipient_group_users }).Repeat.Any();

            Expect.Call(recProv.GetGroups(Fake.recipient_group_netdep))
                .Return(new IRecipientsGroup[] { Fake.recipient_group_activeX }).Repeat.Any();
            
            Expect.Call(recProv.GetGroups(Fake.recipient_group_activeX))
                .Return(new IRecipientsGroup[] { }).Repeat.Any();
            
            Expect.Call(recProv.GetGroups(Fake.recipient_group_administration))
                .Return(new IRecipientsGroup[] { }).Repeat.Any();
            
            Expect.Call(recProv.GetGroups(Fake.recipient_group_users))
                .Return(new IRecipientsGroup[] { }).Repeat.Any();

            mocks.Replay(recProv);
            List<IRecipient> recipients = null;
            
            recipients = topsp.WalkUp(Fake.recipient_anton);
            Assert.AreEqual(3, recipients.Count);
            CollectionAssert.Contains(recipients, Fake.recipient_group_netdep);
            CollectionAssert.Contains(recipients, Fake.recipient_group_activeX);
            CollectionAssert.Contains(recipients, Fake.recipient_group_users);

            recipients = topsp.WalkUp(Fake.recipient_valery);
            Assert.AreEqual(3, recipients.Count);
            CollectionAssert.Contains(recipients, Fake.recipient_group_netdep);
            CollectionAssert.Contains(recipients, Fake.recipient_group_activeX);
            CollectionAssert.Contains(recipients, Fake.recipient_group_users);

            recipients = topsp.WalkUp(Fake.recipient_lev);
            Assert.AreEqual(2, recipients.Count);
            CollectionAssert.Contains(recipients, Fake.recipient_group_administration);
            CollectionAssert.Contains(recipients, Fake.recipient_group_users);
        }
        #endregion

        #region GetRecipients

        [Test]
        public void GetRecipients_Empty()
        {
            CreateMoks();

            Expect.Call(subProv.GetRecipients(Fake.action_new_employee, null)).Return(new IRecipient[] { });
            Expect.Call(recProv.GetGroups(Fake.recipient_anton)).Return(new IRecipientsGroup[] { });

            mocks.ReplayAll();
            var recipients = topsp.GetRecipients(Fake.action_new_employee, null);

            Assert.IsNotNull(recipients);
            Assert.AreEqual(0, recipients.Length);
        }

        [Test]
        public void GetRecipients()
        {
            CreateMoks();

            Expect.Call(subProv.GetRecipients(Fake.action_new_employee, null)).Return(new[] { Fake.recipient_anton });
            Expect.Call(recProv.GetGroups(Fake.recipient_anton)).Return(new IRecipientsGroup[] { });

            mocks.ReplayAll();
            var recipients = topsp.GetRecipients(Fake.action_new_employee, null);

            Assert.IsNotNull(recipients);
            Assert.AreEqual(1, recipients.Length);
        }

        [Test]
        public void GetRecipients_GroupSubscr()
        {
            CreateMoks();

            Expect.Call(subProv.GetRecipients(Fake.action_new_employee, null)).Return(new[] { Fake.recipient_group_netdep });
            Expect.Call(recProv.GetGroups(Fake.recipient_anton)).Return(new IRecipientsGroup[] { Fake.recipient_group_netdep });

            mocks.ReplayAll();
            var recipients = topsp.GetRecipients(Fake.action_new_employee, null);

            Assert.IsNotNull(recipients);
            Assert.AreEqual(1, recipients.Length);
        }



        #endregion

        #region GetSubscriptionMethod
        
        [Test]
        public void GetSubscriptionMethod()
        {
            CreateMoks();

            Expect.Call(subProv.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton))
                .Return(new[] { Fake.sender_email});

            mocks.ReplayAll();
            var senders = topsp.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton);

            Assert.IsNotNull(senders);
            Assert.AreEqual(1, senders.Length);
            Assert.AreEqual(Fake.sender_email, senders[0]);
        }

        [Test]
        public void GetSubscriptionMethod_Empty()
        {
            CreateMoks();

            Expect.Call(subProv.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton))
                .Return(new string[] {  });

            mocks.ReplayAll();
            var senders = topsp.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton);

            Assert.IsNotNull(senders);
            Assert.AreEqual(0, senders.Length);
        }

        [Test]
        public void GetSubscriptionMethod_Null_GoParent()
        {
            CreateMoks();

            Expect.Call(subProv.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton))
                .Return(null);

            Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                .Return(new IRecipientsGroup[] { Fake.recipient_group_netdep });

            Expect.Call(recProv.GetGroups(Fake.recipient_group_netdep))
                .Return(new IRecipientsGroup[] {  });

            Expect.Call(subProv.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_group_netdep))
                .Return(new string[] { Fake.sender_email });

            mocks.ReplayAll();
            var senders = topsp.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton);

            Assert.IsNotNull(senders);
            Assert.AreEqual(1, senders.Length);
            Assert.AreEqual(Fake.sender_email, senders[0]);
        }

        [Test]
        public void GetSubscriptionMethod_Null_GoParent_Null()
        {
            CreateMoks();

            Expect.Call(subProv.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton))
                .Return(null);

            Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                .Return(new IRecipientsGroup[] { Fake.recipient_group_netdep });

            Expect.Call(recProv.GetGroups(Fake.recipient_group_netdep))
                .Return(new IRecipientsGroup[] { });

            Expect.Call(subProv.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_group_netdep))
                .Return(null);

            mocks.ReplayAll();
            var senders = topsp.GetSubscriptionMethod(Fake.action_new_employee, Fake.recipient_anton);
            mocks.VerifyAll();
            CollectionAssert.IsEmpty(senders);
        }
        #endregion

        #region IsUnsubscribe

        [Test]
        public void IsUnsubscribe()
        {
            CreateMoks();

            Expect.Call(subProv.IsUnsubscribe(Fake.recipient_anton,Fake.action_new_employee, null))
                .Return(false);
            Expect.Call(subProv.IsUnsubscribe(Fake.recipient_valery, Fake.action_new_employee, "1"))
                .Return(true);

            mocks.ReplayAll();
            Assert.IsFalse(topsp.IsUnsubscribe(Fake.recipient_anton, Fake.action_new_employee, null));
            Assert.IsTrue(topsp.IsUnsubscribe(Fake.recipient_valery, Fake.action_new_employee, "1"));
            mocks.VerifyAll();
        }

        #endregion

        #region GetSubscriptions(IRecipient recipient, INotifyAction action)

        [Test]
        public void GetSubscriptions()
        {
            CreateMoks();

            //подписан на одно действие
            Expect.Call(subProv.GetSubscriptions(Fake.action_new_employee,Fake.recipient_anton))
               .Return(new string[] { null });

            //в группах не состоит
            Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                .Return(new IRecipientsGroup[] {  });

            mocks.ReplayAll();
            var actions = topsp.GetSubscriptions(Fake.action_new_employee, Fake.recipient_anton);
            mocks.VerifyAll();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1,actions.Length);
            Assert.AreEqual(null, actions[0]);
        }

        [Test]
        public void GetSubscriptions_Empty()
        {
            CreateMoks();

            //не подписан
            Expect.Call(subProv.GetSubscriptions(Fake.action_new_employee, Fake.recipient_anton))
               .Return(new string[] {  });

            //в группах не состоит
            Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                .Return(new IRecipientsGroup[] { });

            mocks.ReplayAll();
            var objects = topsp.GetSubscriptions(Fake.action_new_employee, Fake.recipient_anton);
            mocks.VerifyAll();

            Assert.IsNotNull(objects);
            Assert.AreEqual(0, objects.Length);
        }

        [Test]
        public void GetSubscriptions_OneDirect_ThreeParentOneDuplicateOneUnsubscribed()
        {
            CreateMoks();

            //Expect.Call(subProv.IsUnsubscribe(Fake.recipient_anton, Fake.action_new_employee, null))
            //    .Return(false);

            Expect.Call(subProv.IsUnsubscribe(Fake.recipient_anton, Fake.action_new_employee, "1"))
                .Return(false);
            Expect.Call(subProv.IsUnsubscribe(Fake.recipient_anton, Fake.action_new_employee, "2"))
                .Return(true);

            //anton подписан на одно действие
            Expect.Call(subProv.GetSubscriptions( Fake.action_new_employee,Fake.recipient_anton))
               .Return(new string[] { null });

            //состоит в 
            Expect.Call(recProv.GetGroups(Fake.recipient_anton))
                .Return(new IRecipientsGroup[] { Fake.recipient_group_netdep });

            //.NET нигде не состит
            Expect.Call(recProv.GetGroups(Fake.recipient_group_netdep))
                .Return(new IRecipientsGroup[] { });

            //.NET подписан на одно действие
            Expect.Call(subProv.GetSubscriptions(Fake.action_new_employee,Fake.recipient_group_netdep))
                .Return(new []{null,"1","2"});
            
            mocks.ReplayAll();
            var objects = topsp.GetSubscriptions(Fake.action_new_employee, Fake.recipient_anton);
            mocks.VerifyAll();

            Assert.IsNotNull(objects);
            Assert.AreEqual(2, objects.Length);
            Assert.AreEqual(null, objects[0]);
            Assert.AreEqual("1", objects[1]);

        }

        #endregion

    }
}
#endif