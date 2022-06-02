using System;

namespace ASC.Blogs.Core.Helpers
{
    public static class DateTimeHelper
    {
        public static string ToShortString(this DateTime targetDateTime)
        {
            return String.Format("{0} {1}", targetDateTime.ToShortDateString(), targetDateTime.ToShortTimeString());
        }
    }
}