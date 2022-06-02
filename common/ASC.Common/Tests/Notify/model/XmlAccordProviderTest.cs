#if DEBUG
namespace ASC.Common.Tests.Notify.model
{
    using System.Collections.Generic;
    using ASC.Notify.Model;
    using ASC.Notify.Patterns;
    using NUnit.Framework;

    [TestFixture]
    public class XmlAccordProviderTest
    {
        IActionProvider actionProvider = new ConstActionProvider(new NotifyAction("act1", null), new NotifyAction("act2", null));
        IPatternProvider patternProvider = new ConstPatternProvider(
                new KeyValuePair<IPattern,IPatternFormatter>(new Pattern("pat1",null,"",""),new ReplacePatternFormatter()),
                new KeyValuePair<IPattern,IPatternFormatter>(new Pattern("pat2email",null,"",""),new ReplacePatternFormatter()),
                new KeyValuePair<IPattern,IPatternFormatter>(new Pattern("pat2messanger",null,"",""),new ReplacePatternFormatter())
            );

        
        [Test]
        public void LoadFromFile()
        {
            XmlActionPatternProvider xml = new XmlActionPatternProvider(@"Notify\model\action_pattern_tets_xsd.xml", actionProvider, patternProvider);
        }
        [Test]
        public void LoadFromResource()
        {
            XmlActionPatternProvider xml = new XmlActionPatternProvider(GetType().Assembly, "ASC.Common.Tests.Notify.model.action_pattern_tets_xsd.xml", actionProvider, patternProvider);
        }
        [Test]
        public void TestParse()
        {
            XmlActionPatternProvider xml = new XmlActionPatternProvider(@"Notify\model\action_pattern_tets_xsd.xml", actionProvider, patternProvider);

            Assert.AreEqual(patternProvider.GetPattern("pat1"), xml.GetPattern(actionProvider.GetAction("act1"), "email"));
            Assert.AreEqual(patternProvider.GetPattern("pat1"), xml.GetPattern(actionProvider.GetAction("act1"), "messanger"));
            Assert.IsNull(xml.GetPattern(actionProvider.GetAction("act1")));

            Assert.AreEqual(patternProvider.GetPattern("pat2email"), xml.GetPattern(actionProvider.GetAction("act2"),"email"));
            Assert.AreEqual(patternProvider.GetPattern("pat2messanger"), xml.GetPattern(actionProvider.GetAction("act2"), "messanger"));
            Assert.AreEqual(patternProvider.GetPattern("pat2email"), xml.GetPattern(actionProvider.GetAction("act2")));
        }
        
    }
}
#endif