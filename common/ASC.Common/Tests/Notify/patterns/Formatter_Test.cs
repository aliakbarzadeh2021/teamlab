#if DEBUG
namespace ASC.Common.Tests.Notify.patterns
{
    using ASC.Notify.Messages;
    using ASC.Notify.Patterns;
    using ASC.Notify.Recipients;
    using NUnit.Framework;

    [TestFixture]
    public class Formatter_Test
    {
        [Test]
        public void DummyFormat()
        {
            Pattern pattern = new Pattern("1","","","");
            DummyPatternFormatter formatter = new DummyPatternFormatter();
            ITag[] tags = formatter.GetTags(pattern);

            CollectionAssert.AreEquivalent(
                    new Tag[] {DummyPatternFormatter.SubjectTag,DummyPatternFormatter.BodyTag},
                    tags
                );

            DirectRecipient recipient = new DirectRecipient("1", "rec", new[] { "aa" });
            NoticeMessage notice = new NoticeMessage(recipient,null,null,pattern);
            
            formatter.FormatMessage(
                          notice,
                          new ITagValue[]{
                            new TagValue(DummyPatternFormatter.SubjectTag,"subj"),
                            new TagValue(DummyPatternFormatter.BodyTag,"body")
                          }
                );

            Assert.AreEqual("subj", notice.Subject);
            Assert.AreEqual("body", notice.Body);
        }

        [Test]
        public void ReplaceFormat()
        {
            string body = 
@"Собщаем вам, [%sys_recipient_name%]
что [%service_name%] закрыт.
отписаться можно по адресу [%sys.subscription.email%]";
            string subj = "Привет, [%sys_recipient_name%]!";

            Pattern pattern1 = new Pattern("1", "", subj,body);
            ReplacePatternFormatter formatter = new ReplacePatternFormatter();
            ITag[] tags = formatter.GetTags(pattern1);

            CollectionAssert.AreEquivalent(
                    new Tag[] { 
                        new Tag("sys_recipient_name"), 
                        new Tag("service_name") ,
                        new Tag("sys.subscription.email") 
                    },
                    tags
                );


            DirectRecipient recipient = new DirectRecipient("1", "anton", new[] { "aa" });
            NoticeMessage notice = new NoticeMessage(recipient, null, null, pattern1); ;

            formatter.FormatMessage(notice,
                    new ITagValue[]{
                        new TagValue("sys_recipient_name","anton"),
                        new TagValue("service_name","srv"),
                        new TagValue("sys.subscription.email", "srv@d.ru?asd=111"),
                    }
                );

            Assert.AreEqual(notice.Subject, "Привет, anton!");
            Assert.AreEqual(notice.Body,
@"Собщаем вам, anton
что srv закрыт.
отписаться можно по адресу srv@d.ru?asd=111");
        }

        [Test]
        public void SysTagFormatter()
        {
           // IPatternFormatter _sysTagFormatter = new ReplacePatternFormatter(@"_#(?<tagName>[A-Z0-9_\-.]+)#_", true);
            
        }
    }
}
#endif