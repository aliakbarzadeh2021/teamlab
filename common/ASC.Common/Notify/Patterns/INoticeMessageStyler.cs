using ASC.Notify.Messages;
using ASC.Notify.Patterns;

namespace ASC.Common.Notify.Patterns
{
    public interface IPatternStyler
    {
        void ApplyFormating(NoticeMessage message);
    }
}