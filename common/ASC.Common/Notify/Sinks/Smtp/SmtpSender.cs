using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using ASC.Common.Notify;
using ASC.Notify.Engine;

namespace ASC.Notify.Sinks.Smtp
{
    public class SmtpSender
    {
        private readonly SmtpClient smtpClient;

        public Exception LastError { get; private set; }

        public SmtpSender(string servername, int port, ICredentialsByHost credentials, bool enableSsl)
        {
            smtpClient = new SmtpClient(servername, port) { Credentials = credentials, EnableSsl = enableSsl, };
        }

        public NoticeSendResult Send(MailMessage message)
        {
            if (ConfigurationManager.AppSettings["postman"] == "ASES")
            {
                return SendByAmazonSES(message);
            }
            else
            {
                return SendBySmtpClient(message);
            }
        }

        private NoticeSendResult SendByAmazonSES(MailMessage message)
        {
            var result = NoticeSendResult.TryOnceAgain;
            var AWSEmailClass = new AWSEmail();
            try
            {
                if (message == null) throw new ArgumentNullException("message");
                try
                {
                    AWSEmailClass.SendEmail(message);
                    LastError = null;
                    result = NoticeSendResult.OK;
                }
                catch (Exception exc)
                {
                    LastError = exc;
                    throw;
                }
            }
            catch (ArgumentNullException)
            {
                result = NoticeSendResult.MessageIncorrect;
            }
            catch (Exception)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            return result;
        }

        private NoticeSendResult SendBySmtpClient(MailMessage message)
        {
            var result = NoticeSendResult.TryOnceAgain;
            LastError = null;
            try
            {
                if (message == null) throw new ArgumentNullException("message");
                try
                {
                    smtpClient.Send(message);
                }
                catch (Exception exc)
                {
                    LastError = exc;
                    throw;
                }
                result = NoticeSendResult.OK;
            }
            catch (ArgumentNullException)
            {
                result = NoticeSendResult.MessageIncorrect;
            }
            catch (ArgumentOutOfRangeException)
            {
                result = NoticeSendResult.MessageIncorrect;
            }
            catch (ObjectDisposedException)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            catch (InvalidOperationException)
            {
                if (String.IsNullOrEmpty(smtpClient.Host) || smtpClient.Port == 0)
                    result = NoticeSendResult.SendingImpossible;
                else
                    result = NoticeSendResult.TryOnceAgain;
            }
            catch (SmtpFailedRecipientException ex)
            {
                if (
                    ex.StatusCode == SmtpStatusCode.MailboxBusy ||
                    ex.StatusCode == SmtpStatusCode.MailboxUnavailable ||
                    ex.StatusCode == SmtpStatusCode.ExceededStorageAllocation
                    )
                {
                    result = NoticeSendResult.TryOnceAgain;
                }
                else if (
                    ex.StatusCode == SmtpStatusCode.MailboxNameNotAllowed ||
                    ex.StatusCode == SmtpStatusCode.UserNotLocalWillForward ||
                    ex.StatusCode == SmtpStatusCode.UserNotLocalTryAlternatePath
                    )
                {
                    result = NoticeSendResult.MessageIncorrect;
                }
                else if (ex.StatusCode != SmtpStatusCode.Ok)
                    result = NoticeSendResult.TryOnceAgain;
            }
            catch (SmtpException)
            {
                result = NoticeSendResult.SendingImpossible;
            }
            return result;
        }
    }
}