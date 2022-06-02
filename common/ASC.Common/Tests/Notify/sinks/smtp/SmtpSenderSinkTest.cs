#if DEBUG
namespace ASC.Common.Tests.Notify.sinks.smtp
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Net.Mail;
    using ASC.Notify.Messages;
    using ASC.Notify.Recipients;
    using ASC.Notify.Sinks.Smtp;
    using NUnit.Framework;

    [TestFixture]
    public class SmtpSenderSinkTest
    {
        [Test]
        public void CtorPropertiesFillTest()
        {
            MailAddress address = new MailAddress("sm@m.ru", "anton");
            NetworkCredential ncred = new NetworkCredential("anton", "123");
            Hashtable prop = new Hashtable();
            prop.Add(SmtpSenderSink.SmtpPortParam, 112);
            prop.Add(SmtpSenderSink.SmtpServerNameParam, "m.ru");
            prop.Add(SmtpSenderSink.SmtpCredentialsParam, ncred);
            prop.Add(SmtpSenderSink.SmtpSenderParam, address);

            SmtpSenderSink sink = new SmtpSenderSink(prop);

            Assert.AreEqual(sink._sender, address);
            //Assert.AreEqual(sink._smtpSender.SmtpClient.Credentials, ncred);
            //Assert.AreEqual(sink._smtpSender.SmtpClient.Host, "m.ru");
            //Assert.AreEqual(sink._smtpSender.SmtpClient.Port, 112);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorPropertiesFailedTest1()
        {
            MailAddress address = new MailAddress("sm@m.ru", "anton");
            NetworkCredential ncred = new NetworkCredential("anton", "123");
            Hashtable prop = new Hashtable();
            prop.Add(SmtpSenderSink.SmtpPortParam, 112);
            //prop.Add(SmtpSenderSink.SmtpServerNameParam, "m.ru");
            prop.Add(SmtpSenderSink.SmtpCredentialsParam, ncred);
            prop.Add(SmtpSenderSink.SmtpSenderParam, address);

            SmtpSenderSink sink = new SmtpSenderSink(prop);

        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorPropertiesFailedTest2()
        {
            MailAddress address = new MailAddress("sm@m.ru", "anton");
            NetworkCredential ncred = new NetworkCredential("anton", "123");
            Hashtable prop = new Hashtable();
            prop.Add(SmtpSenderSink.SmtpPortParam, 112);
            prop.Add(SmtpSenderSink.SmtpServerNameParam, "m.ru");
            prop.Add(SmtpSenderSink.SmtpCredentialsParam, ncred);
            
            ////prop.Add(SmtpSenderSink.SmtpSenderParam, address);

            SmtpSenderSink sink = new SmtpSenderSink(prop);

        }

        [Test]
        public void TestBuildEMail() 
        {
            MailAddress address = new MailAddress("sm@m.ru", "anton");
            NetworkCredential ncred = new NetworkCredential("anton", "123");
            Hashtable prop = new Hashtable();
            prop.Add(SmtpSenderSink.SmtpPortParam, 112);
            prop.Add(SmtpSenderSink.SmtpServerNameParam, "m.ru");
            prop.Add(SmtpSenderSink.SmtpCredentialsParam, ncred);
            prop.Add(SmtpSenderSink.SmtpSenderParam, address);

            SmtpSenderSink sink = new SmtpSenderSink(prop);

            NoticeMessage mess = new NoticeMessage(
                    new DirectRecipient("1", "-=SA=-",new[] { "sm_anton@mail.ru" }),
                    "тема",
                    "телооооо",
                    "html"
                );
            MailMessage email =  sink.BuildMailMessage(mess);
        }


        [Test]
        public void TestBuildHtmlEMail()
        {
            MailAddress address = new MailAddress("Anton.Smirnov@avsmedia.net", "ASC System");
            //NetworkCredential ncred = new NetworkCredential("anton", "123");
            Hashtable prop = new Hashtable();
            //prop.Add(SmtpSenderSink.SmtpPortParam, 112);
            prop.Add(SmtpSenderSink.SmtpServerNameParam, "dbserverhp");
            //prop.Add(SmtpSenderSink.SmtpCredentialsParam, ncred);
            prop.Add(SmtpSenderSink.SmtpSenderParam, address);

            SmtpSenderSink sink = new SmtpSenderSink(prop);

            NoticeMessage mess = new NoticeMessage(
                    new DirectRecipient("1", "-=SA=-", new[] { "Anton.Smirnov@avsmedia.net", "smirnoff.anton@gmail.com" }),
                    "тема",
                    "<a href=\"http://www.google.com/intl/ru/about.html\">Всё о Google</a><b>mast be bold</b>",
                    "html"
                );

            sink.ProcessMessage(mess);
            //MailMessage email = sink.BuildMailMessage(mess);
        }
    }
}
#endif