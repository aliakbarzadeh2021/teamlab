#if DEBUG
namespace ASC.Common.Tests.Notify.patterns
{
    using System;
    using System.Diagnostics;
    using ASC.Notify.Messages;
    using ASC.Notify.Patterns;
    using ASC.Notify.Recipients;
    using NUnit.Framework;

    [TestFixture]
    public class NVelocityPatternFormatter_Test
    {
        IDirectRecipient _fakeRecipient = new DirectRecipient("usr1", "anton");

        #region RegExp

        [Test]
        public void SearchTags_Skobochki()
        {
            var patternBody = @"
                               ${BasePath}Milestones
                            ";
            var pattern = new Pattern("1", "2", "12", patternBody);

            NVelocityPatternFormatter formatter = new NVelocityPatternFormatter();
            ITag[] tags = formatter.GetTags(pattern);
            Assert.AreEqual(1, tags.Length);
        }
        [Test]
        public void SearchTags()
        {
            string text = @"$tag0
$tag1 $tag2.get($tag3) [$tag4,$tag5] $tag6 ${tag7} ${tag8.Name}  ${BasePath}Milestones
tag100 tag200.get(tag300) [tag400,tag500] tag600 ${ ${{tag700
asd  \$tag10 \\$tag11 \$tag20.get(\$tag30) [\$tag40,\$tag50] \$tag60
";

            var pattern = new Pattern("1", "2", "12", text);

            NVelocityPatternFormatter formatter = new NVelocityPatternFormatter();

            ITag[] tags = formatter.GetTags(pattern);
            Assert.AreEqual(10, tags.Length);

            Assert.AreEqual("tag0", tags[0].Name);
            Assert.AreEqual("tag1", tags[1].Name);
            Assert.AreEqual("tag2", tags[2].Name);
            Assert.AreEqual("tag3", tags[3].Name);
            Assert.AreEqual("tag4", tags[4].Name);
            Assert.AreEqual("tag5", tags[5].Name);
            Assert.AreEqual("tag6", tags[6].Name);
            Assert.AreEqual("tag7", tags[7].Name);
            Assert.AreEqual("tag8", tags[8].Name);
            Assert.AreEqual("BasePath", tags[9].Name);

        }
        #endregion

        #region форматирование

        [Test]
        public void Format()
        {
            var formatter = new NVelocityPatternFormatter();
            var pattern = new Pattern("1", "1",
                                 "оповещяю тебя, $user.Name",
                                "А вот и оповещение. $datetime");
            
            var msg = new NoticeMessage(
                                _fakeRecipient,
                                null,
                                null,
                                pattern);
            
            DateTime now = DateTime.Now;
            formatter.FormatMessage(
                        msg,
                        new ITagValue[]{
                            new TagValue("user",_fakeRecipient),
                            new TagValue("datetime",now)
                        }
                    );

            Assert.AreEqual("оповещяю тебя, anton", msg.Subject);
            Assert.AreEqual("А вот и оповещение. " + now.ToString(), msg.Body);
        }

        [Test]
        public void NotAllTags()
        {
            var formatter = new NVelocityPatternFormatter();
            var pattern = new Pattern("1", "1",
                                 "оповещяю тебя, $user.Name",
                                "А вот и оповещение. $datetime");

            var msg = new NoticeMessage(
                                _fakeRecipient,
                                null,
                                null,
                                pattern);

            DateTime now = DateTime.Now;
            formatter.FormatMessage(
                        msg,
                        new ITagValue[]{
                            new TagValue("user",_fakeRecipient)
                        }
                    );

            Assert.AreEqual("оповещяю тебя, anton", msg.Subject);
            Assert.AreEqual("А вот и оповещение. $datetime", msg.Body);
        }


        [Test]
        public void Blog_LoveAuthorTest()
        {
            string patternSubject = "Ваш любимый автор $UserName";
            string patternBody = 
@"Пользователь $UserName 
//#if($iscomment) 
прокомментировал блог <a href=""$posturl"">$postname</a>
//#else
добавил новый пост <a href=""$posturl"">$postname</a>
//#end
</hr>
$content
</hr>
<a href=""$actionurl"">$actionurl</a>";

            string postContent = "бла, бла";
            var formatter = new NVelocityPatternFormatter();
            var pattern = new Pattern("1", "1", patternSubject, patternBody);
            var msg = new NoticeMessage(new DirectRecipient("1","user"),null,null,pattern);
            
            formatter.FormatMessage(
                        msg,
                        new ITagValue[]{
                            new TagValue("UserName","Смирнов Антон"),
                            new TagValue("iscomment",false),
                            new TagValue("posturl","http://mediaserver:8080/Addons/Blogs/ViewBlog.aspx?blogID=b471f8c8-e021-4137-a114-c47c14e6693b"),
                            new TagValue("postname","Манифест Agile-разработки ПО"),
                            new TagValue("content",postContent),
                            new TagValue("actionurl","http://mediaserver:8080/Addons/Blogs/ViewBlog.aspx?blogID=b471f8c8-e021-4137-a114-c47c14e6693b")
                        }
                    );
            Trace.WriteLine(msg.Body);

            formatter.FormatMessage(
                        msg,
                        new ITagValue[]{
                            new TagValue("UserName","Смирнов Антон"),
                            new TagValue("iscomment",true),
                            new TagValue("posturl","http://mediaserver:8080/Addons/Blogs/ViewBlog.aspx?blogID=b471f8c8-e021-4137-a114-c47c14e6693b"),
                            new TagValue("postname","Манифест Agile-разработки ПО"),
                            new TagValue("content",postContent),
                            new TagValue("actionurl","http://mediaserver:8080/Addons/Blogs/ViewBlog.aspx?blogID=b471f8c8-e021-4137-a114-c47c14e6693b")
                        }
                    );
            Trace.WriteLine(msg.Body);
        }
        #endregion
    }
}
#endif