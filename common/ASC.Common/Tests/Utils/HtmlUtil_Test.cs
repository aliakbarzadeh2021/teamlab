#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Common.Tests.Utils
{
    using NUnit.Framework;
    using ASC.Common.Utils;


    [TestFixture]
    public class HtmlUtil_Test
    {
        [Test]
        public void GetTextBr()
        {
            string html = "Hello";
            Assert.AreEqual("Hello", HtmlUtil.GetText(html));

            html = "Hello    anton";
            Assert.AreEqual("Hello    anton", HtmlUtil.GetText(html));

            html = "Hello<\\ br>anton";
            //Assert.AreEqual("Hello\n\ranton", HtmlUtil.GetText(html));
        }

        public void Hard()
        {
            string html = @"<a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/ViewBlog.aspx?blogID=94fae49d-2faa-46d3-bf34-655afbc6f7f4""><font size=""+1"">Неадекватное коммерческое предложение</font></a>
<div class=""moz-text-html"" lang=""x-unicode""><hr />
По работе много &quot;листаю&quot; спама, но пришел спам который меня заинтересовал:<br />
<blockquote> План действий, способствующих достижению успеха и богатства Аудиокнига mp3 &quot;ДУМАЙ И БОГАТЕЙ&quot;<br />
<br />
&quot;Думай и богатей&quot; - эта книга получила статус непревзойденного классического учебника по достижению богатства. В каждой главе автор упоминает о секрете добывания денег, пользуясь которым тысячи людей приобрели, преумножили и продолжают ...</blockquote>... <br />
<hr />
опубликовано <a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/UserPage.aspx?userid=731fa2f6-0283-41ab-b4a6-b014cc29f358"">Хурлапов Павел</a> 20 авг 2009 15:53<br />
<a href=""http://mediaserver:8080/Products/Community/Modules/Blogs/ViewBlog.aspx?blogID=94fae49d-2faa-46d3-bf34-655afbc6f7f4#comments"">прокомментировать</a></div>";

            System.Diagnostics.Trace.Write(HtmlUtil.GetText(html));
        }

    }
}
#endif