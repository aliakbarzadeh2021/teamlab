using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace ASC.Common.Notify
{
    public class AWSEmail
    {
        private readonly AmazonSimpleEmailServiceClient _emailService;

        public AWSEmail()
        {
            var accessKey = ConfigurationManager.AppSettings["ses.accessKey"];
            var secretKey = ConfigurationManager.AppSettings["ses.secretKey"];

            _emailService = new AmazonSimpleEmailServiceClient(accessKey, secretKey);
        }

        public void SendEmail(MailMessage mailMessage)
        {
            var destination = new Destination
            {
                ToAddresses = mailMessage.To.Select(adresses => adresses.Address).ToList(),
                BccAddresses = mailMessage.Bcc.Select(adresses => adresses.Address).ToList(),
                CcAddresses = mailMessage.CC.Select(adresses => adresses.Address).ToList()
            };

            var body = new Body(new Content(mailMessage.Body));

            if (mailMessage.AlternateViews.Count > 0)
            {
                //Get html body
                //TODO: add content type check
                var stream = mailMessage.AlternateViews[0].ContentStream;
                var buf = new byte[stream.Length];
                stream.Read(buf, 0, buf.Length);
                body.Html = new Content(Encoding.UTF8.GetString(buf));
            }

            var message = new Message(new Content(mailMessage.Subject), body);

            var seRequest = new SendEmailRequest(mailMessage.From.Address, destination, message);
            seRequest.ReplyToAddresses.Add(mailMessage.ReplyTo.Address);

            _emailService.SendEmail(seRequest);
        }
    }
}
