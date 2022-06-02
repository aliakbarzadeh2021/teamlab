#if DEBUG
namespace ASC.Common.Tests.Notify.channels
{
    using System;
    using System.IO;
    using ASC.Notify.Messages;
    using ASC.Notify.Sinks;

    public class FileSenderSink : SinkSkeleton, ISenderSink
    {

        #region ISink

        public override SendResponse ProcessMessage(INoticeMessage message)
        {
            string directory = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
            if (Properties.Contains("FileSenderSink.Directory"))
                directory = Properties["FileSenderSink.Directory"] as string;
            else
                Directory.CreateDirectory(directory);


            string fileName = DateTime.Now.ToString("dd.MM HH.mm.ss.zzz") + "_" + message.Subject + ".txt";

            var response = new SendResponse(message);
            response.Exception = null;
            response.Result = SendResult.Impossible;
            try
            {
                foreach (var address in message.Recipient.Addresses)
                    File.WriteAllText(
                            Path.Combine(Path.Combine(directory, address), fileName),
                            message.Body
                        );

                response.Result = SendResult.Ok;
            }
            catch (Exception exc)
            {
                response.Result = SendResult.Impossible;
                response.Exception = exc;
            }

            return response;
        }

        #endregion
    }
}
#endif