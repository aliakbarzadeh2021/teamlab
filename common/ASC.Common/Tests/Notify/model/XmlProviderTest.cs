#if DEBUG
namespace ASC.Common.Tests.Notify.model
{
    using ASC.Notify.Model;
    using NUnit.Framework;

    [TestFixture]
    public class XmlProviderTest
    {
        [Test]
        public void LoadFromFile()
        {
            XmlActionProvider xml = new XmlActionProvider(@"Notify\model\action_test_xsd.xml");
        }
        [Test]
        public void LoadFromResource()
        {
            XmlActionProvider xml = new XmlActionProvider(GetType().Assembly, "ASC.Common.Tests.Notify.model.action_test_xsd.xml");
        }
        [Test]
        public void TestParse()
        {
            XmlActionProvider xml = new XmlActionProvider(@"Notify\model\action_test_xsd.xml");

            INotifyAction act1 = xml.GetAction("act1");
            Assert.IsNotNull(act1);
            Assert.AreEqual("act1", act1.ID);
            Assert.IsEmpty(act1.Name);

            INotifyAction act2 = xml.GetAction("act2");
            Assert.IsNotNull(act2);
            Assert.AreEqual("act2", act2.ID);
            Assert.AreEqual("action 2", act2.Name);
        }       
    }
}
#endif