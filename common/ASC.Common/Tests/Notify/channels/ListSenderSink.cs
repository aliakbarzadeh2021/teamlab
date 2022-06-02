#if DEBUG
namespace ASC.Common.Tests.Notify.channels
{
    using System.Collections;
    using ASC.Notify.Messages;
    using ASC.Notify.Sinks;

    public class ListSenderSink : SinkSkeleton, ISenderSink
    {

        readonly IList _SourceList = null;

        public ListSenderSink(IList sourceList)
        {
            _SourceList = sourceList;
        }
        #region ISink

        public override SendResponse ProcessMessage(INoticeMessage message)
        {
            _SourceList.Add(
                    new object[]{
                        message.Recipient.Addresses,
                        message.Subject,
                        message.Body
                    }
                );

            return new ASC.Notify.Messages.SendResponse(message) { Result = SendResult.Ok };
        }

        #endregion
    }
}
#endif