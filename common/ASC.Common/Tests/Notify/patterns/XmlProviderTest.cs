#if DEBUG
namespace ASC.Common.Tests.Notify.patterns
{
    using System.Xml;
    using ASC.Notify.Patterns;
    using NUnit.Framework;

    [TestFixture]
    public class XmlProviderTest
    {
        [Test]
        public void LoadFromFile()
        {
            XmlPatternProvider xml = new XmlPatternProvider(XmlReader.Create(@"Notify\patterns\test_pattern_xsd.xml"));
        }
        [Test]
        public void LoadFromResource()
        {
            XmlPatternProvider xml = new XmlPatternProvider(GetType().Assembly, "ASC.Common.Tests.Notify.patterns.test_pattern_xsd.xml");
        }
        [Test]
        public void TestParse()
        {
            XmlPatternProvider xml = new XmlPatternProvider(XmlReader.Create(@"Notify\patterns\test_pattern_xsd.xml"));

            IPattern pattern1 = xml.GetPattern("1");

            IPattern pattern21 = xml.GetPattern("21");
            Assert.IsNotNull(pattern21);
            Assert.AreEqual("пиво", pattern21.Subject);
            Assert.AreEqual("тело пиво ", pattern21.Body);
            Assert.IsInstanceOfType(typeof(ReplacePatternFormatter), xml.GetFormatter(pattern21));

            IPattern pattern12 = xml.GetPattern("12");
            Assert.IsNotNull(pattern12);
            Assert.AreEqual("112222<>ASD,>...", pattern12.Subject);
            Assert.AreEqual(" А вот это тело шаблона может быть каким угодно <>sdfSDf,.d %%7???     ", pattern12.Body);

            Assert.AreEqual("html", pattern1.ContentType);
            Assert.AreEqual("rtf", pattern21.ContentType);
            Assert.AreEqual("text", pattern12.ContentType);

        }
    }
}
#endif