using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASC.Web.Controls.CalendarInfoHelper
{
    public class CalendarEvent
    {
        public string ID;
        public DateTime StartingDate;
        public DateTime EndingDate;
        public DateTime CreatedAt;
        public Guid Author;
        public string Title;
        public string Description;
        public string EventURl;
        public string BackgroundColor;
        public string FontColor;
    }

    public class CalendarEventRequest
    {
        public int prjID;
        public string title;

        public int startingYear;
        public int startingMonth;
        public int startingDay;
        public int startingHour;
        public int startingMinute;

        public int endingYear;
        public int endingMonth;
        public int endingDay;
        public int endingHour;
        public int endingMinute;

        public string bgColor;
        public string fontColor;
    }

    public class WrapperDayOfWeek
    {
        public string title;
        public string cssClass;
    }
}
